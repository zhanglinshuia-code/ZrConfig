// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class HeroConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 英雄编号
    public int getId() { return v_Id; }
    private final String v_Name;
    // 英雄名称
    public String getName() { return v_Name; }
    private final int v_Skill;
    // 技能 Id；引用 Skill 表
    public int getSkillId() { return v_Skill; }
    private SkillConfig ref_Skill;
    public SkillConfig getSkill() { return ref_Skill; }
    private final int v_OtherSkill;
    // 另一个技能 Id；可复用同一对象
    public int getOtherSkillId() { return v_OtherSkill; }
    private SkillConfig ref_OtherSkill;
    public SkillConfig getOtherSkill() { return ref_OtherSkill; }
    private final int v_Mentor;
    // 导师 Id；引用 Hero 表
    public int getMentorId() { return v_Mentor; }
    private HeroConfig ref_Mentor;
    public HeroConfig getMentor() { return ref_Mentor; }
    private HeroConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Name = ConfigRuntime.string(ConfigRuntime.field(value, "Name"));
        v_Skill = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Skill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_OtherSkill = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "OtherSkill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Mentor = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Mentor"),Integer.MIN_VALUE,Integer.MAX_VALUE);
    }
    static String schema() { return String.join("",
        "{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<HeroConfig> {
        Index_Key(List<HeroConfig> rows) { super(rows, true); }
        @Override protected int hash(HeroConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(HeroConfig a, HeroConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { HeroConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        HeroConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<HeroConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<HeroConfig> {
        Index_Group_ById(List<HeroConfig> rows) { super(rows, false); }
        @Override protected int hash(HeroConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(HeroConfig a, HeroConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { HeroConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        HeroConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<HeroConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<HeroConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        private final Map<Integer,HeroConfig> ids = new HashMap<>();
        Table(List<Object> source) {
            var values = new ArrayList<HeroConfig>(source.size());
            for (Object value : source) values.add(new HeroConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
            for (var row : rows) { if (ids.put(row.v_Id, row) != null) throw ConfigRuntime.invalid("引用 Id 不唯一"); }
        }
        void bind(ConfigSnapshot snapshot) {
            for (var row : rows) if (row.v_Skill != 0) { row.ref_Skill = snapshot.Skill.byId(row.v_Skill); if (row.ref_Skill == null) throw ConfigRuntime.invalid("引用目标不存在：Hero.Skill"); }
            for (var row : rows) if (row.v_OtherSkill != 0) { row.ref_OtherSkill = snapshot.Skill.byId(row.v_OtherSkill); if (row.ref_OtherSkill == null) throw ConfigRuntime.invalid("引用目标不存在：Hero.OtherSkill"); }
            for (var row : rows) if (row.v_Mentor != 0) { row.ref_Mentor = snapshot.Hero.byId(row.v_Mentor); if (row.ref_Mentor == null) throw ConfigRuntime.invalid("引用目标不存在：Hero.Mentor"); }
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        HeroConfig byId(int id) { return ids.get(id); }
        public List<HeroConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public HeroConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public HeroConfig get(int k0) { HeroConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<HeroConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
