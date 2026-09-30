#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Subsistence.EditorTools
{
    /// <summary>Creates a saved launch scene and adds it to the build list on the first project open.</summary>
    [InitializeOnLoad]
    static class FieldTestProjectSetup
    {
        const string ScenePath = "Assets/Scenes/FieldTest.unity";
        static FieldTestProjectSetup() { EditorApplication.delayCall += EnsureLaunchScene; }

        static void EnsureLaunchScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            PlayerSettings.companyName="Subsistence Studio";
            PlayerSettings.productName="SUBSISTENCE";
            PlayerSettings.bundleVersion="alpha-1.1.0";
            PlayerSettings.defaultScreenWidth=1280;
            PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.runInBackground=true;
            string projectRoot=Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string sceneFolder=Path.Combine(projectRoot,"Assets","Scenes");
            string sceneFile=Path.Combine(projectRoot,ScenePath);
            if (!Directory.Exists(sceneFolder)) Directory.CreateDirectory(sceneFolder);
            if (!File.Exists(sceneFile))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes) if (scene.path == ScenePath) return;
            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++) updated[i] = scenes[i];
            updated[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
