-- zrconfig-api-contract:1
-- 公共运行库：Lua 5.4，要求 64 位整数和双精度浮点；所有解析与索引构建发生在加载阶段。
local R = {}
local ARRAY = {}
local LOCKED = 'ZrConfig.ReadOnly'
local parser_context=setmetatable({},{__mode='k'})
local function parser_scope(action,code)
    local thread=coroutine.running();local previous=parser_context[thread];parser_context[thread]=code or'InvalidValue'
    local values=table.pack(pcall(action));parser_context[thread]=previous;if not values[1]then error(values[2],0)end;return table.unpack(values,2,values.n)
end
function R.invalid(message)local code=parser_context[coroutine.running()];if code then R.server_error(code,message)end;error('配置数据无效：' .. message, 2)end
local function reject_write() R.invalid('配置为只读对象') end
R.NULL = setmetatable({}, {__newindex=reject_write, __metatable=LOCKED})
function R.readonly(value, seen)
    if type(value) ~= 'table' or getmetatable(value) == LOCKED then return value end
    seen = seen or {}; local pending = {}
    -- 迭代冻结引用图，长引用链或循环引用不消耗 Lua 调用栈。
    local function wrap(v)
        if type(v) ~= 'table' or getmetatable(v) == LOCKED then return v end
        if seen[v] then return seen[v] end
        local backing, proxy = {}, {}; seen[v] = proxy
        local function iterate(_, key) return next(backing, key) end
        setmetatable(proxy, {__index=backing, __newindex=reject_write, __len=function() return #backing end,
            __pairs=function() return iterate,nil,nil end, __metatable=LOCKED})
        pending[#pending+1] = {v,backing}; return proxy
    end
    local result = wrap(value); local i = 1
    while i <= #pending do
        local source, backing = pending[i][1], pending[i][2]
        for k,v in pairs(source) do backing[k] = wrap(v) end
        i = i + 1
    end
    return result
end
R.empty = R.readonly({})
local function object(v) if type(v)~='table' or v==R.NULL or getmetatable(v)==ARRAY then R.invalid('需要对象') end; return v end
local function array(v) if type(v)~='table' or getmetatable(v)~=ARRAY then R.invalid('需要数组') end; return v end
local function list() return setmetatable({},ARRAY) end
local function text(v)if type(v)~='string'then R.invalid('需要字符串')end;if not utf8.len(v)then if parser_context[coroutine.running()]then R.server_error('InvalidEncoding')end;R.invalid('UTF-8 无效')end;if #v>16777216 then R.invalid('字符串过长')end;return v end
local function integer(v,lo,hi) if type(v)~='number' or v%1~=0 or v<lo or v>hi then R.invalid('整数超出范围') end; return v end
local function count(t) local n=0;for _ in pairs(t) do n=n+1 end;return n end
local function contains(t,v) for _,x in ipairs(t) do if x==v then return true end end;return false end
local function decimal_integer(v, unsigned)
    text(v); local negative=v:sub(1,1)=='-';local digits=negative and v:sub(2) or v
    if not digits:match('^%d+$') or (#digits>1 and digits:sub(1,1)=='0') or (unsigned and negative) then R.invalid('64 位整数格式无效') end
    local limit=unsigned and '18446744073709551615' or (negative and '9223372036854775808' or '9223372036854775807')
    if #digits>#limit or (#digits==#limit and digits>limit) then R.invalid('64 位整数越界') end
    if unsigned then return digits end
    return math.tointeger(tonumber(v)) or R.invalid('需要 64 位 Lua 整数')
end
-- decimal 在加载/构造时规范化为精确字符串；查询应复用此结果，不在循环内解析。
function R.decimal(v)
    text(v);local sign,whole,fraction=v:match('^(%-?)(%d+)%.(%d+)$')
    if not whole then sign,whole=v:match('^(%-?)(%d+)$');fraction='' end
    if not whole or #fraction>28 then R.invalid('decimal 格式或精度无效') end
    local coefficient=(whole..fraction):gsub('^0+','');local limit='79228162514264337593543950335'
    if #coefficient>#limit or (#coefficient==#limit and coefficient>limit) then R.invalid('decimal 越界') end
    whole=whole:gsub('^0+','');if whole=='' then whole='0' end
    fraction=fraction:gsub('0+$','');local magnitude=whole..(fraction~='' and '.'..fraction or '')
    return (magnitude~='0' and sign or '')..magnitude
end
local function parse_json(source,bounds)
    if type(source)~='string' or not utf8.len(source) then R.invalid('JSON UTF-8 无效') end;local pos,nodes=1,0
    local function ws() while source:sub(pos,pos):match('[ \t\r\n]') do pos=pos+1 end end
    local function limit()R.server_error('LimitExceeded','JSON allocation budget exceeded',{stage='parse'})end
    local function string_value()
        pos=pos+1;local pieces={};local bytes=0
        local function push(piece)bytes=bytes+#piece;if bounds and bytes>bounds.string_bytes then limit()end;pieces[#pieces+1]=piece end
        local start=pos
        while pos<=#source do
            if bounds and bytes+pos-start>bounds.string_bytes then limit()end
            local c=source:byte(pos)
            if c==34 then push(source:sub(start,pos-1));pos=pos+1;return text(table.concat(pieces)) end
            if c<32 then R.invalid('字符串含控制字符') end
            if c==92 then
                push(source:sub(start,pos-1));pos=pos+1;local e=source:sub(pos,pos);pos=pos+1
                local escapes={['"']='"',['\\']='\\',['/']='/',b='\b',f='\f',n='\n',r='\r',t='\t'}
                if e=='u' then
                    local hex=source:sub(pos,pos+3);if #hex~=4 or not hex:match('^%x+$') then R.invalid('Unicode 转义无效') end
                    local cp=tonumber(hex,16);pos=pos+4
                    if cp>=0xd800 and cp<=0xdbff then
                        if source:sub(pos,pos+1)~='\\u' then R.invalid('孤立代理项') end
                        local lowhex=source:sub(pos+2,pos+5);local low=lowhex:match('^%x%x%x%x$') and tonumber(lowhex,16)
                        if not low or low<0xdc00 or low>0xdfff then R.invalid('孤立代理项') end
                        cp=0x10000+(cp-0xd800)*1024+low-0xdc00;pos=pos+6
                    elseif cp>=0xdc00 and cp<=0xdfff then R.invalid('孤立代理项') end
                    push(utf8.char(cp))
                elseif escapes[e] then push(escapes[e]) else R.invalid('字符串转义无效') end
                start=pos
            else pos=pos+1 end
        end
        R.invalid('字符串未结束')
    end
    local value
    value=function(depth,rows)
        if bounds and depth>bounds.depth then limit()end
        ws();nodes=nodes+1;if nodes>32000000 or depth>72 then R.invalid('JSON 超限') end
        local c=source:sub(pos,pos)
        if c=='"' then return string_value() end
        if c=='{' then
            pos=pos+1;local v={};local members=0;ws();if source:sub(pos,pos)=='}' then pos=pos+1;return v end
            while true do
                if bounds and members>=bounds.members then limit()end;members=members+1
                ws();if source:sub(pos,pos)~='"' then R.invalid('成员名无效') end
                local name=string_value();if v[name]~=nil then R.invalid('JSON 成员重复') end
                ws();if source:sub(pos,pos)~=':' then R.invalid('缺少冒号') end;pos=pos+1;v[name]=value(depth+1,depth==0 and name==(bounds and bounds.root_array or'rows'));ws()
                local sep=source:sub(pos,pos);pos=pos+1;if sep=='}' then return v end;if sep~=',' then R.invalid('对象分隔符无效') end
            end
        end
        if c=='[' then
            pos=pos+1;local v=list();ws();if source:sub(pos,pos)==']' then pos=pos+1;return v end
            while true do if bounds and #v>=(rows and bounds.rows or bounds.elements)then limit()end;v[#v+1]=value(depth+1);ws();local sep=source:sub(pos,pos);pos=pos+1;if sep==']' then return v end;if sep~=',' then R.invalid('数组分隔符无效') end end
        end
        if source:sub(pos,pos+3)=='true' then pos=pos+4;return true end
        if source:sub(pos,pos+4)=='false' then pos=pos+5;return false end
        if source:sub(pos,pos+3)=='null' then pos=pos+4;return R.NULL end
        local start=pos
        if c=='-' then pos=pos+1 end
        if source:sub(pos,pos)=='0' then pos=pos+1
        elseif source:sub(pos,pos):match('[1-9]') then repeat pos=pos+1 until not source:sub(pos,pos):match('%d')
        else R.invalid('JSON 数字无效') end
        if source:sub(pos,pos)=='.' then pos=pos+1;if not source:sub(pos,pos):match('%d') then R.invalid('小数无效') end;repeat pos=pos+1 until not source:sub(pos,pos):match('%d') end
        if source:sub(pos,pos):match('[eE]') then
            pos=pos+1;if source:sub(pos,pos):match('[+-]') then pos=pos+1 end
            if not source:sub(pos,pos):match('%d') then R.invalid('指数无效') end;repeat pos=pos+1 until not source:sub(pos,pos):match('%d')
        end
        if pos-start>1024 then R.invalid('数字过长') end
        local n=tonumber(source:sub(start,pos-1));if not n or n==math.huge or n==-math.huge then R.invalid('非有限数字') end;return n
    end
    local result=value(0);ws();if pos~=#source+1 then R.invalid('JSON 尾部数据') end;return result
end
function R.parse(source)return parse_json(source,nil)end
local function json_bounds(schema,limits)
    if not limits then return nil end
    local bounds={rows=limits.max_rows,elements=math.max(64,limits.max_container_elements),members=math.max(4,#schema.fields+1),string_bytes=math.max(256,limits.max_string_bytes),depth=math.min(72,limits.max_depth*2+8)}
    local function name(value)bounds.string_bytes=math.max(bounds.string_bytes,#value)end;name(schema.name);for _,field in ipairs(schema.fields)do name(field.name)end;for _,kind in ipairs(schema.types)do name(kind.id);bounds.members=math.max(bounds.members,#kind.fields+1);for _,field in ipairs(kind.fields)do name(field.name)end end;return bounds
end
local function words_decimal(words)
    local digits={}
    repeat
        local carry=0;local nonzero=false
        for i=#words,1,-1 do local n=carry*4294967296+words[i];words[i]=n//10;carry=n%10;if words[i]~=0 then nonzero=true end end
        digits[#digits+1]=string.char(48+carry)
        if not nonzero then break end
    until false
    local result={};for i=#digits,1,-1 do result[#result+1]=digits[i] end;return table.concat(result)
end
local Cursor={};Cursor.__index=Cursor
function Cursor:unpack(format,size)
    if size>(self.limit or #self.data)-self.pos+1 then R.invalid('二进制截断') end
    local n;n,self.pos=string.unpack('<'..format,self.data,self.pos);return n
end
function Cursor:i32() return self:unpack('i4',4) end
function Cursor:count(max) return integer(self:i32(),0,max or 1000000) end
function Cursor:boolean() local v=self:unpack('I1',1);if v>1 then R.invalid('布尔编码无效') end;return v==1 end
function Cursor:string()
    local n=self:i32();if n==-1 then return '' end
    if n<0 or n>16777216 or n>(self.limit or #self.data)-self.pos+1 then R.invalid('字符串长度无效') end
    if self.max_string_bytes and n>self.max_string_bytes then R.server_error('LimitExceeded')end
    local s=self.data:sub(self.pos,self.pos+n-1);self.pos=self.pos+n;return text(s)
end
function Cursor:scalar(kind)
    if kind=='string' then return self:string() elseif kind=='bool' then return self:boolean() end
    if kind=='int64' then return tostring(self:unpack('i8',8)) end
    if kind=='uint64' then return words_decimal({self:unpack('I4',4),self:unpack('I4',4)}) end
    if kind=='decimal' then
        local digits=words_decimal({self:unpack('I4',4),self:unpack('I4',4),self:unpack('I4',4)});local flags=self:unpack('I4',4);local scale=(flags>>16)&255
        if flags&0x7f00ffff~=0 or scale>28 then R.invalid('decimal 编码无效') end
        if scale>0 then if #digits<=scale then digits=string.rep('0',scale-#digits+1)..digits end;digits=digits:sub(1,#digits-scale)..'.'..digits:sub(#digits-scale+1) end
        return (flags>>31~=0 and '-' or '')..digits
    end
    if kind=='float32' then return self:unpack('f',4) elseif kind=='float64' then return self:unpack('d',8) end
    local unsigned=kind:sub(1,4)=='uint';local bits=tonumber(kind:sub(unsigned and 5 or 4));return self:unpack((unsigned and 'I' or 'i')..bits//8,bits//8)
end
local Wire={};Wire.__index=Wire
function Wire:claim(n)if self.limits and(n>self.limits.max_container_elements or n>self.remaining)then R.server_error('LimitExceeded')end;integer(n,0,math.min(1000000,self.remaining));self.remaining=self.remaining-n end
local function field_failure(failure,prefix)
    if type(failure)~='table'or not failure.code then error(failure,0)end
    local child=failure.field_path or'';error(R.config_error(failure.code,failure,{stage=failure.stage,field_path=prefix..(child~=''and child:sub(1,1)~='['and'.'or'')..child}),0)
end
function Wire:read_fields(fields,r,depth)
    local v={};for _,f in ipairs(fields) do local ok,value=pcall(self.read_value,self,f.type,r,depth+1);if not ok then field_failure(value,f.name)end;v[f.name]=value end;return v
end
local function nested_read(self,id,r,depth,path)local ok,value=pcall(self.read_value,self,id,r,depth);if not ok then field_failure(value,path)end;return value end
local function nested_convert(self,id,v,depth,path)local ok,value=pcall(self.convert,self,id,v,depth);if not ok then field_failure(value,path)end;return value end
function Wire:read_value(id,r,depth)
    if self.limits and depth>self.limits.max_depth then R.server_error('LimitExceeded')end;if depth>64 then R.invalid('嵌套过深') end;local t=self.types[id];local kind=t.kind
    if self.limits then r.max_string_bytes=self.limits.max_string_bytes end
    if kind=='primitive' or kind=='enum' or kind=='tablereference' then return r:scalar(t.primitive) end
    if kind=='rawjson' then return r:string() end
    if kind=='nullable' then if r:boolean() then return self:read_value(t.element,r,depth+1) else return R.NULL end end
    if kind=='array' then
        local dims,n=list(),1;for i=1,t.rank do local d=r:count();dims[i]=d;n=n*d;if n>1000000 then R.invalid('矩阵过大') end end
        self:claim(n);local values=list();for i=1,n do values[i]=nested_read(self,t.element,r,depth+1,'['..(i-1)..']') end
        if t.rank==1 then return values else return {dimensions=dims,values=values} end
    end
    if kind=='dictionary' then local n=r:count();self:claim(n);local values=list();for i=1,n do local p=list();p[1]=nested_read(self,t.key,r,depth+1,'['..(i-1)..'].key');p[2]=nested_read(self,t.element,r,depth+1,'['..(i-1)..'].value');values[i]=p end;return values end
    if #t.variants>0 then
        local tag=r:i32();if tag==-1 then return R.NULL end;local name=self.tags[tag+1]
        if not name or not contains(t.variants,name) then R.invalid('多态标签无效') end
        local v=self:read_fields(self.types[name].fields,r,depth);v['$type']=name;return v
    end
    if t.nullableObject and not r:boolean() then return R.NULL end
    return self:read_fields(t.fields,r,depth)
end
function Wire:scalar(kind,v)
    if kind=='string' then return text(v) end
    if kind=='bool' then if type(v)~='boolean' then R.invalid('需要布尔值') end;return v end
    if kind=='int64' or kind=='uint64' then return decimal_integer(v,kind=='uint64') end
    if kind=='decimal' then return R.decimal(v) end
    if kind=='float32' or kind=='float64' then
        if type(v)~='number' or v~=v or math.abs(v)==math.huge then R.invalid('浮点数无效') end
        if kind=='float32' then v=string.unpack('<f',string.pack('<f',v));if math.abs(v)==math.huge then R.invalid('浮点数越界') end end;return v
    end
    local unsigned=kind:sub(1,4)=='uint';local bits=tonumber(kind:sub(unsigned and 5 or 4))
    return integer(v,unsigned and 0 or -(2^(bits-1)),unsigned and 2^bits-1 or 2^(bits-1)-1)
end
function Wire:fields(fields,v,depth,tagged)
    object(v);if count(v)~=#fields+(tagged and 1 or 0) then R.invalid('未知或缺失成员') end
    local result={};for _,f in ipairs(fields) do if v[f.name]==nil then R.invalid('缺失成员') end;local ok,value=pcall(self.convert,self,f.type,v[f.name],depth+1);if not ok then field_failure(value,f.name)end;result[f.name]=value end
    if tagged then result['$type']=v['$type'] end;return result
end
function Wire:convert(id,v,depth)
    if self.limits and depth>self.limits.max_depth then R.server_error('LimitExceeded')end;if depth>64 then R.invalid('嵌套过深') end;local t=self.types[id];local kind=t.kind
    if kind=='primitive' or kind=='enum' or kind=='tablereference' then
        if self.limits and t.primitive=='string'and type(v)=='string'and #v>self.limits.max_string_bytes then R.server_error('LimitExceeded')end
        local result=self:scalar(t.primitive,v);if kind=='enum' and not contains(t.enumValues,tostring(result)) then R.invalid('未知枚举值') end;return result
    end
    if kind=='rawjson' then text(v);if self.limits and #v>self.limits.max_string_bytes then R.server_error('LimitExceeded')end;R.parse(v);return v end
    if kind=='nullable' then if v==R.NULL then return v end;return self:convert(t.element,v,depth+1) end
    if kind=='array' then
        local values,dimensions
        if t.rank==1 then values=array(v) else
            object(v);if count(v)~=2 then R.invalid('矩阵字段无效') end;dimensions=array(v.dimensions);values=array(v.values)
            if #dimensions~=t.rank then R.invalid('矩阵维数错误') end;local n=1
            for _,d in ipairs(dimensions) do n=n*integer(d,0,1000000);if n>1000000 then R.invalid('矩阵过大') end end
            if n~=#values then R.invalid('矩阵长度错误') end
        end
        self:claim(#values);local result={};for i,x in ipairs(values) do result[i]=nested_convert(self,t.element,x,depth+1,'['..(i-1)..']') end
        return R.readonly(t.rank==1 and result or {dimensions=dimensions,values=result})
    end
    if kind=='dictionary' then
        array(v);self:claim(#v);local values,entries={},{}
        for i,p in ipairs(v) do
            if #array(p)~=2 then R.invalid('字典项无效') end
            local k=nested_convert(self,t.key,p[1],depth+1,'['..(i-1)..'].key');local x=nested_convert(self,t.element,p[2],depth+1,'['..(i-1)..'].value')
            if values[k]~=nil then R.invalid('字典键重复') end;values[k]=x;entries[i]={k,x}
        end
        return R.readonly({size=#entries,entries=entries,get=function(k) return values[k] end})
    end
    if v==R.NULL and t.nullableObject then return v end
    if #t.variants>0 then local tag=text(object(v)['$type']);if not contains(t.variants,tag) then R.invalid('未知多态类型') end;return R.readonly(self:fields(self.types[tag].fields,v,depth,true)) end
    return R.readonly(self:fields(t.fields,v,depth,false))
end
function Wire:read(bytes,json)
    local rows
    if json then
        local doc=object(R.parse(bytes));if count(doc)~=4 or doc.format~='ZrConfig.TableJson/1' or doc.table~=self.schema.name or doc.schemaHash~=self.schema.hash then R.invalid('JSON 表头不匹配') end
        rows=array(doc.rows);if #rows>5000000 then R.invalid('行数过大') end
    else
        local r=setmetatable({data=bytes,pos=1},Cursor)
        if r:i32()~=0x4643525a or r:i32()~=4 or r:string()~=self.schema.name or r:string()~=self.schema.hash then R.invalid('二进制表头不匹配') end
        local n=r:count(5000000);if n*12>#bytes-r.pos+1 then R.invalid('行索引过短') end;local nt=r:count(#self.schema.tags);local seen={}
        for i=1,nt do local tag=r:string();if not contains(self.schema.tags,tag) or seen[tag] then R.invalid('类型目录无效') end;seen[tag]=true;self.tags[i]=tag end
        local indexes,finish={},0
        for i=1,n do
            self.remaining=8000000;local keys={}
            for k,name in ipairs(self.schema.keys) do for _,f in ipairs(self.schema.fields) do if f.name==name then keys[k]=self:read_value(f.type,r,0);break end end end
            local id,offset,length=r:i32(),r:i32(),r:i32();if offset~=finish or length<0 then R.invalid('行索引不连续') end;finish=finish+length
            indexes[i]={keys=keys,id=id,offset=offset,length=length}
        end
        local start=r.pos;if start+finish~=#bytes+1 then R.invalid('文件长度不匹配') end;rows={}
        for i,index in ipairs(indexes) do
            self.remaining=8000000;local row=self:read_fields(self.schema.fields,r,0)
            if r.pos~=start+index.offset+index.length or index.id~=(self.schema.hasId and row.Id or 0) then R.invalid('行边界或 Id 不匹配') end
            for k,name in ipairs(self.schema.keys) do if row[name]~=index.keys[k] then R.invalid('主键与索引不匹配') end end;rows[i]=row
        end
    end
    local result={};for i,row in ipairs(rows) do self.remaining=8000000;result[i]=self:fields(self.schema.fields,row,0,false) end;return result
end
local function encode_json(value)
    if value==R.NULL then return 'null' end
    local kind=type(value)
    if kind=='string' then return '"'..value:gsub('[%z\1-\31\\"]',function(c)if c=='"' or c=='\\' then return '\\'..c end;return string.format('\\u%04x',c:byte())end)..'"' end
    if kind=='boolean' then return value and 'true' or 'false' end
    if kind=='number' then return math.type(value)=='integer' and tostring(value) or string.format('%.17g',value) end
    local parts={}
    if getmetatable(value)==ARRAY then for i,v in ipairs(value)do parts[i]=encode_json(v)end;return '['..table.concat(parts,',')..']' end
    local names={};for k in pairs(value)do names[#names+1]=k end;table.sort(names)
    for _,k in ipairs(names)do parts[#parts+1]=encode_json(k)..':'..encode_json(value[k])end;return '{'..table.concat(parts,',')..'}'
end
local function indexed_rows(wire,bytes,json,budget)
    local schema=wire.schema;local keys,ids,offsets,lengths,fragments={},{},{},{},nil;local n;local bounds=json_bounds(schema,wire.limits)
    if json then
        local doc=object(parse_json(bytes,bounds));if count(doc)~=4 or doc.format~='ZrConfig.TableJson/1' or doc.table~=schema.name or doc.schemaHash~=schema.hash then R.server_error('SchemaMismatch')end
        local rows=array(doc.rows);n=integer(#rows,0,5000000);fragments={}
        for i,row in ipairs(rows)do
            wire.remaining=wire.limits and wire.limits.max_container_elements or 8000000;local key={}
            for _,name in ipairs(schema.keys)do for _,field in ipairs(schema.fields)do if field.name==name then key[name]=wire:convert(field.type,row[name],0);break end end end
            keys[i]=key;ids[i]=schema.hasId and integer(row.Id,-2147483648,2147483647)or 0;fragments[i]=encode_json(row)
        end
        bytes=nil
    else
        local cursor=setmetatable({data=bytes,pos=1},Cursor)
        if cursor:i32()~=0x4643525a or cursor:i32()~=4 or cursor:string()~=schema.name or cursor:string()~=schema.hash then R.server_error('SchemaMismatch')end
        n=cursor:count(5000000);if n*12>#bytes-cursor.pos+1 then R.server_error('InvalidValue')end
        local nt=cursor:count(#schema.tags);local seen={}
        for i=1,nt do local tag=cursor:string();if not contains(schema.tags,tag)or seen[tag]then R.server_error('InvalidValue')end;seen[tag]=true;wire.tags[i]=tag end
        local finish=0
        for i=1,n do
            wire.remaining=wire.limits and wire.limits.max_container_elements or 8000000;local key={}
            for _,name in ipairs(schema.keys)do for _,field in ipairs(schema.fields)do if field.name==name then key[name]=wire:convert(field.type,wire:read_value(field.type,cursor,0),0);break end end end
            keys[i]=key;ids[i]=cursor:i32();local offset,length=cursor:i32(),cursor:i32()
            if offset~=finish or length<0 or length>#bytes-finish then R.server_error('InvalidValue')end;offsets[i]=offset;lengths[i]=length;finish=finish+length
        end
        if cursor.pos+finish~=#bytes+1 then R.server_error('InvalidValue')end
        for i=1,n do offsets[i]=offsets[i]+cursor.pos end
    end
    local references={};if schema.hasId then for i,id in ipairs(ids)do references[id]=references[id]and 0 or i end end
    local raw=bytes and #bytes or 0;if fragments then for _,fragment in ipairs(fragments)do raw=raw+#fragment end end;if budget then budget.raw(raw)end
    local result={count=n,schema=schema,keys=R.index(keys,schema.keys,true),budget=budget}
    local index_raw_bytes=fragments and 0 or #bytes;if fragments then for _,part in ipairs(fragments)do index_raw_bytes=index_raw_bytes+#part end end
    function result.estimated_index_bytes(fields)return 256+n*512*math.max(1,fields)+8*index_raw_bytes end
    function result.estimated_row_bytes(index)return 128+4*(fragments and #fragments[index]or lengths[index])end
    -- Replace projected key rows with ordinals without retaining complete business rows.
    for i,key in ipairs(keys)do local node=result.keys;for k,name in ipairs(schema.keys)do if k==#schema.keys then node[key[name]]=i else node=node[key[name]]end end end
    local resolver
    function result.bind_references(resolve)resolver=resolve end
    local function read(index)
        wire.remaining=wire.limits and wire.limits.max_container_elements or 8000000;local raw
        if fragments then raw=parse_json(fragments[index],bounds)else
            local cursor=setmetatable({data=bytes,pos=offsets[index],limit=offsets[index]+lengths[index]-1},Cursor)
            raw=wire:read_fields(schema.fields,cursor,0);if cursor.pos~=offsets[index]+lengths[index]then R.server_error('InvalidValue')end
        end
        wire.remaining=wire.limits and wire.limits.max_container_elements or 8000000;local row=wire:fields(schema.fields,raw,0,false)
        if ids[index]~=(schema.hasId and row.Id or 0)then R.server_error('InvalidValue')end
        for name,value in pairs(keys[index])do if row[name]~=value then R.server_error('InvalidValue')end end
        if resolver then result.validate(row,resolver)end
        return row
    end
    function result.read_transient(index)local estimate=result.estimated_row_bytes(index);if budget then budget.reserve_transient(estimate)end;local ok,value=pcall(result.read,index);if budget then budget.finish_transient(estimate)end;if not ok then error(value,0)end;return value end
    function result.read(index)
        local ok,value=pcall(function()return parser_scope(function()return read(index)end)end)
        if ok then return value end
        if type(value)=='table'and value.code then local parts={};for _,name in ipairs(schema.keys)do parts[#parts+1]=name..'='..tostring(keys[index][name])end;error(R.config_error(value.code,value,{stage=value.stage~=''and value.stage or'row',version_id=result.version or'',table=schema.name,row_key=table.concat(parts,','),field_path=value.field_path or'',relative_path=result.relative_path or''}),0)end;error(value,0)
    end
    function result.reference(id)local ordinal=references[id];if not ordinal or ordinal==0 then R.server_error('MissingReference')end;return ordinal end
    function result.validate(row,resolve)
        for _,field in ipairs(schema.fields)do local t=wire.types[field.type];if t.kind=='tablereference'and row[field.name]~=0 then local ok,failure=pcall(function()return resolve(t.referenceTable).reference(row[field.name])end);if not ok then if type(failure)=='table'and failure.code then error(R.config_error(failure.code,failure,{stage='reference',field_path=field.name}),0)end;error(failure,0)end end end
    end
    return result
end
-- SHA-256 只用于文件完整性检查，不作为签名或身份认证。
local K={0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2}
local function rotate(x,n) return ((x>>n)|(x<<(32-n)))&0xffffffff end
function R.sha256(bytes)
    local size=#bytes;bytes=bytes..'\128'..string.rep('\0',(55-size)%64)..string.pack('>I8',size*8)
    local h={0x6a09e667,0xbb67ae85,0x3c6ef372,0xa54ff53a,0x510e527f,0x9b05688c,0x1f83d9ab,0x5be0cd19};local w={}
    for start=1,#bytes,64 do
        for i=1,16 do w[i]=string.unpack('>I4',bytes,start+(i-1)*4) end
        for i=17,64 do local x,y=w[i-15],w[i-2];w[i]=(w[i-16]+(rotate(x,7)~rotate(x,18)~(x>>3))+w[i-7]+(rotate(y,17)~rotate(y,19)~(y>>10)))&0xffffffff end
        local a,b,c,d,e,f,g,j=table.unpack(h)
        for i=1,64 do
            local t1=(j+(rotate(e,6)~rotate(e,11)~rotate(e,25))+((e&f)~((~e)&g))+K[i]+w[i])&0xffffffff
            local t2=((rotate(a,2)~rotate(a,13)~rotate(a,22))+((a&b)~(a&c)~(b&c)))&0xffffffff
            j,g,f,e,d,c,b,a=g,f,e,(d+t1)&0xffffffff,c,b,a,(t1+t2)&0xffffffff
        end
        local v={a,b,c,d,e,f,g,j};for i=1,8 do h[i]=(h[i]+v[i])&0xffffffff end
    end
    local result={};for i=1,8 do result[i]=string.format('%08x',h[i]) end;return table.concat(result)
end
local function segment(v) text(v);if v=='' or v=='.' or v=='..' or v:find('[\\/:%z\1-\31]') then R.invalid('路径无效') end;return v end
local function hash(v) text(v);if #v~=64 or not v:match('^%x+$') then R.invalid('SHA-256 无效') end;return v:lower() end
local function read_file(path)
    local f=io.open(path,'rb');if not f then R.invalid('无法读取文件：'..path) end
    local bytes=f:read('a');f:close();if not bytes then R.invalid('读取失败') end;return bytes
end
function R.bundle(root)
    if string.packsize('j')~=8 or math.maxinteger~=9223372036854775807 or string.packsize('n')~=8 then R.invalid('需要 64 位整数及双精度 Lua') end
    local manifest=object(R.parse(read_file(root..'/current.json')))
    if not contains({'ZrConfig.Binary/1','ZrConfig.Json/1','ZrConfig.Mixed/1'},manifest.format) then R.invalid('清单格式无效') end
    local version=segment(manifest.versionId);local entries={}
    for _,entry in ipairs(array(manifest.tables)) do
        local name=segment(entry.name);if entries[name] then R.invalid('表重复') end
        if entry.file~=name..(manifest.format=='ZrConfig.Json/1' and '.json' or '.bytes') then R.invalid('表文件名无效') end
        hash(entry.sha256);hash(entry.schemaHash);integer(entry.rows,0,5000000)
        if manifest.format=='ZrConfig.Mixed/1' then if entry.jsonFile~=name..'.json' then R.invalid('JSON 文件名无效') end;hash(entry.jsonSha256)
        elseif (entry.jsonFile and entry.jsonFile~=R.NULL and entry.jsonFile~='') or (entry.jsonSha256 and entry.jsonSha256~=R.NULL and entry.jsonSha256~='') then R.invalid('格式与备用文件不一致') end
        entries[name]=entry
    end
    return {version=version,validate_tables=function(expected)
        local count=0;for _ in pairs(entries) do count=count+1 end
        if count~=#expected then R.invalid('清单表集合不匹配') end
        for _,name in ipairs(expected) do if not entries[name] then R.invalid('清单表集合不匹配') end end
    end,validate_catalog=function(catalog)
        if catalog.target and catalog.target~=""and catalog.target~=manifest.target then R.server_error('CatalogMismatch')end
        if count(entries)~=#catalog.tables then R.server_error('TableSetMismatch')end
        for _,table in ipairs(catalog.tables) do
            if not entries[table.name] then R.server_error('TableSetMismatch')end
            if entries[table.name].schemaHash~=table.schemaHash then R.server_error('SchemaMismatch')end
        end
    end,open_rows=function(schema,prefer_json)
        local s=R.parse(schema);local wire=setmetatable({schema=s,types={},tags={},remaining=8000000},Wire)
        for _,t in ipairs(s.types)do wire.types[t.id]=t end
        local entry=entries[s.name];if not entry or entry.schemaHash~=s.hash then R.server_error('SchemaMismatch')end
        local alternate=manifest.format=='ZrConfig.Mixed/1'and prefer_json
        local bytes=read_file(root..'/versions/'..version..'/'..(alternate and entry.jsonFile or entry.file))
        if R.sha256(bytes)~=hash(alternate and entry.jsonSha256 or entry.sha256)then R.server_error('IntegrityMismatch')end
        local rows=indexed_rows(wire,bytes,manifest.format=='ZrConfig.Json/1'or alternate);rows.version=manifest.versionId;rows.relative_path='versions/'..manifest.versionId..'/'..(alternate and entry.jsonFile or entry.file);if rows.count~=entry.rows then R.server_error('InvalidValue')end;return rows
    end,load=function(schema,prefer_json)
        local s=R.parse(schema);local wire=setmetatable({schema=s,types={},tags={},remaining=8000000},Wire)
        for _,t in ipairs(s.types) do wire.types[t.id]=t end
        local entry=entries[s.name];if not entry or entry.schemaHash~=s.hash then R.invalid('表结构不匹配') end
        local alternate=manifest.format=='ZrConfig.Mixed/1' and prefer_json
        local bytes=read_file(root..'/versions/'..version..'/'..(alternate and entry.jsonFile or entry.file))
        if R.sha256(bytes)~=hash(alternate and entry.jsonSha256 or entry.sha256) then R.invalid('文件完整性校验失败') end
        local rows=wire:read(bytes,manifest.format=='ZrConfig.Json/1' or alternate);if #rows~=entry.rows then R.invalid('行数不匹配') end;return rows
    end}
end
local LIMIT_DEFAULTS={max_manifest_bytes=16<<20,max_table_bytes=512<<20,max_bundle_bytes=1<<30,max_tables=100000,max_rows=5000000,max_decoded_rows=5000000,max_container_elements=1000000,max_depth=64,max_string_bytes=16<<20,max_concurrent_reads=1,max_concurrent_decodes=1,max_pending_requests=4096,max_estimated_decoded_bytes=2<<30,max_estimated_index_bytes=2<<30}
function R.resource_limits(options)
    options=options or{};if type(options)~='table'then R.server_error('InvalidPlan')end
    local result={};for key,value in pairs(options)do if LIMIT_DEFAULTS[key]==nil then R.server_error('UnsupportedCapability','Unknown limit: '..tostring(key))end;if math.type(value)~='integer'or value<=0 then R.server_error('InvalidPlan','Limits must be positive integers')end;result[key]=value end
    for key,value in pairs(LIMIT_DEFAULTS)do if result[key]==nil then result[key]=value end end
    for _,key in ipairs({'max_rows','max_decoded_rows','max_container_elements','max_depth','max_string_bytes'})do if result[key]>LIMIT_DEFAULTS[key]then R.server_error('UnsupportedCapability','Limit exceeds wire capabilities')end end
    if result.max_concurrent_reads~=1 or result.max_concurrent_decodes~=1 then R.server_error('UnsupportedCapability','Lua core uses synchronous sources and decoders')end
    return R.readonly(result)
end
local function source_path(name)
    if type(name)~='string'or name==''or name:find('[\\:%z]')or name:sub(1,1)=='/'or name:sub(-1)=='/'or name:find('//',1,true)then R.server_error('InvalidPlan','Invalid relative source path')end
    for part in name:gmatch('[^/]+')do if part:sub(1,1)=='.'then R.server_error('InvalidPlan','Invalid relative source path')end end
end
function R.memory_source(files)
    if type(files)~='table'then R.server_error('InvalidPlan')end;local owned={}
    for name,bytes in pairs(files)do source_path(name);if type(bytes)~='string'then R.server_error('InvalidPlan','Sources return immutable byte strings')end;owned[name]=bytes end
    return R.readonly({capabilities={synchronous=true,verified_paths=true},read=function(name,maximum,cancellation)
        cancellation.check();source_path(name);if math.type(maximum)~='integer'or maximum<=0 then R.server_error('InvalidPlan')end
        local bytes=owned[name];if bytes==nil then R.server_error('SourceUnavailable')end;if #bytes>maximum then R.server_error('LimitExceeded')end;return bytes
    end})
end
-- Portable Lua cannot lstat. verify_path(root,relative) is a host filesystem adapter:
-- it rejects links/escapes and returns a stable file identity checked before and after reading.
-- Without it the source is only for a host-controlled, immutable root; it does not claim isolation.
function R.file_source(root,verify_path)
    if type(root)~='string'or root==''or(verify_path~=nil and type(verify_path)~='function')then R.server_error('InvalidPlan')end
    return R.readonly({source_namespace=root,capabilities={synchronous=true,verified_paths=verify_path~=nil},read=function(name,maximum,cancellation)
        cancellation.check();source_path(name);if math.type(maximum)~='integer'or maximum<=0 then R.server_error('InvalidPlan')end
        local identity=verify_path and verify_path(root,name);if verify_path and identity==nil then R.server_error('ReadFailed','Path verifier did not return file identity')end
        local handle,reason=io.open(root..'/'..name,'rb');if not handle then R.server_error('SourceUnavailable',reason)end
        local ok,value=pcall(function()
            local size=handle:seek('end');if not size then R.server_error('ReadFailed','Cannot determine file size')end;if size>maximum then R.server_error('LimitExceeded')end
            if not handle:seek('set',0)then R.server_error('ReadFailed')end
            local parts,read={},0;while read<size do cancellation.check();local bytes=handle:read(math.min(1<<20,size-read));if not bytes or #bytes==0 then R.server_error('ReadFailed','Short source read')end;parts[#parts+1]=bytes;read=read+#bytes end
            cancellation.check();local extra,read_error=handle:read(1);if extra or read_error then R.server_error('ReadFailed','Source changed during reading')end
            if handle:seek('end')~=size or(verify_path and verify_path(root,name)~=identity)then R.server_error('ReadFailed','Source changed during reading')end
            return table.concat(parts)
        end)
        local closed,close_error=handle:close();if not ok then error(value,0)end;if not closed then R.server_error('ReadFailed',close_error)end;return value
    end})
end
local function load_budget(limits)
    local state={read_bytes=0,retained_raw_bytes=0,estimated_decoded_bytes=0,estimated_index_bytes=0,declared_rows=0,decoded_rows=0,active_decodes=0,pending_jobs=0,pending_waiters=0,merged_requests=0,faulted_rows=0,faulted_indexes=0};local budget={limits=limits}
    function budget.diagnostics()local value={};for k,v in pairs(state)do value[k]=v end;value.health=state.faulted_rows==0 and state.faulted_indexes==0 and'Healthy'or'Degraded';return R.readonly(value)end
    function budget.read(bytes)if bytes>limits.max_bundle_bytes-state.read_bytes then R.server_error('LimitExceeded')end;state.read_bytes=state.read_bytes+bytes end
    function budget.rows(rows)if rows>limits.max_rows-state.declared_rows then R.server_error('LimitExceeded')end;state.declared_rows=state.declared_rows+rows end
    function budget.raw(bytes)state.retained_raw_bytes=state.retained_raw_bytes+bytes end
    function budget.read_limit()local maximum=math.min(limits.max_table_bytes,limits.max_bundle_bytes-state.read_bytes);if maximum<=0 then R.server_error('LimitExceeded')end;return maximum end
    function budget.reserve_transient(bytes)if state.active_decodes>=limits.max_concurrent_decodes or bytes>limits.max_estimated_decoded_bytes-state.estimated_decoded_bytes then R.server_error("LimitExceeded")end;state.active_decodes=state.active_decodes+1;state.estimated_decoded_bytes=state.estimated_decoded_bytes+bytes end
    function budget.finish_transient(bytes)state.active_decodes=state.active_decodes-1;state.estimated_decoded_bytes=state.estimated_decoded_bytes-bytes end
    function budget.reserve(bytes)
        if state.active_decodes>=limits.max_concurrent_decodes or state.pending_jobs>=limits.max_pending_requests or state.decoded_rows>=limits.max_decoded_rows or bytes>limits.max_estimated_decoded_bytes-state.estimated_decoded_bytes then R.server_error('LimitExceeded')end
        state.active_decodes=state.active_decodes+1;state.pending_jobs=state.pending_jobs+1;state.decoded_rows=state.decoded_rows+1;state.estimated_decoded_bytes=state.estimated_decoded_bytes+bytes
    end
    function budget.finish(bytes,committed)state.active_decodes=state.active_decodes-1;state.pending_jobs=state.pending_jobs-1;if not committed then state.decoded_rows=state.decoded_rows-1;state.estimated_decoded_bytes=state.estimated_decoded_bytes-bytes end end
    function budget.reserve_index(bytes)if bytes>limits.max_estimated_index_bytes-state.estimated_index_bytes then R.server_error('BudgetExceeded','Derived index budget exhausted')end;state.estimated_index_bytes=state.estimated_index_bytes+bytes end
    function budget.release_index(bytes)state.estimated_index_bytes=state.estimated_index_bytes-bytes end
    function budget.index_fault()state.faulted_indexes=state.faulted_indexes+1 end
    function budget.fault()state.faulted_rows=state.faulted_rows+1 end
    return budget
end
function R.source_bundle(source,plan,cancellation)
    local limits=R.resource_limits(plan.limits);if type(source)~='table'or type(source.read)~='function'then R.server_error('InvalidPlan','Missing synchronous source')end
    if plan.require_verified_paths and not(source.capabilities and source.capabilities.verified_paths)then R.server_error('UnsupportedCapability','Source cannot verify filesystem paths')end
    local budget=load_budget(limits)
    local function read(name,maximum)
        local ok,value=pcall(function()cancellation.check();local bytes=source.read(name,maximum,cancellation);cancellation.check();if type(bytes)~='string'then R.server_error('ReadFailed','Source must return bytes')end;if #bytes>maximum then R.server_error('LimitExceeded')end;budget.read(#bytes);return bytes end)
        if ok then return value end
        if type(value)=='table'and value.code then local version,file=name:match('^versions/([^/]+)/([^/]+)$');error(R.config_error(value.code,value,{stage='read',version_id=version or'',table=file and file:gsub('%.[^.]+$','')or'',relative_path=name}),0)end
        error(value,0)
    end
    local bytes=read('current.json',math.min(limits.max_manifest_bytes,limits.max_bundle_bytes))
    local manifest=parser_scope(function()return object(parse_json(bytes,{rows=limits.max_tables,elements=64,members=32,string_bytes=4096,depth=8,root_array='tables'}))end,'InvalidManifest')
    if type(manifest.format)~='string'or manifest.format==''then R.server_error('InvalidManifest')end
    if not contains({'ZrConfig.Binary/1','ZrConfig.Json/1','ZrConfig.Mixed/1'},manifest.format)then R.server_error('UnsupportedFormat')end
    local entries,version=parser_scope(function()
        local version=segment(manifest.versionId);local entries={};local tables=array(manifest.tables);if #tables>limits.max_tables then R.server_error('LimitExceeded')end
        for _,raw in ipairs(tables)do local entry=object(raw);local name=segment(entry.name);if entries[name]then R.invalid('表重复')end
            if entry.file~=name..(manifest.format=='ZrConfig.Json/1'and'.json'or'.bytes')then R.invalid('表文件名无效')end
            hash(entry.sha256);hash(entry.schemaHash);integer(entry.rows,0,5000000)
            if manifest.format=='ZrConfig.Mixed/1'then if entry.jsonFile~=name..'.json'then R.invalid('JSON 文件名无效')end;hash(entry.jsonSha256)
            elseif(entry.jsonFile and entry.jsonFile~=R.NULL and entry.jsonFile~='')or(entry.jsonSha256 and entry.jsonSha256~=R.NULL and entry.jsonSha256~='')then R.invalid('格式与备用文件不一致')end
            entries[name]=entry
        end;return entries,version
    end,'InvalidManifest')
    local result={version=version,manifest_digest=R.sha256(bytes),budget=budget}
    function result.validate_catalog(catalog)if catalog.target and catalog.target~=""and catalog.target~=manifest.target then R.server_error('CatalogMismatch')end;if count(entries)~=#catalog.tables then R.server_error('TableSetMismatch')end;for _,table in ipairs(catalog.tables)do if not entries[table.name]then R.server_error('TableSetMismatch')end;if entries[table.name].schemaHash~=table.schemaHash then R.server_error('SchemaMismatch')end end end
    function result.open_rows(schema,prefer_json)
        local s=R.parse(schema);local wire=setmetatable({schema=s,types={},tags={},remaining=limits.max_container_elements,limits=limits},Wire);for _,t in ipairs(s.types)do wire.types[t.id]=t end
        local entry=entries[s.name];if not entry or entry.schemaHash~=s.hash then R.server_error('SchemaMismatch')end;budget.rows(entry.rows)
        local alternate=manifest.format=='ZrConfig.Mixed/1'and prefer_json;local bytes=read('versions/'..version..'/'..(alternate and entry.jsonFile or entry.file),budget.read_limit())
        if R.sha256(bytes)~=hash(alternate and entry.jsonSha256 or entry.sha256)then R.server_error('IntegrityMismatch')end
        local rows=parser_scope(function()return indexed_rows(wire,bytes,manifest.format=='ZrConfig.Json/1'or alternate,budget)end)
        rows.version=version;rows.relative_path='versions/'..version..'/'..(alternate and entry.jsonFile or entry.file);if rows.count~=entry.rows then R.server_error('InvalidValue')end;return rows
    end
    return result
end
function R.index(rows,fields,unique)
    local root={}
    for _,row in ipairs(rows) do
        local node=root
        for i,name in ipairs(fields) do
            local k=row[name]
            if i==#fields then
                if unique then if node[k] then R.invalid('主键重复') end;node[k]=row
                else local group=node[k];if not group then group={};node[k]=group end;group[#group+1]=row end
            else local child=node[k];if not child then child={};node[k]=child end;node=child end
        end
    end
    return root
end
-- Server family uses a host monotonic clock: Lua's os.clock measures CPU time, not I/O deadlines.
local retained_owners=setmetatable({},{__mode='k'})
function R.retain_owner(value,owner)
    local pending,seen={value},{};local i=1
    while i<=#pending do
        local item=pending[i];i=i+1
        if type(item)=='table' and item~=R.NULL and not seen[item] then
            seen[item]=true;retained_owners[item]=owner
            for _,child in pairs(item) do if type(child)=='table' then pending[#pending+1]=child end end
        end
    end
end
local function config_error(code,cause,context)
    local values={code=code,stage='',operation_id=0,cause=cause,version_id='',table='',row_key='',field_path='',relative_path=''}
    for key,value in pairs(context or{})do if values[key]~=nil then values[key]=value end end
    return setmetatable({},{__index=values,__newindex=function()error('Configuration errors are immutable',0)end,__pairs=function()return next,values,nil end,__metatable=false,__tostring=function()return values.code..(values.cause and ': '..tostring(values.cause)or'')end})
end
R.config_error=config_error
local function server_error(code,cause,context)error(config_error(code,cause,context),0)end
R.server_error=server_error
-- Internal storage is captured by generated view closures, never returned as a
-- mutable business object. Loads do not yield unless the host wraps the operation.
function R.row_storage(indexed,make,owner)
    local rows,errors,groups,views,group_errors={},{},{},{},{};local loaded=0;local all
    local storage={count=indexed.count,keys=indexed.keys}
    function storage.loaded_count()return loaded end
    function storage.is_complete()return all~=nil end
    function storage.get(index)
        if not index then return nil end;if errors[index]then error(errors[index],0)end
        return rows[index]or server_error('RowNotLoaded')
    end
    function storage.load(index,cancellation)
        if cancellation then cancellation.check()end
        if not index then return nil end;if errors[index]then error(errors[index],0)end
        if rows[index]then return rows[index]end
        local budget=indexed.budget;local bytes=indexed.estimated_row_bytes(index);if budget then budget.reserve(bytes)end
        local ok,row=pcall(function()local value=indexed.read(index);local row=make(value);row=R.readonly(row);R.retain_owner(row,owner());return row end)
        if budget then budget.finish(bytes,ok)end
        if not ok then
            local terminal=type(row)=='table'and(row.code=='InvalidValue'or row.code=='InvalidEncoding'or row.code=='MissingReference'or row.code=='SchemaMismatch'or row.code=='IntegrityMismatch')
            if terminal then errors[index]=row;if budget then budget.fault()end end;error(row,0)
        end
        rows[index]=row;loaded=loaded+1;return row
    end
    function storage.reference(id,load,cancellation)
        if id==0 then return nil end;local ordinal=indexed.reference(id)
        return load and storage.load(ordinal,cancellation)or storage.get(ordinal)
    end
    function storage.group_index(name,load,cancellation)
        if groups[name]then return groups[name]end;if group_errors[name]then error(group_errors[name],0)end;if not load then server_error('TableNotLoaded')end
        local fields
        for _,group in ipairs(indexed.schema.groups)do if group.name==name then fields=group.fields;break end end
        if not fields then server_error('InvalidPlan')end
        local estimate=indexed.estimated_index_bytes(#fields);if indexed.budget then indexed.budget.reserve_index(estimate)end
        local ok,result=pcall(function()
        local root={};local retained=256+indexed.count*512*math.max(1,#fields)
        for ordinal=1,indexed.count do
            if cancellation then cancellation.check()end;local row=rows[ordinal]or indexed.read_transient(ordinal);local node=root
            for i,name in ipairs(fields)do local key=row[name];if type(key)=='string'then retained=retained+#key*4 end
                if i==#fields then local ids=node[key];if not ids then ids={};node[key]=ids end;ids[#ids+1]=ordinal
                else local child=node[key];if not child then child={};node[key]=child end;node=child end
            end
        end
        if indexed.budget then if retained>estimate then indexed.budget.reserve_index(retained-estimate)else indexed.budget.release_index(estimate-retained)end end;estimate=retained;return root end)
        if not ok then if indexed.budget then indexed.budget.release_index(estimate)end;local terminal=type(result)=='table'and(result.code=='InvalidValue'or result.code=='InvalidEncoding'or result.code=='MissingReference'or result.code=='SchemaMismatch'or result.code=='IntegrityMismatch');if terminal then group_errors[name]=result;if indexed.budget then indexed.budget.index_fault()end end;error(result,0)end
        groups[name]=result;return result
    end
    function storage.group(ids,load,cancellation)
        if not ids then return R.empty end;if views[ids]then return views[ids]end
        local result={};for i,id in ipairs(ids)do result[i]=load and storage.load(id,cancellation)or storage.get(id)end
        result=R.readonly(result);R.retain_owner(result,owner());views[ids]=result;return result
    end
    function storage.load_all(cancellation)
        if cancellation then cancellation.check()end;if all then return all end
        local result={};for i=1,indexed.count do result[i]=storage.load(i,cancellation)end
        for _,group in ipairs(indexed.schema.groups)do storage.group_index(group.name,true,cancellation)end
        all=R.readonly(result);R.retain_owner(all,owner());return all
    end
    function storage.all()return all or server_error('TableNotLoaded')end
    return storage
end
function R.load_plan_fingerprint(mode,json,tables)
    local names={};for _,name in ipairs(tables)do names[#names+1]=(name:gsub('.',function(c)return string.format('%02x',string.byte(c))end))end;table.sort(names)
    return R.sha256('zrconfig-load-plan/1\n'..mode..'\n'..(json and'json-preferred'or'binary')..'\n'..table.concat(names,'\n'))
end
function R.catalog(source)
    local definition=R.parse(source);if definition.format~=1 and definition.format~=2 then server_error('InvalidPlan')end
    if definition.format==2 then local supported={["binary-v4"]=true,["json-v1"]=true,["server-default"]=true,["selected-closure"]=true,["lazy-strict"]=true,["lazy-on-access"]=true,["fixed-source-bytes"]=true,["snapshot-owned-handles"]=true,["explicit-reference-load"]=true,["row-cache"]=true,["group-cache"]=true,["typed-warmup"]=true,["bounded-source"]=true,["structured-errors"]=true,["manager-lifecycle"]=true};if definition.runtimeContract~='server-v1'then server_error('UnsupportedCapability')end;for _,value in ipairs(array(definition.requiredCapabilities))do if not supported[text(value)]then server_error('UnsupportedCapability')end end end
    local names,components={},{}
    for _,table in ipairs(definition.tables)do
        if names[table.name] then server_error('CatalogMismatch')end;names[table.name]=table
        local group=components[table.component];if not group then group={};components[table.component]=group end;group[#group+1]=table
    end
    for _,table in ipairs(definition.tables)do for _,dependency in ipairs(table.dependencies)do if not names[dependency] then server_error('CatalogMismatch')end end end
    local catalog={target=definition.target,fingerprint=definition.fingerprint,tables=R.readonly(definition.tables)}
    function catalog.select(requested)
        local input,queue,selected,seen={},{},{},{}
        if requested==nil then requested={};for _,descriptor in ipairs(definition.tables)do requested[#requested+1]=descriptor.name end end
        for _,name in ipairs(requested)do input[#input+1]=name;queue[#queue+1]=name end
        local i=1
        while i<=#queue do
            local name=queue[i];i=i+1;local table=names[name];if not table then server_error('InvalidPlan','Unknown selected table: '..tostring(name))end
            if not seen[table.component] then
                seen[table.component]=true
                for _,member in ipairs(components[table.component])do selected[member.name]=true;for _,dependency in ipairs(member.dependencies)do queue[#queue+1]=dependency end end
            end
        end
        local result={};for _,table in ipairs(definition.tables)do if selected[table.name] then result[#result+1]=table.name end end
        table.sort(input);return R.readonly({requested=input,tables=result,catalog_fingerprint=definition.fingerprint})
    end
    return R.readonly(catalog)
end
function R.cancellation(parent,monotonic_seconds,deadline)
    local cancelled=false
    return R.readonly({cancel=function()cancelled=true end,is_cancelled=function()return cancelled or(parent and parent.is_cancelled())or false end,
        check=function()if cancelled then server_error('Cancelled')end;if parent then parent.check()end;if deadline and monotonic_seconds()>=deadline then server_error('DeadlineExceeded')end end})
end
function R.server_manager(monotonic_seconds,policy)
    if type(monotonic_seconds)~='function'then server_error('InvalidPlan','A host monotonic clock is required')end
    policy=policy or{};local allowed={identity=true,validators=true,event_capacity=true,identity_capacity=true,preparation_capacity=true,max_estimated_live_bytes=true,live_estimator=true,max_tracked_snapshots=true}
    for key in pairs(policy)do if not allowed[key]then server_error('UnsupportedCapability','Unknown manager policy')end end
    local event_capacity,identity_capacity,preparation_capacity=policy.event_capacity or 64,policy.identity_capacity or 32,policy.preparation_capacity or 64
    for _,value in ipairs({event_capacity,identity_capacity,preparation_capacity})do if math.type(value)~='integer'or value<=0 then server_error('InvalidPlan')end end
    local live_maximum,live_estimator,live_capacity=policy.max_estimated_live_bytes,policy.live_estimator,policy.max_tracked_snapshots or 1024
    if math.type(live_capacity)~='integer'or live_capacity<1 or(live_maximum~=nil and(math.type(live_maximum)~='integer'or live_maximum<1 or type(live_estimator)~='function'))or(live_estimator~=nil and type(live_estimator)~='function')then server_error('InvalidPlan')end
    local live=setmetatable({},{__mode='k'})
    local function live_usage(candidate,enforce)
        if live_maximum==nil then return nil end
        local owners={};for snapshot in pairs(live)do owners[#owners+1]=snapshot end;if candidate and not live[candidate]then owners[#owners+1]=candidate end
        local total=0;for _,snapshot in ipairs(owners)do local estimate=live_estimator(snapshot);if math.type(estimate)~='integer'or estimate<0 then server_error('InvalidValue')end;total=estimate>math.maxinteger-total and math.maxinteger or total+estimate;if enforce and total>live_maximum then server_error('BudgetExceeded')end end;return total
    end
    local function track_live(snapshot)if live_maximum==nil or live[snapshot]then return end;if count(live)>=live_capacity then server_error('BudgetExceeded')end;live[snapshot]=true end
    local identity_resolver=policy.identity;if identity_resolver~=nil and type(identity_resolver)~='function'then server_error('InvalidPlan')end
    local validators={};for _,validator in ipairs(policy.validators or{})do if type(validator)~='function'then server_error('InvalidPlan')end;validators[#validators+1]=validator end
    local manager,owner_identity={},{};local current,closed,generation,serial,pending,building,expires=nil,false,0,0,0,false,0
    local timings={{stage="Prepare",operation_id=0,elapsed_milliseconds=0,succeeded=false},{stage="Commit",operation_id=0,elapsed_milliseconds=0,succeeded=false}}
    -- Process CPU milliseconds: os.clock excludes blocked waits; host deadline clock is separate.
    function manager.stage_timings()return R.readonly({R.readonly(timings[1]),R.readonly(timings[2])})end
    local active,ticket,operation_token=0,nil,nil;local identities={};local candidates=setmetatable({},{__mode='k'})
    local listeners,listener_serial={},0;local queue,head,queued,draining,dropped,callback_failures={},1,0,false,0,0
    local last_time=-math.huge
    local function now()local value=monotonic_seconds();if type(value)~='number'or value~=value or value<=-math.huge or value>=math.huge or value<last_time then server_error('InvalidPlan','Clock must be finite and monotonic')end;last_time=value;return value end
    local function advance(value)if value==math.maxinteger then server_error('LimitExceeded')end;return value+1 end
    local function prune()if ticket and ticket.discarded then if operation_token then operation_token.cancel()end;pending=0;ticket=nil;operation_token=nil end end
    local function check(expected,operation)prune();if closed then server_error('ManagerClosed')end;if generation~=expected or pending~=operation then server_error('StaleCandidate')end end
    local function identity(snapshot)
        local value=identity_resolver and identity_resolver(snapshot)or nil
        if not identity_resolver and snapshot.manifest_digest~=nil then value={source_namespace=snapshot.source_namespace or'',version_id=snapshot.version,manifest_digest=snapshot.manifest_digest}end
        if value==nil then return nil end
        if type(value)~='table'or type(value.source_namespace)~='string'or type(value.version_id)~='string'or value.version_id==''or type(value.manifest_digest)~='string'or value.manifest_digest==''then server_error('InvalidPlan','Incomplete snapshot identity')end
        return R.readonly({source_namespace=value.source_namespace,version_id=value.version_id,manifest_digest=value.manifest_digest})
    end
    local function check_identity(value)
        if value then for _,old in ipairs(identities)do if old.source_namespace==value.source_namespace and old.version_id==value.version_id and old.manifest_digest~=value.manifest_digest then server_error('VersionIdentityConflict')end end end
    end
    local function enqueue(event)
        if next(listeners)==nil then return end
        if queued==event_capacity then queue[head]=nil;head=head%event_capacity+1;queued=queued-1;dropped=dropped+1 end
        queue[(head+queued-1)%event_capacity+1]=event;queued=queued+1
    end
    local function drain()
        if draining then return end;draining=true
        local ok=pcall(function()
            while queued>0 do local event=queue[head];queue[head]=nil;head=head%event_capacity+1;queued=queued-1
                local ids={};for id in pairs(listeners)do ids[#ids+1]=id end;table.sort(ids);local callbacks={};for _,id in ipairs(ids)do callbacks[#callbacks+1]=listeners[id]end
                for _,callback in ipairs(callbacks)do if not pcall(callback,event)then callback_failures=callback_failures+1 end end
            end
        end)
        draining=false;if not ok then dropped=dropped+queued+1;queue={};head=1;queued=0 end
    end
    function manager.subscribe(callback)if type(callback)~='function'then server_error('InvalidPlan')end;listener_serial=advance(listener_serial);listeners[listener_serial]=callback;return listener_serial end
    function manager.unsubscribe(id)if listeners[id]then listeners[id]=nil;return true end;return false end
    function manager.diagnostics()return R.readonly({pending_events=queued,dropped_events=dropped,callback_failures=callback_failures,active_preparations=active,retained_identities=#identities,subscribers=count(listeners)})end
    function manager.estimated_live_bytes()return live_usage(nil,false)end
    function manager.try_acquire_snapshot()if closed then server_error('ManagerClosed')end;return current end
    function manager.acquire_snapshot()return manager.try_acquire_snapshot()or server_error('NotInitialized')end
    function manager.status()prune();return R.readonly({phase=closed and'Closed'or current and'Ready'or'Empty',generation=generation,preparing_operation=pending})end
    function manager.prepare(factory,cancellation,options)return manager.prepare_with_cancellation(function()return factory()end,cancellation,options)end
    function manager.prepare_with_cancellation(factory,cancellation,options)
        local started=os.clock()
        cancellation.check();prune();if closed then server_error('ManagerClosed')end
        options=options or{};local timeout,lifetime=options.timeout or 300,options.candidate_lifetime or 300
        for key in pairs(options)do if key~='timeout'and key~='candidate_lifetime'then server_error('UnsupportedCapability')end end
        if type(timeout)~='number'or type(lifetime)~='number'or timeout~=timeout or lifetime~=lifetime or timeout<=0 or lifetime<=0 or timeout>=math.huge or lifetime>=math.huge then server_error('InvalidPlan')end
        local time=now();if closed then server_error('ManagerClosed')end;if pending~=0 and(building or time<expires)then server_error('ReloadBusy')end;if active>=preparation_capacity then server_error('LimitExceeded')end
        serial=advance(serial);local operation,expected,deadline=serial,generation,time+timeout;if deadline>=math.huge then server_error('InvalidPlan')end
        local next_ticket={discarded=false};local token=R.cancellation(cancellation,now,deadline);pending=operation;building=true;expires=deadline;ticket=next_ticket;operation_token=token;active=active+1
        local ok,result=pcall(function()
            live_usage(nil,true);check(expected,operation);token.check();local snapshot=factory(token);check(expected,operation);token.check();if type(snapshot)~='table'then server_error('InvalidValue','Factory must return a snapshot')end
            for _,validator in ipairs(validators)do validator(snapshot,token);check(expected,operation);token.check()end
            local metadata=identity(snapshot);token.check();check_identity(metadata);check(expected,operation)
            live_usage(snapshot,true);check(expected,operation);token.check();track_live(snapshot);
            local expires_at=now()+lifetime;if expires_at>=math.huge then server_error('InvalidPlan')end;check(expected,operation)
            local state={owner=owner_identity,value=snapshot,generation=expected,operation=operation,expires=expires_at,committed=false,identity=metadata,ticket=next_ticket}
            local candidate={snapshot=function()return state.value or server_error('StaleCandidate')end,discard=function()if not state.committed then state.ticket.discarded=true end;state.value=nil end}
            local proxy=R.readonly(candidate);candidates[proxy]=state;building=false;expires=expires_at;operation_token=nil;return proxy
        end)
        active=active-1;if operation>=timings[1].operation_id then timings[1]={stage="Prepare",operation_id=operation,elapsed_milliseconds=(os.clock()-started)*1000,succeeded=ok}end;if not ok then
            if pending==operation then pending=0;building=false;ticket=nil;operation_token=nil end
            if not pcall(function()enqueue(R.readonly({kind='PrepareFailed',generation=expected,operation_id=operation,code=type(result)=='table'and result.code or'InternalError'}))end)then dropped=dropped+1 end
            drain();error(result,0)
        end;return result
    end
    function manager.commit(candidate,cancellation)
        local started=os.clock()
        local state=candidates[candidate];if not state or state.owner~=owner_identity then server_error('ForeignCandidate')end
        if closed then server_error('ManagerClosed')end;if state.committed then server_error('AlreadyCommitted')end;check(state.generation,state.operation);if not state.value then server_error('StaleCandidate')end
        cancellation.check();if now()>=state.expires then server_error('DeadlineExceeded')end;check(state.generation,state.operation);check_identity(state.identity)
        live_usage(state.value,true);check(state.generation,state.operation);if not state.value then server_error('StaleCandidate')end;cancellation.check();if now()>=state.expires then server_error('DeadlineExceeded')end;check(state.generation,state.operation)
        local next_generation=advance(generation)
        local result=R.readonly({snapshot=state.value,generation=next_generation,operation_id=state.operation,committed=true})
        local event=R.readonly({kind='Committed',generation=next_generation,operation_id=state.operation,identity=state.identity})
        if state.identity then for i,old in ipairs(identities)do if old.source_namespace==state.identity.source_namespace and old.version_id==state.identity.version_id then table.remove(identities,i);break end end;if #identities>=identity_capacity then table.remove(identities,1)end;identities[#identities+1]=state.identity end
        current=state.value;generation=next_generation;pending=0;ticket=nil;state.committed=true
        timings[2]={stage="Commit",operation_id=state.operation,elapsed_milliseconds=(os.clock()-started)*1000,succeeded=true}
        if not pcall(enqueue,event)then dropped=dropped+1 end;drain();return result
    end
    function manager.reload(factory,cancellation,options)local candidate=manager.prepare(factory,cancellation,options);local ok,result=pcall(manager.commit,candidate,cancellation);candidate.discard();if not ok then error(result,0)end;return result end
    local function detach(close)
        if closed then if close then return end;server_error('ManagerClosed')end
        local next_generation=advance(generation);local event=R.readonly({kind=close and'Closed'or'Cleared',generation=next_generation,operation_id=0})
        generation=next_generation;closed=close;current=nil;pending=0;ticket=nil;if operation_token then operation_token.cancel();operation_token=nil end
        if not pcall(enqueue,event)then dropped=dropped+1 end;drain()
    end
    function manager.clear()detach(false)end
    function manager.close()detach(true)end
    return R.readonly(manager)
end
return R
