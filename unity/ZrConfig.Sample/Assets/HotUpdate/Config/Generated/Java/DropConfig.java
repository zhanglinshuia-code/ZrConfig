// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class DropConfig {
    private ConfigSnapshot serverOwner;
    private final int v_GroupId;
    // 掉落组编号
    public int getGroupId() { return v_GroupId; }
    private final int v_Id;
    // 物品编号
    public int getId() { return v_Id; }
    private final int v_Count;
    // 数量
    public int getCount() { return v_Count; }
    private DropConfig(Object value) {
        v_GroupId = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "GroupId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Count = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Count"),Integer.MIN_VALUE,Integer.MAX_VALUE);
    }
    static String schema() { return String.join("",
        "{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<DropConfig> {
        Index_Key(List<DropConfig> rows) { super(rows, true); }
        @Override protected int hash(DropConfig row) { return ((1 * 31 + (int)row.v_GroupId) * 31 + (int)row.v_Id); }
        @Override protected boolean same(DropConfig a, DropConfig b) { return a.v_GroupId == b.v_GroupId && a.v_Id == b.v_Id; }
        int find(int k0, int k1) {
            int slot = mix(((1 * 31 + (int)k0) * 31 + (int)k1)) & mask;
            while (slots[slot] >= 0) { DropConfig row = rows.get(slots[slot]); if (row.v_GroupId == k0 && row.v_Id == k1) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        DropConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<DropConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ByGroupId extends ConfigRuntime.Index<DropConfig> {
        Index_Group_ByGroupId(List<DropConfig> rows) { super(rows, false); }
        @Override protected int hash(DropConfig row) { return (1 * 31 + (int)row.v_GroupId); }
        @Override protected boolean same(DropConfig a, DropConfig b) { return a.v_GroupId == b.v_GroupId; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { DropConfig row = rows.get(slots[slot]); if (row.v_GroupId == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        DropConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<DropConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<DropConfig> {
        Index_Group_ById(List<DropConfig> rows) { super(rows, false); }
        @Override protected int hash(DropConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(DropConfig a, DropConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { DropConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        DropConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<DropConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<DropConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ByGroupId index_Group_ByGroupId;
        private final Index_Group_ById index_Group_ById;
        Table(List<Object> source) {
            var values = new ArrayList<DropConfig>(source.size());
            for (Object value : source) values.add(new DropConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ByGroupId = new Index_Group_ByGroupId(rows);
            index_Group_ById = new Index_Group_ById(rows);
        }
        void bind(ConfigSnapshot snapshot) {
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        public List<DropConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public DropConfig find(int k0, int k1) { return index_Key.row(index_Key.find(k0, k1)); }
        public DropConfig get(int k0, int k1) { DropConfig value = find(k0, k1); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<DropConfig> getByGroupId(int k0) { return index_Group_ByGroupId.group(index_Group_ByGroupId.find(k0)); }
        public List<DropConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
