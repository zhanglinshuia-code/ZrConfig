-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class ShopOffersConfig
-- 商品编号
---@field Id integer
-- 引用 ItemCatalog 的 Id
---@field Item ItemCatalogConfig|ZrConfigNull
---@field ItemId integer
-- 价格：非负整数；0 为免费
---@field Price integer
-- 每日限购：0 为不限购
---@field DailyLimit integer
-- 是否上架
---@field Enabled boolean
local schema = ([=[{"name":"ShopOffers","hash":"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"商品编号"},{"name":"Item","type":"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1","description":"引用 ItemCatalog 的 Id"},{"name":"Price","type":"System.Int32","description":"价格：非负整数；0 为免费"},{"name":"DailyLimit","type":"System.Int32","description":"每日限购：0 为不限购"},{"name":"Enabled","type":"System.Boolean","description":"是否上架"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]}],"types":[{"id":"System.Boolean","kind":"primitive","primitive":"bool","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"ItemCatalog"}],"tags":[],"dependencies":["ItemCatalog"]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    for _,row in ipairs(rows) do row[([=[ItemId]=])] = row[([=[Item]=])]; row[([=[Item]=])]=R.NULL end
    function result.bind(snapshot)
        for _,row in ipairs(rows) do if row[([=[ItemId]=])] ~= 0 then row[([=[Item]=])] = snapshot[([=[ItemCatalog]=])].byId(row[([=[ItemId]=])]) or R.invalid(([=[引用目标不存在：ShopOffers.Item]=])) end end
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ById=R.readonly(index_Group_ById, seen)
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
