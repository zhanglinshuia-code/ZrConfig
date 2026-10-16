-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
local legacy=require(prefix..'snapshot')
local M={}
function M.load(root,prefer_json) local snapshot=legacy.load(root,prefer_json);R.retain_owner(snapshot,snapshot);return snapshot end
M.manager=R.server_manager
M.cancellation=R.cancellation
M.catalog=R.catalog(([=[{"format":2,"fingerprint":"ADE42D7364E51BC5A62B63F35E32A11E50424B1A690E12E56AFD23760D81DDC6","tables":[{"name":"AllTypes","schemaHash":"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1","dependencies":[],"component":0},{"name":"AllTypes10k","schemaHash":"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432","dependencies":["AllTypes10k","Hero","Skill"],"component":1},{"name":"Drop","schemaHash":"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242","dependencies":[],"component":2},{"name":"Hero","schemaHash":"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE","dependencies":["Hero","Skill"],"component":3},{"name":"ItemCatalog","schemaHash":"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0","dependencies":[],"component":4},{"name":"QuestRewards","schemaHash":"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35","dependencies":["ItemCatalog"],"component":5},{"name":"ShopOffers","schemaHash":"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1","dependencies":["ItemCatalog"],"component":6},{"name":"Skill","schemaHash":"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB","dependencies":["Hero"],"component":3},{"name":"SkillLevel","schemaHash":"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2","dependencies":[],"component":7}],"components":[["AllTypes"],["AllTypes10k"],["Drop"],["Hero","Skill"],["ItemCatalog"],["QuestRewards"],["ShopOffers"],["SkillLevel"]],"target":"C","runtimeContract":"server-v1","requiredCapabilities":["binary-v4","json-v1","server-default","selected-closure","lazy-strict","lazy-on-access","fixed-source-bytes","snapshot-owned-handles","explicit-reference-load","row-cache","group-cache","typed-warmup","bounded-source","structured-errors","manager-lifecycle"]}]=]))
local factories={}
factories[([=[AllTypes]=])]=require(prefix..([=[AllTypesConfig]=]))
factories[([=[AllTypes10k]=])]=require(prefix..([=[AllTypes10kConfig]=]))
factories[([=[Drop]=])]=require(prefix..([=[DropConfig]=]))
factories[([=[Hero]=])]=require(prefix..([=[HeroConfig]=]))
factories[([=[ItemCatalog]=])]=require(prefix..([=[ItemCatalogConfig]=]))
factories[([=[QuestRewards]=])]=require(prefix..([=[QuestRewardsConfig]=]))
factories[([=[ShopOffers]=])]=require(prefix..([=[ShopOffersConfig]=]))
factories[([=[Skill]=])]=require(prefix..([=[SkillConfig]=]))
factories[([=[SkillLevel]=])]=require(prefix..([=[SkillLevelConfig]=]))
function M.select(root,requested,prefer_json)
    local plan=M.catalog.select(requested);local bundle=R.bundle(root);bundle.validate_catalog(M.catalog)
    local storage={version=bundle.version};local result={version=bundle.version,selection=plan}
    for _,name in ipairs(plan.tables)do storage[name]=factories[name](bundle,prefer_json)end
    for _,name in ipairs(plan.tables)do storage[name].bind(storage)end
    local seen={}
    for _,name in ipairs(plan.tables)do storage[name].seal(seen)end
    storage=R.readonly(storage,seen)
    function result.get_AllTypes()return storage[([=[AllTypes]=])] or R.server_error('TableNotSelected',([=[AllTypes]=]))end
    function result.get_AllTypes10k()return storage[([=[AllTypes10k]=])] or R.server_error('TableNotSelected',([=[AllTypes10k]=]))end
    function result.get_Drop()return storage[([=[Drop]=])] or R.server_error('TableNotSelected',([=[Drop]=]))end
    function result.get_Hero()return storage[([=[Hero]=])] or R.server_error('TableNotSelected',([=[Hero]=]))end
    function result.get_ItemCatalog()return storage[([=[ItemCatalog]=])] or R.server_error('TableNotSelected',([=[ItemCatalog]=]))end
    function result.get_QuestRewards()return storage[([=[QuestRewards]=])] or R.server_error('TableNotSelected',([=[QuestRewards]=]))end
    function result.get_ShopOffers()return storage[([=[ShopOffers]=])] or R.server_error('TableNotSelected',([=[ShopOffers]=]))end
    function result.get_Skill()return storage[([=[Skill]=])] or R.server_error('TableNotSelected',([=[Skill]=]))end
    function result.get_SkillLevel()return storage[([=[SkillLevel]=])] or R.server_error('TableNotSelected',([=[SkillLevel]=]))end
    result=R.readonly(result);R.retain_owner(storage,result);return result
end
return R.readonly(M)
