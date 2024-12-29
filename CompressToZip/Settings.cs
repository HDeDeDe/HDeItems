using System.Collections;

internal static class Settings {
//-----------------------------------------------------Customize--------------------------------------------------------
    // ReSharper disable once InconsistentNaming
    public const bool giveMePDBs = true;
    public const bool weave = true;

    public const string pluginName = HDeMods.HDeItems.Plugin.PluginName;
    public const string pluginAuthor = HDeMods.HDeItems.Plugin.PluginAuthor;
    public const string pluginVersion = HDeMods.HDeItems.Plugin.PluginVersion;
    public const string changelog = "../CHANGELOG.md";
    public const string readme = "../README.md";

    public const string icon =
        "../Resources/icon.png";

    public const string riskOfRain2Install =
        @"C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed\";

    public static readonly ArrayList extraFiles = new() {
        new FileInfo("../HDeItemsUnity/Assets/AssetBundle/hdeitems"),
        new FileInfo("../Resources/HDeItems.language"),
        new FileInfo("../Resources/HDeItems.itemdisplayrules"),
    };

    public const string manifestWebsiteUrl = "https://github.com/HDeDeDe/HDeItems";

    public const string manifestDescription =
        "Just some items I decided to make on a whim :)";

    public const string manifestDependencies = "[\n" +
                                               "\t\t\"bbepis-BepInExPack-5.4.2117\",\n" +
                                               "\t\t\"RiskofThunder-R2API_Items-1.0.4\",\n" +
                                               "\t\t\"RiskofThunder-R2API_Prefab-1.0.4\",\n" +
                                               "\t\t\"RiskofThunder-R2API_RecalculateStats-1.4.0\",\n" +
                                               "\t\t\"HDeDeDe-HealthComponentAPI-1.2.0\",\n" +
                                               "\t\t\"Smooth_Salad-ShaderSwapper-1.0.1\"\n" +
                                               "\t]";

    public static void PreRun() {
        
    }
    
    public static void PostRun() {
        
    }
}