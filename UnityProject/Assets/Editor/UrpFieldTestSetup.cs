#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Subsistence.EditorTools
{
    /// <summary>
    /// Safe one-click URP 2D setup. The previous [InitializeOnLoad] auto-run hook was removed because it
    /// caused Unity to patch immutable package assets (Packages/.../UniversalRendererData.asset) when
    /// Renderer2DData was created incomplete on editor load. Now setup is triggered explicitly via
    /// the Tools menu; assets live only under Assets/Settings and are only assigned to Graphics /
    /// Quality settings after the user confirms.
    /// </summary>
    static class UrpFieldTestSetup
    {
        const string SettingsFolder = "Assets/Settings";
        const string RendererPath = SettingsFolder + "/Subsistence2DRenderer.asset";
        const string PipelinePath = SettingsFolder + "/SubsistenceURP.asset";

        [MenuItem("Tools/Subsistence/Setup URP 2D Pipeline", false, 1000)]
        static void RunSetup()
        {
            EnsureUrpAssets();
        }

        [MenuItem("Tools/Subsistence/Setup URP 2D Pipeline", true)]
        static bool ValidateSetup()
        {
            Type urpAssetType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            Type rendererDataType = Type.GetType("UnityEngine.Rendering.Universal.Renderer2DData, Unity.RenderPipelines.Universal.Runtime");
            return urpAssetType != null && rendererDataType != null;
        }

        static void EnsureUrpAssets()
        {
            Type urpAssetType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            Type rendererDataType = Type.GetType("UnityEngine.Rendering.Universal.Renderer2DData, Unity.RenderPipelines.Universal.Runtime");
            if (urpAssetType == null || rendererDataType == null)
            {
                EditorUtility.DisplayDialog("URP not installed",
                    "Universal Render Pipeline package is not present. Install com.unity.render-pipelines.universal 14.x via Package Manager first.",
                    "OK");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string settingsFolderPath = Path.Combine(projectRoot, SettingsFolder);
            if (!Directory.Exists(settingsFolderPath)) Directory.CreateDirectory(settingsFolderPath);

            AssetDatabase.Refresh();

            ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererDataType);
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            ScriptableObject pipelineAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(PipelinePath);
            if (pipelineAsset == null)
            {
                MethodInfo createMethod = urpAssetType.GetMethod("Create", new[] { typeof(ScriptableObject) });
                if (createMethod != null)
                {
                    pipelineAsset = createMethod.Invoke(null, new object[] { rendererData }) as ScriptableObject;
                    if (pipelineAsset != null) AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
                }
            }

            if (pipelineAsset != null)
            {
                SerializedObject so = new SerializedObject(pipelineAsset);
                if (so.FindProperty("m_SupportsHDR") != null) so.FindProperty("m_SupportsHDR").boolValue = true;
                if (so.FindProperty("m_RenderScale") != null) so.FindProperty("m_RenderScale").floatValue = 1f;
                if (so.FindProperty("m_MSAA") != null) so.FindProperty("m_MSAA").intValue = 2;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pipelineAsset);
                EditorUtility.SetDirty(rendererData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            bool alreadyAssigned = GraphicsSettings.defaultRenderPipeline != null
                && AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline) == PipelinePath;

            if (!alreadyAssigned && EditorUtility.DisplayDialog(
                "Apply URP 2D pipeline?",
                "Assets/Settings/SubsistenceURP.asset is ready. Assign it as the project render pipeline in Graphics & Quality settings now?\n\n(You can also assign it manually in Edit > Project Settings > Graphics.)",
                "Assign now", "I'll assign it manually"))
            {
                var rpAsset = (RenderPipelineAsset)pipelineAsset;
                GraphicsSettings.defaultRenderPipeline = rpAsset;
                QualitySettings.renderPipeline = rpAsset;
                EditorUtility.SetDirty(pipelineAsset);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = pipelineAsset;
        }
    }
}
#endif
