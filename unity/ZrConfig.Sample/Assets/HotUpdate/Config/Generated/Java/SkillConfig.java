// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class SkillConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 技能编号
    public int getId() { return v_Id; }
    private final String v_Name;
    // 技能名称
    public String getName() { return v_Name; }
    private final int v_Hero;
    // 英雄 Id；反向引用 Hero 表
    public int getHeroId() { return v_Hero; }
    private HeroConfig ref_Hero;
    public HeroConfig getHero() { return ref_Hero; }
    private SkillConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Name = ConfigRuntime.string(ConfigRuntime.field(value, "Name"));
        v_Hero = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Hero"),Integer.MIN_VALUE,Integer.MAX_VALUE);
    }
    static String schema() { return String.join("",
        "{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<SkillConfig> {
        Index_Key(List<SkillConfig> rows) { super(rows, true); }
        @Override protected int hash(SkillConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(SkillConfig a, SkillConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { SkillConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<SkillConfig> {
        Index_Group_ById(List<SkillConfig> rows) { super(rows, false); }
        @Override protected int hash(SkillConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(SkillConfig a, SkillConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { SkillConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        SkillConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<SkillConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<SkillConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        private final Map<Integer,SkillConfig> ids = new HashMap<>();
        Table(List<Object> source) {
            var values = new ArrayList<SkillConfig>(source.size());
            for (Object value : source) values.add(new SkillConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
            for (var row : rows) { if (ids.put(row.v_Id, row) != null) throw ConfigRuntime.invalid("引用 Id 不唯一"); }
        }
        void bind(ConfigSnapshot snapshot) {
            for (var row : rows) if (row.v_Hero != 0) { row.ref_Hero = snapshot.Hero.byId(row.v_Hero); if (row.ref_Hero == null) throw ConfigRuntime.invalid("引用目标不存在：Skill.Hero"); }
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        SkillConfig byId(int id) { return ids.get(id); }
        public List<SkillConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public SkillConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public SkillConfig get(int k0) { SkillConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<SkillConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
