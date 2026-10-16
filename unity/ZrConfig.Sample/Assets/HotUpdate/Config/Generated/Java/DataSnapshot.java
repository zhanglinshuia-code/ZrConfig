// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
import java.nio.file.Path;
import java.io.IOException;
import static zrconfig.ConfigRuntime.*;
/** Independent server snapshot; explicit loads share the same row slots in every mode. */
public final class DataSnapshot implements SnapshotIdentityProvider {
    private final LoadBudget budget;
    private final String namespace,manifestDigest,loadPlanFingerprint;
    public final String version;
    public final SelectionPlan selection;
    public final LoadMode mode;
    private final AllTypesTable table_AllTypes;
    private final AllTypes10kTable table_AllTypes10k;
    private final DropTable table_Drop;
    private final HeroTable table_Hero;
    private final ItemCatalogTable table_ItemCatalog;
    private final QuestRewardsTable table_QuestRewards;
    private final ShopOffersTable table_ShopOffers;
    private final SkillTable table_Skill;
    private final SkillLevelTable table_SkillLevel;
    private DataSnapshot(ConfigSource source,LoadPlan plan,SelectionPlan selection,Cancellation cancellation) throws IOException {
        this.selection = selection; mode = plan.mode(); namespace=plan.sourceNamespace();loadPlanFingerprint=ConfigRuntime.loadPlanFingerprint(mode,plan.preferJson(),selection.tables());
        var bundle = new Bundle(source,plan.limits(),cancellation); bundle.validateCatalog(ServerConfig.catalog()); version = bundle.version;budget=bundle.budget();manifestDigest=bundle.manifestDigest;
        var indexed = new HashMap<String,IndexedRows>(); var selected = new HashSet<>(selection.tables());
        if (selected.contains("AllTypes")) { cancellation.check(); indexed.put("AllTypes",bundle.openRows(AllTypesConfig.schema(),plan.preferJson())); }
        if (selected.contains("AllTypes10k")) { cancellation.check(); indexed.put("AllTypes10k",bundle.openRows(AllTypes10kConfig.schema(),plan.preferJson())); }
        if (selected.contains("Drop")) { cancellation.check(); indexed.put("Drop",bundle.openRows(DropConfig.schema(),plan.preferJson())); }
        if (selected.contains("Hero")) { cancellation.check(); indexed.put("Hero",bundle.openRows(HeroConfig.schema(),plan.preferJson())); }
        if (selected.contains("ItemCatalog")) { cancellation.check(); indexed.put("ItemCatalog",bundle.openRows(ItemCatalogConfig.schema(),plan.preferJson())); }
        if (selected.contains("QuestRewards")) { cancellation.check(); indexed.put("QuestRewards",bundle.openRows(QuestRewardsConfig.schema(),plan.preferJson())); }
        if (selected.contains("ShopOffers")) { cancellation.check(); indexed.put("ShopOffers",bundle.openRows(ShopOffersConfig.schema(),plan.preferJson())); }
        if (selected.contains("Skill")) { cancellation.check(); indexed.put("Skill",bundle.openRows(SkillConfig.schema(),plan.preferJson())); }
        if (selected.contains("SkillLevel")) { cancellation.check(); indexed.put("SkillLevel",bundle.openRows(SkillLevelConfig.schema(),plan.preferJson())); }
        table_AllTypes = selected.contains("AllTypes") ? new AllTypesTable(indexed.get("AllTypes"),this) : null;
        table_AllTypes10k = selected.contains("AllTypes10k") ? new AllTypes10kTable(indexed.get("AllTypes10k"),this) : null;
        table_Drop = selected.contains("Drop") ? new DropTable(indexed.get("Drop"),this) : null;
        table_Hero = selected.contains("Hero") ? new HeroTable(indexed.get("Hero"),this) : null;
        table_ItemCatalog = selected.contains("ItemCatalog") ? new ItemCatalogTable(indexed.get("ItemCatalog"),this) : null;
        table_QuestRewards = selected.contains("QuestRewards") ? new QuestRewardsTable(indexed.get("QuestRewards"),this) : null;
        table_ShopOffers = selected.contains("ShopOffers") ? new ShopOffersTable(indexed.get("ShopOffers"),this) : null;
        table_Skill = selected.contains("Skill") ? new SkillTable(indexed.get("Skill"),this) : null;
        table_SkillLevel = selected.contains("SkillLevel") ? new SkillLevelTable(indexed.get("SkillLevel"),this) : null;
        for (var data : indexed.values()) data.bindReferences(indexed::get);
        if (mode != LoadMode.LazyRowsOnAccess) {
            if (table_AllTypes != null) table_AllTypes.validate(indexed::get,cancellation);
            if (table_AllTypes10k != null) table_AllTypes10k.validate(indexed::get,cancellation);
            if (table_Drop != null) table_Drop.validate(indexed::get,cancellation);
            if (table_Hero != null) table_Hero.validate(indexed::get,cancellation);
            if (table_ItemCatalog != null) table_ItemCatalog.validate(indexed::get,cancellation);
            if (table_QuestRewards != null) table_QuestRewards.validate(indexed::get,cancellation);
            if (table_ShopOffers != null) table_ShopOffers.validate(indexed::get,cancellation);
            if (table_Skill != null) table_Skill.validate(indexed::get,cancellation);
            if (table_SkillLevel != null) table_SkillLevel.validate(indexed::get,cancellation);
        }
        if (mode == LoadMode.ServerDefault || mode == LoadMode.SelectedTables) loadAll(cancellation);
        for (String name : plan.warmupTables()) switch(name) {
            case "AllTypes" -> get_AllTypes().loadAll(cancellation);
            case "AllTypes10k" -> get_AllTypes10k().loadAll(cancellation);
            case "Drop" -> get_Drop().loadAll(cancellation);
            case "Hero" -> get_Hero().loadAll(cancellation);
            case "ItemCatalog" -> get_ItemCatalog().loadAll(cancellation);
            case "QuestRewards" -> get_QuestRewards().loadAll(cancellation);
            case "ShopOffers" -> get_ShopOffers().loadAll(cancellation);
            case "Skill" -> get_Skill().loadAll(cancellation);
            case "SkillLevel" -> get_SkillLevel().loadAll(cancellation);
            default -> throw new ConfigException(ErrorCode.InvalidPlan,"Unknown warmup table");
        }
        cancellation.check();
    }
    public static DataSnapshot load(Path root,LoadPlan plan,Cancellation cancellation) throws IOException {
        var source=new FileConfigSource(root);return load(source,new LoadPlan(plan.mode(),plan.tables(),plan.preferJson(),plan.warmupTables(),plan.limits(),plan.sourceNamespace().isEmpty()?source.sourceNamespace():plan.sourceNamespace()),cancellation);
    }
    public String version(){return version;}
    public String manifestDigest(){return manifestDigest;}
    public String sourceNamespace(){return namespace;}
    public String catalogFingerprint(){return selection.catalogFingerprint();}
    public String loadPlanFingerprint(){return loadPlanFingerprint;}
    public SnapshotDiagnostics diagnostics(){return budget.diagnostics();}
    public static DataSnapshot load(ConfigSource source,LoadPlan plan,Cancellation cancellation) throws IOException {
        Collection<String> requested = plan.tables() == null ? ServerConfig.catalog().tables.stream().map(TableDescriptor::name).toList() : plan.tables();
        var selection = ServerConfig.catalog().select(requested);
        for (String name : plan.warmupTables()) if (!selection.tables().contains(name)) throw new ConfigException(ErrorCode.InvalidPlan,"Warmup outside selection");
        cancellation.check(); try{return new DataSnapshot(source,plan,selection,cancellation);}catch(IOException error){throw new ConfigException(ErrorCode.ReadFailed,error.getMessage(),"read",0,error);}catch(RuntimeException error){throw serverFailure(error);}
    }
    public static ServerManager<DataSnapshot> manager() { return new ServerManager<>(); }
    public void loadAll(Cancellation cancellation) {
        if (table_AllTypes != null) table_AllTypes.loadAll(cancellation);
        if (table_AllTypes10k != null) table_AllTypes10k.loadAll(cancellation);
        if (table_Drop != null) table_Drop.loadAll(cancellation);
        if (table_Hero != null) table_Hero.loadAll(cancellation);
        if (table_ItemCatalog != null) table_ItemCatalog.loadAll(cancellation);
        if (table_QuestRewards != null) table_QuestRewards.loadAll(cancellation);
        if (table_ShopOffers != null) table_ShopOffers.loadAll(cancellation);
        if (table_Skill != null) table_Skill.loadAll(cancellation);
        if (table_SkillLevel != null) table_SkillLevel.loadAll(cancellation);
    }
    public int loadedRowCount() { return (table_AllTypes == null ? 0 : table_AllTypes.loadedCount()) + (table_AllTypes10k == null ? 0 : table_AllTypes10k.loadedCount()) + (table_Drop == null ? 0 : table_Drop.loadedCount()) + (table_Hero == null ? 0 : table_Hero.loadedCount()) + (table_ItemCatalog == null ? 0 : table_ItemCatalog.loadedCount()) + (table_QuestRewards == null ? 0 : table_QuestRewards.loadedCount()) + (table_ShopOffers == null ? 0 : table_ShopOffers.loadedCount()) + (table_Skill == null ? 0 : table_Skill.loadedCount()) + (table_SkillLevel == null ? 0 : table_SkillLevel.loadedCount()); }
    public int completedTableCount(){return (table_AllTypes!=null && table_AllTypes.isComplete()?1:0)+(table_AllTypes10k!=null && table_AllTypes10k.isComplete()?1:0)+(table_Drop!=null && table_Drop.isComplete()?1:0)+(table_Hero!=null && table_Hero.isComplete()?1:0)+(table_ItemCatalog!=null && table_ItemCatalog.isComplete()?1:0)+(table_QuestRewards!=null && table_QuestRewards.isComplete()?1:0)+(table_ShopOffers!=null && table_ShopOffers.isComplete()?1:0)+(table_Skill!=null && table_Skill.isComplete()?1:0)+(table_SkillLevel!=null && table_SkillLevel.isComplete()?1:0);}
    public static final class Warmup {
        private final String table; private final java.util.function.BiConsumer<DataSnapshot,Cancellation> action;
        private Warmup(String table,java.util.function.BiConsumer<DataSnapshot,Cancellation> action){this.table=table;this.action=action;}
        public String table(){return table;}
    }
    public static DataSnapshot loadWithWarmup(ConfigSource source,LoadPlan plan,List<Warmup> requests,Cancellation cancellation)throws IOException {
        if(requests==null||requests.stream().anyMatch(Objects::isNull))throw new ConfigException(ErrorCode.InvalidPlan,"Missing warmup request");var steps=List.copyOf(requests);
        var selected=ServerConfig.catalog().select(plan.tables()==null?ServerConfig.catalog().tables.stream().map(TableDescriptor::name).toList():plan.tables());
        for(var step:steps)if(!selected.tables().contains(step.table))throw new ConfigException(ErrorCode.InvalidPlan,"Warmup outside selection");
        var next=load(source,plan,cancellation);for(var step:steps){cancellation.check();step.action.accept(next,cancellation);}cancellation.check();return next;
    }
    public static DataSnapshot loadWithWarmup(Path root,LoadPlan plan,List<Warmup> requests,Cancellation cancellation)throws IOException {
        var source=new FileConfigSource(root);return loadWithWarmup(source,new LoadPlan(plan.mode(),plan.tables(),plan.preferJson(),plan.warmupTables(),plan.limits(),plan.sourceNamespace().isEmpty()?source.sourceNamespace():plan.sourceNamespace()),requests,cancellation);
    }
    public static Warmup warmupTable_AllTypes(){return new Warmup("AllTypes",(snapshot,token)->snapshot.get_AllTypes().loadAll(token));}
    public static Warmup warmupRow_AllTypes(int k0){return new Warmup("AllTypes",(snapshot,token)->snapshot.get_AllTypes().load(k0,token));}
    public static Warmup warmupGroup_AllTypes_ById(int k0){return new Warmup("AllTypes",(snapshot,token)->snapshot.get_AllTypes().loadGroup_ById(k0,token));}
    public static Warmup warmupGroup_AllTypes_ByQuality(ConfigTypes.T_Game_Config_Quality k0){return new Warmup("AllTypes",(snapshot,token)->snapshot.get_AllTypes().loadGroup_ByQuality(k0,token));}
    public static Warmup warmupTable_AllTypes10k(){return new Warmup("AllTypes10k",(snapshot,token)->snapshot.get_AllTypes10k().loadAll(token));}
    public static Warmup warmupRow_AllTypes10k(int k0){return new Warmup("AllTypes10k",(snapshot,token)->snapshot.get_AllTypes10k().load(k0,token));}
    public static Warmup warmupGroup_AllTypes10k_ById(int k0){return new Warmup("AllTypes10k",(snapshot,token)->snapshot.get_AllTypes10k().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_Drop(){return new Warmup("Drop",(snapshot,token)->snapshot.get_Drop().loadAll(token));}
    public static Warmup warmupRow_Drop(int k0,int k1){return new Warmup("Drop",(snapshot,token)->snapshot.get_Drop().load(k0,k1,token));}
    public static Warmup warmupGroup_Drop_ByGroupId(int k0){return new Warmup("Drop",(snapshot,token)->snapshot.get_Drop().loadGroup_ByGroupId(k0,token));}
    public static Warmup warmupGroup_Drop_ById(int k0){return new Warmup("Drop",(snapshot,token)->snapshot.get_Drop().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_Hero(){return new Warmup("Hero",(snapshot,token)->snapshot.get_Hero().loadAll(token));}
    public static Warmup warmupRow_Hero(int k0){return new Warmup("Hero",(snapshot,token)->snapshot.get_Hero().load(k0,token));}
    public static Warmup warmupGroup_Hero_ById(int k0){return new Warmup("Hero",(snapshot,token)->snapshot.get_Hero().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_ItemCatalog(){return new Warmup("ItemCatalog",(snapshot,token)->snapshot.get_ItemCatalog().loadAll(token));}
    public static Warmup warmupRow_ItemCatalog(int k0){return new Warmup("ItemCatalog",(snapshot,token)->snapshot.get_ItemCatalog().load(k0,token));}
    public static Warmup warmupGroup_ItemCatalog_ById(int k0){return new Warmup("ItemCatalog",(snapshot,token)->snapshot.get_ItemCatalog().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_QuestRewards(){return new Warmup("QuestRewards",(snapshot,token)->snapshot.get_QuestRewards().loadAll(token));}
    public static Warmup warmupRow_QuestRewards(int k0){return new Warmup("QuestRewards",(snapshot,token)->snapshot.get_QuestRewards().load(k0,token));}
    public static Warmup warmupGroup_QuestRewards_ById(int k0){return new Warmup("QuestRewards",(snapshot,token)->snapshot.get_QuestRewards().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_ShopOffers(){return new Warmup("ShopOffers",(snapshot,token)->snapshot.get_ShopOffers().loadAll(token));}
    public static Warmup warmupRow_ShopOffers(int k0){return new Warmup("ShopOffers",(snapshot,token)->snapshot.get_ShopOffers().load(k0,token));}
    public static Warmup warmupGroup_ShopOffers_ById(int k0){return new Warmup("ShopOffers",(snapshot,token)->snapshot.get_ShopOffers().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_Skill(){return new Warmup("Skill",(snapshot,token)->snapshot.get_Skill().loadAll(token));}
    public static Warmup warmupRow_Skill(int k0){return new Warmup("Skill",(snapshot,token)->snapshot.get_Skill().load(k0,token));}
    public static Warmup warmupGroup_Skill_ById(int k0){return new Warmup("Skill",(snapshot,token)->snapshot.get_Skill().loadGroup_ById(k0,token));}
    public static Warmup warmupTable_SkillLevel(){return new Warmup("SkillLevel",(snapshot,token)->snapshot.get_SkillLevel().loadAll(token));}
    public static Warmup warmupRow_SkillLevel(int k0,int k1,int k2){return new Warmup("SkillLevel",(snapshot,token)->snapshot.get_SkillLevel().load(k0,k1,k2,token));}
    public static Warmup warmupGroup_SkillLevel_ByLevel(int k0){return new Warmup("SkillLevel",(snapshot,token)->snapshot.get_SkillLevel().loadGroup_ByLevel(k0,token));}
    public static Warmup warmupGroup_SkillLevel_ByQuality(int k0){return new Warmup("SkillLevel",(snapshot,token)->snapshot.get_SkillLevel().loadGroup_ByQuality(k0,token));}
    public static Warmup warmupGroup_SkillLevel_BySkillId(int k0){return new Warmup("SkillLevel",(snapshot,token)->snapshot.get_SkillLevel().loadGroup_BySkillId(k0,token));}
    public AllTypesTable get_AllTypes() { if (table_AllTypes == null) throw new ConfigException(ErrorCode.TableNotSelected,"AllTypes"); return table_AllTypes; }
    public AllTypes10kTable get_AllTypes10k() { if (table_AllTypes10k == null) throw new ConfigException(ErrorCode.TableNotSelected,"AllTypes10k"); return table_AllTypes10k; }
    public DropTable get_Drop() { if (table_Drop == null) throw new ConfigException(ErrorCode.TableNotSelected,"Drop"); return table_Drop; }
    public HeroTable get_Hero() { if (table_Hero == null) throw new ConfigException(ErrorCode.TableNotSelected,"Hero"); return table_Hero; }
    public ItemCatalogTable get_ItemCatalog() { if (table_ItemCatalog == null) throw new ConfigException(ErrorCode.TableNotSelected,"ItemCatalog"); return table_ItemCatalog; }
    public QuestRewardsTable get_QuestRewards() { if (table_QuestRewards == null) throw new ConfigException(ErrorCode.TableNotSelected,"QuestRewards"); return table_QuestRewards; }
    public ShopOffersTable get_ShopOffers() { if (table_ShopOffers == null) throw new ConfigException(ErrorCode.TableNotSelected,"ShopOffers"); return table_ShopOffers; }
    public SkillTable get_Skill() { if (table_Skill == null) throw new ConfigException(ErrorCode.TableNotSelected,"Skill"); return table_Skill; }
    public SkillLevelTable get_SkillLevel() { if (table_SkillLevel == null) throw new ConfigException(ErrorCode.TableNotSelected,"SkillLevel"); return table_SkillLevel; }
    public static final class AllTypesRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final short value_ByteValue;
        private final short value_ShortValue;
        private final int value_IntValue;
        private final long value_LongValue;
        private final float value_FloatValue;
        private final double value_DoubleValue;
        private final boolean value_BoolValue;
        private final String value_StringValue;
        private final byte value_SByteValue;
        private final int value_UShortValue;
        private final long value_UIntValue;
        private final java.math.BigInteger value_ULongValue;
        private final ConfigRuntime.Decimal value_DecimalValue;
        private final ConfigTypes.T_Game_Config_Quality value_QualityValue;
        private final ConfigTypes.T_ZrConfig_Types_Vector2 value_Vector2Value;
        private final ConfigTypes.T_ZrConfig_Types_Vector3 value_Vector3Value;
        private final ConfigTypes.T_ZrConfig_Types_Kv value_KvValue;
        private final ConfigTypes.T_Game_Config_Money value_MoneyValue;
        private final List<Short> value_ByteArray;
        private final List<Short> value_ShortArray;
        private final List<Integer> value_IntArray;
        private final List<Long> value_LongArray;
        private final List<Float> value_FloatArray;
        private final List<Double> value_DoubleArray;
        private final List<Boolean> value_BoolArray;
        private final List<String> value_StringArray;
        private final List<Byte> value_SByteArray;
        private final List<Integer> value_UShortArray;
        private final List<Long> value_UIntArray;
        private final List<java.math.BigInteger> value_ULongArray;
        private final List<ConfigRuntime.Decimal> value_DecimalArray;
        private final List<ConfigTypes.T_Game_Config_Quality> value_QualityArray;
        private final List<ConfigTypes.T_ZrConfig_Types_Vector2> value_Vector2Array;
        private final List<ConfigTypes.T_ZrConfig_Types_Vector3> value_Vector3Array;
        private final List<ConfigTypes.T_ZrConfig_Types_Kv> value_KvArray;
        private final List<ConfigTypes.T_Game_Config_Money> value_MoneyArray;
        private final List<List<Short>> value_ByteArray2D;
        private final List<List<Short>> value_ShortArray2D;
        private final List<List<Integer>> value_IntArray2D;
        private final List<List<Long>> value_LongArray2D;
        private final List<List<Float>> value_FloatArray2D;
        private final List<List<Double>> value_DoubleArray2D;
        private final List<List<Boolean>> value_BoolArray2D;
        private final List<List<String>> value_StringArray2D;
        private final List<List<Byte>> value_SByteArray2D;
        private final List<List<Integer>> value_UShortArray2D;
        private final List<List<Long>> value_UIntArray2D;
        private final List<List<java.math.BigInteger>> value_ULongArray2D;
        private final List<List<ConfigRuntime.Decimal>> value_DecimalArray2D;
        private final List<List<ConfigTypes.T_Game_Config_Quality>> value_QualityArray2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> value_Vector2Array2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> value_Vector3Array2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Kv>> value_KvArray2D;
        private final List<List<ConfigTypes.T_Game_Config_Money>> value_MoneyArray2D;
        private final List<List<List<Short>>> value_ByteArray3D;
        private final List<List<List<Short>>> value_ShortArray3D;
        private final List<List<List<Integer>>> value_IntArray3D;
        private final List<List<List<Long>>> value_LongArray3D;
        private final List<List<List<Float>>> value_FloatArray3D;
        private final List<List<List<Double>>> value_DoubleArray3D;
        private final List<List<List<Boolean>>> value_BoolArray3D;
        private final List<List<List<String>>> value_StringArray3D;
        private final List<List<List<Byte>>> value_SByteArray3D;
        private final List<List<List<Integer>>> value_UShortArray3D;
        private final List<List<List<Long>>> value_UIntArray3D;
        private final List<List<List<java.math.BigInteger>>> value_ULongArray3D;
        private final List<List<List<ConfigRuntime.Decimal>>> value_DecimalArray3D;
        private final List<List<List<ConfigTypes.T_Game_Config_Quality>>> value_QualityArray3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> value_Vector2Array3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> value_Vector3Array3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> value_KvArray3D;
        private final List<List<List<ConfigTypes.T_Game_Config_Money>>> value_MoneyArray3D;
        private final ConfigTypes.T_Game_Config_BattleSettings value_Settings;
        private final ConfigTypes.T_Game_Config_ConfigRange value_Range;
        private final ConfigTypes.T_Game_Config_SampleOptions value_Options;
        private final ConfigTypes.T_Game_Config_BattleAction value_Action;
        private final List<ConfigTypes.T_Game_Config_BattleAction> value_Actions;
        private final List<ConfigTypes.T_Game_Config_ConfigRange> value_Ranges;
        private final List<ConfigTypes.T_Game_Config_SampleOptions> value_OptionsList;
        private final List<ConfigTypes.T_Game_Config_ActionGroup> value_Groups;
        private final List<ConfigTypes.T_Game_Config_BattlePhase> value_Phases;
        private final ConfigTypes.T_Game_Config_ConfigRange value_OptionalRange;
        private final Integer value_OptionalInt;
        private final Long value_OptionalUInt;
        private final java.math.BigInteger value_OptionalULong;
        private final ConfigRuntime.Decimal value_OptionalDecimal;
        private final Boolean value_OptionalBool;
        private final ConfigTypes.T_Game_Config_Quality value_OptionalQuality;
        private final ConfigTypes.T_Game_Config_Money value_OptionalMoney;
        private final List<Integer> value_OptionalValues;
        private final Map<String,Integer> value_StatMap;
        private final Map<Integer,ConfigRuntime.Decimal> value_PriceMap;
        private final Map<ConfigTypes.T_Game_Config_Quality,Long> value_QualityMap;
        private final Map<String,List<Integer>> value_ListMap;
        private final ConfigRuntime.Matrix<Integer> value_IntMatrix;
        private final ConfigRuntime.Matrix<ConfigRuntime.Decimal> value_DecimalCube;
        private final ConfigRuntime.Matrix<Long> value_OptionalMatrix;
        private final String value_RawJsonValue;
        private final List<String> value_RawJsonArray;
        private final List<List<String>> value_RawJsonArray2D;
        private final List<List<List<String>>> value_RawJsonArray3D;
        private final List<Integer> value_JsonIntArray;
        private AllTypesRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_ByteValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value,"ByteValue"),0,255);
            value_ShortValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value,"ShortValue"),-32768,32767);
            value_IntValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"IntValue"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_LongValue = ConfigRuntime.int64(ConfigRuntime.field(value,"LongValue"));
            value_FloatValue = ConfigRuntime.float32(ConfigRuntime.field(value,"FloatValue"));
            value_DoubleValue = ConfigRuntime.float64(ConfigRuntime.field(value,"DoubleValue"));
            value_BoolValue = ConfigRuntime.bool(ConfigRuntime.field(value,"BoolValue"));
            value_StringValue = ConfigRuntime.string(ConfigRuntime.field(value,"StringValue"));
            value_SByteValue = (byte)ConfigRuntime.integer(ConfigRuntime.field(value,"SByteValue"),-128,127);
            value_UShortValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"UShortValue"),0,65535);
            value_UIntValue = ConfigRuntime.integer(ConfigRuntime.field(value,"UIntValue"),0,0xffffffffL);
            value_ULongValue = ConfigRuntime.uint64(ConfigRuntime.field(value,"ULongValue"));
            value_DecimalValue = new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value,"DecimalValue")));
            value_QualityValue = ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value,"QualityValue"),Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_Vector2Value = ConfigTypes.read_T_ZrConfig_Types_Vector2(ConfigRuntime.field(value,"Vector2Value"));
            value_Vector3Value = ConfigTypes.read_T_ZrConfig_Types_Vector3(ConfigRuntime.field(value,"Vector3Value"));
            value_KvValue = ConfigTypes.read_T_ZrConfig_Types_Kv(ConfigRuntime.field(value,"KvValue"));
            value_MoneyValue = ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value,"MoneyValue"));
            value_ByteArray = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray"), item0 -> (short)ConfigRuntime.integer(item0,0,255));
            value_ShortArray = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray"), item0 -> (short)ConfigRuntime.integer(item0,-32768,32767));
            value_IntArray = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_LongArray = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray"), item0 -> ConfigRuntime.int64(item0));
            value_FloatArray = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray"), item0 -> ConfigRuntime.float32(item0));
            value_DoubleArray = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray"), item0 -> ConfigRuntime.float64(item0));
            value_BoolArray = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray"), item0 -> ConfigRuntime.bool(item0));
            value_StringArray = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray"), item0 -> ConfigRuntime.string(item0));
            value_SByteArray = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray"), item0 -> (byte)ConfigRuntime.integer(item0,-128,127));
            value_UShortArray = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray"), item0 -> (int)ConfigRuntime.integer(item0,0,65535));
            value_UIntArray = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray"), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
            value_ULongArray = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray"), item0 -> ConfigRuntime.uint64(item0));
            value_DecimalArray = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
            value_QualityArray = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_Vector2Array = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item0));
            value_Vector3Array = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item0));
            value_KvArray = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item0));
            value_MoneyArray = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray"), item0 -> ConfigTypes.read_T_Game_Config_Money(item0));
            value_ByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,0,255)));
            value_ShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,-32768,32767)));
            value_IntArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_LongArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.int64(item1)));
            value_FloatArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float32(item1)));
            value_DoubleArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float64(item1)));
            value_BoolArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.bool(item1)));
            value_StringArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
            value_SByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (byte)ConfigRuntime.integer(item1,-128,127)));
            value_UShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,0,65535)));
            value_UIntArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.integer(item1,0,0xffffffffL)));
            value_ULongArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.uint64(item1)));
            value_DecimalArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item1))));
            value_QualityArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE))));
            value_Vector2Array2D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item1)));
            value_Vector3Array2D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item1)));
            value_KvArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item1)));
            value_MoneyArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_Game_Config_Money(item1)));
            value_ByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,0,255))));
            value_ShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,-32768,32767))));
            value_IntArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE))));
            value_LongArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.int64(item2))));
            value_FloatArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float32(item2))));
            value_DoubleArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float64(item2))));
            value_BoolArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.bool(item2))));
            value_StringArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
            value_SByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (byte)ConfigRuntime.integer(item2,-128,127))));
            value_UShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,0,65535))));
            value_UIntArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.integer(item2,0,0xffffffffL))));
            value_ULongArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.uint64(item2))));
            value_DecimalArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item2)))));
            value_QualityArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE)))));
            value_Vector2Array3D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item2))));
            value_Vector3Array3D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item2))));
            value_KvArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item2))));
            value_MoneyArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_Game_Config_Money(item2))));
            value_Settings = ConfigTypes.read_T_Game_Config_BattleSettings(ConfigRuntime.field(value,"Settings"));
            value_Range = ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value,"Range"));
            value_Options = ConfigTypes.read_T_Game_Config_SampleOptions(ConfigRuntime.field(value,"Options"));
            value_Action = ConfigTypes.read_T_Game_Config_BattleAction(ConfigRuntime.field(value,"Action"));
            value_Actions = ConfigRuntime.list(ConfigRuntime.field(value,"Actions"), item0 -> ConfigTypes.read_T_Game_Config_BattleAction(item0));
            value_Ranges = ConfigRuntime.list(ConfigRuntime.field(value,"Ranges"), item0 -> ConfigTypes.read_T_Game_Config_ConfigRange(item0));
            value_OptionsList = ConfigRuntime.list(ConfigRuntime.field(value,"OptionsList"), item0 -> ConfigTypes.read_T_Game_Config_SampleOptions(item0));
            value_Groups = ConfigRuntime.list(ConfigRuntime.field(value,"Groups"), item0 -> ConfigTypes.read_T_Game_Config_ActionGroup(item0));
            value_Phases = ConfigRuntime.list(ConfigRuntime.field(value,"Phases"), item0 -> ConfigTypes.read_T_Game_Config_BattlePhase(item0));
            value_OptionalRange = (ConfigRuntime.field(value,"OptionalRange") == null ? null : ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value,"OptionalRange")));
            value_OptionalInt = (ConfigRuntime.field(value,"OptionalInt") == null ? null : (int)ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalInt"),Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_OptionalUInt = (ConfigRuntime.field(value,"OptionalUInt") == null ? null : ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalUInt"),0,0xffffffffL));
            value_OptionalULong = (ConfigRuntime.field(value,"OptionalULong") == null ? null : ConfigRuntime.uint64(ConfigRuntime.field(value,"OptionalULong")));
            value_OptionalDecimal = (ConfigRuntime.field(value,"OptionalDecimal") == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value,"OptionalDecimal"))));
            value_OptionalBool = (ConfigRuntime.field(value,"OptionalBool") == null ? null : ConfigRuntime.bool(ConfigRuntime.field(value,"OptionalBool")));
            value_OptionalQuality = (ConfigRuntime.field(value,"OptionalQuality") == null ? null : ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalQuality"),Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_OptionalMoney = (ConfigRuntime.field(value,"OptionalMoney") == null ? null : ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value,"OptionalMoney")));
            value_OptionalValues = ConfigRuntime.list(ConfigRuntime.field(value,"OptionalValues"), item0 -> (item0 == null ? null : (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_StatMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"StatMap"), item0 -> ConfigRuntime.string(item0), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_PriceMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"PriceMap"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE), item0 -> (item0 == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(item0))));
            value_QualityMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"QualityMap"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
            value_ListMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"ListMap"), item0 -> ConfigRuntime.string(item0), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_IntMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value,"IntMatrix"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_DecimalCube = ConfigRuntime.matrix(ConfigRuntime.field(value,"DecimalCube"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
            value_OptionalMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value,"OptionalMatrix"), item0 -> (item0 == null ? null : ConfigRuntime.integer(item0,0,0xffffffffL)));
            value_RawJsonValue = ConfigRuntime.string(ConfigRuntime.field(value,"RawJsonValue"));
            value_RawJsonArray = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray"), item0 -> ConfigRuntime.string(item0));
            value_RawJsonArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
            value_RawJsonArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
            value_JsonIntArray = ConfigRuntime.list(ConfigRuntime.field(value,"JsonIntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
        }
        public int getId() { return value_Id; }
        public short getByteValue() { return value_ByteValue; }
        public short getShortValue() { return value_ShortValue; }
        public int getIntValue() { return value_IntValue; }
        public long getLongValue() { return value_LongValue; }
        public float getFloatValue() { return value_FloatValue; }
        public double getDoubleValue() { return value_DoubleValue; }
        public boolean getBoolValue() { return value_BoolValue; }
        public String getStringValue() { return value_StringValue; }
        public byte getSByteValue() { return value_SByteValue; }
        public int getUShortValue() { return value_UShortValue; }
        public long getUIntValue() { return value_UIntValue; }
        public java.math.BigInteger getULongValue() { return value_ULongValue; }
        public ConfigRuntime.Decimal getDecimalValue() { return value_DecimalValue; }
        public ConfigTypes.T_Game_Config_Quality getQualityValue() { return value_QualityValue; }
        public ConfigTypes.T_ZrConfig_Types_Vector2 getVector2Value() { return value_Vector2Value; }
        public ConfigTypes.T_ZrConfig_Types_Vector3 getVector3Value() { return value_Vector3Value; }
        public ConfigTypes.T_ZrConfig_Types_Kv getKvValue() { return value_KvValue; }
        public ConfigTypes.T_Game_Config_Money getMoneyValue() { return value_MoneyValue; }
        public List<Short> getByteArray() { return value_ByteArray; }
        public List<Short> getShortArray() { return value_ShortArray; }
        public List<Integer> getIntArray() { return value_IntArray; }
        public List<Long> getLongArray() { return value_LongArray; }
        public List<Float> getFloatArray() { return value_FloatArray; }
        public List<Double> getDoubleArray() { return value_DoubleArray; }
        public List<Boolean> getBoolArray() { return value_BoolArray; }
        public List<String> getStringArray() { return value_StringArray; }
        public List<Byte> getSByteArray() { return value_SByteArray; }
        public List<Integer> getUShortArray() { return value_UShortArray; }
        public List<Long> getUIntArray() { return value_UIntArray; }
        public List<java.math.BigInteger> getULongArray() { return value_ULongArray; }
        public List<ConfigRuntime.Decimal> getDecimalArray() { return value_DecimalArray; }
        public List<ConfigTypes.T_Game_Config_Quality> getQualityArray() { return value_QualityArray; }
        public List<ConfigTypes.T_ZrConfig_Types_Vector2> getVector2Array() { return value_Vector2Array; }
        public List<ConfigTypes.T_ZrConfig_Types_Vector3> getVector3Array() { return value_Vector3Array; }
        public List<ConfigTypes.T_ZrConfig_Types_Kv> getKvArray() { return value_KvArray; }
        public List<ConfigTypes.T_Game_Config_Money> getMoneyArray() { return value_MoneyArray; }
        public List<List<Short>> getByteArray2D() { return value_ByteArray2D; }
        public List<List<Short>> getShortArray2D() { return value_ShortArray2D; }
        public List<List<Integer>> getIntArray2D() { return value_IntArray2D; }
        public List<List<Long>> getLongArray2D() { return value_LongArray2D; }
        public List<List<Float>> getFloatArray2D() { return value_FloatArray2D; }
        public List<List<Double>> getDoubleArray2D() { return value_DoubleArray2D; }
        public List<List<Boolean>> getBoolArray2D() { return value_BoolArray2D; }
        public List<List<String>> getStringArray2D() { return value_StringArray2D; }
        public List<List<Byte>> getSByteArray2D() { return value_SByteArray2D; }
        public List<List<Integer>> getUShortArray2D() { return value_UShortArray2D; }
        public List<List<Long>> getUIntArray2D() { return value_UIntArray2D; }
        public List<List<java.math.BigInteger>> getULongArray2D() { return value_ULongArray2D; }
        public List<List<ConfigRuntime.Decimal>> getDecimalArray2D() { return value_DecimalArray2D; }
        public List<List<ConfigTypes.T_Game_Config_Quality>> getQualityArray2D() { return value_QualityArray2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> getVector2Array2D() { return value_Vector2Array2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> getVector3Array2D() { return value_Vector3Array2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Kv>> getKvArray2D() { return value_KvArray2D; }
        public List<List<ConfigTypes.T_Game_Config_Money>> getMoneyArray2D() { return value_MoneyArray2D; }
        public List<List<List<Short>>> getByteArray3D() { return value_ByteArray3D; }
        public List<List<List<Short>>> getShortArray3D() { return value_ShortArray3D; }
        public List<List<List<Integer>>> getIntArray3D() { return value_IntArray3D; }
        public List<List<List<Long>>> getLongArray3D() { return value_LongArray3D; }
        public List<List<List<Float>>> getFloatArray3D() { return value_FloatArray3D; }
        public List<List<List<Double>>> getDoubleArray3D() { return value_DoubleArray3D; }
        public List<List<List<Boolean>>> getBoolArray3D() { return value_BoolArray3D; }
        public List<List<List<String>>> getStringArray3D() { return value_StringArray3D; }
        public List<List<List<Byte>>> getSByteArray3D() { return value_SByteArray3D; }
        public List<List<List<Integer>>> getUShortArray3D() { return value_UShortArray3D; }
        public List<List<List<Long>>> getUIntArray3D() { return value_UIntArray3D; }
        public List<List<List<java.math.BigInteger>>> getULongArray3D() { return value_ULongArray3D; }
        public List<List<List<ConfigRuntime.Decimal>>> getDecimalArray3D() { return value_DecimalArray3D; }
        public List<List<List<ConfigTypes.T_Game_Config_Quality>>> getQualityArray3D() { return value_QualityArray3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> getVector2Array3D() { return value_Vector2Array3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> getVector3Array3D() { return value_Vector3Array3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> getKvArray3D() { return value_KvArray3D; }
        public List<List<List<ConfigTypes.T_Game_Config_Money>>> getMoneyArray3D() { return value_MoneyArray3D; }
        public ConfigTypes.T_Game_Config_BattleSettings getSettings() { return value_Settings; }
        public ConfigTypes.T_Game_Config_ConfigRange getRange() { return value_Range; }
        public ConfigTypes.T_Game_Config_SampleOptions getOptions() { return value_Options; }
        public ConfigTypes.T_Game_Config_BattleAction getAction() { return value_Action; }
        public List<ConfigTypes.T_Game_Config_BattleAction> getActions() { return value_Actions; }
        public List<ConfigTypes.T_Game_Config_ConfigRange> getRanges() { return value_Ranges; }
        public List<ConfigTypes.T_Game_Config_SampleOptions> getOptionsList() { return value_OptionsList; }
        public List<ConfigTypes.T_Game_Config_ActionGroup> getGroups() { return value_Groups; }
        public List<ConfigTypes.T_Game_Config_BattlePhase> getPhases() { return value_Phases; }
        public ConfigTypes.T_Game_Config_ConfigRange getOptionalRange() { return value_OptionalRange; }
        public Integer getOptionalInt() { return value_OptionalInt; }
        public Long getOptionalUInt() { return value_OptionalUInt; }
        public java.math.BigInteger getOptionalULong() { return value_OptionalULong; }
        public ConfigRuntime.Decimal getOptionalDecimal() { return value_OptionalDecimal; }
        public Boolean getOptionalBool() { return value_OptionalBool; }
        public ConfigTypes.T_Game_Config_Quality getOptionalQuality() { return value_OptionalQuality; }
        public ConfigTypes.T_Game_Config_Money getOptionalMoney() { return value_OptionalMoney; }
        public List<Integer> getOptionalValues() { return value_OptionalValues; }
        public Map<String,Integer> getStatMap() { return value_StatMap; }
        public Map<Integer,ConfigRuntime.Decimal> getPriceMap() { return value_PriceMap; }
        public Map<ConfigTypes.T_Game_Config_Quality,Long> getQualityMap() { return value_QualityMap; }
        public Map<String,List<Integer>> getListMap() { return value_ListMap; }
        public ConfigRuntime.Matrix<Integer> getIntMatrix() { return value_IntMatrix; }
        public ConfigRuntime.Matrix<ConfigRuntime.Decimal> getDecimalCube() { return value_DecimalCube; }
        public ConfigRuntime.Matrix<Long> getOptionalMatrix() { return value_OptionalMatrix; }
        public String getRawJsonValue() { return value_RawJsonValue; }
        public List<String> getRawJsonArray() { return value_RawJsonArray; }
        public List<List<String>> getRawJsonArray2D() { return value_RawJsonArray2D; }
        public List<List<List<String>>> getRawJsonArray3D() { return value_RawJsonArray3D; }
        public List<Integer> getJsonIntArray() { return value_JsonIntArray; }
    }
    public static final class AllTypesTable extends RowStorage<AllTypesRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private final DerivedValue<Index_Group_ByQuality> group_ByQuality = new DerivedValue<>();
        private AllTypesTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new AllTypesRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public AllTypesRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public AllTypesRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public AllTypesRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public AllTypesRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<AllTypesRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(AllTypesRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<AllTypesRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<AllTypesRow> view(int slot,AllTypesTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<AllTypesRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<AllTypesRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<AllTypesRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<AllTypesRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        private static final class Key_Group_ByQuality {
            final int ordinal;
            final ConfigTypes.T_Game_Config_Quality k0;
            Key_Group_ByQuality(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(row,"QualityValue"),Integer.MIN_VALUE,Integer.MAX_VALUE));
            }
            Key_Group_ByQuality(AllTypesRow row,int ordinal) { this.ordinal=ordinal; k0=row.getQualityValue(); }
        }
        private static final class Index_Group_ByQuality extends Index<Key_Group_ByQuality> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<AllTypesRow>> views;
            Index_Group_ByQuality(List<Key_Group_ByQuality> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ByQuality row) { return (1 * 31 + row.k0.hashCode()); }
            @Override protected boolean same(Key_Group_ByQuality a,Key_Group_ByQuality b) { return a.k0.equals(b.k0); }
            int find(ConfigTypes.T_Game_Config_Quality k0) { int slot = mix((1 * 31 + k0.hashCode())) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0.equals(k0)) return slot; slot = (slot + 1) & mask; } return -1; }
            List<AllTypesRow> view(int slot,AllTypesTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<AllTypesRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<AllTypesRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ByQuality index_ByQuality(boolean load,Cancellation cancellation) {
            if (!load) return group_ByQuality.get();return group_ByQuality.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ByQuality>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ByQuality(indexed.readTransient(i),i):new Key_Group_ByQuality(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ByQuality(keys),retained); });
        }
        public List<AllTypesRow> findGroup_ByQuality(ConfigTypes.T_Game_Config_Quality k0) { var index = index_ByQuality(false,null); return index.view(index.find(k0),this,false,null); }
        public List<AllTypesRow> loadGroup_ByQuality(ConfigTypes.T_Game_Config_Quality k0,Cancellation cancellation) { var index = index_ByQuality(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
            index_ByQuality(true,cancellation);
        }
    }
    public static final class AllTypes10kRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final short value_ByteValue;
        private final short value_ShortValue;
        private final int value_IntValue;
        private final long value_LongValue;
        private final float value_FloatValue;
        private final double value_DoubleValue;
        private final boolean value_BoolValue;
        private final String value_StringValue;
        private final byte value_SByteValue;
        private final int value_UShortValue;
        private final long value_UIntValue;
        private final java.math.BigInteger value_ULongValue;
        private final ConfigRuntime.Decimal value_DecimalValue;
        private final ConfigTypes.T_Game_Config_Quality value_QualityValue;
        private final ConfigTypes.T_ZrConfig_Types_Vector2 value_Vector2Value;
        private final ConfigTypes.T_ZrConfig_Types_Vector3 value_Vector3Value;
        private final ConfigTypes.T_ZrConfig_Types_Kv value_KvValue;
        private final ConfigTypes.T_Game_Config_Money value_MoneyValue;
        private final List<Short> value_ByteArray;
        private final List<Short> value_ShortArray;
        private final List<Integer> value_IntArray;
        private final List<Long> value_LongArray;
        private final List<Float> value_FloatArray;
        private final List<Double> value_DoubleArray;
        private final List<Boolean> value_BoolArray;
        private final List<String> value_StringArray;
        private final List<Byte> value_SByteArray;
        private final List<Integer> value_UShortArray;
        private final List<Long> value_UIntArray;
        private final List<java.math.BigInteger> value_ULongArray;
        private final List<ConfigRuntime.Decimal> value_DecimalArray;
        private final List<ConfigTypes.T_Game_Config_Quality> value_QualityArray;
        private final List<ConfigTypes.T_ZrConfig_Types_Vector2> value_Vector2Array;
        private final List<ConfigTypes.T_ZrConfig_Types_Vector3> value_Vector3Array;
        private final List<ConfigTypes.T_ZrConfig_Types_Kv> value_KvArray;
        private final List<ConfigTypes.T_Game_Config_Money> value_MoneyArray;
        private final List<List<Short>> value_ByteArray2D;
        private final List<List<Short>> value_ShortArray2D;
        private final List<List<Integer>> value_IntArray2D;
        private final List<List<Long>> value_LongArray2D;
        private final List<List<Float>> value_FloatArray2D;
        private final List<List<Double>> value_DoubleArray2D;
        private final List<List<Boolean>> value_BoolArray2D;
        private final List<List<String>> value_StringArray2D;
        private final List<List<Byte>> value_SByteArray2D;
        private final List<List<Integer>> value_UShortArray2D;
        private final List<List<Long>> value_UIntArray2D;
        private final List<List<java.math.BigInteger>> value_ULongArray2D;
        private final List<List<ConfigRuntime.Decimal>> value_DecimalArray2D;
        private final List<List<ConfigTypes.T_Game_Config_Quality>> value_QualityArray2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> value_Vector2Array2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> value_Vector3Array2D;
        private final List<List<ConfigTypes.T_ZrConfig_Types_Kv>> value_KvArray2D;
        private final List<List<ConfigTypes.T_Game_Config_Money>> value_MoneyArray2D;
        private final List<List<List<Short>>> value_ByteArray3D;
        private final List<List<List<Short>>> value_ShortArray3D;
        private final List<List<List<Integer>>> value_IntArray3D;
        private final List<List<List<Long>>> value_LongArray3D;
        private final List<List<List<Float>>> value_FloatArray3D;
        private final List<List<List<Double>>> value_DoubleArray3D;
        private final List<List<List<Boolean>>> value_BoolArray3D;
        private final List<List<List<String>>> value_StringArray3D;
        private final List<List<List<Byte>>> value_SByteArray3D;
        private final List<List<List<Integer>>> value_UShortArray3D;
        private final List<List<List<Long>>> value_UIntArray3D;
        private final List<List<List<java.math.BigInteger>>> value_ULongArray3D;
        private final List<List<List<ConfigRuntime.Decimal>>> value_DecimalArray3D;
        private final List<List<List<ConfigTypes.T_Game_Config_Quality>>> value_QualityArray3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> value_Vector2Array3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> value_Vector3Array3D;
        private final List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> value_KvArray3D;
        private final List<List<List<ConfigTypes.T_Game_Config_Money>>> value_MoneyArray3D;
        private final ConfigTypes.T_Game_Config_BattleSettings value_Settings;
        private final ConfigTypes.T_Game_Config_ConfigRange value_Range;
        private final ConfigTypes.T_Game_Config_SampleOptions value_Options;
        private final ConfigTypes.T_Game_Config_BattleAction value_Action;
        private final List<ConfigTypes.T_Game_Config_BattleAction> value_Actions;
        private final List<ConfigTypes.T_Game_Config_ConfigRange> value_Ranges;
        private final List<ConfigTypes.T_Game_Config_SampleOptions> value_OptionsList;
        private final List<ConfigTypes.T_Game_Config_ActionGroup> value_Groups;
        private final List<ConfigTypes.T_Game_Config_BattlePhase> value_Phases;
        private final ConfigTypes.T_Game_Config_ConfigRange value_OptionalRange;
        private final Integer value_OptionalInt;
        private final Long value_OptionalUInt;
        private final java.math.BigInteger value_OptionalULong;
        private final ConfigRuntime.Decimal value_OptionalDecimal;
        private final Boolean value_OptionalBool;
        private final ConfigTypes.T_Game_Config_Quality value_OptionalQuality;
        private final ConfigTypes.T_Game_Config_Money value_OptionalMoney;
        private final List<Integer> value_OptionalValues;
        private final Map<String,Integer> value_StatMap;
        private final Map<Integer,ConfigRuntime.Decimal> value_PriceMap;
        private final Map<ConfigTypes.T_Game_Config_Quality,Long> value_QualityMap;
        private final Map<String,List<Integer>> value_ListMap;
        private final ConfigRuntime.Matrix<Integer> value_IntMatrix;
        private final ConfigRuntime.Matrix<ConfigRuntime.Decimal> value_DecimalCube;
        private final ConfigRuntime.Matrix<Long> value_OptionalMatrix;
        private final String value_RawJsonValue;
        private final List<String> value_RawJsonArray;
        private final List<List<String>> value_RawJsonArray2D;
        private final List<List<List<String>>> value_RawJsonArray3D;
        private final List<Integer> value_JsonIntArray;
        private final int value_Hero;
        private final int value_Skill;
        private final int value_Previous;
        private AllTypes10kRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_ByteValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value,"ByteValue"),0,255);
            value_ShortValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value,"ShortValue"),-32768,32767);
            value_IntValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"IntValue"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_LongValue = ConfigRuntime.int64(ConfigRuntime.field(value,"LongValue"));
            value_FloatValue = ConfigRuntime.float32(ConfigRuntime.field(value,"FloatValue"));
            value_DoubleValue = ConfigRuntime.float64(ConfigRuntime.field(value,"DoubleValue"));
            value_BoolValue = ConfigRuntime.bool(ConfigRuntime.field(value,"BoolValue"));
            value_StringValue = ConfigRuntime.string(ConfigRuntime.field(value,"StringValue"));
            value_SByteValue = (byte)ConfigRuntime.integer(ConfigRuntime.field(value,"SByteValue"),-128,127);
            value_UShortValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"UShortValue"),0,65535);
            value_UIntValue = ConfigRuntime.integer(ConfigRuntime.field(value,"UIntValue"),0,0xffffffffL);
            value_ULongValue = ConfigRuntime.uint64(ConfigRuntime.field(value,"ULongValue"));
            value_DecimalValue = new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value,"DecimalValue")));
            value_QualityValue = ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value,"QualityValue"),Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_Vector2Value = ConfigTypes.read_T_ZrConfig_Types_Vector2(ConfigRuntime.field(value,"Vector2Value"));
            value_Vector3Value = ConfigTypes.read_T_ZrConfig_Types_Vector3(ConfigRuntime.field(value,"Vector3Value"));
            value_KvValue = ConfigTypes.read_T_ZrConfig_Types_Kv(ConfigRuntime.field(value,"KvValue"));
            value_MoneyValue = ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value,"MoneyValue"));
            value_ByteArray = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray"), item0 -> (short)ConfigRuntime.integer(item0,0,255));
            value_ShortArray = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray"), item0 -> (short)ConfigRuntime.integer(item0,-32768,32767));
            value_IntArray = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_LongArray = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray"), item0 -> ConfigRuntime.int64(item0));
            value_FloatArray = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray"), item0 -> ConfigRuntime.float32(item0));
            value_DoubleArray = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray"), item0 -> ConfigRuntime.float64(item0));
            value_BoolArray = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray"), item0 -> ConfigRuntime.bool(item0));
            value_StringArray = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray"), item0 -> ConfigRuntime.string(item0));
            value_SByteArray = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray"), item0 -> (byte)ConfigRuntime.integer(item0,-128,127));
            value_UShortArray = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray"), item0 -> (int)ConfigRuntime.integer(item0,0,65535));
            value_UIntArray = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray"), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
            value_ULongArray = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray"), item0 -> ConfigRuntime.uint64(item0));
            value_DecimalArray = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
            value_QualityArray = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_Vector2Array = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item0));
            value_Vector3Array = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item0));
            value_KvArray = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item0));
            value_MoneyArray = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray"), item0 -> ConfigTypes.read_T_Game_Config_Money(item0));
            value_ByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,0,255)));
            value_ShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,-32768,32767)));
            value_IntArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_LongArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.int64(item1)));
            value_FloatArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float32(item1)));
            value_DoubleArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float64(item1)));
            value_BoolArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.bool(item1)));
            value_StringArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
            value_SByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (byte)ConfigRuntime.integer(item1,-128,127)));
            value_UShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,0,65535)));
            value_UIntArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.integer(item1,0,0xffffffffL)));
            value_ULongArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.uint64(item1)));
            value_DecimalArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item1))));
            value_QualityArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE))));
            value_Vector2Array2D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item1)));
            value_Vector3Array2D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item1)));
            value_KvArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item1)));
            value_MoneyArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_Game_Config_Money(item1)));
            value_ByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,0,255))));
            value_ShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,-32768,32767))));
            value_IntArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"IntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE))));
            value_LongArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"LongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.int64(item2))));
            value_FloatArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"FloatArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float32(item2))));
            value_DoubleArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"DoubleArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float64(item2))));
            value_BoolArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"BoolArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.bool(item2))));
            value_StringArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"StringArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
            value_SByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"SByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (byte)ConfigRuntime.integer(item2,-128,127))));
            value_UShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"UShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,0,65535))));
            value_UIntArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"UIntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.integer(item2,0,0xffffffffL))));
            value_ULongArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"ULongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.uint64(item2))));
            value_DecimalArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"DecimalArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item2)))));
            value_QualityArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"QualityArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE)))));
            value_Vector2Array3D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector2Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item2))));
            value_Vector3Array3D = ConfigRuntime.list(ConfigRuntime.field(value,"Vector3Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item2))));
            value_KvArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"KvArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item2))));
            value_MoneyArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"MoneyArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_Game_Config_Money(item2))));
            value_Settings = ConfigTypes.read_T_Game_Config_BattleSettings(ConfigRuntime.field(value,"Settings"));
            value_Range = ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value,"Range"));
            value_Options = ConfigTypes.read_T_Game_Config_SampleOptions(ConfigRuntime.field(value,"Options"));
            value_Action = ConfigTypes.read_T_Game_Config_BattleAction(ConfigRuntime.field(value,"Action"));
            value_Actions = ConfigRuntime.list(ConfigRuntime.field(value,"Actions"), item0 -> ConfigTypes.read_T_Game_Config_BattleAction(item0));
            value_Ranges = ConfigRuntime.list(ConfigRuntime.field(value,"Ranges"), item0 -> ConfigTypes.read_T_Game_Config_ConfigRange(item0));
            value_OptionsList = ConfigRuntime.list(ConfigRuntime.field(value,"OptionsList"), item0 -> ConfigTypes.read_T_Game_Config_SampleOptions(item0));
            value_Groups = ConfigRuntime.list(ConfigRuntime.field(value,"Groups"), item0 -> ConfigTypes.read_T_Game_Config_ActionGroup(item0));
            value_Phases = ConfigRuntime.list(ConfigRuntime.field(value,"Phases"), item0 -> ConfigTypes.read_T_Game_Config_BattlePhase(item0));
            value_OptionalRange = (ConfigRuntime.field(value,"OptionalRange") == null ? null : ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value,"OptionalRange")));
            value_OptionalInt = (ConfigRuntime.field(value,"OptionalInt") == null ? null : (int)ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalInt"),Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_OptionalUInt = (ConfigRuntime.field(value,"OptionalUInt") == null ? null : ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalUInt"),0,0xffffffffL));
            value_OptionalULong = (ConfigRuntime.field(value,"OptionalULong") == null ? null : ConfigRuntime.uint64(ConfigRuntime.field(value,"OptionalULong")));
            value_OptionalDecimal = (ConfigRuntime.field(value,"OptionalDecimal") == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value,"OptionalDecimal"))));
            value_OptionalBool = (ConfigRuntime.field(value,"OptionalBool") == null ? null : ConfigRuntime.bool(ConfigRuntime.field(value,"OptionalBool")));
            value_OptionalQuality = (ConfigRuntime.field(value,"OptionalQuality") == null ? null : ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value,"OptionalQuality"),Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_OptionalMoney = (ConfigRuntime.field(value,"OptionalMoney") == null ? null : ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value,"OptionalMoney")));
            value_OptionalValues = ConfigRuntime.list(ConfigRuntime.field(value,"OptionalValues"), item0 -> (item0 == null ? null : (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_StatMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"StatMap"), item0 -> ConfigRuntime.string(item0), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_PriceMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"PriceMap"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE), item0 -> (item0 == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(item0))));
            value_QualityMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"QualityMap"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
            value_ListMap = ConfigRuntime.dictionary(ConfigRuntime.field(value,"ListMap"), item0 -> ConfigRuntime.string(item0), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
            value_IntMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value,"IntMatrix"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_DecimalCube = ConfigRuntime.matrix(ConfigRuntime.field(value,"DecimalCube"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
            value_OptionalMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value,"OptionalMatrix"), item0 -> (item0 == null ? null : ConfigRuntime.integer(item0,0,0xffffffffL)));
            value_RawJsonValue = ConfigRuntime.string(ConfigRuntime.field(value,"RawJsonValue"));
            value_RawJsonArray = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray"), item0 -> ConfigRuntime.string(item0));
            value_RawJsonArray2D = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
            value_RawJsonArray3D = ConfigRuntime.list(ConfigRuntime.field(value,"RawJsonArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
            value_JsonIntArray = ConfigRuntime.list(ConfigRuntime.field(value,"JsonIntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
            value_Hero = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Hero"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Skill = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Skill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Previous = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Previous"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        }
        public int getId() { return value_Id; }
        public short getByteValue() { return value_ByteValue; }
        public short getShortValue() { return value_ShortValue; }
        public int getIntValue() { return value_IntValue; }
        public long getLongValue() { return value_LongValue; }
        public float getFloatValue() { return value_FloatValue; }
        public double getDoubleValue() { return value_DoubleValue; }
        public boolean getBoolValue() { return value_BoolValue; }
        public String getStringValue() { return value_StringValue; }
        public byte getSByteValue() { return value_SByteValue; }
        public int getUShortValue() { return value_UShortValue; }
        public long getUIntValue() { return value_UIntValue; }
        public java.math.BigInteger getULongValue() { return value_ULongValue; }
        public ConfigRuntime.Decimal getDecimalValue() { return value_DecimalValue; }
        public ConfigTypes.T_Game_Config_Quality getQualityValue() { return value_QualityValue; }
        public ConfigTypes.T_ZrConfig_Types_Vector2 getVector2Value() { return value_Vector2Value; }
        public ConfigTypes.T_ZrConfig_Types_Vector3 getVector3Value() { return value_Vector3Value; }
        public ConfigTypes.T_ZrConfig_Types_Kv getKvValue() { return value_KvValue; }
        public ConfigTypes.T_Game_Config_Money getMoneyValue() { return value_MoneyValue; }
        public List<Short> getByteArray() { return value_ByteArray; }
        public List<Short> getShortArray() { return value_ShortArray; }
        public List<Integer> getIntArray() { return value_IntArray; }
        public List<Long> getLongArray() { return value_LongArray; }
        public List<Float> getFloatArray() { return value_FloatArray; }
        public List<Double> getDoubleArray() { return value_DoubleArray; }
        public List<Boolean> getBoolArray() { return value_BoolArray; }
        public List<String> getStringArray() { return value_StringArray; }
        public List<Byte> getSByteArray() { return value_SByteArray; }
        public List<Integer> getUShortArray() { return value_UShortArray; }
        public List<Long> getUIntArray() { return value_UIntArray; }
        public List<java.math.BigInteger> getULongArray() { return value_ULongArray; }
        public List<ConfigRuntime.Decimal> getDecimalArray() { return value_DecimalArray; }
        public List<ConfigTypes.T_Game_Config_Quality> getQualityArray() { return value_QualityArray; }
        public List<ConfigTypes.T_ZrConfig_Types_Vector2> getVector2Array() { return value_Vector2Array; }
        public List<ConfigTypes.T_ZrConfig_Types_Vector3> getVector3Array() { return value_Vector3Array; }
        public List<ConfigTypes.T_ZrConfig_Types_Kv> getKvArray() { return value_KvArray; }
        public List<ConfigTypes.T_Game_Config_Money> getMoneyArray() { return value_MoneyArray; }
        public List<List<Short>> getByteArray2D() { return value_ByteArray2D; }
        public List<List<Short>> getShortArray2D() { return value_ShortArray2D; }
        public List<List<Integer>> getIntArray2D() { return value_IntArray2D; }
        public List<List<Long>> getLongArray2D() { return value_LongArray2D; }
        public List<List<Float>> getFloatArray2D() { return value_FloatArray2D; }
        public List<List<Double>> getDoubleArray2D() { return value_DoubleArray2D; }
        public List<List<Boolean>> getBoolArray2D() { return value_BoolArray2D; }
        public List<List<String>> getStringArray2D() { return value_StringArray2D; }
        public List<List<Byte>> getSByteArray2D() { return value_SByteArray2D; }
        public List<List<Integer>> getUShortArray2D() { return value_UShortArray2D; }
        public List<List<Long>> getUIntArray2D() { return value_UIntArray2D; }
        public List<List<java.math.BigInteger>> getULongArray2D() { return value_ULongArray2D; }
        public List<List<ConfigRuntime.Decimal>> getDecimalArray2D() { return value_DecimalArray2D; }
        public List<List<ConfigTypes.T_Game_Config_Quality>> getQualityArray2D() { return value_QualityArray2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> getVector2Array2D() { return value_Vector2Array2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> getVector3Array2D() { return value_Vector3Array2D; }
        public List<List<ConfigTypes.T_ZrConfig_Types_Kv>> getKvArray2D() { return value_KvArray2D; }
        public List<List<ConfigTypes.T_Game_Config_Money>> getMoneyArray2D() { return value_MoneyArray2D; }
        public List<List<List<Short>>> getByteArray3D() { return value_ByteArray3D; }
        public List<List<List<Short>>> getShortArray3D() { return value_ShortArray3D; }
        public List<List<List<Integer>>> getIntArray3D() { return value_IntArray3D; }
        public List<List<List<Long>>> getLongArray3D() { return value_LongArray3D; }
        public List<List<List<Float>>> getFloatArray3D() { return value_FloatArray3D; }
        public List<List<List<Double>>> getDoubleArray3D() { return value_DoubleArray3D; }
        public List<List<List<Boolean>>> getBoolArray3D() { return value_BoolArray3D; }
        public List<List<List<String>>> getStringArray3D() { return value_StringArray3D; }
        public List<List<List<Byte>>> getSByteArray3D() { return value_SByteArray3D; }
        public List<List<List<Integer>>> getUShortArray3D() { return value_UShortArray3D; }
        public List<List<List<Long>>> getUIntArray3D() { return value_UIntArray3D; }
        public List<List<List<java.math.BigInteger>>> getULongArray3D() { return value_ULongArray3D; }
        public List<List<List<ConfigRuntime.Decimal>>> getDecimalArray3D() { return value_DecimalArray3D; }
        public List<List<List<ConfigTypes.T_Game_Config_Quality>>> getQualityArray3D() { return value_QualityArray3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> getVector2Array3D() { return value_Vector2Array3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> getVector3Array3D() { return value_Vector3Array3D; }
        public List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> getKvArray3D() { return value_KvArray3D; }
        public List<List<List<ConfigTypes.T_Game_Config_Money>>> getMoneyArray3D() { return value_MoneyArray3D; }
        public ConfigTypes.T_Game_Config_BattleSettings getSettings() { return value_Settings; }
        public ConfigTypes.T_Game_Config_ConfigRange getRange() { return value_Range; }
        public ConfigTypes.T_Game_Config_SampleOptions getOptions() { return value_Options; }
        public ConfigTypes.T_Game_Config_BattleAction getAction() { return value_Action; }
        public List<ConfigTypes.T_Game_Config_BattleAction> getActions() { return value_Actions; }
        public List<ConfigTypes.T_Game_Config_ConfigRange> getRanges() { return value_Ranges; }
        public List<ConfigTypes.T_Game_Config_SampleOptions> getOptionsList() { return value_OptionsList; }
        public List<ConfigTypes.T_Game_Config_ActionGroup> getGroups() { return value_Groups; }
        public List<ConfigTypes.T_Game_Config_BattlePhase> getPhases() { return value_Phases; }
        public ConfigTypes.T_Game_Config_ConfigRange getOptionalRange() { return value_OptionalRange; }
        public Integer getOptionalInt() { return value_OptionalInt; }
        public Long getOptionalUInt() { return value_OptionalUInt; }
        public java.math.BigInteger getOptionalULong() { return value_OptionalULong; }
        public ConfigRuntime.Decimal getOptionalDecimal() { return value_OptionalDecimal; }
        public Boolean getOptionalBool() { return value_OptionalBool; }
        public ConfigTypes.T_Game_Config_Quality getOptionalQuality() { return value_OptionalQuality; }
        public ConfigTypes.T_Game_Config_Money getOptionalMoney() { return value_OptionalMoney; }
        public List<Integer> getOptionalValues() { return value_OptionalValues; }
        public Map<String,Integer> getStatMap() { return value_StatMap; }
        public Map<Integer,ConfigRuntime.Decimal> getPriceMap() { return value_PriceMap; }
        public Map<ConfigTypes.T_Game_Config_Quality,Long> getQualityMap() { return value_QualityMap; }
        public Map<String,List<Integer>> getListMap() { return value_ListMap; }
        public ConfigRuntime.Matrix<Integer> getIntMatrix() { return value_IntMatrix; }
        public ConfigRuntime.Matrix<ConfigRuntime.Decimal> getDecimalCube() { return value_DecimalCube; }
        public ConfigRuntime.Matrix<Long> getOptionalMatrix() { return value_OptionalMatrix; }
        public String getRawJsonValue() { return value_RawJsonValue; }
        public List<String> getRawJsonArray() { return value_RawJsonArray; }
        public List<List<String>> getRawJsonArray2D() { return value_RawJsonArray2D; }
        public List<List<List<String>>> getRawJsonArray3D() { return value_RawJsonArray3D; }
        public List<Integer> getJsonIntArray() { return value_JsonIntArray; }
        public int getHeroId() { return value_Hero; }
        public HeroRow get_Hero() { return owner.get_Hero().reference(value_Hero,false); }
        public HeroRow load_Hero() { return owner.get_Hero().reference(value_Hero,true); }
        public int getSkillId() { return value_Skill; }
        public SkillRow get_Skill() { return owner.get_Skill().reference(value_Skill,false); }
        public SkillRow load_Skill() { return owner.get_Skill().reference(value_Skill,true); }
        public int getPreviousId() { return value_Previous; }
        public AllTypes10kRow get_Previous() { return owner.get_AllTypes10k().reference(value_Previous,false); }
        public AllTypes10kRow load_Previous() { return owner.get_AllTypes10k().reference(value_Previous,true); }
    }
    public static final class AllTypes10kTable extends RowStorage<AllTypes10kRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private AllTypes10kTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new AllTypes10kRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public AllTypes10kRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public AllTypes10kRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public AllTypes10kRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public AllTypes10kRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<AllTypes10kRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(AllTypes10kRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<AllTypes10kRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<AllTypes10kRow> view(int slot,AllTypes10kTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<AllTypes10kRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<AllTypes10kRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<AllTypes10kRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<AllTypes10kRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class DropRow {
        private final DataSnapshot owner;
        private final int value_GroupId;
        private final int value_Id;
        private final int value_Count;
        private DropRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_GroupId = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"GroupId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Count = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Count"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        }
        public int getGroupId() { return value_GroupId; }
        public int getId() { return value_Id; }
        public int getCount() { return value_Count; }
    }
    public static final class DropTable extends RowStorage<DropRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ByGroupId> group_ByGroupId = new DerivedValue<>();
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private DropTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new DropRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            final int k1;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"GroupId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
                k1 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return ((1 * 31 + (int)row.k0) * 31 + (int)row.k1); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0 && a.k1 == b.k1; }
            int find(int k0, int k1) { int slot = mix(((1 * 31 + (int)k0) * 31 + (int)k1)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0 && row.k1 == k1) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public DropRow find(int k0, int k1) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0, k1))); }
        public DropRow get(int k0, int k1) { var row = find(k0, k1); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public DropRow tryLoad(int k0, int k1,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0, k1)),cancellation); }
        public DropRow load(int k0, int k1,Cancellation cancellation) { var row = tryLoad(k0, k1,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<DropRow> loadAsync(int k0, int k1,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0, k1)),cancellation,executor)); }
        private static final class Key_Group_ByGroupId {
            final int ordinal;
            final int k0;
            Key_Group_ByGroupId(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"GroupId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ByGroupId(DropRow row,int ordinal) { this.ordinal=ordinal; k0=row.getGroupId(); }
        }
        private static final class Index_Group_ByGroupId extends Index<Key_Group_ByGroupId> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<DropRow>> views;
            Index_Group_ByGroupId(List<Key_Group_ByGroupId> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ByGroupId row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ByGroupId a,Key_Group_ByGroupId b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<DropRow> view(int slot,DropTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<DropRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<DropRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ByGroupId index_ByGroupId(boolean load,Cancellation cancellation) {
            if (!load) return group_ByGroupId.get();return group_ByGroupId.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ByGroupId>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ByGroupId(indexed.readTransient(i),i):new Key_Group_ByGroupId(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ByGroupId(keys),retained); });
        }
        public List<DropRow> findGroup_ByGroupId(int k0) { var index = index_ByGroupId(false,null); return index.view(index.find(k0),this,false,null); }
        public List<DropRow> loadGroup_ByGroupId(int k0,Cancellation cancellation) { var index = index_ByGroupId(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(DropRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<DropRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<DropRow> view(int slot,DropTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<DropRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<DropRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<DropRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<DropRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ByGroupId(true,cancellation);
            index_ById(true,cancellation);
        }
    }
    public static final class HeroRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final String value_Name;
        private final int value_Skill;
        private final int value_OtherSkill;
        private final int value_Mentor;
        private HeroRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Name = ConfigRuntime.string(ConfigRuntime.field(value,"Name"));
            value_Skill = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Skill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_OtherSkill = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"OtherSkill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Mentor = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Mentor"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        }
        public int getId() { return value_Id; }
        public String getName() { return value_Name; }
        public int getSkillId() { return value_Skill; }
        public SkillRow get_Skill() { return owner.get_Skill().reference(value_Skill,false); }
        public SkillRow load_Skill() { return owner.get_Skill().reference(value_Skill,true); }
        public int getOtherSkillId() { return value_OtherSkill; }
        public SkillRow get_OtherSkill() { return owner.get_Skill().reference(value_OtherSkill,false); }
        public SkillRow load_OtherSkill() { return owner.get_Skill().reference(value_OtherSkill,true); }
        public int getMentorId() { return value_Mentor; }
        public HeroRow get_Mentor() { return owner.get_Hero().reference(value_Mentor,false); }
        public HeroRow load_Mentor() { return owner.get_Hero().reference(value_Mentor,true); }
    }
    public static final class HeroTable extends RowStorage<HeroRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private HeroTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new HeroRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public HeroRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public HeroRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public HeroRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public HeroRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<HeroRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(HeroRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<HeroRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<HeroRow> view(int slot,HeroTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<HeroRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<HeroRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<HeroRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<HeroRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class ItemCatalogRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final String value_Name;
        private final int value_MaxStack;
        private final int value_Rarity;
        private final boolean value_Enabled;
        private ItemCatalogRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Name = ConfigRuntime.string(ConfigRuntime.field(value,"Name"));
            value_MaxStack = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"MaxStack"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Rarity = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Rarity"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value,"Enabled"));
        }
        public int getId() { return value_Id; }
        public String getName() { return value_Name; }
        public int getMaxStack() { return value_MaxStack; }
        public int getRarity() { return value_Rarity; }
        public boolean getEnabled() { return value_Enabled; }
    }
    public static final class ItemCatalogTable extends RowStorage<ItemCatalogRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private ItemCatalogTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new ItemCatalogRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public ItemCatalogRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public ItemCatalogRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public ItemCatalogRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public ItemCatalogRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<ItemCatalogRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(ItemCatalogRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<ItemCatalogRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<ItemCatalogRow> view(int slot,ItemCatalogTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<ItemCatalogRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<ItemCatalogRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<ItemCatalogRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<ItemCatalogRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class QuestRewardsRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final String value_Name;
        private final int value_Item;
        private final int value_Count;
        private final int value_RequiredLevel;
        private final boolean value_Enabled;
        private QuestRewardsRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Name = ConfigRuntime.string(ConfigRuntime.field(value,"Name"));
            value_Item = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Item"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Count = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Count"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_RequiredLevel = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"RequiredLevel"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value,"Enabled"));
        }
        public int getId() { return value_Id; }
        public String getName() { return value_Name; }
        public int getItemId() { return value_Item; }
        public ItemCatalogRow get_Item() { return owner.get_ItemCatalog().reference(value_Item,false); }
        public ItemCatalogRow load_Item() { return owner.get_ItemCatalog().reference(value_Item,true); }
        public int getCount() { return value_Count; }
        public int getRequiredLevel() { return value_RequiredLevel; }
        public boolean getEnabled() { return value_Enabled; }
    }
    public static final class QuestRewardsTable extends RowStorage<QuestRewardsRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private QuestRewardsTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new QuestRewardsRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public QuestRewardsRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public QuestRewardsRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public QuestRewardsRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public QuestRewardsRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<QuestRewardsRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(QuestRewardsRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<QuestRewardsRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<QuestRewardsRow> view(int slot,QuestRewardsTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<QuestRewardsRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<QuestRewardsRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<QuestRewardsRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<QuestRewardsRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class ShopOffersRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final int value_Item;
        private final int value_Price;
        private final int value_DailyLimit;
        private final boolean value_Enabled;
        private ShopOffersRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Item = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Item"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Price = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Price"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_DailyLimit = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"DailyLimit"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Enabled = ConfigRuntime.bool(ConfigRuntime.field(value,"Enabled"));
        }
        public int getId() { return value_Id; }
        public int getItemId() { return value_Item; }
        public ItemCatalogRow get_Item() { return owner.get_ItemCatalog().reference(value_Item,false); }
        public ItemCatalogRow load_Item() { return owner.get_ItemCatalog().reference(value_Item,true); }
        public int getPrice() { return value_Price; }
        public int getDailyLimit() { return value_DailyLimit; }
        public boolean getEnabled() { return value_Enabled; }
    }
    public static final class ShopOffersTable extends RowStorage<ShopOffersRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private ShopOffersTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new ShopOffersRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public ShopOffersRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public ShopOffersRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public ShopOffersRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public ShopOffersRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<ShopOffersRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(ShopOffersRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<ShopOffersRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<ShopOffersRow> view(int slot,ShopOffersTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<ShopOffersRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<ShopOffersRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<ShopOffersRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<ShopOffersRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class SkillRow {
        private final DataSnapshot owner;
        private final int value_Id;
        private final String value_Name;
        private final int value_Hero;
        private SkillRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Name = ConfigRuntime.string(ConfigRuntime.field(value,"Name"));
            value_Hero = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Hero"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        }
        public int getId() { return value_Id; }
        public String getName() { return value_Name; }
        public int getHeroId() { return value_Hero; }
        public HeroRow get_Hero() { return owner.get_Hero().reference(value_Hero,false); }
        public HeroRow load_Hero() { return owner.get_Hero().reference(value_Hero,true); }
    }
    public static final class SkillTable extends RowStorage<SkillRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ById> group_ById = new DerivedValue<>();
        private SkillTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new SkillRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public SkillRow find(int k0) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0))); }
        public SkillRow get(int k0) { var row = find(k0); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public SkillRow tryLoad(int k0,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0)),cancellation); }
        public SkillRow load(int k0,Cancellation cancellation) { var row = tryLoad(k0,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<SkillRow> loadAsync(int k0,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0)),cancellation,executor)); }
        private static final class Key_Group_ById {
            final int ordinal;
            final int k0;
            Key_Group_ById(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ById(SkillRow row,int ordinal) { this.ordinal=ordinal; k0=row.getId(); }
        }
        private static final class Index_Group_ById extends Index<Key_Group_ById> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<SkillRow>> views;
            Index_Group_ById(List<Key_Group_ById> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ById row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ById a,Key_Group_ById b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<SkillRow> view(int slot,SkillTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<SkillRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<SkillRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ById index_ById(boolean load,Cancellation cancellation) {
            if (!load) return group_ById.get();return group_ById.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ById>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ById(indexed.readTransient(i),i):new Key_Group_ById(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ById(keys),retained); });
        }
        public List<SkillRow> findGroup_ById(int k0) { var index = index_ById(false,null); return index.view(index.find(k0),this,false,null); }
        public List<SkillRow> loadGroup_ById(int k0,Cancellation cancellation) { var index = index_ById(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ById(true,cancellation);
        }
    }
    public static final class SkillLevelRow {
        private final DataSnapshot owner;
        private final int value_SkillId;
        private final String value_Name;
        private final int value_Level;
        private final int value_Quality;
        private final int value_Damage;
        private SkillLevelRow(Object value,DataSnapshot owner) {
            this.owner = owner;
            value_SkillId = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"SkillId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Name = ConfigRuntime.string(ConfigRuntime.field(value,"Name"));
            value_Level = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Level"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Quality = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Quality"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            value_Damage = (int)ConfigRuntime.integer(ConfigRuntime.field(value,"Damage"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        }
        public int getSkillId() { return value_SkillId; }
        public String getName() { return value_Name; }
        public int getLevel() { return value_Level; }
        public int getQuality() { return value_Quality; }
        public int getDamage() { return value_Damage; }
    }
    public static final class SkillLevelTable extends RowStorage<SkillLevelRow> {
        private final IndexedRows indexed;
        private final Index_Key keyIndex;
        private final DerivedValue<Index_Group_ByLevel> group_ByLevel = new DerivedValue<>();
        private final DerivedValue<Index_Group_ByQuality> group_ByQuality = new DerivedValue<>();
        private final DerivedValue<Index_Group_BySkillId> group_BySkillId = new DerivedValue<>();
        private SkillLevelTable(IndexedRows indexed,DataSnapshot owner) {
            super(indexed,value -> new SkillLevelRow(value,owner)); this.indexed = indexed;
            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);
        }
        private static final class Key_Key {
            final int ordinal;
            final int k0;
            final int k1;
            final int k2;
            Key_Key(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"SkillId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
                k1 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Level"),Integer.MIN_VALUE,Integer.MAX_VALUE);
                k2 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Quality"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
        }
        private static final class Index_Key extends Index<Key_Key> {
            Index_Key(List<Key_Key> keys) { super(keys,true);  }
            @Override protected int hash(Key_Key row) { return (((1 * 31 + (int)row.k0) * 31 + (int)row.k1) * 31 + (int)row.k2); }
            @Override protected boolean same(Key_Key a,Key_Key b) { return a.k0 == b.k0 && a.k1 == b.k1 && a.k2 == b.k2; }
            int find(int k0, int k1, int k2) { int slot = mix((((1 * 31 + (int)k0) * 31 + (int)k1) * 31 + (int)k2)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0 && row.k1 == k1 && row.k2 == k2) return slot; slot = (slot + 1) & mask; } return -1; }
            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }
        }
        public SkillLevelRow find(int k0, int k1, int k2) { return queryOrdinal(keyIndex.ordinal(keyIndex.find(k0, k1, k2))); }
        public SkillLevelRow get(int k0, int k1, int k2) { var row = find(k0, k1, k2); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public SkillLevelRow tryLoad(int k0, int k1, int k2,Cancellation cancellation) { return loadOrdinal(keyIndex.ordinal(keyIndex.find(k0, k1, k2)),cancellation); }
        public SkillLevelRow load(int k0, int k1, int k2,Cancellation cancellation) { var row = tryLoad(k0, k1, k2,cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found"); return row; }
        public java.util.concurrent.CompletableFuture<SkillLevelRow> loadAsync(int k0, int k1, int k2,Cancellation cancellation,java.util.concurrent.Executor executor) { return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find(k0, k1, k2)),cancellation,executor)); }
        private static final class Key_Group_ByLevel {
            final int ordinal;
            final int k0;
            Key_Group_ByLevel(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Level"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ByLevel(SkillLevelRow row,int ordinal) { this.ordinal=ordinal; k0=row.getLevel(); }
        }
        private static final class Index_Group_ByLevel extends Index<Key_Group_ByLevel> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<SkillLevelRow>> views;
            Index_Group_ByLevel(List<Key_Group_ByLevel> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ByLevel row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ByLevel a,Key_Group_ByLevel b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<SkillLevelRow> view(int slot,SkillLevelTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<SkillLevelRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<SkillLevelRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ByLevel index_ByLevel(boolean load,Cancellation cancellation) {
            if (!load) return group_ByLevel.get();return group_ByLevel.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ByLevel>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ByLevel(indexed.readTransient(i),i):new Key_Group_ByLevel(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ByLevel(keys),retained); });
        }
        public List<SkillLevelRow> findGroup_ByLevel(int k0) { var index = index_ByLevel(false,null); return index.view(index.find(k0),this,false,null); }
        public List<SkillLevelRow> loadGroup_ByLevel(int k0,Cancellation cancellation) { var index = index_ByLevel(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        private static final class Key_Group_ByQuality {
            final int ordinal;
            final int k0;
            Key_Group_ByQuality(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"Quality"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_ByQuality(SkillLevelRow row,int ordinal) { this.ordinal=ordinal; k0=row.getQuality(); }
        }
        private static final class Index_Group_ByQuality extends Index<Key_Group_ByQuality> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<SkillLevelRow>> views;
            Index_Group_ByQuality(List<Key_Group_ByQuality> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_ByQuality row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_ByQuality a,Key_Group_ByQuality b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<SkillLevelRow> view(int slot,SkillLevelTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<SkillLevelRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<SkillLevelRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_ByQuality index_ByQuality(boolean load,Cancellation cancellation) {
            if (!load) return group_ByQuality.get();return group_ByQuality.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_ByQuality>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_ByQuality(indexed.readTransient(i),i):new Key_Group_ByQuality(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_ByQuality(keys),retained); });
        }
        public List<SkillLevelRow> findGroup_ByQuality(int k0) { var index = index_ByQuality(false,null); return index.view(index.find(k0),this,false,null); }
        public List<SkillLevelRow> loadGroup_ByQuality(int k0,Cancellation cancellation) { var index = index_ByQuality(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        private static final class Key_Group_BySkillId {
            final int ordinal;
            final int k0;
            Key_Group_BySkillId(Object row,int ordinal) { this.ordinal = ordinal;
                k0 = (int)ConfigRuntime.integer(ConfigRuntime.field(row,"SkillId"),Integer.MIN_VALUE,Integer.MAX_VALUE);
            }
            Key_Group_BySkillId(SkillLevelRow row,int ordinal) { this.ordinal=ordinal; k0=row.getSkillId(); }
        }
        private static final class Index_Group_BySkillId extends Index<Key_Group_BySkillId> {
            final java.util.concurrent.atomic.AtomicReferenceArray<List<SkillLevelRow>> views;
            Index_Group_BySkillId(List<Key_Group_BySkillId> keys) { super(keys,false); views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length); }
            @Override protected int hash(Key_Group_BySkillId row) { return (1 * 31 + (int)row.k0); }
            @Override protected boolean same(Key_Group_BySkillId a,Key_Group_BySkillId b) { return a.k0 == b.k0; }
            int find(int k0) { int slot = mix((1 * 31 + (int)k0)) & mask; while(slots[slot] >= 0) { var row = rows.get(slots[slot]); if (row.k0 == k0) return slot; slot = (slot + 1) & mask; } return -1; }
            List<SkillLevelRow> view(int slot,SkillLevelTable table,boolean load,Cancellation cancellation) {
                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;
                var result = new ArrayList<SkillLevelRow>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));
                List<SkillLevelRow> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);
            }
        }
        private Index_Group_BySkillId index_BySkillId(boolean load,Cancellation cancellation) {
            if (!load) return group_BySkillId.get();return group_BySkillId.load(cancellation,indexed.budget,indexed.estimatedIndexBytes(1),()->{ var keys = new ArrayList<Key_Group_BySkillId>(count()); long retained=256L+512L*count()*1; for (int i=0;i<count();i++) { var cached=cachedOrdinal(i); var key=cached==null?new Key_Group_BySkillId(indexed.readTransient(i),i):new Key_Group_BySkillId(cached,i); keys.add(key); } return new DerivedResult<>(new Index_Group_BySkillId(keys),retained); });
        }
        public List<SkillLevelRow> findGroup_BySkillId(int k0) { var index = index_BySkillId(false,null); return index.view(index.find(k0),this,false,null); }
        public List<SkillLevelRow> loadGroup_BySkillId(int k0,Cancellation cancellation) { var index = index_BySkillId(true,cancellation); return index.view(index.find(k0),this,true,cancellation); }
        @Override protected void prepareGroups(Cancellation cancellation) {
            index_ByLevel(true,cancellation);
            index_ByQuality(true,cancellation);
            index_BySkillId(true,cancellation);
        }
    }
}
