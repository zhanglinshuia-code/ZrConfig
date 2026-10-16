// zrconfig-api-contract:1
package zrconfig;
import java.io.IOException;
import java.nio.file.Path;
/** Server entry: escaped rows retain their complete original snapshot. */
public final class ServerConfig {
    private ServerConfig() {}
    public static ConfigSnapshot load(Path root, boolean preferJson) throws IOException {
        ConfigSnapshot snapshot = ConfigSnapshot.load(root, preferJson);
        snapshot.AllTypes.retain(snapshot);
        snapshot.AllTypes10k.retain(snapshot);
        snapshot.Drop.retain(snapshot);
        snapshot.Hero.retain(snapshot);
        snapshot.ItemCatalog.retain(snapshot);
        snapshot.QuestRewards.retain(snapshot);
        snapshot.ShopOffers.retain(snapshot);
        snapshot.Skill.retain(snapshot);
        snapshot.SkillLevel.retain(snapshot);
        return snapshot;
    }
    public static ConfigRuntime.ServerManager<ConfigSnapshot> manager() { return new ConfigRuntime.ServerManager<>(); }
    private static final class CatalogHolder { static final ConfigRuntime.Catalog VALUE = new ConfigRuntime.Catalog(String.join("",
"{\"format\":2,\"fingerprint\":\"ADE42D7364E51BC5A62B63F35E32A11E50424B1A690E12E56AFD23760D81DDC6\",\"tables\":[{\"name\":\"AllTypes\",\"schemaHash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"dependencies\":[],\"component\":0},{\"name\":\"AllTypes10k\",\"schemaHash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"],\"component\":1},{\"name\":\"Drop\",\"schemaHash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"dependencies\":[],\"component\":2},{\"name\":\"Hero\",\"schemaHash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"dependencies\":[\"Hero\",\"Skill\"],\"component\":3},{\"name\":\"ItemCatalog\",\"schemaHash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"dependencies\":[],\"component\":4},{\"name\":\"QuestRewards\",\"schemaHash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"dependencies\":[\"ItemCatalog\"],\"component\":5},{\"name\":\"ShopOffers\",\"schemaHash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"dependencies\":[\"ItemCatalog\"],\"component\":6},{\"name\":\"Skill\",\"schemaHash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"dependencies\":[\"Hero\"],\"component\":3},{\"name\":\"SkillLevel\",\"schemaHash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"dependencies\":[],\"component\":7}],\"components\":[[\"AllTypes\"],[\"AllTypes10k\"],[\"Drop\"],[\"Hero\",\"Skill\"],[\"ItemCatalog\"],[\"QuestRewards\"],[\"ShopOffers\"],[\"SkillLevel\"]],\"target\":\"C\",\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"binary-v4\",\"json-v1\",\"server-default\",\"selected-closure\",\"lazy-strict\",\"lazy-on-access\",\"fixed-source-bytes\",\"snapshot-owned-handles\",\"explicit-reference-load\",\"row-cache\",\"group-cache\",\"typed-warmup\",\"bounded-source\",\"structured-errors\",\"manager-lifecycle\"]}"
    )); }
    public static ConfigRuntime.Catalog catalog() { return CatalogHolder.VALUE; }
}
