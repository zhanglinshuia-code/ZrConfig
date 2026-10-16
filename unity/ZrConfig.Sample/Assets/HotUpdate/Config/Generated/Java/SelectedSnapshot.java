// zrconfig-api-contract:1
package zrconfig;
import java.io.IOException;
import java.nio.file.Path;
import java.util.*;
public final class SelectedSnapshot {
    private final ConfigSnapshot storage;
    public final ConfigRuntime.SelectionPlan selection;
    public final String version;
    private SelectedSnapshot(ConfigRuntime.Bundle bundle, ConfigRuntime.SelectionPlan plan, boolean preferJson) throws IOException {
        bundle.validateCatalog(ServerConfig.catalog());
        selection = plan;
        storage = ConfigSnapshot.fromSelection(bundle, preferJson, new HashSet<>(selection.tables()));
        version = storage.version;
        if (storage.AllTypes != null) storage.AllTypes.retain(storage);
        if (storage.AllTypes10k != null) storage.AllTypes10k.retain(storage);
        if (storage.Drop != null) storage.Drop.retain(storage);
        if (storage.Hero != null) storage.Hero.retain(storage);
        if (storage.ItemCatalog != null) storage.ItemCatalog.retain(storage);
        if (storage.QuestRewards != null) storage.QuestRewards.retain(storage);
        if (storage.ShopOffers != null) storage.ShopOffers.retain(storage);
        if (storage.Skill != null) storage.Skill.retain(storage);
        if (storage.SkillLevel != null) storage.SkillLevel.retain(storage);
    }
    public static SelectedSnapshot load(Path root, Collection<String> requested, boolean preferJson) throws IOException { var plan = ServerConfig.catalog().select(requested); return new SelectedSnapshot(new ConfigRuntime.Bundle(root), plan, preferJson); }
    public AllTypesConfig.Table get_AllTypes() { if (storage.AllTypes == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "AllTypes: table not selected"); return storage.AllTypes; }
    public AllTypes10kConfig.Table get_AllTypes10k() { if (storage.AllTypes10k == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "AllTypes10k: table not selected"); return storage.AllTypes10k; }
    public DropConfig.Table get_Drop() { if (storage.Drop == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "Drop: table not selected"); return storage.Drop; }
    public HeroConfig.Table get_Hero() { if (storage.Hero == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "Hero: table not selected"); return storage.Hero; }
    public ItemCatalogConfig.Table get_ItemCatalog() { if (storage.ItemCatalog == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "ItemCatalog: table not selected"); return storage.ItemCatalog; }
    public QuestRewardsConfig.Table get_QuestRewards() { if (storage.QuestRewards == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "QuestRewards: table not selected"); return storage.QuestRewards; }
    public ShopOffersConfig.Table get_ShopOffers() { if (storage.ShopOffers == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "ShopOffers: table not selected"); return storage.ShopOffers; }
    public SkillConfig.Table get_Skill() { if (storage.Skill == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "Skill: table not selected"); return storage.Skill; }
    public SkillLevelConfig.Table get_SkillLevel() { if (storage.SkillLevel == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, "SkillLevel: table not selected"); return storage.SkillLevel; }
}
