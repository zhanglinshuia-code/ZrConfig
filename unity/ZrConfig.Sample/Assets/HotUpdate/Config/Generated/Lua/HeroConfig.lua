-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class HeroConfig
-- 英雄编号
---@field Id integer
-- 英雄名称
---@field Name string
-- 技能 Id；引用 Skill 表
---@field Skill SkillConfig|ZrConfigNull
---@field SkillId integer
-- 另一个技能 Id；可复用同一对象
---@field OtherSkill SkillConfig|ZrConfigNull
---@field OtherSkillId integer
-- 导师 Id；引用 Hero 表
---@field Mentor HeroConfig|ZrConfigNull
---@field MentorId integer
local schema = ([=[{"name":"Hero","hash":"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"英雄编号"},{"name":"Name","type":"System.String","description":"英雄名称"},{"name":"Skill","type":"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1","description":"技能 Id；引用 Skill 表"},{"name":"OtherSkill","type":"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1","description":"另一个技能 Id；可复用同一对象"},{"name":"Mentor","type":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","description":"导师 Id；引用 Hero 表"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]}],"types":[{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"Hero"},{"id":"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"Skill"}],"tags":[],"dependencies":["Hero","Skill"]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    local ids = {}
    for _,row in ipairs(rows) do if ids[row.Id] then R.invalid('引用 Id 不唯一') end; ids[row.Id]=row end
    function result.byId(id) return ids[id] end
    for _,row in ipairs(rows) do row[([=[SkillId]=])] = row[([=[Skill]=])]; row[([=[Skill]=])]=R.NULL end
    for _,row in ipairs(rows) do row[([=[OtherSkillId]=])] = row[([=[OtherSkill]=])]; row[([=[OtherSkill]=])]=R.NULL end
    for _,row in ipairs(rows) do row[([=[MentorId]=])] = row[([=[Mentor]=])]; row[([=[Mentor]=])]=R.NULL end
    function result.bind(snapshot)
        for _,row in ipairs(rows) do if row[([=[SkillId]=])] ~= 0 then row[([=[Skill]=])] = snapshot[([=[Skill]=])].byId(row[([=[SkillId]=])]) or R.invalid(([=[引用目标不存在：Hero.Skill]=])) end end
        for _,row in ipairs(rows) do if row[([=[OtherSkillId]=])] ~= 0 then row[([=[OtherSkill]=])] = snapshot[([=[Skill]=])].byId(row[([=[OtherSkillId]=])]) or R.invalid(([=[引用目标不存在：Hero.OtherSkill]=])) end end
        for _,row in ipairs(rows) do if row[([=[MentorId]=])] ~= 0 then row[([=[Mentor]=])] = snapshot[([=[Hero]=])].byId(row[([=[MentorId]=])]) or R.invalid(([=[引用目标不存在：Hero.Mentor]=])) end end
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ById=R.readonly(index_Group_ById, seen)
        ids=R.readonly(ids, seen)
        result.bind=nil; result.seal=nil
    end
    function result.find(k0)
        local node = index_Key
        node=node[k0]; if node==nil then return nil end
        return node
    end
    function result.get(k0) return result.find(k0) or R.invalid('配置主键不存在') end
    function result.getById(k0)
        local node = index_Group_ById
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    return result
end
