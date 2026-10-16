// zrconfig-api-contract:1
// Generated independent server views.
package zrconfig
import("context";"sync/atomic")
type DataSnapshot struct{version string;selection SelectionPlan;mode LoadMode;manifestDigest,sourceNamespace,loadPlanFingerprint string;budget *loadBudget;memory *SnapshotMemory
table_AllTypes *AllTypesRows
table_AllTypes10k *AllTypes10kRows
table_Drop *DropRows
table_Hero *HeroRows
table_ItemCatalog *ItemCatalogRows
table_QuestRewards *QuestRewardsRows
table_ShopOffers *ShopOffersRows
table_Skill *SkillRows
table_SkillLevel *SkillLevelRows
}
type DataManager = ServerManager[DataSnapshot]
func(s *DataSnapshot)Version()string{return s.version}
func(s *DataSnapshot)Selection()SelectionPlan{return s.selection}
func(s *DataSnapshot)Mode()LoadMode{return s.mode}
func LoadData(ctx context.Context,root string,plan LoadPlan)(*DataSnapshot,error){source,err:=NewFileConfigSource(root);if err!=nil{return nil,err};if plan.SourceNamespace==""{plan.SourceNamespace=source.root};return LoadDataSource(ctx,source,plan)}
func(s *DataSnapshot)ManifestDigest()string{return s.manifestDigest}
func(s *DataSnapshot)SourceNamespace()string{return s.sourceNamespace}
func(s *DataSnapshot)CatalogFingerprint()string{return s.selection.CatalogFingerprint()}
func(s *DataSnapshot)LoadPlanFingerprint()string{return s.loadPlanFingerprint}
func(s *DataSnapshot)Diagnostics()SnapshotDiagnostics{return s.budget.diagnostics()}
func(s *DataSnapshot)SnapshotMemory()*SnapshotMemory{return s.memory}
func LoadDataSource(ctx context.Context,source ConfigSource,plan LoadPlan)(snapshot *DataSnapshot,err error){
defer recoverServer(&err)
limits,err:=plan.Limits.validated();if err!=nil{return nil,err};if source==nil{return nil,serverError(InvalidPlan)}
if err=checkContext(ctx);err!=nil{return nil,err};mode:=plan.Mode;if mode==""{mode=ServerDefault};if mode!=ServerDefault&&mode!=SelectedTables&&mode!=LazyRowsStrict&&mode!=LazyRowsOnAccess{return nil,serverError(InvalidPlan)}
requested:=append([]string(nil),plan.Tables...);if plan.Tables==nil{for _,table:=range serverCatalog.tables.values{requested=append(requested,table.name)}};selection,err:=serverCatalog.Select(requested);if err!=nil{return nil,err};selected:=map[string]bool{};for _,name:=range selection.tables.values{selected[name]=true};warmup:=append([]string(nil),plan.WarmupTables...);for _,name:=range warmup{if !selected[name]{return nil,serverError(InvalidPlan)}}
b:=openServerBundle(ctx,source,limits);if err=b.validateCatalog(serverCatalog);err!=nil{return nil,err};indexed:=map[string]*indexedRows{};next:=&DataSnapshot{loadPlanFingerprint:loadPlanFingerprint(mode,plan.PreferJSON,selection.tables.values),version:b.version,selection:selection,mode:mode,manifestDigest:b.digest,sourceNamespace:plan.SourceNamespace,budget:b.budget,memory:newBudgetSnapshotMemory(b.budget)}
if selected["AllTypes"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["AllTypes"]=b.openRows(schemaAllTypes,plan.PreferJSON);next.table_AllTypes=newAllTypesRows(indexed["AllTypes"],next)}
if selected["AllTypes10k"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["AllTypes10k"]=b.openRows(schemaAllTypes10k,plan.PreferJSON);next.table_AllTypes10k=newAllTypes10kRows(indexed["AllTypes10k"],next)}
if selected["Drop"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["Drop"]=b.openRows(schemaDrop,plan.PreferJSON);next.table_Drop=newDropRows(indexed["Drop"],next)}
if selected["Hero"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["Hero"]=b.openRows(schemaHero,plan.PreferJSON);next.table_Hero=newHeroRows(indexed["Hero"],next)}
if selected["ItemCatalog"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["ItemCatalog"]=b.openRows(schemaItemCatalog,plan.PreferJSON);next.table_ItemCatalog=newItemCatalogRows(indexed["ItemCatalog"],next)}
if selected["QuestRewards"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["QuestRewards"]=b.openRows(schemaQuestRewards,plan.PreferJSON);next.table_QuestRewards=newQuestRewardsRows(indexed["QuestRewards"],next)}
if selected["ShopOffers"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["ShopOffers"]=b.openRows(schemaShopOffers,plan.PreferJSON);next.table_ShopOffers=newShopOffersRows(indexed["ShopOffers"],next)}
if selected["Skill"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["Skill"]=b.openRows(schemaSkill,plan.PreferJSON);next.table_Skill=newSkillRows(indexed["Skill"],next)}
if selected["SkillLevel"]{if err=checkContext(ctx);err!=nil{return nil,err};indexed["SkillLevel"]=b.openRows(schemaSkillLevel,plan.PreferJSON);next.table_SkillLevel=newSkillLevelRows(indexed["SkillLevel"],next)}
for _,data:=range indexed{data.resolve=indexed}
if mode!=LazyRowsOnAccess{
if data:=indexed["AllTypes"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readAllTypesRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["AllTypes10k"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readAllTypes10kRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["Drop"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readDropRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["Hero"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readHeroRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["ItemCatalog"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readItemCatalogRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["QuestRewards"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readQuestRewardsRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["ShopOffers"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readShopOffersRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["Skill"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readSkillRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
if data:=indexed["SkillLevel"];data!=nil{for i:=range data.ids{if err=checkContext(ctx);err!=nil{return nil,err};value:=data.readTransient(i);_=readSkillLevelRow(value,next);if err=data.validateReferences(value,indexed);err!=nil{return nil,err}}}
}
if mode==ServerDefault||mode==SelectedTables{if err=next.LoadAll(ctx);err!=nil{return nil,err}}
for _,name:=range warmup{switch name{
case "AllTypes":_,err=next.table_AllTypes.LoadAll(ctx)
case "AllTypes10k":_,err=next.table_AllTypes10k.LoadAll(ctx)
case "Drop":_,err=next.table_Drop.LoadAll(ctx)
case "Hero":_,err=next.table_Hero.LoadAll(ctx)
case "ItemCatalog":_,err=next.table_ItemCatalog.LoadAll(ctx)
case "QuestRewards":_,err=next.table_QuestRewards.LoadAll(ctx)
case "ShopOffers":_,err=next.table_ShopOffers.LoadAll(ctx)
case "Skill":_,err=next.table_Skill.LoadAll(ctx)
case "SkillLevel":_,err=next.table_SkillLevel.LoadAll(ctx)
};if err!=nil{return nil,err}};if err=checkContext(ctx);err!=nil{return nil,err};return next,nil
}
func(s *DataSnapshot)LoadAll(ctx context.Context)error{
if s.table_AllTypes!=nil{if _,err:=s.table_AllTypes.LoadAll(ctx);err!=nil{return err}}
if s.table_AllTypes10k!=nil{if _,err:=s.table_AllTypes10k.LoadAll(ctx);err!=nil{return err}}
if s.table_Drop!=nil{if _,err:=s.table_Drop.LoadAll(ctx);err!=nil{return err}}
if s.table_Hero!=nil{if _,err:=s.table_Hero.LoadAll(ctx);err!=nil{return err}}
if s.table_ItemCatalog!=nil{if _,err:=s.table_ItemCatalog.LoadAll(ctx);err!=nil{return err}}
if s.table_QuestRewards!=nil{if _,err:=s.table_QuestRewards.LoadAll(ctx);err!=nil{return err}}
if s.table_ShopOffers!=nil{if _,err:=s.table_ShopOffers.LoadAll(ctx);err!=nil{return err}}
if s.table_Skill!=nil{if _,err:=s.table_Skill.LoadAll(ctx);err!=nil{return err}}
if s.table_SkillLevel!=nil{if _,err:=s.table_SkillLevel.LoadAll(ctx);err!=nil{return err}}
return nil
}
func(s *DataSnapshot)LoadedRowCount()int{count:=0
if s.table_AllTypes!=nil{count+=s.table_AllTypes.LoadedCount()}
if s.table_AllTypes10k!=nil{count+=s.table_AllTypes10k.LoadedCount()}
if s.table_Drop!=nil{count+=s.table_Drop.LoadedCount()}
if s.table_Hero!=nil{count+=s.table_Hero.LoadedCount()}
if s.table_ItemCatalog!=nil{count+=s.table_ItemCatalog.LoadedCount()}
if s.table_QuestRewards!=nil{count+=s.table_QuestRewards.LoadedCount()}
if s.table_ShopOffers!=nil{count+=s.table_ShopOffers.LoadedCount()}
if s.table_Skill!=nil{count+=s.table_Skill.LoadedCount()}
if s.table_SkillLevel!=nil{count+=s.table_SkillLevel.LoadedCount()}
return count
}
func(s *DataSnapshot)CompletedTableCount()int{count:=0
if s.table_AllTypes!=nil&&s.table_AllTypes.full.Load(){count++}
if s.table_AllTypes10k!=nil&&s.table_AllTypes10k.full.Load(){count++}
if s.table_Drop!=nil&&s.table_Drop.full.Load(){count++}
if s.table_Hero!=nil&&s.table_Hero.full.Load(){count++}
if s.table_ItemCatalog!=nil&&s.table_ItemCatalog.full.Load(){count++}
if s.table_QuestRewards!=nil&&s.table_QuestRewards.full.Load(){count++}
if s.table_ShopOffers!=nil&&s.table_ShopOffers.full.Load(){count++}
if s.table_Skill!=nil&&s.table_Skill.full.Load(){count++}
if s.table_SkillLevel!=nil&&s.table_SkillLevel.full.Load(){count++}
return count}
type DataWarmup struct{table string;action func(context.Context,*DataSnapshot)error}
func(w DataWarmup)Table()string{return w.table}
func LoadDataSourceWithWarmup(ctx context.Context,source ConfigSource,plan LoadPlan,requests []DataWarmup)(*DataSnapshot,error){
steps:=append([]DataWarmup(nil),requests...);names:=plan.Tables;if names==nil{for _,table:=range serverCatalog.tables.values{names=append(names,table.name)}};selection,err:=serverCatalog.Select(names);if err!=nil{return nil,err};selected:=map[string]bool{};for _,name:=range selection.tables.values{selected[name]=true};for _,step:=range steps{if step.action==nil||!selected[step.table]{return nil,serverError(InvalidPlan)}}
next,err:=LoadDataSource(ctx,source,plan);if err!=nil{return nil,err};for _,step:=range steps{if err=checkContext(ctx);err!=nil{return nil,err};if err=step.action(ctx,next);err!=nil{return nil,err}};if err=checkContext(ctx);err!=nil{return nil,err};return next,nil
}
func LoadDataWithWarmup(ctx context.Context,root string,plan LoadPlan,requests []DataWarmup)(*DataSnapshot,error){source,err:=NewFileConfigSource(root);if err!=nil{return nil,err};if plan.SourceNamespace==""{plan.SourceNamespace=source.root};return LoadDataSourceWithWarmup(ctx,source,plan,requests)}
func WarmupTable_AllTypes()DataWarmup{return DataWarmup{table:"AllTypes",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_AllTypes(k0 int32)DataWarmup{return DataWarmup{table:"AllTypes",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_AllTypes_ById(k0 int32)DataWarmup{return DataWarmup{table:"AllTypes",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupGroup_AllTypes_ByQuality(k0 T_Game_Config_Quality)DataWarmup{return DataWarmup{table:"AllTypes",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes();if err!=nil{return err};_,err=table.LoadGroup_ByQuality(ctx,k0);return err}}}
func WarmupTable_AllTypes10k()DataWarmup{return DataWarmup{table:"AllTypes10k",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes10k();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_AllTypes10k(k0 int32)DataWarmup{return DataWarmup{table:"AllTypes10k",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes10k();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_AllTypes10k_ById(k0 int32)DataWarmup{return DataWarmup{table:"AllTypes10k",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_AllTypes10k();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_Drop()DataWarmup{return DataWarmup{table:"Drop",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Drop();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_Drop(k0 int32,k1 int32)DataWarmup{return DataWarmup{table:"Drop",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Drop();if err!=nil{return err};_,err=table.Load(ctx,k0,k1);return err}}}
func WarmupGroup_Drop_ByGroupId(k0 int32)DataWarmup{return DataWarmup{table:"Drop",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Drop();if err!=nil{return err};_,err=table.LoadGroup_ByGroupId(ctx,k0);return err}}}
func WarmupGroup_Drop_ById(k0 int32)DataWarmup{return DataWarmup{table:"Drop",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Drop();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_Hero()DataWarmup{return DataWarmup{table:"Hero",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Hero();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_Hero(k0 int32)DataWarmup{return DataWarmup{table:"Hero",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Hero();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_Hero_ById(k0 int32)DataWarmup{return DataWarmup{table:"Hero",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Hero();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_ItemCatalog()DataWarmup{return DataWarmup{table:"ItemCatalog",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ItemCatalog();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_ItemCatalog(k0 int32)DataWarmup{return DataWarmup{table:"ItemCatalog",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ItemCatalog();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_ItemCatalog_ById(k0 int32)DataWarmup{return DataWarmup{table:"ItemCatalog",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ItemCatalog();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_QuestRewards()DataWarmup{return DataWarmup{table:"QuestRewards",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_QuestRewards();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_QuestRewards(k0 int32)DataWarmup{return DataWarmup{table:"QuestRewards",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_QuestRewards();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_QuestRewards_ById(k0 int32)DataWarmup{return DataWarmup{table:"QuestRewards",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_QuestRewards();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_ShopOffers()DataWarmup{return DataWarmup{table:"ShopOffers",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ShopOffers();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_ShopOffers(k0 int32)DataWarmup{return DataWarmup{table:"ShopOffers",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ShopOffers();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_ShopOffers_ById(k0 int32)DataWarmup{return DataWarmup{table:"ShopOffers",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_ShopOffers();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_Skill()DataWarmup{return DataWarmup{table:"Skill",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Skill();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_Skill(k0 int32)DataWarmup{return DataWarmup{table:"Skill",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Skill();if err!=nil{return err};_,err=table.Load(ctx,k0);return err}}}
func WarmupGroup_Skill_ById(k0 int32)DataWarmup{return DataWarmup{table:"Skill",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_Skill();if err!=nil{return err};_,err=table.LoadGroup_ById(ctx,k0);return err}}}
func WarmupTable_SkillLevel()DataWarmup{return DataWarmup{table:"SkillLevel",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_SkillLevel();if err!=nil{return err};_,err=table.LoadAll(ctx);return err}}}
func WarmupRow_SkillLevel(k0 int32,k1 int32,k2 int32)DataWarmup{return DataWarmup{table:"SkillLevel",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_SkillLevel();if err!=nil{return err};_,err=table.Load(ctx,k0,k1,k2);return err}}}
func WarmupGroup_SkillLevel_ByLevel(k0 int32)DataWarmup{return DataWarmup{table:"SkillLevel",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_SkillLevel();if err!=nil{return err};_,err=table.LoadGroup_ByLevel(ctx,k0);return err}}}
func WarmupGroup_SkillLevel_ByQuality(k0 int32)DataWarmup{return DataWarmup{table:"SkillLevel",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_SkillLevel();if err!=nil{return err};_,err=table.LoadGroup_ByQuality(ctx,k0);return err}}}
func WarmupGroup_SkillLevel_BySkillId(k0 int32)DataWarmup{return DataWarmup{table:"SkillLevel",action:func(ctx context.Context,snapshot *DataSnapshot)error{table,err:=snapshot.Get_SkillLevel();if err!=nil{return err};_,err=table.LoadGroup_BySkillId(ctx,k0);return err}}}
func(s *DataSnapshot)Get_AllTypes()(*AllTypesRows,error){if s.table_AllTypes==nil{return nil,serverError(TableNotSelected)};return s.table_AllTypes,nil}
func(s *DataSnapshot)Get_AllTypes10k()(*AllTypes10kRows,error){if s.table_AllTypes10k==nil{return nil,serverError(TableNotSelected)};return s.table_AllTypes10k,nil}
func(s *DataSnapshot)Get_Drop()(*DropRows,error){if s.table_Drop==nil{return nil,serverError(TableNotSelected)};return s.table_Drop,nil}
func(s *DataSnapshot)Get_Hero()(*HeroRows,error){if s.table_Hero==nil{return nil,serverError(TableNotSelected)};return s.table_Hero,nil}
func(s *DataSnapshot)Get_ItemCatalog()(*ItemCatalogRows,error){if s.table_ItemCatalog==nil{return nil,serverError(TableNotSelected)};return s.table_ItemCatalog,nil}
func(s *DataSnapshot)Get_QuestRewards()(*QuestRewardsRows,error){if s.table_QuestRewards==nil{return nil,serverError(TableNotSelected)};return s.table_QuestRewards,nil}
func(s *DataSnapshot)Get_ShopOffers()(*ShopOffersRows,error){if s.table_ShopOffers==nil{return nil,serverError(TableNotSelected)};return s.table_ShopOffers,nil}
func(s *DataSnapshot)Get_Skill()(*SkillRows,error){if s.table_Skill==nil{return nil,serverError(TableNotSelected)};return s.table_Skill,nil}
func(s *DataSnapshot)Get_SkillLevel()(*SkillLevelRows,error){if s.table_SkillLevel==nil{return nil,serverError(TableNotSelected)};return s.table_SkillLevel,nil}
type dataGroup[T any]struct{ordinals []int;ready atomic.Pointer[List[*T]]}
func(g *dataGroup[T])view(ctx context.Context,storage *rowStorage[T],load bool)(List[*T],error){if g==nil{return List[*T]{},nil};if ready:=g.ready.Load();ready!=nil{return *ready,nil};rows:=List[*T]{values:make([]*T,len(g.ordinals))};for i,index:=range g.ordinals{var row *T;var err error;if load{row,err=storage.load(ctx,index)}else{row,err=storage.query(index)};if err!=nil{return List[*T]{},err};rows.values[i]=row};g.ready.CompareAndSwap(nil,&rows);return *g.ready.Load(),nil}
type AllTypesRow struct{owner *DataSnapshot
v_Id int32
v_ByteValue uint8
v_ShortValue int16
v_IntValue int32
v_LongValue int64
v_FloatValue float32
v_DoubleValue float64
v_BoolValue bool
v_StringValue string
v_SByteValue int8
v_UShortValue uint16
v_UIntValue uint32
v_ULongValue uint64
v_DecimalValue Decimal
v_QualityValue T_Game_Config_Quality
v_Vector2Value T_ZrConfig_Types_Vector2
v_Vector3Value T_ZrConfig_Types_Vector3
v_KvValue T_ZrConfig_Types_Kv
v_MoneyValue T_Game_Config_Money
v_ByteArray List[uint8]
v_ShortArray List[int16]
v_IntArray List[int32]
v_LongArray List[int64]
v_FloatArray List[float32]
v_DoubleArray List[float64]
v_BoolArray List[bool]
v_StringArray List[string]
v_SByteArray List[int8]
v_UShortArray List[uint16]
v_UIntArray List[uint32]
v_ULongArray List[uint64]
v_DecimalArray List[Decimal]
v_QualityArray List[T_Game_Config_Quality]
v_Vector2Array List[T_ZrConfig_Types_Vector2]
v_Vector3Array List[T_ZrConfig_Types_Vector3]
v_KvArray List[T_ZrConfig_Types_Kv]
v_MoneyArray List[T_Game_Config_Money]
v_ByteArray2D List[List[uint8]]
v_ShortArray2D List[List[int16]]
v_IntArray2D List[List[int32]]
v_LongArray2D List[List[int64]]
v_FloatArray2D List[List[float32]]
v_DoubleArray2D List[List[float64]]
v_BoolArray2D List[List[bool]]
v_StringArray2D List[List[string]]
v_SByteArray2D List[List[int8]]
v_UShortArray2D List[List[uint16]]
v_UIntArray2D List[List[uint32]]
v_ULongArray2D List[List[uint64]]
v_DecimalArray2D List[List[Decimal]]
v_QualityArray2D List[List[T_Game_Config_Quality]]
v_Vector2Array2D List[List[T_ZrConfig_Types_Vector2]]
v_Vector3Array2D List[List[T_ZrConfig_Types_Vector3]]
v_KvArray2D List[List[T_ZrConfig_Types_Kv]]
v_MoneyArray2D List[List[T_Game_Config_Money]]
v_ByteArray3D List[List[List[uint8]]]
v_ShortArray3D List[List[List[int16]]]
v_IntArray3D List[List[List[int32]]]
v_LongArray3D List[List[List[int64]]]
v_FloatArray3D List[List[List[float32]]]
v_DoubleArray3D List[List[List[float64]]]
v_BoolArray3D List[List[List[bool]]]
v_StringArray3D List[List[List[string]]]
v_SByteArray3D List[List[List[int8]]]
v_UShortArray3D List[List[List[uint16]]]
v_UIntArray3D List[List[List[uint32]]]
v_ULongArray3D List[List[List[uint64]]]
v_DecimalArray3D List[List[List[Decimal]]]
v_QualityArray3D List[List[List[T_Game_Config_Quality]]]
v_Vector2Array3D List[List[List[T_ZrConfig_Types_Vector2]]]
v_Vector3Array3D List[List[List[T_ZrConfig_Types_Vector3]]]
v_KvArray3D List[List[List[T_ZrConfig_Types_Kv]]]
v_MoneyArray3D List[List[List[T_Game_Config_Money]]]
v_Settings *T_Game_Config_BattleSettings
v_Range T_Game_Config_ConfigRange
v_Options *T_Game_Config_SampleOptions
v_Action T_Game_Config_BattleAction
v_Actions List[T_Game_Config_BattleAction]
v_Ranges List[T_Game_Config_ConfigRange]
v_OptionsList List[*T_Game_Config_SampleOptions]
v_Groups List[*T_Game_Config_ActionGroup]
v_Phases List[*T_Game_Config_BattlePhase]
v_OptionalRange Optional[T_Game_Config_ConfigRange]
v_OptionalInt Optional[int32]
v_OptionalUInt Optional[uint32]
v_OptionalULong Optional[uint64]
v_OptionalDecimal Optional[Decimal]
v_OptionalBool Optional[bool]
v_OptionalQuality Optional[T_Game_Config_Quality]
v_OptionalMoney Optional[T_Game_Config_Money]
v_OptionalValues List[Optional[int32]]
v_StatMap Dictionary[string,int32]
v_PriceMap Dictionary[int32,Optional[Decimal]]
v_QualityMap Dictionary[T_Game_Config_Quality,uint32]
v_ListMap Dictionary[string,List[int32]]
v_IntMatrix Matrix[int32]
v_DecimalCube Matrix[Decimal]
v_OptionalMatrix Matrix[Optional[uint32]]
v_RawJsonValue string
v_RawJsonArray List[string]
v_RawJsonArray2D List[List[string]]
v_RawJsonArray3D List[List[List[string]]]
v_JsonIntArray List[int32]
}
func readAllTypesRow(value any,owner *DataSnapshot)*AllTypesRow{return &AllTypesRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_ByteValue:uint8(unsigned(field(value,"ByteValue"),8)),
v_ShortValue:int16(signed(field(value,"ShortValue"),16)),
v_IntValue:int32(signed(field(value,"IntValue"),32)),
v_LongValue:int64(signed(field(value,"LongValue"),64)),
v_FloatValue:float32(floating(field(value,"FloatValue"),32)),
v_DoubleValue:float64(floating(field(value,"DoubleValue"),64)),
v_BoolValue:boolean(field(value,"BoolValue")),
v_StringValue:text(field(value,"StringValue")),
v_SByteValue:int8(signed(field(value,"SByteValue"),8)),
v_UShortValue:uint16(unsigned(field(value,"UShortValue"),16)),
v_UIntValue:uint32(unsigned(field(value,"UIntValue"),32)),
v_ULongValue:uint64(unsigned(field(value,"ULongValue"),64)),
v_DecimalValue:decimalValue(text(field(value,"DecimalValue"))),
v_QualityValue:T_Game_Config_Quality(int32(signed(field(value,"QualityValue"),32))),
v_Vector2Value:read_T_ZrConfig_Types_Vector2(field(value,"Vector2Value")),
v_Vector3Value:read_T_ZrConfig_Types_Vector3(field(value,"Vector3Value")),
v_KvValue:read_T_ZrConfig_Types_Kv(field(value,"KvValue")),
v_MoneyValue:read_T_Game_Config_Money(field(value,"MoneyValue")),
v_ByteArray:decodeList(field(value,"ByteArray"),func(item0 any) uint8 { return uint8(unsigned(item0,8)) }),
v_ShortArray:decodeList(field(value,"ShortArray"),func(item0 any) int16 { return int16(signed(item0,16)) }),
v_IntArray:decodeList(field(value,"IntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_LongArray:decodeList(field(value,"LongArray"),func(item0 any) int64 { return int64(signed(item0,64)) }),
v_FloatArray:decodeList(field(value,"FloatArray"),func(item0 any) float32 { return float32(floating(item0,32)) }),
v_DoubleArray:decodeList(field(value,"DoubleArray"),func(item0 any) float64 { return float64(floating(item0,64)) }),
v_BoolArray:decodeList(field(value,"BoolArray"),func(item0 any) bool { return boolean(item0) }),
v_StringArray:decodeList(field(value,"StringArray"),func(item0 any) string { return text(item0) }),
v_SByteArray:decodeList(field(value,"SByteArray"),func(item0 any) int8 { return int8(signed(item0,8)) }),
v_UShortArray:decodeList(field(value,"UShortArray"),func(item0 any) uint16 { return uint16(unsigned(item0,16)) }),
v_UIntArray:decodeList(field(value,"UIntArray"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_ULongArray:decodeList(field(value,"ULongArray"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
v_DecimalArray:decodeList(field(value,"DecimalArray"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_QualityArray:decodeList(field(value,"QualityArray"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
v_Vector2Array:decodeList(field(value,"Vector2Array"),func(item0 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item0) }),
v_Vector3Array:decodeList(field(value,"Vector3Array"),func(item0 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item0) }),
v_KvArray:decodeList(field(value,"KvArray"),func(item0 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item0) }),
v_MoneyArray:decodeList(field(value,"MoneyArray"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
v_ByteArray2D:decodeList(field(value,"ByteArray2D"),func(item0 any) List[uint8] { return decodeList(item0,func(item1 any) uint8 { return uint8(unsigned(item1,8)) }) }),
v_ShortArray2D:decodeList(field(value,"ShortArray2D"),func(item0 any) List[int16] { return decodeList(item0,func(item1 any) int16 { return int16(signed(item1,16)) }) }),
v_IntArray2D:decodeList(field(value,"IntArray2D"),func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_LongArray2D:decodeList(field(value,"LongArray2D"),func(item0 any) List[int64] { return decodeList(item0,func(item1 any) int64 { return int64(signed(item1,64)) }) }),
v_FloatArray2D:decodeList(field(value,"FloatArray2D"),func(item0 any) List[float32] { return decodeList(item0,func(item1 any) float32 { return float32(floating(item1,32)) }) }),
v_DoubleArray2D:decodeList(field(value,"DoubleArray2D"),func(item0 any) List[float64] { return decodeList(item0,func(item1 any) float64 { return float64(floating(item1,64)) }) }),
v_BoolArray2D:decodeList(field(value,"BoolArray2D"),func(item0 any) List[bool] { return decodeList(item0,func(item1 any) bool { return boolean(item1) }) }),
v_StringArray2D:decodeList(field(value,"StringArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
v_SByteArray2D:decodeList(field(value,"SByteArray2D"),func(item0 any) List[int8] { return decodeList(item0,func(item1 any) int8 { return int8(signed(item1,8)) }) }),
v_UShortArray2D:decodeList(field(value,"UShortArray2D"),func(item0 any) List[uint16] { return decodeList(item0,func(item1 any) uint16 { return uint16(unsigned(item1,16)) }) }),
v_UIntArray2D:decodeList(field(value,"UIntArray2D"),func(item0 any) List[uint32] { return decodeList(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
v_ULongArray2D:decodeList(field(value,"ULongArray2D"),func(item0 any) List[uint64] { return decodeList(item0,func(item1 any) uint64 { return uint64(unsigned(item1,64)) }) }),
v_DecimalArray2D:decodeList(field(value,"DecimalArray2D"),func(item0 any) List[Decimal] { return decodeList(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
v_QualityArray2D:decodeList(field(value,"QualityArray2D"),func(item0 any) List[T_Game_Config_Quality] { return decodeList(item0,func(item1 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item1,32))) }) }),
v_Vector2Array2D:decodeList(field(value,"Vector2Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item1) }) }),
v_Vector3Array2D:decodeList(field(value,"Vector3Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item1) }) }),
v_KvArray2D:decodeList(field(value,"KvArray2D"),func(item0 any) List[T_ZrConfig_Types_Kv] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item1) }) }),
v_MoneyArray2D:decodeList(field(value,"MoneyArray2D"),func(item0 any) List[T_Game_Config_Money] { return decodeList(item0,func(item1 any) T_Game_Config_Money { return read_T_Game_Config_Money(item1) }) }),
v_ByteArray3D:decodeList(field(value,"ByteArray3D"),func(item0 any) List[List[uint8]] { return decodeList(item0,func(item1 any) List[uint8] { return decodeList(item1,func(item2 any) uint8 { return uint8(unsigned(item2,8)) }) }) }),
v_ShortArray3D:decodeList(field(value,"ShortArray3D"),func(item0 any) List[List[int16]] { return decodeList(item0,func(item1 any) List[int16] { return decodeList(item1,func(item2 any) int16 { return int16(signed(item2,16)) }) }) }),
v_IntArray3D:decodeList(field(value,"IntArray3D"),func(item0 any) List[List[int32]] { return decodeList(item0,func(item1 any) List[int32] { return decodeList(item1,func(item2 any) int32 { return int32(signed(item2,32)) }) }) }),
v_LongArray3D:decodeList(field(value,"LongArray3D"),func(item0 any) List[List[int64]] { return decodeList(item0,func(item1 any) List[int64] { return decodeList(item1,func(item2 any) int64 { return int64(signed(item2,64)) }) }) }),
v_FloatArray3D:decodeList(field(value,"FloatArray3D"),func(item0 any) List[List[float32]] { return decodeList(item0,func(item1 any) List[float32] { return decodeList(item1,func(item2 any) float32 { return float32(floating(item2,32)) }) }) }),
v_DoubleArray3D:decodeList(field(value,"DoubleArray3D"),func(item0 any) List[List[float64]] { return decodeList(item0,func(item1 any) List[float64] { return decodeList(item1,func(item2 any) float64 { return float64(floating(item2,64)) }) }) }),
v_BoolArray3D:decodeList(field(value,"BoolArray3D"),func(item0 any) List[List[bool]] { return decodeList(item0,func(item1 any) List[bool] { return decodeList(item1,func(item2 any) bool { return boolean(item2) }) }) }),
v_StringArray3D:decodeList(field(value,"StringArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
v_SByteArray3D:decodeList(field(value,"SByteArray3D"),func(item0 any) List[List[int8]] { return decodeList(item0,func(item1 any) List[int8] { return decodeList(item1,func(item2 any) int8 { return int8(signed(item2,8)) }) }) }),
v_UShortArray3D:decodeList(field(value,"UShortArray3D"),func(item0 any) List[List[uint16]] { return decodeList(item0,func(item1 any) List[uint16] { return decodeList(item1,func(item2 any) uint16 { return uint16(unsigned(item2,16)) }) }) }),
v_UIntArray3D:decodeList(field(value,"UIntArray3D"),func(item0 any) List[List[uint32]] { return decodeList(item0,func(item1 any) List[uint32] { return decodeList(item1,func(item2 any) uint32 { return uint32(unsigned(item2,32)) }) }) }),
v_ULongArray3D:decodeList(field(value,"ULongArray3D"),func(item0 any) List[List[uint64]] { return decodeList(item0,func(item1 any) List[uint64] { return decodeList(item1,func(item2 any) uint64 { return uint64(unsigned(item2,64)) }) }) }),
v_DecimalArray3D:decodeList(field(value,"DecimalArray3D"),func(item0 any) List[List[Decimal]] { return decodeList(item0,func(item1 any) List[Decimal] { return decodeList(item1,func(item2 any) Decimal { return decimalValue(text(item2)) }) }) }),
v_QualityArray3D:decodeList(field(value,"QualityArray3D"),func(item0 any) List[List[T_Game_Config_Quality]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Quality] { return decodeList(item1,func(item2 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item2,32))) }) }) }),
v_Vector2Array3D:decodeList(field(value,"Vector2Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector2]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item2) }) }) }),
v_Vector3Array3D:decodeList(field(value,"Vector3Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector3]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item2) }) }) }),
v_KvArray3D:decodeList(field(value,"KvArray3D"),func(item0 any) List[List[T_ZrConfig_Types_Kv]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Kv] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item2) }) }) }),
v_MoneyArray3D:decodeList(field(value,"MoneyArray3D"),func(item0 any) List[List[T_Game_Config_Money]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Money] { return decodeList(item1,func(item2 any) T_Game_Config_Money { return read_T_Game_Config_Money(item2) }) }) }),
v_Settings:read_T_Game_Config_BattleSettings(field(value,"Settings")),
v_Range:read_T_Game_Config_ConfigRange(field(value,"Range")),
v_Options:read_T_Game_Config_SampleOptions(field(value,"Options")),
v_Action:read_T_Game_Config_BattleAction(field(value,"Action")),
v_Actions:decodeList(field(value,"Actions"),func(item0 any) T_Game_Config_BattleAction { return read_T_Game_Config_BattleAction(item0) }),
v_Ranges:decodeList(field(value,"Ranges"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
v_OptionsList:decodeList(field(value,"OptionsList"),func(item0 any) *T_Game_Config_SampleOptions { return read_T_Game_Config_SampleOptions(item0) }),
v_Groups:decodeList(field(value,"Groups"),func(item0 any) *T_Game_Config_ActionGroup { return read_T_Game_Config_ActionGroup(item0) }),
v_Phases:decodeList(field(value,"Phases"),func(item0 any) *T_Game_Config_BattlePhase { return read_T_Game_Config_BattlePhase(item0) }),
v_OptionalRange:decodeOptional(field(value,"OptionalRange"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
v_OptionalInt:decodeOptional(field(value,"OptionalInt"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_OptionalUInt:decodeOptional(field(value,"OptionalUInt"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_OptionalULong:decodeOptional(field(value,"OptionalULong"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
v_OptionalDecimal:decodeOptional(field(value,"OptionalDecimal"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_OptionalBool:decodeOptional(field(value,"OptionalBool"),func(item0 any) bool { return boolean(item0) }),
v_OptionalQuality:decodeOptional(field(value,"OptionalQuality"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
v_OptionalMoney:decodeOptional(field(value,"OptionalMoney"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
v_OptionalValues:decodeList(field(value,"OptionalValues"),func(item0 any) Optional[int32] { return decodeOptional(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_StatMap:decodeDictionary(field(value,"StatMap"),func(item0 any) string { return text(item0) },func(item0 any) int32 { return int32(signed(item0,32)) }),
v_PriceMap:decodeDictionary(field(value,"PriceMap"),func(item0 any) int32 { return int32(signed(item0,32)) },func(item0 any) Optional[Decimal] { return decodeOptional(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
v_QualityMap:decodeDictionary(field(value,"QualityMap"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) },func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_ListMap:decodeDictionary(field(value,"ListMap"),func(item0 any) string { return text(item0) },func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_IntMatrix:decodeMatrix(field(value,"IntMatrix"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_DecimalCube:decodeMatrix(field(value,"DecimalCube"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_OptionalMatrix:decodeMatrix(field(value,"OptionalMatrix"),func(item0 any) Optional[uint32] { return decodeOptional(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
v_RawJsonValue:text(field(value,"RawJsonValue")),
v_RawJsonArray:decodeList(field(value,"RawJsonArray"),func(item0 any) string { return text(item0) }),
v_RawJsonArray2D:decodeList(field(value,"RawJsonArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
v_RawJsonArray3D:decodeList(field(value,"RawJsonArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
v_JsonIntArray:decodeList(field(value,"JsonIntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
}}
func(v *AllTypesRow)Id()int32{return v.v_Id}
func(v *AllTypesRow)ByteValue()uint8{return v.v_ByteValue}
func(v *AllTypesRow)ShortValue()int16{return v.v_ShortValue}
func(v *AllTypesRow)IntValue()int32{return v.v_IntValue}
func(v *AllTypesRow)LongValue()int64{return v.v_LongValue}
func(v *AllTypesRow)FloatValue()float32{return v.v_FloatValue}
func(v *AllTypesRow)DoubleValue()float64{return v.v_DoubleValue}
func(v *AllTypesRow)BoolValue()bool{return v.v_BoolValue}
func(v *AllTypesRow)StringValue()string{return v.v_StringValue}
func(v *AllTypesRow)SByteValue()int8{return v.v_SByteValue}
func(v *AllTypesRow)UShortValue()uint16{return v.v_UShortValue}
func(v *AllTypesRow)UIntValue()uint32{return v.v_UIntValue}
func(v *AllTypesRow)ULongValue()uint64{return v.v_ULongValue}
func(v *AllTypesRow)DecimalValue()Decimal{return v.v_DecimalValue}
func(v *AllTypesRow)QualityValue()T_Game_Config_Quality{return v.v_QualityValue}
func(v *AllTypesRow)Vector2Value()T_ZrConfig_Types_Vector2{return v.v_Vector2Value}
func(v *AllTypesRow)Vector3Value()T_ZrConfig_Types_Vector3{return v.v_Vector3Value}
func(v *AllTypesRow)KvValue()T_ZrConfig_Types_Kv{return v.v_KvValue}
func(v *AllTypesRow)MoneyValue()T_Game_Config_Money{return v.v_MoneyValue}
func(v *AllTypesRow)ByteArray()List[uint8]{return v.v_ByteArray}
func(v *AllTypesRow)ShortArray()List[int16]{return v.v_ShortArray}
func(v *AllTypesRow)IntArray()List[int32]{return v.v_IntArray}
func(v *AllTypesRow)LongArray()List[int64]{return v.v_LongArray}
func(v *AllTypesRow)FloatArray()List[float32]{return v.v_FloatArray}
func(v *AllTypesRow)DoubleArray()List[float64]{return v.v_DoubleArray}
func(v *AllTypesRow)BoolArray()List[bool]{return v.v_BoolArray}
func(v *AllTypesRow)StringArray()List[string]{return v.v_StringArray}
func(v *AllTypesRow)SByteArray()List[int8]{return v.v_SByteArray}
func(v *AllTypesRow)UShortArray()List[uint16]{return v.v_UShortArray}
func(v *AllTypesRow)UIntArray()List[uint32]{return v.v_UIntArray}
func(v *AllTypesRow)ULongArray()List[uint64]{return v.v_ULongArray}
func(v *AllTypesRow)DecimalArray()List[Decimal]{return v.v_DecimalArray}
func(v *AllTypesRow)QualityArray()List[T_Game_Config_Quality]{return v.v_QualityArray}
func(v *AllTypesRow)Vector2Array()List[T_ZrConfig_Types_Vector2]{return v.v_Vector2Array}
func(v *AllTypesRow)Vector3Array()List[T_ZrConfig_Types_Vector3]{return v.v_Vector3Array}
func(v *AllTypesRow)KvArray()List[T_ZrConfig_Types_Kv]{return v.v_KvArray}
func(v *AllTypesRow)MoneyArray()List[T_Game_Config_Money]{return v.v_MoneyArray}
func(v *AllTypesRow)ByteArray2D()List[List[uint8]]{return v.v_ByteArray2D}
func(v *AllTypesRow)ShortArray2D()List[List[int16]]{return v.v_ShortArray2D}
func(v *AllTypesRow)IntArray2D()List[List[int32]]{return v.v_IntArray2D}
func(v *AllTypesRow)LongArray2D()List[List[int64]]{return v.v_LongArray2D}
func(v *AllTypesRow)FloatArray2D()List[List[float32]]{return v.v_FloatArray2D}
func(v *AllTypesRow)DoubleArray2D()List[List[float64]]{return v.v_DoubleArray2D}
func(v *AllTypesRow)BoolArray2D()List[List[bool]]{return v.v_BoolArray2D}
func(v *AllTypesRow)StringArray2D()List[List[string]]{return v.v_StringArray2D}
func(v *AllTypesRow)SByteArray2D()List[List[int8]]{return v.v_SByteArray2D}
func(v *AllTypesRow)UShortArray2D()List[List[uint16]]{return v.v_UShortArray2D}
func(v *AllTypesRow)UIntArray2D()List[List[uint32]]{return v.v_UIntArray2D}
func(v *AllTypesRow)ULongArray2D()List[List[uint64]]{return v.v_ULongArray2D}
func(v *AllTypesRow)DecimalArray2D()List[List[Decimal]]{return v.v_DecimalArray2D}
func(v *AllTypesRow)QualityArray2D()List[List[T_Game_Config_Quality]]{return v.v_QualityArray2D}
func(v *AllTypesRow)Vector2Array2D()List[List[T_ZrConfig_Types_Vector2]]{return v.v_Vector2Array2D}
func(v *AllTypesRow)Vector3Array2D()List[List[T_ZrConfig_Types_Vector3]]{return v.v_Vector3Array2D}
func(v *AllTypesRow)KvArray2D()List[List[T_ZrConfig_Types_Kv]]{return v.v_KvArray2D}
func(v *AllTypesRow)MoneyArray2D()List[List[T_Game_Config_Money]]{return v.v_MoneyArray2D}
func(v *AllTypesRow)ByteArray3D()List[List[List[uint8]]]{return v.v_ByteArray3D}
func(v *AllTypesRow)ShortArray3D()List[List[List[int16]]]{return v.v_ShortArray3D}
func(v *AllTypesRow)IntArray3D()List[List[List[int32]]]{return v.v_IntArray3D}
func(v *AllTypesRow)LongArray3D()List[List[List[int64]]]{return v.v_LongArray3D}
func(v *AllTypesRow)FloatArray3D()List[List[List[float32]]]{return v.v_FloatArray3D}
func(v *AllTypesRow)DoubleArray3D()List[List[List[float64]]]{return v.v_DoubleArray3D}
func(v *AllTypesRow)BoolArray3D()List[List[List[bool]]]{return v.v_BoolArray3D}
func(v *AllTypesRow)StringArray3D()List[List[List[string]]]{return v.v_StringArray3D}
func(v *AllTypesRow)SByteArray3D()List[List[List[int8]]]{return v.v_SByteArray3D}
func(v *AllTypesRow)UShortArray3D()List[List[List[uint16]]]{return v.v_UShortArray3D}
func(v *AllTypesRow)UIntArray3D()List[List[List[uint32]]]{return v.v_UIntArray3D}
func(v *AllTypesRow)ULongArray3D()List[List[List[uint64]]]{return v.v_ULongArray3D}
func(v *AllTypesRow)DecimalArray3D()List[List[List[Decimal]]]{return v.v_DecimalArray3D}
func(v *AllTypesRow)QualityArray3D()List[List[List[T_Game_Config_Quality]]]{return v.v_QualityArray3D}
func(v *AllTypesRow)Vector2Array3D()List[List[List[T_ZrConfig_Types_Vector2]]]{return v.v_Vector2Array3D}
func(v *AllTypesRow)Vector3Array3D()List[List[List[T_ZrConfig_Types_Vector3]]]{return v.v_Vector3Array3D}
func(v *AllTypesRow)KvArray3D()List[List[List[T_ZrConfig_Types_Kv]]]{return v.v_KvArray3D}
func(v *AllTypesRow)MoneyArray3D()List[List[List[T_Game_Config_Money]]]{return v.v_MoneyArray3D}
func(v *AllTypesRow)Settings()*T_Game_Config_BattleSettings{return v.v_Settings}
func(v *AllTypesRow)Range()T_Game_Config_ConfigRange{return v.v_Range}
func(v *AllTypesRow)Options()*T_Game_Config_SampleOptions{return v.v_Options}
func(v *AllTypesRow)Action()T_Game_Config_BattleAction{return v.v_Action}
func(v *AllTypesRow)Actions()List[T_Game_Config_BattleAction]{return v.v_Actions}
func(v *AllTypesRow)Ranges()List[T_Game_Config_ConfigRange]{return v.v_Ranges}
func(v *AllTypesRow)OptionsList()List[*T_Game_Config_SampleOptions]{return v.v_OptionsList}
func(v *AllTypesRow)Groups()List[*T_Game_Config_ActionGroup]{return v.v_Groups}
func(v *AllTypesRow)Phases()List[*T_Game_Config_BattlePhase]{return v.v_Phases}
func(v *AllTypesRow)OptionalRange()Optional[T_Game_Config_ConfigRange]{return v.v_OptionalRange}
func(v *AllTypesRow)OptionalInt()Optional[int32]{return v.v_OptionalInt}
func(v *AllTypesRow)OptionalUInt()Optional[uint32]{return v.v_OptionalUInt}
func(v *AllTypesRow)OptionalULong()Optional[uint64]{return v.v_OptionalULong}
func(v *AllTypesRow)OptionalDecimal()Optional[Decimal]{return v.v_OptionalDecimal}
func(v *AllTypesRow)OptionalBool()Optional[bool]{return v.v_OptionalBool}
func(v *AllTypesRow)OptionalQuality()Optional[T_Game_Config_Quality]{return v.v_OptionalQuality}
func(v *AllTypesRow)OptionalMoney()Optional[T_Game_Config_Money]{return v.v_OptionalMoney}
func(v *AllTypesRow)OptionalValues()List[Optional[int32]]{return v.v_OptionalValues}
func(v *AllTypesRow)StatMap()Dictionary[string,int32]{return v.v_StatMap}
func(v *AllTypesRow)PriceMap()Dictionary[int32,Optional[Decimal]]{return v.v_PriceMap}
func(v *AllTypesRow)QualityMap()Dictionary[T_Game_Config_Quality,uint32]{return v.v_QualityMap}
func(v *AllTypesRow)ListMap()Dictionary[string,List[int32]]{return v.v_ListMap}
func(v *AllTypesRow)IntMatrix()Matrix[int32]{return v.v_IntMatrix}
func(v *AllTypesRow)DecimalCube()Matrix[Decimal]{return v.v_DecimalCube}
func(v *AllTypesRow)OptionalMatrix()Matrix[Optional[uint32]]{return v.v_OptionalMatrix}
func(v *AllTypesRow)RawJsonValue()string{return v.v_RawJsonValue}
func(v *AllTypesRow)RawJsonArray()List[string]{return v.v_RawJsonArray}
func(v *AllTypesRow)RawJsonArray2D()List[List[string]]{return v.v_RawJsonArray2D}
func(v *AllTypesRow)RawJsonArray3D()List[List[List[string]]]{return v.v_RawJsonArray3D}
func(v *AllTypesRow)JsonIntArray()List[int32]{return v.v_JsonIntArray}
type AllTypesRows struct{storage *rowStorage[AllTypesRow];keyIndex map[AllTypesKey_Key]int;full atomic.Bool
group_ById oneValue[map[AllTypesKey_Group_ById]*dataGroup[AllTypesRow]]
group_ByQuality oneValue[map[AllTypesKey_Group_ByQuality]*dataGroup[AllTypesRow]]
}
func newAllTypesRows(indexed *indexedRows,owner *DataSnapshot)*AllTypesRows{result:=&AllTypesRows{storage:newRowStorage(indexed,func(value any)*AllTypesRow{return readAllTypesRow(value,owner)}),keyIndex:make(map[AllTypesKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=AllTypesKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *AllTypesRows)Find(k0 int32)(*AllTypesRow,bool,error){index,ok:=t.keyIndex[AllTypesKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *AllTypesRows)Get(k0 int32)(*AllTypesRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *AllTypesRows)TryLoad(ctx context.Context,k0 int32)(*AllTypesRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[AllTypesKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *AllTypesRows)Load(ctx context.Context,k0 int32)(*AllTypesRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *AllTypesRows)Count()int{return len(t.storage.ready)}
func(t *AllTypesRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *AllTypesRows)Rows()(List[*AllTypesRow],error){if !t.full.Load(){return List[*AllTypesRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *AllTypesRows)LoadAll(ctx context.Context)(List[*AllTypesRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*AllTypesRow]{},err}
if _,err=t.index_ByQuality(ctx,true);err!=nil{return List[*AllTypesRow]{},err}
t.full.Store(true);return rows,nil}
func(t *AllTypesRows)index_ById(ctx context.Context,load bool)(map[AllTypesKey_Group_ById]*dataGroup[AllTypesRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *AllTypesRows)buildIndex_ById(ctx context.Context)(map[AllTypesKey_Group_ById]*dataGroup[AllTypesRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[AllTypesKey_Group_ById]*dataGroup[AllTypesRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[AllTypesKey_Group_ById]*dataGroup[AllTypesRow]);for i:=0;i<t.Count();i++{var key AllTypesKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=AllTypesKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=AllTypesKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[AllTypesRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *AllTypesRows)FindGroup_ById(k0 int32)(List[*AllTypesRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*AllTypesRow]{},err};return index[AllTypesKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *AllTypesRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*AllTypesRow],error){if err:=checkContext(ctx);err!=nil{return List[*AllTypesRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*AllTypesRow]{},err};return index[AllTypesKey_Group_ById{k0}].view(ctx,t.storage,true)}
func(t *AllTypesRows)index_ByQuality(ctx context.Context,load bool)(map[AllTypesKey_Group_ByQuality]*dataGroup[AllTypesRow],error){if cached:=t.group_ByQuality.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ByQuality.get()};return t.buildIndex_ByQuality(ctx)}
func(t *AllTypesRows)buildIndex_ByQuality(ctx context.Context)(map[AllTypesKey_Group_ByQuality]*dataGroup[AllTypesRow],error){return t.group_ByQuality.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[AllTypesKey_Group_ByQuality]*dataGroup[AllTypesRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[AllTypesKey_Group_ByQuality]*dataGroup[AllTypesRow]);for i:=0;i<t.Count();i++{var key AllTypesKey_Group_ByQuality;if cached:=t.storage.ready[i].Load();cached!=nil{key=AllTypesKey_Group_ByQuality{cached.v_QualityValue}}else{value:=t.storage.indexed.readTransient(i);key=AllTypesKey_Group_ByQuality{T_Game_Config_Quality(int32(signed(field(value,"QualityValue"),32)))}};group:=result[key];if group==nil{group=&dataGroup[AllTypesRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *AllTypesRows)FindGroup_ByQuality(k0 T_Game_Config_Quality)(List[*AllTypesRow],error){index,err:=t.index_ByQuality(nil,false);if err!=nil{return List[*AllTypesRow]{},err};return index[AllTypesKey_Group_ByQuality{k0}].view(nil,t.storage,false)}
func(t *AllTypesRows)LoadGroup_ByQuality(ctx context.Context,k0 T_Game_Config_Quality)(List[*AllTypesRow],error){if err:=checkContext(ctx);err!=nil{return List[*AllTypesRow]{},err};index,err:=t.index_ByQuality(ctx,true);if err!=nil{return List[*AllTypesRow]{},err};return index[AllTypesKey_Group_ByQuality{k0}].view(ctx,t.storage,true)}
type AllTypes10kRow struct{owner *DataSnapshot
v_Id int32
v_ByteValue uint8
v_ShortValue int16
v_IntValue int32
v_LongValue int64
v_FloatValue float32
v_DoubleValue float64
v_BoolValue bool
v_StringValue string
v_SByteValue int8
v_UShortValue uint16
v_UIntValue uint32
v_ULongValue uint64
v_DecimalValue Decimal
v_QualityValue T_Game_Config_Quality
v_Vector2Value T_ZrConfig_Types_Vector2
v_Vector3Value T_ZrConfig_Types_Vector3
v_KvValue T_ZrConfig_Types_Kv
v_MoneyValue T_Game_Config_Money
v_ByteArray List[uint8]
v_ShortArray List[int16]
v_IntArray List[int32]
v_LongArray List[int64]
v_FloatArray List[float32]
v_DoubleArray List[float64]
v_BoolArray List[bool]
v_StringArray List[string]
v_SByteArray List[int8]
v_UShortArray List[uint16]
v_UIntArray List[uint32]
v_ULongArray List[uint64]
v_DecimalArray List[Decimal]
v_QualityArray List[T_Game_Config_Quality]
v_Vector2Array List[T_ZrConfig_Types_Vector2]
v_Vector3Array List[T_ZrConfig_Types_Vector3]
v_KvArray List[T_ZrConfig_Types_Kv]
v_MoneyArray List[T_Game_Config_Money]
v_ByteArray2D List[List[uint8]]
v_ShortArray2D List[List[int16]]
v_IntArray2D List[List[int32]]
v_LongArray2D List[List[int64]]
v_FloatArray2D List[List[float32]]
v_DoubleArray2D List[List[float64]]
v_BoolArray2D List[List[bool]]
v_StringArray2D List[List[string]]
v_SByteArray2D List[List[int8]]
v_UShortArray2D List[List[uint16]]
v_UIntArray2D List[List[uint32]]
v_ULongArray2D List[List[uint64]]
v_DecimalArray2D List[List[Decimal]]
v_QualityArray2D List[List[T_Game_Config_Quality]]
v_Vector2Array2D List[List[T_ZrConfig_Types_Vector2]]
v_Vector3Array2D List[List[T_ZrConfig_Types_Vector3]]
v_KvArray2D List[List[T_ZrConfig_Types_Kv]]
v_MoneyArray2D List[List[T_Game_Config_Money]]
v_ByteArray3D List[List[List[uint8]]]
v_ShortArray3D List[List[List[int16]]]
v_IntArray3D List[List[List[int32]]]
v_LongArray3D List[List[List[int64]]]
v_FloatArray3D List[List[List[float32]]]
v_DoubleArray3D List[List[List[float64]]]
v_BoolArray3D List[List[List[bool]]]
v_StringArray3D List[List[List[string]]]
v_SByteArray3D List[List[List[int8]]]
v_UShortArray3D List[List[List[uint16]]]
v_UIntArray3D List[List[List[uint32]]]
v_ULongArray3D List[List[List[uint64]]]
v_DecimalArray3D List[List[List[Decimal]]]
v_QualityArray3D List[List[List[T_Game_Config_Quality]]]
v_Vector2Array3D List[List[List[T_ZrConfig_Types_Vector2]]]
v_Vector3Array3D List[List[List[T_ZrConfig_Types_Vector3]]]
v_KvArray3D List[List[List[T_ZrConfig_Types_Kv]]]
v_MoneyArray3D List[List[List[T_Game_Config_Money]]]
v_Settings *T_Game_Config_BattleSettings
v_Range T_Game_Config_ConfigRange
v_Options *T_Game_Config_SampleOptions
v_Action T_Game_Config_BattleAction
v_Actions List[T_Game_Config_BattleAction]
v_Ranges List[T_Game_Config_ConfigRange]
v_OptionsList List[*T_Game_Config_SampleOptions]
v_Groups List[*T_Game_Config_ActionGroup]
v_Phases List[*T_Game_Config_BattlePhase]
v_OptionalRange Optional[T_Game_Config_ConfigRange]
v_OptionalInt Optional[int32]
v_OptionalUInt Optional[uint32]
v_OptionalULong Optional[uint64]
v_OptionalDecimal Optional[Decimal]
v_OptionalBool Optional[bool]
v_OptionalQuality Optional[T_Game_Config_Quality]
v_OptionalMoney Optional[T_Game_Config_Money]
v_OptionalValues List[Optional[int32]]
v_StatMap Dictionary[string,int32]
v_PriceMap Dictionary[int32,Optional[Decimal]]
v_QualityMap Dictionary[T_Game_Config_Quality,uint32]
v_ListMap Dictionary[string,List[int32]]
v_IntMatrix Matrix[int32]
v_DecimalCube Matrix[Decimal]
v_OptionalMatrix Matrix[Optional[uint32]]
v_RawJsonValue string
v_RawJsonArray List[string]
v_RawJsonArray2D List[List[string]]
v_RawJsonArray3D List[List[List[string]]]
v_JsonIntArray List[int32]
v_Hero int32
v_Skill int32
v_Previous int32
}
func readAllTypes10kRow(value any,owner *DataSnapshot)*AllTypes10kRow{return &AllTypes10kRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_ByteValue:uint8(unsigned(field(value,"ByteValue"),8)),
v_ShortValue:int16(signed(field(value,"ShortValue"),16)),
v_IntValue:int32(signed(field(value,"IntValue"),32)),
v_LongValue:int64(signed(field(value,"LongValue"),64)),
v_FloatValue:float32(floating(field(value,"FloatValue"),32)),
v_DoubleValue:float64(floating(field(value,"DoubleValue"),64)),
v_BoolValue:boolean(field(value,"BoolValue")),
v_StringValue:text(field(value,"StringValue")),
v_SByteValue:int8(signed(field(value,"SByteValue"),8)),
v_UShortValue:uint16(unsigned(field(value,"UShortValue"),16)),
v_UIntValue:uint32(unsigned(field(value,"UIntValue"),32)),
v_ULongValue:uint64(unsigned(field(value,"ULongValue"),64)),
v_DecimalValue:decimalValue(text(field(value,"DecimalValue"))),
v_QualityValue:T_Game_Config_Quality(int32(signed(field(value,"QualityValue"),32))),
v_Vector2Value:read_T_ZrConfig_Types_Vector2(field(value,"Vector2Value")),
v_Vector3Value:read_T_ZrConfig_Types_Vector3(field(value,"Vector3Value")),
v_KvValue:read_T_ZrConfig_Types_Kv(field(value,"KvValue")),
v_MoneyValue:read_T_Game_Config_Money(field(value,"MoneyValue")),
v_ByteArray:decodeList(field(value,"ByteArray"),func(item0 any) uint8 { return uint8(unsigned(item0,8)) }),
v_ShortArray:decodeList(field(value,"ShortArray"),func(item0 any) int16 { return int16(signed(item0,16)) }),
v_IntArray:decodeList(field(value,"IntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_LongArray:decodeList(field(value,"LongArray"),func(item0 any) int64 { return int64(signed(item0,64)) }),
v_FloatArray:decodeList(field(value,"FloatArray"),func(item0 any) float32 { return float32(floating(item0,32)) }),
v_DoubleArray:decodeList(field(value,"DoubleArray"),func(item0 any) float64 { return float64(floating(item0,64)) }),
v_BoolArray:decodeList(field(value,"BoolArray"),func(item0 any) bool { return boolean(item0) }),
v_StringArray:decodeList(field(value,"StringArray"),func(item0 any) string { return text(item0) }),
v_SByteArray:decodeList(field(value,"SByteArray"),func(item0 any) int8 { return int8(signed(item0,8)) }),
v_UShortArray:decodeList(field(value,"UShortArray"),func(item0 any) uint16 { return uint16(unsigned(item0,16)) }),
v_UIntArray:decodeList(field(value,"UIntArray"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_ULongArray:decodeList(field(value,"ULongArray"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
v_DecimalArray:decodeList(field(value,"DecimalArray"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_QualityArray:decodeList(field(value,"QualityArray"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
v_Vector2Array:decodeList(field(value,"Vector2Array"),func(item0 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item0) }),
v_Vector3Array:decodeList(field(value,"Vector3Array"),func(item0 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item0) }),
v_KvArray:decodeList(field(value,"KvArray"),func(item0 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item0) }),
v_MoneyArray:decodeList(field(value,"MoneyArray"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
v_ByteArray2D:decodeList(field(value,"ByteArray2D"),func(item0 any) List[uint8] { return decodeList(item0,func(item1 any) uint8 { return uint8(unsigned(item1,8)) }) }),
v_ShortArray2D:decodeList(field(value,"ShortArray2D"),func(item0 any) List[int16] { return decodeList(item0,func(item1 any) int16 { return int16(signed(item1,16)) }) }),
v_IntArray2D:decodeList(field(value,"IntArray2D"),func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_LongArray2D:decodeList(field(value,"LongArray2D"),func(item0 any) List[int64] { return decodeList(item0,func(item1 any) int64 { return int64(signed(item1,64)) }) }),
v_FloatArray2D:decodeList(field(value,"FloatArray2D"),func(item0 any) List[float32] { return decodeList(item0,func(item1 any) float32 { return float32(floating(item1,32)) }) }),
v_DoubleArray2D:decodeList(field(value,"DoubleArray2D"),func(item0 any) List[float64] { return decodeList(item0,func(item1 any) float64 { return float64(floating(item1,64)) }) }),
v_BoolArray2D:decodeList(field(value,"BoolArray2D"),func(item0 any) List[bool] { return decodeList(item0,func(item1 any) bool { return boolean(item1) }) }),
v_StringArray2D:decodeList(field(value,"StringArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
v_SByteArray2D:decodeList(field(value,"SByteArray2D"),func(item0 any) List[int8] { return decodeList(item0,func(item1 any) int8 { return int8(signed(item1,8)) }) }),
v_UShortArray2D:decodeList(field(value,"UShortArray2D"),func(item0 any) List[uint16] { return decodeList(item0,func(item1 any) uint16 { return uint16(unsigned(item1,16)) }) }),
v_UIntArray2D:decodeList(field(value,"UIntArray2D"),func(item0 any) List[uint32] { return decodeList(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
v_ULongArray2D:decodeList(field(value,"ULongArray2D"),func(item0 any) List[uint64] { return decodeList(item0,func(item1 any) uint64 { return uint64(unsigned(item1,64)) }) }),
v_DecimalArray2D:decodeList(field(value,"DecimalArray2D"),func(item0 any) List[Decimal] { return decodeList(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
v_QualityArray2D:decodeList(field(value,"QualityArray2D"),func(item0 any) List[T_Game_Config_Quality] { return decodeList(item0,func(item1 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item1,32))) }) }),
v_Vector2Array2D:decodeList(field(value,"Vector2Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item1) }) }),
v_Vector3Array2D:decodeList(field(value,"Vector3Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item1) }) }),
v_KvArray2D:decodeList(field(value,"KvArray2D"),func(item0 any) List[T_ZrConfig_Types_Kv] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item1) }) }),
v_MoneyArray2D:decodeList(field(value,"MoneyArray2D"),func(item0 any) List[T_Game_Config_Money] { return decodeList(item0,func(item1 any) T_Game_Config_Money { return read_T_Game_Config_Money(item1) }) }),
v_ByteArray3D:decodeList(field(value,"ByteArray3D"),func(item0 any) List[List[uint8]] { return decodeList(item0,func(item1 any) List[uint8] { return decodeList(item1,func(item2 any) uint8 { return uint8(unsigned(item2,8)) }) }) }),
v_ShortArray3D:decodeList(field(value,"ShortArray3D"),func(item0 any) List[List[int16]] { return decodeList(item0,func(item1 any) List[int16] { return decodeList(item1,func(item2 any) int16 { return int16(signed(item2,16)) }) }) }),
v_IntArray3D:decodeList(field(value,"IntArray3D"),func(item0 any) List[List[int32]] { return decodeList(item0,func(item1 any) List[int32] { return decodeList(item1,func(item2 any) int32 { return int32(signed(item2,32)) }) }) }),
v_LongArray3D:decodeList(field(value,"LongArray3D"),func(item0 any) List[List[int64]] { return decodeList(item0,func(item1 any) List[int64] { return decodeList(item1,func(item2 any) int64 { return int64(signed(item2,64)) }) }) }),
v_FloatArray3D:decodeList(field(value,"FloatArray3D"),func(item0 any) List[List[float32]] { return decodeList(item0,func(item1 any) List[float32] { return decodeList(item1,func(item2 any) float32 { return float32(floating(item2,32)) }) }) }),
v_DoubleArray3D:decodeList(field(value,"DoubleArray3D"),func(item0 any) List[List[float64]] { return decodeList(item0,func(item1 any) List[float64] { return decodeList(item1,func(item2 any) float64 { return float64(floating(item2,64)) }) }) }),
v_BoolArray3D:decodeList(field(value,"BoolArray3D"),func(item0 any) List[List[bool]] { return decodeList(item0,func(item1 any) List[bool] { return decodeList(item1,func(item2 any) bool { return boolean(item2) }) }) }),
v_StringArray3D:decodeList(field(value,"StringArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
v_SByteArray3D:decodeList(field(value,"SByteArray3D"),func(item0 any) List[List[int8]] { return decodeList(item0,func(item1 any) List[int8] { return decodeList(item1,func(item2 any) int8 { return int8(signed(item2,8)) }) }) }),
v_UShortArray3D:decodeList(field(value,"UShortArray3D"),func(item0 any) List[List[uint16]] { return decodeList(item0,func(item1 any) List[uint16] { return decodeList(item1,func(item2 any) uint16 { return uint16(unsigned(item2,16)) }) }) }),
v_UIntArray3D:decodeList(field(value,"UIntArray3D"),func(item0 any) List[List[uint32]] { return decodeList(item0,func(item1 any) List[uint32] { return decodeList(item1,func(item2 any) uint32 { return uint32(unsigned(item2,32)) }) }) }),
v_ULongArray3D:decodeList(field(value,"ULongArray3D"),func(item0 any) List[List[uint64]] { return decodeList(item0,func(item1 any) List[uint64] { return decodeList(item1,func(item2 any) uint64 { return uint64(unsigned(item2,64)) }) }) }),
v_DecimalArray3D:decodeList(field(value,"DecimalArray3D"),func(item0 any) List[List[Decimal]] { return decodeList(item0,func(item1 any) List[Decimal] { return decodeList(item1,func(item2 any) Decimal { return decimalValue(text(item2)) }) }) }),
v_QualityArray3D:decodeList(field(value,"QualityArray3D"),func(item0 any) List[List[T_Game_Config_Quality]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Quality] { return decodeList(item1,func(item2 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item2,32))) }) }) }),
v_Vector2Array3D:decodeList(field(value,"Vector2Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector2]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item2) }) }) }),
v_Vector3Array3D:decodeList(field(value,"Vector3Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector3]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item2) }) }) }),
v_KvArray3D:decodeList(field(value,"KvArray3D"),func(item0 any) List[List[T_ZrConfig_Types_Kv]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Kv] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item2) }) }) }),
v_MoneyArray3D:decodeList(field(value,"MoneyArray3D"),func(item0 any) List[List[T_Game_Config_Money]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Money] { return decodeList(item1,func(item2 any) T_Game_Config_Money { return read_T_Game_Config_Money(item2) }) }) }),
v_Settings:read_T_Game_Config_BattleSettings(field(value,"Settings")),
v_Range:read_T_Game_Config_ConfigRange(field(value,"Range")),
v_Options:read_T_Game_Config_SampleOptions(field(value,"Options")),
v_Action:read_T_Game_Config_BattleAction(field(value,"Action")),
v_Actions:decodeList(field(value,"Actions"),func(item0 any) T_Game_Config_BattleAction { return read_T_Game_Config_BattleAction(item0) }),
v_Ranges:decodeList(field(value,"Ranges"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
v_OptionsList:decodeList(field(value,"OptionsList"),func(item0 any) *T_Game_Config_SampleOptions { return read_T_Game_Config_SampleOptions(item0) }),
v_Groups:decodeList(field(value,"Groups"),func(item0 any) *T_Game_Config_ActionGroup { return read_T_Game_Config_ActionGroup(item0) }),
v_Phases:decodeList(field(value,"Phases"),func(item0 any) *T_Game_Config_BattlePhase { return read_T_Game_Config_BattlePhase(item0) }),
v_OptionalRange:decodeOptional(field(value,"OptionalRange"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
v_OptionalInt:decodeOptional(field(value,"OptionalInt"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_OptionalUInt:decodeOptional(field(value,"OptionalUInt"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_OptionalULong:decodeOptional(field(value,"OptionalULong"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
v_OptionalDecimal:decodeOptional(field(value,"OptionalDecimal"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_OptionalBool:decodeOptional(field(value,"OptionalBool"),func(item0 any) bool { return boolean(item0) }),
v_OptionalQuality:decodeOptional(field(value,"OptionalQuality"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
v_OptionalMoney:decodeOptional(field(value,"OptionalMoney"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
v_OptionalValues:decodeList(field(value,"OptionalValues"),func(item0 any) Optional[int32] { return decodeOptional(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_StatMap:decodeDictionary(field(value,"StatMap"),func(item0 any) string { return text(item0) },func(item0 any) int32 { return int32(signed(item0,32)) }),
v_PriceMap:decodeDictionary(field(value,"PriceMap"),func(item0 any) int32 { return int32(signed(item0,32)) },func(item0 any) Optional[Decimal] { return decodeOptional(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
v_QualityMap:decodeDictionary(field(value,"QualityMap"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) },func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
v_ListMap:decodeDictionary(field(value,"ListMap"),func(item0 any) string { return text(item0) },func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
v_IntMatrix:decodeMatrix(field(value,"IntMatrix"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_DecimalCube:decodeMatrix(field(value,"DecimalCube"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
v_OptionalMatrix:decodeMatrix(field(value,"OptionalMatrix"),func(item0 any) Optional[uint32] { return decodeOptional(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
v_RawJsonValue:text(field(value,"RawJsonValue")),
v_RawJsonArray:decodeList(field(value,"RawJsonArray"),func(item0 any) string { return text(item0) }),
v_RawJsonArray2D:decodeList(field(value,"RawJsonArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
v_RawJsonArray3D:decodeList(field(value,"RawJsonArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
v_JsonIntArray:decodeList(field(value,"JsonIntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
v_Hero:int32(signed(field(value,"Hero"),32)),
v_Skill:int32(signed(field(value,"Skill"),32)),
v_Previous:int32(signed(field(value,"Previous"),32)),
}}
func(v *AllTypes10kRow)Id()int32{return v.v_Id}
func(v *AllTypes10kRow)ByteValue()uint8{return v.v_ByteValue}
func(v *AllTypes10kRow)ShortValue()int16{return v.v_ShortValue}
func(v *AllTypes10kRow)IntValue()int32{return v.v_IntValue}
func(v *AllTypes10kRow)LongValue()int64{return v.v_LongValue}
func(v *AllTypes10kRow)FloatValue()float32{return v.v_FloatValue}
func(v *AllTypes10kRow)DoubleValue()float64{return v.v_DoubleValue}
func(v *AllTypes10kRow)BoolValue()bool{return v.v_BoolValue}
func(v *AllTypes10kRow)StringValue()string{return v.v_StringValue}
func(v *AllTypes10kRow)SByteValue()int8{return v.v_SByteValue}
func(v *AllTypes10kRow)UShortValue()uint16{return v.v_UShortValue}
func(v *AllTypes10kRow)UIntValue()uint32{return v.v_UIntValue}
func(v *AllTypes10kRow)ULongValue()uint64{return v.v_ULongValue}
func(v *AllTypes10kRow)DecimalValue()Decimal{return v.v_DecimalValue}
func(v *AllTypes10kRow)QualityValue()T_Game_Config_Quality{return v.v_QualityValue}
func(v *AllTypes10kRow)Vector2Value()T_ZrConfig_Types_Vector2{return v.v_Vector2Value}
func(v *AllTypes10kRow)Vector3Value()T_ZrConfig_Types_Vector3{return v.v_Vector3Value}
func(v *AllTypes10kRow)KvValue()T_ZrConfig_Types_Kv{return v.v_KvValue}
func(v *AllTypes10kRow)MoneyValue()T_Game_Config_Money{return v.v_MoneyValue}
func(v *AllTypes10kRow)ByteArray()List[uint8]{return v.v_ByteArray}
func(v *AllTypes10kRow)ShortArray()List[int16]{return v.v_ShortArray}
func(v *AllTypes10kRow)IntArray()List[int32]{return v.v_IntArray}
func(v *AllTypes10kRow)LongArray()List[int64]{return v.v_LongArray}
func(v *AllTypes10kRow)FloatArray()List[float32]{return v.v_FloatArray}
func(v *AllTypes10kRow)DoubleArray()List[float64]{return v.v_DoubleArray}
func(v *AllTypes10kRow)BoolArray()List[bool]{return v.v_BoolArray}
func(v *AllTypes10kRow)StringArray()List[string]{return v.v_StringArray}
func(v *AllTypes10kRow)SByteArray()List[int8]{return v.v_SByteArray}
func(v *AllTypes10kRow)UShortArray()List[uint16]{return v.v_UShortArray}
func(v *AllTypes10kRow)UIntArray()List[uint32]{return v.v_UIntArray}
func(v *AllTypes10kRow)ULongArray()List[uint64]{return v.v_ULongArray}
func(v *AllTypes10kRow)DecimalArray()List[Decimal]{return v.v_DecimalArray}
func(v *AllTypes10kRow)QualityArray()List[T_Game_Config_Quality]{return v.v_QualityArray}
func(v *AllTypes10kRow)Vector2Array()List[T_ZrConfig_Types_Vector2]{return v.v_Vector2Array}
func(v *AllTypes10kRow)Vector3Array()List[T_ZrConfig_Types_Vector3]{return v.v_Vector3Array}
func(v *AllTypes10kRow)KvArray()List[T_ZrConfig_Types_Kv]{return v.v_KvArray}
func(v *AllTypes10kRow)MoneyArray()List[T_Game_Config_Money]{return v.v_MoneyArray}
func(v *AllTypes10kRow)ByteArray2D()List[List[uint8]]{return v.v_ByteArray2D}
func(v *AllTypes10kRow)ShortArray2D()List[List[int16]]{return v.v_ShortArray2D}
func(v *AllTypes10kRow)IntArray2D()List[List[int32]]{return v.v_IntArray2D}
func(v *AllTypes10kRow)LongArray2D()List[List[int64]]{return v.v_LongArray2D}
func(v *AllTypes10kRow)FloatArray2D()List[List[float32]]{return v.v_FloatArray2D}
func(v *AllTypes10kRow)DoubleArray2D()List[List[float64]]{return v.v_DoubleArray2D}
func(v *AllTypes10kRow)BoolArray2D()List[List[bool]]{return v.v_BoolArray2D}
func(v *AllTypes10kRow)StringArray2D()List[List[string]]{return v.v_StringArray2D}
func(v *AllTypes10kRow)SByteArray2D()List[List[int8]]{return v.v_SByteArray2D}
func(v *AllTypes10kRow)UShortArray2D()List[List[uint16]]{return v.v_UShortArray2D}
func(v *AllTypes10kRow)UIntArray2D()List[List[uint32]]{return v.v_UIntArray2D}
func(v *AllTypes10kRow)ULongArray2D()List[List[uint64]]{return v.v_ULongArray2D}
func(v *AllTypes10kRow)DecimalArray2D()List[List[Decimal]]{return v.v_DecimalArray2D}
func(v *AllTypes10kRow)QualityArray2D()List[List[T_Game_Config_Quality]]{return v.v_QualityArray2D}
func(v *AllTypes10kRow)Vector2Array2D()List[List[T_ZrConfig_Types_Vector2]]{return v.v_Vector2Array2D}
func(v *AllTypes10kRow)Vector3Array2D()List[List[T_ZrConfig_Types_Vector3]]{return v.v_Vector3Array2D}
func(v *AllTypes10kRow)KvArray2D()List[List[T_ZrConfig_Types_Kv]]{return v.v_KvArray2D}
func(v *AllTypes10kRow)MoneyArray2D()List[List[T_Game_Config_Money]]{return v.v_MoneyArray2D}
func(v *AllTypes10kRow)ByteArray3D()List[List[List[uint8]]]{return v.v_ByteArray3D}
func(v *AllTypes10kRow)ShortArray3D()List[List[List[int16]]]{return v.v_ShortArray3D}
func(v *AllTypes10kRow)IntArray3D()List[List[List[int32]]]{return v.v_IntArray3D}
func(v *AllTypes10kRow)LongArray3D()List[List[List[int64]]]{return v.v_LongArray3D}
func(v *AllTypes10kRow)FloatArray3D()List[List[List[float32]]]{return v.v_FloatArray3D}
func(v *AllTypes10kRow)DoubleArray3D()List[List[List[float64]]]{return v.v_DoubleArray3D}
func(v *AllTypes10kRow)BoolArray3D()List[List[List[bool]]]{return v.v_BoolArray3D}
func(v *AllTypes10kRow)StringArray3D()List[List[List[string]]]{return v.v_StringArray3D}
func(v *AllTypes10kRow)SByteArray3D()List[List[List[int8]]]{return v.v_SByteArray3D}
func(v *AllTypes10kRow)UShortArray3D()List[List[List[uint16]]]{return v.v_UShortArray3D}
func(v *AllTypes10kRow)UIntArray3D()List[List[List[uint32]]]{return v.v_UIntArray3D}
func(v *AllTypes10kRow)ULongArray3D()List[List[List[uint64]]]{return v.v_ULongArray3D}
func(v *AllTypes10kRow)DecimalArray3D()List[List[List[Decimal]]]{return v.v_DecimalArray3D}
func(v *AllTypes10kRow)QualityArray3D()List[List[List[T_Game_Config_Quality]]]{return v.v_QualityArray3D}
func(v *AllTypes10kRow)Vector2Array3D()List[List[List[T_ZrConfig_Types_Vector2]]]{return v.v_Vector2Array3D}
func(v *AllTypes10kRow)Vector3Array3D()List[List[List[T_ZrConfig_Types_Vector3]]]{return v.v_Vector3Array3D}
func(v *AllTypes10kRow)KvArray3D()List[List[List[T_ZrConfig_Types_Kv]]]{return v.v_KvArray3D}
func(v *AllTypes10kRow)MoneyArray3D()List[List[List[T_Game_Config_Money]]]{return v.v_MoneyArray3D}
func(v *AllTypes10kRow)Settings()*T_Game_Config_BattleSettings{return v.v_Settings}
func(v *AllTypes10kRow)Range()T_Game_Config_ConfigRange{return v.v_Range}
func(v *AllTypes10kRow)Options()*T_Game_Config_SampleOptions{return v.v_Options}
func(v *AllTypes10kRow)Action()T_Game_Config_BattleAction{return v.v_Action}
func(v *AllTypes10kRow)Actions()List[T_Game_Config_BattleAction]{return v.v_Actions}
func(v *AllTypes10kRow)Ranges()List[T_Game_Config_ConfigRange]{return v.v_Ranges}
func(v *AllTypes10kRow)OptionsList()List[*T_Game_Config_SampleOptions]{return v.v_OptionsList}
func(v *AllTypes10kRow)Groups()List[*T_Game_Config_ActionGroup]{return v.v_Groups}
func(v *AllTypes10kRow)Phases()List[*T_Game_Config_BattlePhase]{return v.v_Phases}
func(v *AllTypes10kRow)OptionalRange()Optional[T_Game_Config_ConfigRange]{return v.v_OptionalRange}
func(v *AllTypes10kRow)OptionalInt()Optional[int32]{return v.v_OptionalInt}
func(v *AllTypes10kRow)OptionalUInt()Optional[uint32]{return v.v_OptionalUInt}
func(v *AllTypes10kRow)OptionalULong()Optional[uint64]{return v.v_OptionalULong}
func(v *AllTypes10kRow)OptionalDecimal()Optional[Decimal]{return v.v_OptionalDecimal}
func(v *AllTypes10kRow)OptionalBool()Optional[bool]{return v.v_OptionalBool}
func(v *AllTypes10kRow)OptionalQuality()Optional[T_Game_Config_Quality]{return v.v_OptionalQuality}
func(v *AllTypes10kRow)OptionalMoney()Optional[T_Game_Config_Money]{return v.v_OptionalMoney}
func(v *AllTypes10kRow)OptionalValues()List[Optional[int32]]{return v.v_OptionalValues}
func(v *AllTypes10kRow)StatMap()Dictionary[string,int32]{return v.v_StatMap}
func(v *AllTypes10kRow)PriceMap()Dictionary[int32,Optional[Decimal]]{return v.v_PriceMap}
func(v *AllTypes10kRow)QualityMap()Dictionary[T_Game_Config_Quality,uint32]{return v.v_QualityMap}
func(v *AllTypes10kRow)ListMap()Dictionary[string,List[int32]]{return v.v_ListMap}
func(v *AllTypes10kRow)IntMatrix()Matrix[int32]{return v.v_IntMatrix}
func(v *AllTypes10kRow)DecimalCube()Matrix[Decimal]{return v.v_DecimalCube}
func(v *AllTypes10kRow)OptionalMatrix()Matrix[Optional[uint32]]{return v.v_OptionalMatrix}
func(v *AllTypes10kRow)RawJsonValue()string{return v.v_RawJsonValue}
func(v *AllTypes10kRow)RawJsonArray()List[string]{return v.v_RawJsonArray}
func(v *AllTypes10kRow)RawJsonArray2D()List[List[string]]{return v.v_RawJsonArray2D}
func(v *AllTypes10kRow)RawJsonArray3D()List[List[List[string]]]{return v.v_RawJsonArray3D}
func(v *AllTypes10kRow)JsonIntArray()List[int32]{return v.v_JsonIntArray}
func(v *AllTypes10kRow)HeroId()int32{return v.v_Hero}
func(v *AllTypes10kRow)Get_Hero()(*HeroRow,error){return v.owner.table_Hero.storage.reference(nil,v.v_Hero,false)}
func(v *AllTypes10kRow)Load_Hero(ctx context.Context)(*HeroRow,error){return v.owner.table_Hero.storage.reference(ctx,v.v_Hero,true)}
func(v *AllTypes10kRow)SkillId()int32{return v.v_Skill}
func(v *AllTypes10kRow)Get_Skill()(*SkillRow,error){return v.owner.table_Skill.storage.reference(nil,v.v_Skill,false)}
func(v *AllTypes10kRow)Load_Skill(ctx context.Context)(*SkillRow,error){return v.owner.table_Skill.storage.reference(ctx,v.v_Skill,true)}
func(v *AllTypes10kRow)PreviousId()int32{return v.v_Previous}
func(v *AllTypes10kRow)Get_Previous()(*AllTypes10kRow,error){return v.owner.table_AllTypes10k.storage.reference(nil,v.v_Previous,false)}
func(v *AllTypes10kRow)Load_Previous(ctx context.Context)(*AllTypes10kRow,error){return v.owner.table_AllTypes10k.storage.reference(ctx,v.v_Previous,true)}
type AllTypes10kRows struct{storage *rowStorage[AllTypes10kRow];keyIndex map[AllTypes10kKey_Key]int;full atomic.Bool
group_ById oneValue[map[AllTypes10kKey_Group_ById]*dataGroup[AllTypes10kRow]]
}
func newAllTypes10kRows(indexed *indexedRows,owner *DataSnapshot)*AllTypes10kRows{result:=&AllTypes10kRows{storage:newRowStorage(indexed,func(value any)*AllTypes10kRow{return readAllTypes10kRow(value,owner)}),keyIndex:make(map[AllTypes10kKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=AllTypes10kKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *AllTypes10kRows)Find(k0 int32)(*AllTypes10kRow,bool,error){index,ok:=t.keyIndex[AllTypes10kKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *AllTypes10kRows)Get(k0 int32)(*AllTypes10kRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *AllTypes10kRows)TryLoad(ctx context.Context,k0 int32)(*AllTypes10kRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[AllTypes10kKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *AllTypes10kRows)Load(ctx context.Context,k0 int32)(*AllTypes10kRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *AllTypes10kRows)Count()int{return len(t.storage.ready)}
func(t *AllTypes10kRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *AllTypes10kRows)Rows()(List[*AllTypes10kRow],error){if !t.full.Load(){return List[*AllTypes10kRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *AllTypes10kRows)LoadAll(ctx context.Context)(List[*AllTypes10kRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*AllTypes10kRow]{},err}
t.full.Store(true);return rows,nil}
func(t *AllTypes10kRows)index_ById(ctx context.Context,load bool)(map[AllTypes10kKey_Group_ById]*dataGroup[AllTypes10kRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *AllTypes10kRows)buildIndex_ById(ctx context.Context)(map[AllTypes10kKey_Group_ById]*dataGroup[AllTypes10kRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[AllTypes10kKey_Group_ById]*dataGroup[AllTypes10kRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[AllTypes10kKey_Group_ById]*dataGroup[AllTypes10kRow]);for i:=0;i<t.Count();i++{var key AllTypes10kKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=AllTypes10kKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=AllTypes10kKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[AllTypes10kRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *AllTypes10kRows)FindGroup_ById(k0 int32)(List[*AllTypes10kRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*AllTypes10kRow]{},err};return index[AllTypes10kKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *AllTypes10kRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*AllTypes10kRow],error){if err:=checkContext(ctx);err!=nil{return List[*AllTypes10kRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*AllTypes10kRow]{},err};return index[AllTypes10kKey_Group_ById{k0}].view(ctx,t.storage,true)}
type DropRow struct{owner *DataSnapshot
v_GroupId int32
v_Id int32
v_Count int32
}
func readDropRow(value any,owner *DataSnapshot)*DropRow{return &DropRow{owner:owner,
v_GroupId:int32(signed(field(value,"GroupId"),32)),
v_Id:int32(signed(field(value,"Id"),32)),
v_Count:int32(signed(field(value,"Count"),32)),
}}
func(v *DropRow)GroupId()int32{return v.v_GroupId}
func(v *DropRow)Id()int32{return v.v_Id}
func(v *DropRow)Count()int32{return v.v_Count}
type DropRows struct{storage *rowStorage[DropRow];keyIndex map[DropKey_Key]int;full atomic.Bool
group_ByGroupId oneValue[map[DropKey_Group_ByGroupId]*dataGroup[DropRow]]
group_ById oneValue[map[DropKey_Group_ById]*dataGroup[DropRow]]
}
func newDropRows(indexed *indexedRows,owner *DataSnapshot)*DropRows{result:=&DropRows{storage:newRowStorage(indexed,func(value any)*DropRow{return readDropRow(value,owner)}),keyIndex:make(map[DropKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=DropKey_Key{int32(signed(field(value,"GroupId"),32)),int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *DropRows)Find(k0 int32,k1 int32)(*DropRow,bool,error){index,ok:=t.keyIndex[DropKey_Key{k0,k1}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *DropRows)Get(k0 int32,k1 int32)(*DropRow,error){row,found,err:=t.Find(k0,k1);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *DropRows)TryLoad(ctx context.Context,k0 int32,k1 int32)(*DropRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[DropKey_Key{k0,k1}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *DropRows)Load(ctx context.Context,k0 int32,k1 int32)(*DropRow,error){row,found,err:=t.TryLoad(ctx,k0,k1);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *DropRows)Count()int{return len(t.storage.ready)}
func(t *DropRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *DropRows)Rows()(List[*DropRow],error){if !t.full.Load(){return List[*DropRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *DropRows)LoadAll(ctx context.Context)(List[*DropRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ByGroupId(ctx,true);err!=nil{return List[*DropRow]{},err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*DropRow]{},err}
t.full.Store(true);return rows,nil}
func(t *DropRows)index_ByGroupId(ctx context.Context,load bool)(map[DropKey_Group_ByGroupId]*dataGroup[DropRow],error){if cached:=t.group_ByGroupId.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ByGroupId.get()};return t.buildIndex_ByGroupId(ctx)}
func(t *DropRows)buildIndex_ByGroupId(ctx context.Context)(map[DropKey_Group_ByGroupId]*dataGroup[DropRow],error){return t.group_ByGroupId.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[DropKey_Group_ByGroupId]*dataGroup[DropRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[DropKey_Group_ByGroupId]*dataGroup[DropRow]);for i:=0;i<t.Count();i++{var key DropKey_Group_ByGroupId;if cached:=t.storage.ready[i].Load();cached!=nil{key=DropKey_Group_ByGroupId{cached.v_GroupId}}else{value:=t.storage.indexed.readTransient(i);key=DropKey_Group_ByGroupId{int32(signed(field(value,"GroupId"),32))}};group:=result[key];if group==nil{group=&dataGroup[DropRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *DropRows)FindGroup_ByGroupId(k0 int32)(List[*DropRow],error){index,err:=t.index_ByGroupId(nil,false);if err!=nil{return List[*DropRow]{},err};return index[DropKey_Group_ByGroupId{k0}].view(nil,t.storage,false)}
func(t *DropRows)LoadGroup_ByGroupId(ctx context.Context,k0 int32)(List[*DropRow],error){if err:=checkContext(ctx);err!=nil{return List[*DropRow]{},err};index,err:=t.index_ByGroupId(ctx,true);if err!=nil{return List[*DropRow]{},err};return index[DropKey_Group_ByGroupId{k0}].view(ctx,t.storage,true)}
func(t *DropRows)index_ById(ctx context.Context,load bool)(map[DropKey_Group_ById]*dataGroup[DropRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *DropRows)buildIndex_ById(ctx context.Context)(map[DropKey_Group_ById]*dataGroup[DropRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[DropKey_Group_ById]*dataGroup[DropRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[DropKey_Group_ById]*dataGroup[DropRow]);for i:=0;i<t.Count();i++{var key DropKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=DropKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=DropKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[DropRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *DropRows)FindGroup_ById(k0 int32)(List[*DropRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*DropRow]{},err};return index[DropKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *DropRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*DropRow],error){if err:=checkContext(ctx);err!=nil{return List[*DropRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*DropRow]{},err};return index[DropKey_Group_ById{k0}].view(ctx,t.storage,true)}
type HeroRow struct{owner *DataSnapshot
v_Id int32
v_Name string
v_Skill int32
v_OtherSkill int32
v_Mentor int32
}
func readHeroRow(value any,owner *DataSnapshot)*HeroRow{return &HeroRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_Name:text(field(value,"Name")),
v_Skill:int32(signed(field(value,"Skill"),32)),
v_OtherSkill:int32(signed(field(value,"OtherSkill"),32)),
v_Mentor:int32(signed(field(value,"Mentor"),32)),
}}
func(v *HeroRow)Id()int32{return v.v_Id}
func(v *HeroRow)Name()string{return v.v_Name}
func(v *HeroRow)SkillId()int32{return v.v_Skill}
func(v *HeroRow)Get_Skill()(*SkillRow,error){return v.owner.table_Skill.storage.reference(nil,v.v_Skill,false)}
func(v *HeroRow)Load_Skill(ctx context.Context)(*SkillRow,error){return v.owner.table_Skill.storage.reference(ctx,v.v_Skill,true)}
func(v *HeroRow)OtherSkillId()int32{return v.v_OtherSkill}
func(v *HeroRow)Get_OtherSkill()(*SkillRow,error){return v.owner.table_Skill.storage.reference(nil,v.v_OtherSkill,false)}
func(v *HeroRow)Load_OtherSkill(ctx context.Context)(*SkillRow,error){return v.owner.table_Skill.storage.reference(ctx,v.v_OtherSkill,true)}
func(v *HeroRow)MentorId()int32{return v.v_Mentor}
func(v *HeroRow)Get_Mentor()(*HeroRow,error){return v.owner.table_Hero.storage.reference(nil,v.v_Mentor,false)}
func(v *HeroRow)Load_Mentor(ctx context.Context)(*HeroRow,error){return v.owner.table_Hero.storage.reference(ctx,v.v_Mentor,true)}
type HeroRows struct{storage *rowStorage[HeroRow];keyIndex map[HeroKey_Key]int;full atomic.Bool
group_ById oneValue[map[HeroKey_Group_ById]*dataGroup[HeroRow]]
}
func newHeroRows(indexed *indexedRows,owner *DataSnapshot)*HeroRows{result:=&HeroRows{storage:newRowStorage(indexed,func(value any)*HeroRow{return readHeroRow(value,owner)}),keyIndex:make(map[HeroKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=HeroKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *HeroRows)Find(k0 int32)(*HeroRow,bool,error){index,ok:=t.keyIndex[HeroKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *HeroRows)Get(k0 int32)(*HeroRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *HeroRows)TryLoad(ctx context.Context,k0 int32)(*HeroRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[HeroKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *HeroRows)Load(ctx context.Context,k0 int32)(*HeroRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *HeroRows)Count()int{return len(t.storage.ready)}
func(t *HeroRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *HeroRows)Rows()(List[*HeroRow],error){if !t.full.Load(){return List[*HeroRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *HeroRows)LoadAll(ctx context.Context)(List[*HeroRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*HeroRow]{},err}
t.full.Store(true);return rows,nil}
func(t *HeroRows)index_ById(ctx context.Context,load bool)(map[HeroKey_Group_ById]*dataGroup[HeroRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *HeroRows)buildIndex_ById(ctx context.Context)(map[HeroKey_Group_ById]*dataGroup[HeroRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[HeroKey_Group_ById]*dataGroup[HeroRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[HeroKey_Group_ById]*dataGroup[HeroRow]);for i:=0;i<t.Count();i++{var key HeroKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=HeroKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=HeroKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[HeroRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *HeroRows)FindGroup_ById(k0 int32)(List[*HeroRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*HeroRow]{},err};return index[HeroKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *HeroRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*HeroRow],error){if err:=checkContext(ctx);err!=nil{return List[*HeroRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*HeroRow]{},err};return index[HeroKey_Group_ById{k0}].view(ctx,t.storage,true)}
type ItemCatalogRow struct{owner *DataSnapshot
v_Id int32
v_Name string
v_MaxStack int32
v_Rarity int32
v_Enabled bool
}
func readItemCatalogRow(value any,owner *DataSnapshot)*ItemCatalogRow{return &ItemCatalogRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_Name:text(field(value,"Name")),
v_MaxStack:int32(signed(field(value,"MaxStack"),32)),
v_Rarity:int32(signed(field(value,"Rarity"),32)),
v_Enabled:boolean(field(value,"Enabled")),
}}
func(v *ItemCatalogRow)Id()int32{return v.v_Id}
func(v *ItemCatalogRow)Name()string{return v.v_Name}
func(v *ItemCatalogRow)MaxStack()int32{return v.v_MaxStack}
func(v *ItemCatalogRow)Rarity()int32{return v.v_Rarity}
func(v *ItemCatalogRow)Enabled()bool{return v.v_Enabled}
type ItemCatalogRows struct{storage *rowStorage[ItemCatalogRow];keyIndex map[ItemCatalogKey_Key]int;full atomic.Bool
group_ById oneValue[map[ItemCatalogKey_Group_ById]*dataGroup[ItemCatalogRow]]
}
func newItemCatalogRows(indexed *indexedRows,owner *DataSnapshot)*ItemCatalogRows{result:=&ItemCatalogRows{storage:newRowStorage(indexed,func(value any)*ItemCatalogRow{return readItemCatalogRow(value,owner)}),keyIndex:make(map[ItemCatalogKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=ItemCatalogKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *ItemCatalogRows)Find(k0 int32)(*ItemCatalogRow,bool,error){index,ok:=t.keyIndex[ItemCatalogKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *ItemCatalogRows)Get(k0 int32)(*ItemCatalogRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *ItemCatalogRows)TryLoad(ctx context.Context,k0 int32)(*ItemCatalogRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[ItemCatalogKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *ItemCatalogRows)Load(ctx context.Context,k0 int32)(*ItemCatalogRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *ItemCatalogRows)Count()int{return len(t.storage.ready)}
func(t *ItemCatalogRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *ItemCatalogRows)Rows()(List[*ItemCatalogRow],error){if !t.full.Load(){return List[*ItemCatalogRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *ItemCatalogRows)LoadAll(ctx context.Context)(List[*ItemCatalogRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*ItemCatalogRow]{},err}
t.full.Store(true);return rows,nil}
func(t *ItemCatalogRows)index_ById(ctx context.Context,load bool)(map[ItemCatalogKey_Group_ById]*dataGroup[ItemCatalogRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *ItemCatalogRows)buildIndex_ById(ctx context.Context)(map[ItemCatalogKey_Group_ById]*dataGroup[ItemCatalogRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[ItemCatalogKey_Group_ById]*dataGroup[ItemCatalogRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[ItemCatalogKey_Group_ById]*dataGroup[ItemCatalogRow]);for i:=0;i<t.Count();i++{var key ItemCatalogKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=ItemCatalogKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=ItemCatalogKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[ItemCatalogRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *ItemCatalogRows)FindGroup_ById(k0 int32)(List[*ItemCatalogRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*ItemCatalogRow]{},err};return index[ItemCatalogKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *ItemCatalogRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*ItemCatalogRow],error){if err:=checkContext(ctx);err!=nil{return List[*ItemCatalogRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*ItemCatalogRow]{},err};return index[ItemCatalogKey_Group_ById{k0}].view(ctx,t.storage,true)}
type QuestRewardsRow struct{owner *DataSnapshot
v_Id int32
v_Name string
v_Item int32
v_Count int32
v_RequiredLevel int32
v_Enabled bool
}
func readQuestRewardsRow(value any,owner *DataSnapshot)*QuestRewardsRow{return &QuestRewardsRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_Name:text(field(value,"Name")),
v_Item:int32(signed(field(value,"Item"),32)),
v_Count:int32(signed(field(value,"Count"),32)),
v_RequiredLevel:int32(signed(field(value,"RequiredLevel"),32)),
v_Enabled:boolean(field(value,"Enabled")),
}}
func(v *QuestRewardsRow)Id()int32{return v.v_Id}
func(v *QuestRewardsRow)Name()string{return v.v_Name}
func(v *QuestRewardsRow)ItemId()int32{return v.v_Item}
func(v *QuestRewardsRow)Get_Item()(*ItemCatalogRow,error){return v.owner.table_ItemCatalog.storage.reference(nil,v.v_Item,false)}
func(v *QuestRewardsRow)Load_Item(ctx context.Context)(*ItemCatalogRow,error){return v.owner.table_ItemCatalog.storage.reference(ctx,v.v_Item,true)}
func(v *QuestRewardsRow)Count()int32{return v.v_Count}
func(v *QuestRewardsRow)RequiredLevel()int32{return v.v_RequiredLevel}
func(v *QuestRewardsRow)Enabled()bool{return v.v_Enabled}
type QuestRewardsRows struct{storage *rowStorage[QuestRewardsRow];keyIndex map[QuestRewardsKey_Key]int;full atomic.Bool
group_ById oneValue[map[QuestRewardsKey_Group_ById]*dataGroup[QuestRewardsRow]]
}
func newQuestRewardsRows(indexed *indexedRows,owner *DataSnapshot)*QuestRewardsRows{result:=&QuestRewardsRows{storage:newRowStorage(indexed,func(value any)*QuestRewardsRow{return readQuestRewardsRow(value,owner)}),keyIndex:make(map[QuestRewardsKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=QuestRewardsKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *QuestRewardsRows)Find(k0 int32)(*QuestRewardsRow,bool,error){index,ok:=t.keyIndex[QuestRewardsKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *QuestRewardsRows)Get(k0 int32)(*QuestRewardsRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *QuestRewardsRows)TryLoad(ctx context.Context,k0 int32)(*QuestRewardsRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[QuestRewardsKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *QuestRewardsRows)Load(ctx context.Context,k0 int32)(*QuestRewardsRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *QuestRewardsRows)Count()int{return len(t.storage.ready)}
func(t *QuestRewardsRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *QuestRewardsRows)Rows()(List[*QuestRewardsRow],error){if !t.full.Load(){return List[*QuestRewardsRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *QuestRewardsRows)LoadAll(ctx context.Context)(List[*QuestRewardsRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*QuestRewardsRow]{},err}
t.full.Store(true);return rows,nil}
func(t *QuestRewardsRows)index_ById(ctx context.Context,load bool)(map[QuestRewardsKey_Group_ById]*dataGroup[QuestRewardsRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *QuestRewardsRows)buildIndex_ById(ctx context.Context)(map[QuestRewardsKey_Group_ById]*dataGroup[QuestRewardsRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[QuestRewardsKey_Group_ById]*dataGroup[QuestRewardsRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[QuestRewardsKey_Group_ById]*dataGroup[QuestRewardsRow]);for i:=0;i<t.Count();i++{var key QuestRewardsKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=QuestRewardsKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=QuestRewardsKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[QuestRewardsRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *QuestRewardsRows)FindGroup_ById(k0 int32)(List[*QuestRewardsRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*QuestRewardsRow]{},err};return index[QuestRewardsKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *QuestRewardsRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*QuestRewardsRow],error){if err:=checkContext(ctx);err!=nil{return List[*QuestRewardsRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*QuestRewardsRow]{},err};return index[QuestRewardsKey_Group_ById{k0}].view(ctx,t.storage,true)}
type ShopOffersRow struct{owner *DataSnapshot
v_Id int32
v_Item int32
v_Price int32
v_DailyLimit int32
v_Enabled bool
}
func readShopOffersRow(value any,owner *DataSnapshot)*ShopOffersRow{return &ShopOffersRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_Item:int32(signed(field(value,"Item"),32)),
v_Price:int32(signed(field(value,"Price"),32)),
v_DailyLimit:int32(signed(field(value,"DailyLimit"),32)),
v_Enabled:boolean(field(value,"Enabled")),
}}
func(v *ShopOffersRow)Id()int32{return v.v_Id}
func(v *ShopOffersRow)ItemId()int32{return v.v_Item}
func(v *ShopOffersRow)Get_Item()(*ItemCatalogRow,error){return v.owner.table_ItemCatalog.storage.reference(nil,v.v_Item,false)}
func(v *ShopOffersRow)Load_Item(ctx context.Context)(*ItemCatalogRow,error){return v.owner.table_ItemCatalog.storage.reference(ctx,v.v_Item,true)}
func(v *ShopOffersRow)Price()int32{return v.v_Price}
func(v *ShopOffersRow)DailyLimit()int32{return v.v_DailyLimit}
func(v *ShopOffersRow)Enabled()bool{return v.v_Enabled}
type ShopOffersRows struct{storage *rowStorage[ShopOffersRow];keyIndex map[ShopOffersKey_Key]int;full atomic.Bool
group_ById oneValue[map[ShopOffersKey_Group_ById]*dataGroup[ShopOffersRow]]
}
func newShopOffersRows(indexed *indexedRows,owner *DataSnapshot)*ShopOffersRows{result:=&ShopOffersRows{storage:newRowStorage(indexed,func(value any)*ShopOffersRow{return readShopOffersRow(value,owner)}),keyIndex:make(map[ShopOffersKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=ShopOffersKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *ShopOffersRows)Find(k0 int32)(*ShopOffersRow,bool,error){index,ok:=t.keyIndex[ShopOffersKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *ShopOffersRows)Get(k0 int32)(*ShopOffersRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *ShopOffersRows)TryLoad(ctx context.Context,k0 int32)(*ShopOffersRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[ShopOffersKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *ShopOffersRows)Load(ctx context.Context,k0 int32)(*ShopOffersRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *ShopOffersRows)Count()int{return len(t.storage.ready)}
func(t *ShopOffersRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *ShopOffersRows)Rows()(List[*ShopOffersRow],error){if !t.full.Load(){return List[*ShopOffersRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *ShopOffersRows)LoadAll(ctx context.Context)(List[*ShopOffersRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*ShopOffersRow]{},err}
t.full.Store(true);return rows,nil}
func(t *ShopOffersRows)index_ById(ctx context.Context,load bool)(map[ShopOffersKey_Group_ById]*dataGroup[ShopOffersRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *ShopOffersRows)buildIndex_ById(ctx context.Context)(map[ShopOffersKey_Group_ById]*dataGroup[ShopOffersRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[ShopOffersKey_Group_ById]*dataGroup[ShopOffersRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[ShopOffersKey_Group_ById]*dataGroup[ShopOffersRow]);for i:=0;i<t.Count();i++{var key ShopOffersKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=ShopOffersKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=ShopOffersKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[ShopOffersRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *ShopOffersRows)FindGroup_ById(k0 int32)(List[*ShopOffersRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*ShopOffersRow]{},err};return index[ShopOffersKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *ShopOffersRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*ShopOffersRow],error){if err:=checkContext(ctx);err!=nil{return List[*ShopOffersRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*ShopOffersRow]{},err};return index[ShopOffersKey_Group_ById{k0}].view(ctx,t.storage,true)}
type SkillRow struct{owner *DataSnapshot
v_Id int32
v_Name string
v_Hero int32
}
func readSkillRow(value any,owner *DataSnapshot)*SkillRow{return &SkillRow{owner:owner,
v_Id:int32(signed(field(value,"Id"),32)),
v_Name:text(field(value,"Name")),
v_Hero:int32(signed(field(value,"Hero"),32)),
}}
func(v *SkillRow)Id()int32{return v.v_Id}
func(v *SkillRow)Name()string{return v.v_Name}
func(v *SkillRow)HeroId()int32{return v.v_Hero}
func(v *SkillRow)Get_Hero()(*HeroRow,error){return v.owner.table_Hero.storage.reference(nil,v.v_Hero,false)}
func(v *SkillRow)Load_Hero(ctx context.Context)(*HeroRow,error){return v.owner.table_Hero.storage.reference(ctx,v.v_Hero,true)}
type SkillRows struct{storage *rowStorage[SkillRow];keyIndex map[SkillKey_Key]int;full atomic.Bool
group_ById oneValue[map[SkillKey_Group_ById]*dataGroup[SkillRow]]
}
func newSkillRows(indexed *indexedRows,owner *DataSnapshot)*SkillRows{result:=&SkillRows{storage:newRowStorage(indexed,func(value any)*SkillRow{return readSkillRow(value,owner)}),keyIndex:make(map[SkillKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=SkillKey_Key{int32(signed(field(value,"Id"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *SkillRows)Find(k0 int32)(*SkillRow,bool,error){index,ok:=t.keyIndex[SkillKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *SkillRows)Get(k0 int32)(*SkillRow,error){row,found,err:=t.Find(k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *SkillRows)TryLoad(ctx context.Context,k0 int32)(*SkillRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[SkillKey_Key{k0}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *SkillRows)Load(ctx context.Context,k0 int32)(*SkillRow,error){row,found,err:=t.TryLoad(ctx,k0);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *SkillRows)Count()int{return len(t.storage.ready)}
func(t *SkillRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *SkillRows)Rows()(List[*SkillRow],error){if !t.full.Load(){return List[*SkillRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *SkillRows)LoadAll(ctx context.Context)(List[*SkillRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ById(ctx,true);err!=nil{return List[*SkillRow]{},err}
t.full.Store(true);return rows,nil}
func(t *SkillRows)index_ById(ctx context.Context,load bool)(map[SkillKey_Group_ById]*dataGroup[SkillRow],error){if cached:=t.group_ById.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ById.get()};return t.buildIndex_ById(ctx)}
func(t *SkillRows)buildIndex_ById(ctx context.Context)(map[SkillKey_Group_ById]*dataGroup[SkillRow],error){return t.group_ById.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[SkillKey_Group_ById]*dataGroup[SkillRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[SkillKey_Group_ById]*dataGroup[SkillRow]);for i:=0;i<t.Count();i++{var key SkillKey_Group_ById;if cached:=t.storage.ready[i].Load();cached!=nil{key=SkillKey_Group_ById{cached.v_Id}}else{value:=t.storage.indexed.readTransient(i);key=SkillKey_Group_ById{int32(signed(field(value,"Id"),32))}};group:=result[key];if group==nil{group=&dataGroup[SkillRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *SkillRows)FindGroup_ById(k0 int32)(List[*SkillRow],error){index,err:=t.index_ById(nil,false);if err!=nil{return List[*SkillRow]{},err};return index[SkillKey_Group_ById{k0}].view(nil,t.storage,false)}
func(t *SkillRows)LoadGroup_ById(ctx context.Context,k0 int32)(List[*SkillRow],error){if err:=checkContext(ctx);err!=nil{return List[*SkillRow]{},err};index,err:=t.index_ById(ctx,true);if err!=nil{return List[*SkillRow]{},err};return index[SkillKey_Group_ById{k0}].view(ctx,t.storage,true)}
type SkillLevelRow struct{owner *DataSnapshot
v_SkillId int32
v_Name string
v_Level int32
v_Quality int32
v_Damage int32
}
func readSkillLevelRow(value any,owner *DataSnapshot)*SkillLevelRow{return &SkillLevelRow{owner:owner,
v_SkillId:int32(signed(field(value,"SkillId"),32)),
v_Name:text(field(value,"Name")),
v_Level:int32(signed(field(value,"Level"),32)),
v_Quality:int32(signed(field(value,"Quality"),32)),
v_Damage:int32(signed(field(value,"Damage"),32)),
}}
func(v *SkillLevelRow)SkillId()int32{return v.v_SkillId}
func(v *SkillLevelRow)Name()string{return v.v_Name}
func(v *SkillLevelRow)Level()int32{return v.v_Level}
func(v *SkillLevelRow)Quality()int32{return v.v_Quality}
func(v *SkillLevelRow)Damage()int32{return v.v_Damage}
type SkillLevelRows struct{storage *rowStorage[SkillLevelRow];keyIndex map[SkillLevelKey_Key]int;full atomic.Bool
group_ByLevel oneValue[map[SkillLevelKey_Group_ByLevel]*dataGroup[SkillLevelRow]]
group_ByQuality oneValue[map[SkillLevelKey_Group_ByQuality]*dataGroup[SkillLevelRow]]
group_BySkillId oneValue[map[SkillLevelKey_Group_BySkillId]*dataGroup[SkillLevelRow]]
}
func newSkillLevelRows(indexed *indexedRows,owner *DataSnapshot)*SkillLevelRows{result:=&SkillLevelRows{storage:newRowStorage(indexed,func(value any)*SkillLevelRow{return readSkillLevelRow(value,owner)}),keyIndex:make(map[SkillLevelKey_Key]int,len(indexed.ids))};for i,value:=range indexed.keys{key:=SkillLevelKey_Key{int32(signed(field(value,"SkillId"),32)),int32(signed(field(value,"Level"),32)),int32(signed(field(value,"Quality"),32))};if _,exists:=result.keyIndex[key];exists{panic(serverError(InvalidValue))};result.keyIndex[key]=i};return result}
func(t *SkillLevelRows)Find(k0 int32,k1 int32,k2 int32)(*SkillLevelRow,bool,error){index,ok:=t.keyIndex[SkillLevelKey_Key{k0,k1,k2}];if !ok{return nil,false,nil};row,err:=t.storage.query(index);return row,err==nil,err}
func(t *SkillLevelRows)Get(k0 int32,k1 int32,k2 int32)(*SkillLevelRow,error){row,found,err:=t.Find(k0,k1,k2);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *SkillLevelRows)TryLoad(ctx context.Context,k0 int32,k1 int32,k2 int32)(*SkillLevelRow,bool,error){if err:=checkContext(ctx);err!=nil{return nil,false,err};index,ok:=t.keyIndex[SkillLevelKey_Key{k0,k1,k2}];if !ok{return nil,false,nil};row,err:=t.storage.load(ctx,index);return row,err==nil,err}
func(t *SkillLevelRows)Load(ctx context.Context,k0 int32,k1 int32,k2 int32)(*SkillLevelRow,error){row,found,err:=t.TryLoad(ctx,k0,k1,k2);if err==nil&&!found{err=serverError(KeyNotFound)};return row,err}
func(t *SkillLevelRows)Count()int{return len(t.storage.ready)}
func(t *SkillLevelRows)LoadedCount()int{return int(t.storage.loaded.Load())}
func(t *SkillLevelRows)Rows()(List[*SkillLevelRow],error){if !t.full.Load(){return List[*SkillLevelRow]{},serverError(TableNotLoaded)};return *t.storage.all.Load(),nil}
func(t *SkillLevelRows)LoadAll(ctx context.Context)(List[*SkillLevelRow],error){rows,err:=t.storage.loadAll(ctx);if err!=nil{return rows,err}
if _,err=t.index_ByLevel(ctx,true);err!=nil{return List[*SkillLevelRow]{},err}
if _,err=t.index_ByQuality(ctx,true);err!=nil{return List[*SkillLevelRow]{},err}
if _,err=t.index_BySkillId(ctx,true);err!=nil{return List[*SkillLevelRow]{},err}
t.full.Store(true);return rows,nil}
func(t *SkillLevelRows)index_ByLevel(ctx context.Context,load bool)(map[SkillLevelKey_Group_ByLevel]*dataGroup[SkillLevelRow],error){if cached:=t.group_ByLevel.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ByLevel.get()};return t.buildIndex_ByLevel(ctx)}
func(t *SkillLevelRows)buildIndex_ByLevel(ctx context.Context)(map[SkillLevelKey_Group_ByLevel]*dataGroup[SkillLevelRow],error){return t.group_ByLevel.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[SkillLevelKey_Group_ByLevel]*dataGroup[SkillLevelRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[SkillLevelKey_Group_ByLevel]*dataGroup[SkillLevelRow]);for i:=0;i<t.Count();i++{var key SkillLevelKey_Group_ByLevel;if cached:=t.storage.ready[i].Load();cached!=nil{key=SkillLevelKey_Group_ByLevel{cached.v_Level}}else{value:=t.storage.indexed.readTransient(i);key=SkillLevelKey_Group_ByLevel{int32(signed(field(value,"Level"),32))}};group:=result[key];if group==nil{group=&dataGroup[SkillLevelRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *SkillLevelRows)FindGroup_ByLevel(k0 int32)(List[*SkillLevelRow],error){index,err:=t.index_ByLevel(nil,false);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_ByLevel{k0}].view(nil,t.storage,false)}
func(t *SkillLevelRows)LoadGroup_ByLevel(ctx context.Context,k0 int32)(List[*SkillLevelRow],error){if err:=checkContext(ctx);err!=nil{return List[*SkillLevelRow]{},err};index,err:=t.index_ByLevel(ctx,true);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_ByLevel{k0}].view(ctx,t.storage,true)}
func(t *SkillLevelRows)index_ByQuality(ctx context.Context,load bool)(map[SkillLevelKey_Group_ByQuality]*dataGroup[SkillLevelRow],error){if cached:=t.group_ByQuality.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_ByQuality.get()};return t.buildIndex_ByQuality(ctx)}
func(t *SkillLevelRows)buildIndex_ByQuality(ctx context.Context)(map[SkillLevelKey_Group_ByQuality]*dataGroup[SkillLevelRow],error){return t.group_ByQuality.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[SkillLevelKey_Group_ByQuality]*dataGroup[SkillLevelRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[SkillLevelKey_Group_ByQuality]*dataGroup[SkillLevelRow]);for i:=0;i<t.Count();i++{var key SkillLevelKey_Group_ByQuality;if cached:=t.storage.ready[i].Load();cached!=nil{key=SkillLevelKey_Group_ByQuality{cached.v_Quality}}else{value:=t.storage.indexed.readTransient(i);key=SkillLevelKey_Group_ByQuality{int32(signed(field(value,"Quality"),32))}};group:=result[key];if group==nil{group=&dataGroup[SkillLevelRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *SkillLevelRows)FindGroup_ByQuality(k0 int32)(List[*SkillLevelRow],error){index,err:=t.index_ByQuality(nil,false);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_ByQuality{k0}].view(nil,t.storage,false)}
func(t *SkillLevelRows)LoadGroup_ByQuality(ctx context.Context,k0 int32)(List[*SkillLevelRow],error){if err:=checkContext(ctx);err!=nil{return List[*SkillLevelRow]{},err};index,err:=t.index_ByQuality(ctx,true);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_ByQuality{k0}].view(ctx,t.storage,true)}
func(t *SkillLevelRows)index_BySkillId(ctx context.Context,load bool)(map[SkillLevelKey_Group_BySkillId]*dataGroup[SkillLevelRow],error){if cached:=t.group_BySkillId.ready.Load();cached!=nil{return *cached,nil};if !load{return t.group_BySkillId.get()};return t.buildIndex_BySkillId(ctx)}
func(t *SkillLevelRows)buildIndex_BySkillId(ctx context.Context)(map[SkillLevelKey_Group_BySkillId]*dataGroup[SkillLevelRow],error){return t.group_BySkillId.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes(1),func()(map[SkillLevelKey_Group_BySkillId]*dataGroup[SkillLevelRow],int64,error){retained:=int64(256)+int64(t.Count())*512*1;result:=make(map[SkillLevelKey_Group_BySkillId]*dataGroup[SkillLevelRow]);for i:=0;i<t.Count();i++{var key SkillLevelKey_Group_BySkillId;if cached:=t.storage.ready[i].Load();cached!=nil{key=SkillLevelKey_Group_BySkillId{cached.v_SkillId}}else{value:=t.storage.indexed.readTransient(i);key=SkillLevelKey_Group_BySkillId{int32(signed(field(value,"SkillId"),32))}};group:=result[key];if group==nil{group=&dataGroup[SkillLevelRow]{};result[key]=group};group.ordinals=append(group.ordinals,i)};return result,retained,nil})}
func(t *SkillLevelRows)FindGroup_BySkillId(k0 int32)(List[*SkillLevelRow],error){index,err:=t.index_BySkillId(nil,false);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_BySkillId{k0}].view(nil,t.storage,false)}
func(t *SkillLevelRows)LoadGroup_BySkillId(ctx context.Context,k0 int32)(List[*SkillLevelRow],error){if err:=checkContext(ctx);err!=nil{return List[*SkillLevelRow]{},err};index,err:=t.index_BySkillId(ctx,true);if err!=nil{return List[*SkillLevelRow]{},err};return index[SkillLevelKey_Group_BySkillId{k0}].view(ctx,t.storage,true)}
