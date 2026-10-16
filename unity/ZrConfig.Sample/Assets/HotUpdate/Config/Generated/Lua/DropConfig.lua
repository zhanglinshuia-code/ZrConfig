-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class DropConfig
-- 掉落组编号
---@field GroupId integer
-- 物品编号
---@field Id integer
-- 数量
---@field Count integer
local schema = ([=[{"name":"Drop","hash":"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242","hasId":true,"fields":[{"name":"GroupId","type":"System.Int32","description":"掉落组编号"},{"name":"Id","type":"System.Int32","description":"物品编号"},{"name":"Count","type":"System.Int32","description":"数量"}],"keys":["GroupId","Id"],"groups":[{"name":"ByGroupId","fields":["GroupId"]},{"name":"ById","fields":["Id"]}],"types":[{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null}],"tags":[],"dependencies":[]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[GroupId]=]), ([=[Id]=])}, true)
    local index_Group_ByGroupId = R.index(rows, {([=[GroupId]=])}, false)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    function result.bind(snapshot)
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ByGroupId=R.readonly(index_Group_ByGroupId, seen)
        index_Group_ById=R.readonly(index_Group_ById, seen)
        result.bind=nil; result.seal=nil
    end
    function result.find(k0, k1)
        local node = index_Key
        node=node[k0]; if node==nil then return nil end
        node=node[k1]; if node==nil then return nil end
        return node
    end
    function result.get(k0, k1) return result.find(k0, k1) or R.invalid('配置主键不存在') end
    function result.getByGroupId(k0)
        local node = index_Group_ByGroupId
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    function result.getById(k0)
        local node = index_Group_ById
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    return result
end
