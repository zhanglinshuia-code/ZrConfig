-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class SkillLevelConfig
-- 技能编号
---@field SkillId integer
-- 技能名称
---@field Name string
-- 技能等级
---@field Level integer
-- 品质
---@field Quality integer
-- 伤害
---@field Damage integer
local schema = ([=[{"name":"SkillLevel","hash":"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2","hasId":false,"fields":[{"name":"SkillId","type":"System.Int32","description":"技能编号"},{"name":"Name","type":"System.String","description":"技能名称"},{"name":"Level","type":"System.Int32","description":"技能等级"},{"name":"Quality","type":"System.Int32","description":"品质"},{"name":"Damage","type":"System.Int32","description":"伤害"}],"keys":["SkillId","Level","Quality"],"groups":[{"name":"ByLevel","fields":["Level"]},{"name":"ByQuality","fields":["Quality"]},{"name":"BySkillId","fields":["SkillId"]}],"types":[{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null}],"tags":[],"dependencies":[]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[SkillId]=]), ([=[Level]=]), ([=[Quality]=])}, true)
    local index_Group_ByLevel = R.index(rows, {([=[Level]=])}, false)
    local index_Group_ByQuality = R.index(rows, {([=[Quality]=])}, false)
    local index_Group_BySkillId = R.index(rows, {([=[SkillId]=])}, false)
    function result.bind(snapshot)
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ByLevel=R.readonly(index_Group_ByLevel, seen)
        index_Group_ByQuality=R.readonly(index_Group_ByQuality, seen)
        index_Group_BySkillId=R.readonly(index_Group_BySkillId, seen)
        result.bind=nil; result.seal=nil
    end
    function result.find(k0, k1, k2)
        local node = index_Key
        node=node[k0]; if node==nil then return nil end
        node=node[k1]; if node==nil then return nil end
        node=node[k2]; if node==nil then return nil end
        return node
    end
    function result.get(k0, k1, k2) return result.find(k0, k1, k2) or R.invalid('配置主键不存在') end
    function result.getByLevel(k0)
        local node = index_Group_ByLevel
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    function result.getByQuality(k0)
        local node = index_Group_ByQuality
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    function result.getBySkillId(k0)
        local node = index_Group_BySkillId
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    return result
end
