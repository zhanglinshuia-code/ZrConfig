// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class ItemCatalogConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 道具编号
    public int getId() { return v_Id; }
    private final String v_Name;
    // 显示名称
    public String getName() { return v_Name; }
    private final int v_MaxStack;
    // 堆叠上限：1～10000
    public int getMaxStack() { return v_MaxStack; }
    private final int v_Rarity;
    // 品质：1～5
    public int getRarity() { return v_Rarity; }
    private final boolean v_Enabled;
    // 是否启用
    public boolean getEnabled() { return v_Enabled; }
    private ItemCatalogConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Name = ConfigRuntime.string(ConfigRuntime.field(value, "Name"));
        v_MaxStack = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "MaxStack"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Rarity = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Rarity"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value, "Enabled"));
    }
    static String schema() { return String.join("",
        "{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<ItemCatalogConfig> {
        Index_Key(List<ItemCatalogConfig> rows) { super(rows, true); }
        @Override protected int hash(ItemCatalogConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(ItemCatalogConfig a, ItemCatalogConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { ItemCatalogConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        ItemCatalogConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<ItemCatalogConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<ItemCatalogConfig> {
        Index_Group_ById(List<ItemCatalogConfig> rows) { super(rows, false); }
        @Override protected int hash(ItemCatalogConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(ItemCatalogConfig a, ItemCatalogConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { ItemCatalogConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        ItemCatalogConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<ItemCatalogConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<ItemCatalogConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        private final Map<Integer,ItemCatalogConfig> ids = new HashMap<>();
        Table(List<Object> source) {
            var values = new ArrayList<ItemCatalogConfig>(source.size());
            for (Object value : source) values.add(new ItemCatalogConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
            for (var row : rows) { if (ids.put(row.v_Id, row) != null) throw ConfigRuntime.invalid("引用 Id 不唯一"); }
        }
        void bind(ConfigSnapshot snapshot) {
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        ItemCatalogConfig byId(int id) { return ids.get(id); }
        public List<ItemCatalogConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public ItemCatalogConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public ItemCatalogConfig get(int k0) { ItemCatalogConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<ItemCatalogConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
