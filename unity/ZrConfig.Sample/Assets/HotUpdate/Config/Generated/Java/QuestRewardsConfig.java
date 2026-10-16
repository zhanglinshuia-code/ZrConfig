// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class QuestRewardsConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 任务编号
    public int getId() { return v_Id; }
    private final String v_Name;
    // 任务名称
    public String getName() { return v_Name; }
    private final int v_Item;
    // 引用 ItemCatalog 的 Id
    public int getItemId() { return v_Item; }
    private ItemCatalogConfig ref_Item;
    public ItemCatalogConfig getItem() { return ref_Item; }
    private final int v_Count;
    // 奖励数量：大于零
    public int getCount() { return v_Count; }
    private final int v_RequiredLevel;
    // 开放等级：大于零
    public int getRequiredLevel() { return v_RequiredLevel; }
    private final boolean v_Enabled;
    // 是否启用
    public boolean getEnabled() { return v_Enabled; }
    private QuestRewardsConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Name = ConfigRuntime.string(ConfigRuntime.field(value, "Name"));
        v_Item = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Item"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Count = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Count"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_RequiredLevel = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "RequiredLevel"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value, "Enabled"));
    }
    static String schema() { return String.join("",
        "{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<QuestRewardsConfig> {
        Index_Key(List<QuestRewardsConfig> rows) { super(rows, true); }
        @Override protected int hash(QuestRewardsConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(QuestRewardsConfig a, QuestRewardsConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { QuestRewardsConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        QuestRewardsConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<QuestRewardsConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<QuestRewardsConfig> {
        Index_Group_ById(List<QuestRewardsConfig> rows) { super(rows, false); }
        @Override protected int hash(QuestRewardsConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(QuestRewardsConfig a, QuestRewardsConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { QuestRewardsConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        QuestRewardsConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<QuestRewardsConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<QuestRewardsConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        Table(List<Object> source) {
            var values = new ArrayList<QuestRewardsConfig>(source.size());
            for (Object value : source) values.add(new QuestRewardsConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
        }
        void bind(ConfigSnapshot snapshot) {
            for (var row : rows) if (row.v_Item != 0) { row.ref_Item = snapshot.ItemCatalog.byId(row.v_Item); if (row.ref_Item == null) throw ConfigRuntime.invalid("引用目标不存在：QuestRewards.Item"); }
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        public List<QuestRewardsConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public QuestRewardsConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public QuestRewardsConfig get(int k0) { QuestRewardsConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<QuestRewardsConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
