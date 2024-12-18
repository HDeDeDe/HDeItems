using System.IO.Compression;
using System.Diagnostics;

//---------------------------------------------------Program------------------------------------------------------------

#if DEBUG
const string targetFile = "../" + Settings.pluginName + "/bin/" + Settings.pluginName + "_debug.zip";
const string dllPath = "../" + Settings.pluginName + "/bin/Debug/netstandard2.1/";
const string dllPathWindows = "..\\" + Settings.pluginName + "\\bin\\Debug\\netstandard2.1\\";
#endif

#if RELEASE
const string targetFile = "../" + Settings.pluginName + "/bin/" + Settings.pluginName + ".zip";
const string dllPath = "../" + Settings.pluginName + "/bin/Release/netstandard2.1/";
const string dllPathWindows = "..\\" + Settings.pluginName + "\\bin\\Release\\netstandard2.1\\";
#endif

if (File.Exists(dllPath + Settings.pluginName + ".dll.prepatch")) 
	File.Delete(dllPath + Settings.pluginName + ".dll.prepatch");
if (File.Exists(dllPath + Settings.pluginName + ".pdb.prepatch")) 
	File.Delete(dllPath + Settings.pluginName + ".pdb.prepatch");

#pragma warning disable CS0162 // Unreachable code detected
if (Settings.weave) {
	Console.WriteLine("Weaving " + Settings.pluginName + ".dll");
	File.Copy(dllPath + Settings.pluginName + ".dll", dllPath + Settings.pluginName + ".dll.prepatch");
	File.Copy(dllPath + Settings.pluginName + ".pdb", dllPath + Settings.pluginName + ".pdb.prepatch");
	Process weaver = new Process();
	
	if (Settings.giveMePDBs) weaver.StartInfo.FileName = @".\NetWeaver\Unity.UNetWeaver2.exe";
	else weaver.StartInfo.FileName = @".\NetWeaver\Unity.UNetWeaver.exe";

	weaver.StartInfo.Arguments = "\"" + Settings.riskOfRain2Install + "UnityEngine.CoreModule.dll\" " +
	                             "\"" + Settings.riskOfRain2Install + "com.unity.multiplayer-hlapi.Runtime.dll\" " +
	                             dllPathWindows + " " +
	                             dllPathWindows + Settings.pluginName + ".dll " +
	                             // Dependency folders
	                             "\"" + Settings.riskOfRain2Install + "\" " +
	                             dllPathWindows + " " +
	                             "\"" + Environment.GetEnvironmentVariable("HOMEPATH") + "\\.nuget\\packages\\\"";
	weaver.StartInfo.RedirectStandardOutput = true;
	weaver.Start();
	string output;
	while ((output = weaver.StandardOutput.ReadLine()!) != null) Console.WriteLine(output);

	weaver.WaitForExit();
}

Console.WriteLine("Creating " + Settings.pluginName + ".Zip");
if (File.Exists(targetFile)) File.Delete(targetFile);

ZipArchive archive = ZipFile.Open(targetFile, ZipArchiveMode.Create);

if (Settings.changelog != "") archive.CreateEntryFromFile(Settings.changelog, "CHANGELOG.md", CompressionLevel.Optimal);
if (Settings.readme != "") archive.CreateEntryFromFile(Settings.readme, "README.md", CompressionLevel.Optimal);
archive.CreateEntryFromFile(dllPath + Settings.pluginName + ".dll", Settings.pluginName + ".dll",
	CompressionLevel.Optimal);
if (Settings.giveMePDBs)
	archive.CreateEntryFromFile(dllPath + Settings.pluginName + ".pdb", Settings.pluginName + ".pdb",
		CompressionLevel.Optimal);
if (Settings.icon != "") archive.CreateEntryFromFile(Settings.icon, "icon.png", CompressionLevel.Optimal);

foreach (FileInfo file in Settings.extraFiles) {
	if (file.Name.EndsWith(".bnk")) {
		archive.CreateEntryFromFile(file.FullName, file.Name.Replace(".bnk", ".sound"), CompressionLevel.Optimal);
		continue;
	}

	archive.CreateEntryFromFile(file.FullName, file.Name, CompressionLevel.Optimal);
}
#pragma warning restore CS0162 // Unreachable code detected

ZipArchiveEntry manifest = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
using (StreamWriter writer = new(manifest.Open())) {
	writer.WriteLine("{");
	writer.WriteLine("\t\"author\": \"" + Settings.pluginAuthor + "\",");
	writer.WriteLine("\t\"name\": \"" + Settings.pluginName + "\",");
	writer.WriteLine("\t\"version_number\": \"" + Settings.pluginVersion + "\",");
	writer.WriteLine("\t\"website_url\": \"" + Settings.manifestWebsiteUrl + "\",");
	writer.WriteLine("\t\"description\": \"" + Settings.manifestDescription + "\",");
	writer.WriteLine("\t\"dependencies\": " + Settings.manifestDependencies);
	writer.WriteLine("}");

	writer.Close();
}

archive.Dispose();