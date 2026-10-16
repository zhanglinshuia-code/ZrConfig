// zrconfig-api-contract:1
package zrconfig;
import java.io.*;
import java.nio.file.*;
import java.util.*;
import java.util.concurrent.atomic.AtomicReference;
// 快照在发布前完成加载和引用绑定，旧快照由已有请求继续持有。
public final class ConfigSnapshot {
    public final String version;
    public final AllTypesConfig.Table AllTypes;
    public final AllTypes10kConfig.Table AllTypes10k;
    public final DropConfig.Table Drop;
    public final HeroConfig.Table Hero;
    public final ItemCatalogConfig.Table ItemCatalog;
    public final QuestRewardsConfig.Table QuestRewards;
    public final ShopOffersConfig.Table ShopOffers;
    public final SkillConfig.Table Skill;
    public final SkillLevelConfig.Table SkillLevel;
    private ConfigSnapshot(ConfigRuntime.Bundle bundle, boolean preferJson) throws IOException { this(bundle, preferJson, null); }
    private ConfigSnapshot(ConfigRuntime.Bundle bundle, boolean preferJson, Set<String> selected) throws IOException {
        version = bundle.version;
        bundle.validateTables(new String[] {"AllTypes", "AllTypes10k", "Drop", "Hero", "ItemCatalog", "QuestRewards", "ShopOffers", "Skill", "SkillLevel"});
        this.AllTypes = selected == null || selected.contains("AllTypes") ? new AllTypesConfig.Table(bundle.load(AllTypesConfig.schema(), preferJson)) : null;
        this.AllTypes10k = selected == null || selected.contains("AllTypes10k") ? new AllTypes10kConfig.Table(bundle.load(AllTypes10kConfig.schema(), preferJson)) : null;
        this.Drop = selected == null || selected.contains("Drop") ? new DropConfig.Table(bundle.load(DropConfig.schema(), preferJson)) : null;
        this.Hero = selected == null || selected.contains("Hero") ? new HeroConfig.Table(bundle.load(HeroConfig.schema(), preferJson)) : null;
        this.ItemCatalog = selected == null || selected.contains("ItemCatalog") ? new ItemCatalogConfig.Table(bundle.load(ItemCatalogConfig.schema(), preferJson)) : null;
        this.QuestRewards = selected == null || selected.contains("QuestRewards") ? new QuestRewardsConfig.Table(bundle.load(QuestRewardsConfig.schema(), preferJson)) : null;
        this.ShopOffers = selected == null || selected.contains("ShopOffers") ? new ShopOffersConfig.Table(bundle.load(ShopOffersConfig.schema(), preferJson)) : null;
        this.Skill = selected == null || selected.contains("Skill") ? new SkillConfig.Table(bundle.load(SkillConfig.schema(), preferJson)) : null;
        this.SkillLevel = selected == null || selected.contains("SkillLevel") ? new SkillLevelConfig.Table(bundle.load(SkillLevelConfig.schema(), preferJson)) : null;
        if (this.AllTypes != null) this.AllTypes.bind(this);
        if (this.AllTypes10k != null) this.AllTypes10k.bind(this);
        if (this.Drop != null) this.Drop.bind(this);
        if (this.Hero != null) this.Hero.bind(this);
        if (this.ItemCatalog != null) this.ItemCatalog.bind(this);
        if (this.QuestRewards != null) this.QuestRewards.bind(this);
        if (this.ShopOffers != null) this.ShopOffers.bind(this);
        if (this.Skill != null) this.Skill.bind(this);
        if (this.SkillLevel != null) this.SkillLevel.bind(this);
    }
    public static ConfigSnapshot load(Path root) throws IOException { return new ConfigSnapshot(new ConfigRuntime.Bundle(root), false); }
    public static ConfigSnapshot load(Path root, boolean preferJson) throws IOException { return new ConfigSnapshot(new ConfigRuntime.Bundle(root), preferJson); }
    public static final class Manager {
        private final AtomicReference<ConfigSnapshot> current = new AtomicReference<>();
        public ConfigSnapshot current() { ConfigSnapshot value = current.get(); if (value == null) throw new IllegalStateException("配置尚未初始化"); return value; }
        public synchronized ConfigSnapshot reload(Path root, boolean preferJson) throws IOException { ConfigSnapshot next = load(root, preferJson); current.set(next); return next; }
    }
    static ConfigSnapshot fromSelection(ConfigRuntime.Bundle bundle, boolean preferJson, Set<String> selected) throws IOException { return new ConfigSnapshot(bundle, preferJson, selected); }
}
