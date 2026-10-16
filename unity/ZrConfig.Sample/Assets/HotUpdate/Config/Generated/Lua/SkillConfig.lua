-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class SkillConfig
-- 技能编号
---@field Id integer
-- 技能名称
---@field Name string
-- 英雄 Id；反向引用 Hero 表
---@field Hero HeroConfig|ZrConfigNull
---@field HeroId integer
local schema = ([=[{"name":"Skill","hash":"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"技能编号"},{"name":"Name","type":"System.String","description":"技能名称"},{"name":"Hero","type":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","description":"英雄 Id；反向引用 Hero 表"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]}],"types":[{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"Hero"}],"tags":[],"dependencies":["Hero"]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    local ids = {}
    for _,row in ipairs(rows) do if ids[row.Id] then R.invalid('引用 Id 不唯一') end; ids[row.Id]=row end
    function result.byId(id) return ids[id] end
    for _,row in ipairs(rows) do row[([=[HeroId]=])] = row[([=[Hero]=])]; row[([=[Hero]=])]=R.NULL end
    function result.bind(snapshot)
        for _,row in ipairs(rows) do if row[([=[HeroId]=])] ~= 0 then row[([=[Hero]=])] = snapshot[([=[Hero]=])].byId(row[([=[HeroId]=])]) or R.invalid(([=[引用目标不存在：Skill.Hero]=])) end end
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
