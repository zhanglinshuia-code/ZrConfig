// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class ShopOffersConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 商品编号
    public int getId() { return v_Id; }
    private final int v_Item;
    // 引用 ItemCatalog 的 Id
    public int getItemId() { return v_Item; }
    private ItemCatalogConfig ref_Item;
    public ItemCatalogConfig getItem() { return ref_Item; }
    private final int v_Price;
    // 价格：非负整数；0 为免费
    public int getPrice() { return v_Price; }
    private final int v_DailyLimit;
    // 每日限购：0 为不限购
    public int getDailyLimit() { return v_DailyLimit; }
    private final boolean v_Enabled;
    // 是否上架
    public boolean getEnabled() { return v_Enabled; }
    private ShopOffersConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Item = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Item"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Price = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Price"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_DailyLimit = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "DailyLimit"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value, "Enabled"));
    }
    static String schema() { return String.join("",
        "{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<ShopOffersConfig> {
        Index_Key(List<ShopOffersConfig> rows) { super(rows, true); }
        @Override protected int hash(ShopOffersConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(ShopOffersConfig a, ShopOffersConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { ShopOffersConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        ShopOffersConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<ShopOffersConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<ShopOffersConfig> {
        Index_Group_ById(List<ShopOffersConfig> rows) { super(rows, false); }
        @Override protected int hash(ShopOffersConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(ShopOffersConfig a, ShopOffersConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { ShopOffersConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        ShopOffersConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<ShopOffersConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<ShopOffersConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        Table(List<Object> source) {
            var values = new ArrayList<ShopOffersConfig>(source.size());
            for (Object value : source) values.add(new ShopOffersConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
        }
        void bind(ConfigSnapshot snapshot) {
            for (var row : rows) if (row.v_Item != 0) { row.ref_Item = snapshot.ItemCatalog.byId(row.v_Item); if (row.ref_Item == null) throw ConfigRuntime.invalid("引用目标不存在：ShopOffers.Item"); }
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        public List<ShopOffersConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public ShopOffersConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public ShopOffersConfig get(int k0) { ShopOffersConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<ShopOffersConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
