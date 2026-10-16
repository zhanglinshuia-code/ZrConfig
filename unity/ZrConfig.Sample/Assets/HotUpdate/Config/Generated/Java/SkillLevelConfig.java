// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class SkillLevelConfig {
    private ConfigSnapshot serverOwner;
    private final int v_SkillId;
    // 技能编号
    public int getSkillId() { return v_SkillId; }
    private final String v_Name;
    // 技能名称
    public String getName() { return v_Name; }
    private final int v_Level;
    // 技能等级
    public int getLevel() { return v_Level; }
    private final int v_Quality;
    // 品质
    public int getQuality() { return v_Quality; }
    private final int v_Damage;
    // 伤害
    public int getDamage() { return v_Damage; }
    private SkillLevelConfig(Object value) {
        v_SkillId = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "SkillId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Name = ConfigRuntime.string(ConfigRuntime.field(value, "Name"));
        v_Level = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Level"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Quality = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Quality"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Damage = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Damage"),Integer.MIN_VALUE,Integer.MAX_VALUE);
    }
    static String schema() { return String.join("",
        "{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<SkillLevelConfig> {
        Index_Key(List<SkillLevelConfig> rows) { super(rows, true); }
        @Override protected int hash(SkillLevelConfig row) { return (((1 * 31 + (int)row.v_SkillId) * 31 + (int)row.v_Level) * 31 + (int)row.v_Quality); }
        @Override protected boolean same(SkillLevelConfig a, SkillLevelConfig b) { return a.v_SkillId == b.v_SkillId && a.v_Level == b.v_Level && a.v_Quality == b.v_Quality; }
        int find(int k0, int k1, int k2) {
            int slot = mix((((1 * 31 + (int)k0) * 31 + (int)k1) * 31 + (int)k2)) & mask;
            while (slots[slot] >= 0) { SkillLevelConfig row = rows.get(slots[slot]); if (row.v_SkillId == k0 && row.v_Level == k1 && row.v_Quality == k2) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillLevelConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillLevelConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ByLevel extends ConfigRuntime.Index<SkillLevelConfig> {
        Index_Group_ByLevel(List<SkillLevelConfig> rows) { super(rows, false); }
        @Override protected int hash(SkillLevelConfig row) { return (1 * 31 + (int)row.v_Level); }
        @Override protected boolean same(SkillLevelConfig a, SkillLevelConfig b) { return a.v_Level == b.v_Level; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { SkillLevelConfig row = rows.get(slots[slot]); if (row.v_Level == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillLevelConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillLevelConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ByQuality extends ConfigRuntime.Index<SkillLevelConfig> {
        Index_Group_ByQuality(List<SkillLevelConfig> rows) { super(rows, false); }
        @Override protected int hash(SkillLevelConfig row) { return (1 * 31 + (int)row.v_Quality); }
        @Override protected boolean same(SkillLevelConfig a, SkillLevelConfig b) { return a.v_Quality == b.v_Quality; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { SkillLevelConfig row = rows.get(slots[slot]); if (row.v_Quality == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillLevelConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillLevelConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_BySkillId extends ConfigRuntime.Index<SkillLevelConfig> {
        Index_Group_BySkillId(List<SkillLevelConfig> rows) { super(rows, false); }
        @Override protected int hash(SkillLevelConfig row) { return (1 * 31 + (int)row.v_SkillId); }
        @Override protected boolean same(SkillLevelConfig a, SkillLevelConfig b) { return a.v_SkillId == b.v_SkillId; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { SkillLevelConfig row = rows.get(slots[slot]); if (row.v_SkillId == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillLevelConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillLevelConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<SkillLevelConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ByLevel index_Group_ByLevel;
        private final Index_Group_ByQuality index_Group_ByQuality;
        private final Index_Group_BySkillId index_Group_BySkillId;
        Table(List<Object> source) {
            var values = new ArrayList<SkillLevelConfig>(source.size());
            for (Object value : source) values.add(new SkillLevelConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ByLevel = new Index_Group_ByLevel(rows);
            index_Group_ByQuality = new Index_Group_ByQuality(rows);
            index_Group_BySkillId = new Index_Group_BySkillId(rows);
        }
        void bind(ConfigSnapshot snapshot) {
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        public List<SkillLevelConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public SkillLevelConfig find(int k0, int k1, int k2) { return index_Key.row(index_Key.find(k0, k1, k2)); }
        public SkillLevelConfig get(int k0, int k1, int k2) { SkillLevelConfig value = find(k0, k1, k2); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<SkillLevelConfig> getByLevel(int k0) { return index_Group_ByLevel.group(index_Group_ByLevel.find(k0)); }
        public List<SkillLevelConfig> getByQuality(int k0) { return index_Group_ByQuality.group(index_Group_ByQuality.find(k0)); }
        public List<SkillLevelConfig> getBySkillId(int k0) { return index_Group_BySkillId.group(index_Group_BySkillId.find(k0)); }
    }
}
