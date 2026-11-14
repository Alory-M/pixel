using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad] public static class ProjectSetup {
 static ProjectSetup(){EditorApplication.delayCall+=EnsureScene;}
 static void EnsureScene(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;PlayerSettings.companyName="Little Garden";PlayerSettings.productName="像素重力场";PlayerSettings.defaultScreenWidth=420;PlayerSettings.defaultScreenHeight=900;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Garden.unity",true)};if(System.IO.File.Exists("Assets/Scenes/Garden.unity"))return;System.IO.Directory.CreateDirectory("Assets/Scenes");var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Pixel Garden").AddComponent<PixelGarden>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Garden.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Garden.unity",true)};PlayerSettings.companyName="Little Garden";PlayerSettings.productName="像素重力场";PlayerSettings.defaultScreenWidth=420;PlayerSettings.defaultScreenHeight=900;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;AssetDatabase.SaveAssets();}
 [MenuItem("Garden/Build Desktop")] public static void Build(){EnsureScene();BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Garden.unity"},"Builds/PixelGarden/PixelGarden.x86_64",BuildTarget.StandaloneLinux64,BuildOptions.None);}
}
