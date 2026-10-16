-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
local factories = {}
factories[([=[AllTypes]=])] = require(prefix .. ([=[AllTypesConfig]=]))
factories[([=[AllTypes10k]=])] = require(prefix .. ([=[AllTypes10kConfig]=]))
factories[([=[Drop]=])] = require(prefix .. ([=[DropConfig]=]))
factories[([=[Hero]=])] = require(prefix .. ([=[HeroConfig]=]))
factories[([=[ItemCatalog]=])] = require(prefix .. ([=[ItemCatalogConfig]=]))
factories[([=[QuestRewards]=])] = require(prefix .. ([=[QuestRewardsConfig]=]))
factories[([=[ShopOffers]=])] = require(prefix .. ([=[ShopOffersConfig]=]))
factories[([=[Skill]=])] = require(prefix .. ([=[SkillConfig]=]))
factories[([=[SkillLevel]=])] = require(prefix .. ([=[SkillLevelConfig]=]))
local M = {}
-- 加载不调用协程让出；同一 Lua 状态内完整候选构建成功后一次替换引用。
function M.load(root, prefer_json)
    local bundle = R.bundle(root)
    local snapshot = {version=bundle.version}

    bundle.validate_tables({([=[AllTypes]=]), ([=[AllTypes10k]=]), ([=[Drop]=]), ([=[Hero]=]), ([=[ItemCatalog]=]), ([=[QuestRewards]=]), ([=[ShopOffers]=]), ([=[Skill]=]), ([=[SkillLevel]=])})
    snapshot[([=[AllTypes]=])] = factories[([=[AllTypes]=])](bundle, prefer_json)
    snapshot[([=[AllTypes10k]=])] = factories[([=[AllTypes10k]=])](bundle, prefer_json)
    snapshot[([=[Drop]=])] = factories[([=[Drop]=])](bundle, prefer_json)
    snapshot[([=[Hero]=])] = factories[([=[Hero]=])](bundle, prefer_json)
    snapshot[([=[ItemCatalog]=])] = factories[([=[ItemCatalog]=])](bundle, prefer_json)
    snapshot[([=[QuestRewards]=])] = factories[([=[QuestRewards]=])](bundle, prefer_json)
    snapshot[([=[ShopOffers]=])] = factories[([=[ShopOffers]=])](bundle, prefer_json)
    snapshot[([=[Skill]=])] = factories[([=[Skill]=])](bundle, prefer_json)
    snapshot[([=[SkillLevel]=])] = factories[([=[SkillLevel]=])](bundle, prefer_json)
    snapshot[([=[AllTypes]=])].bind(snapshot)
    snapshot[([=[AllTypes10k]=])].bind(snapshot)
    snapshot[([=[Drop]=])].bind(snapshot)
    snapshot[([=[Hero]=])].bind(snapshot)
    snapshot[([=[ItemCatalog]=])].bind(snapshot)
    snapshot[([=[QuestRewards]=])].bind(snapshot)
    snapshot[([=[ShopOffers]=])].bind(snapshot)
    snapshot[([=[Skill]=])].bind(snapshot)
    snapshot[([=[SkillLevel]=])].bind(snapshot)
    local seen = {}
    snapshot[([=[AllTypes]=])].seal(seen)
    snapshot[([=[AllTypes10k]=])].seal(seen)
    snapshot[([=[Drop]=])].seal(seen)
    snapshot[([=[Hero]=])].seal(seen)
    snapshot[([=[ItemCatalog]=])].seal(seen)
    snapshot[([=[QuestRewards]=])].seal(seen)
    snapshot[([=[ShopOffers]=])].seal(seen)
    snapshot[([=[Skill]=])].seal(seen)
    snapshot[([=[SkillLevel]=])].seal(seen)
    return R.readonly(snapshot, seen)
end
function M.manager()
    local current
    return R.readonly({
        current=function() return current or R.invalid('配置尚未加载') end,
        reload=function(root, prefer_json)
            local next_snapshot=M.load(root, prefer_json)
            current=next_snapshot
            return next_snapshot
        end
    })
end
M.NULL=R.NULL
M.decimal=R.decimal
return R.readonly(M)
