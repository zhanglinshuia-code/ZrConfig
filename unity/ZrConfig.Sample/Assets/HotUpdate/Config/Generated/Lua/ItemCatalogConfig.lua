-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class ItemCatalogConfig
-- 道具编号
---@field Id integer
-- 显示名称
---@field Name string
-- 堆叠上限：1～10000
---@field MaxStack integer
-- 品质：1～5
---@field Rarity integer
-- 是否启用
---@field Enabled boolean
local schema = ([=[{"name":"ItemCatalog","hash":"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"道具编号"},{"name":"Name","type":"System.String","description":"显示名称"},{"name":"MaxStack","type":"System.Int32","description":"堆叠上限：1～10000"},{"name":"Rarity","type":"System.Int32","description":"品质：1～5"},{"name":"Enabled","type":"System.Boolean","description":"是否启用"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]}],"types":[{"id":"System.Boolean","kind":"primitive","primitive":"bool","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null}],"tags":[],"dependencies":[]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    local ids = {}
    for _,row in ipairs(rows) do if ids[row.Id] then R.invalid('引用 Id 不唯一') end; ids[row.Id]=row end
    function result.byId(id) return ids[id] end
    function result.bind(snapshot)
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
