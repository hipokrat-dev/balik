using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Balik;
public static class BuildGame {
    [MenuItem("Balık/Projeyi Hazırla")]
    public static void Setup(){
        PlayerSettings.companyName="Yener Game Studio";PlayerSettings.productName="Derinliklerin Hazinesi";
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.hipokrat.balik");
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
        foreach(string path in AssetDatabase.GetAllAssetPaths())if(path.StartsWith("Assets/Resources/Art/")&&path.EndsWith(".png")){
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/app-icon.png");
        if(icon==null)throw new Exception("Fishing app icon is missing");
        PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
        foreach(var kind in PlayerSettings.GetSupportedIconKinds(UnityEditor.Build.NamedBuildTarget.Android)){
            var slots=PlayerSettings.GetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,kind);
            foreach(var slot in slots){var layers=new Texture2D[slot.maxLayerCount];for(int i=0;i<slot.minLayerCount;i++)layers[i]=icon;slot.SetTextures(layers);}
            PlayerSettings.SetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,kind,slots);
        }
        PlayerSettings.SplashScreen.show=false;
        Directory.CreateDirectory("Assets/Scenes");var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Fishing Game").AddComponent<FishingGame>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Fishing.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Fishing.unity",true)};AssetDatabase.SaveAssets();RunRulesTests();Debug.Log("BALIK_SETUP_PASS");
    }
    [MenuItem("Balık/Kural Testleri")]
    public static void RunRulesTests(){
        void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        var s=new MapSchedule();Check(!s.Poll(17),"early map");Check(s.Poll(18),"first map");Check(!s.Poll(20),"duplicate map");Check(s.Poll(48),"second map");Check(!s.Poll(99),"third map");s.Reset();s.Collect();Check(!s.Poll(100),"collected map returned");
        Check(FishingRules.RewardTime(179,20)==180,"time cap");Check(FishingRules.GripAfter(.1f,10,2)==0,"escape grip");Check(FishingRules.Goal(2)>FishingRules.Goal(1),"stage progression");
        var rng=new System.Random(123);var seen=new System.Collections.Generic.HashSet<string>();for(int i=0;i<10000;i++)seen.Add(FishingRules.Pick(rng).id);Check(seen.Count==FishingRules.Catalog.Length,"missing spawn kind");foreach(var d in FishingRules.Catalog){Check(d.sheet=="trash"?d.value==0&&d.Seconds==0:d.Seconds>0,"reward policy");Check(d.depthMin<d.depthMax,"depth");}
        Debug.Log("BALIK_RULES_PASS: two map opportunities, collection, time cap, escape, progression, all spawn species");
    }
    [MenuItem("Balık/Mac Test Sürümü Oluştur")]
    public static void BuildMac(){Setup();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Fishing.unity"},locationPathName="Builds/Mac/Balik.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Mac build failed");Debug.Log("BALIK_MAC_BUILD_PASS");}
    [MenuItem("Balık/Android APK Oluştur")]
    public static void BuildAndroid(){Setup();PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64|AndroidArchitecture.ARMv7;PlayerSettings.Android.bundleVersionCode=3;PlayerSettings.bundleVersion="1.0.2";EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Fishing.unity"},locationPathName="Builds/Android/Balik.apk",target=BuildTarget.Android,options=BuildOptions.None});if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Android build failed");}
}
