#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Subsistence.EditorTools
{
    [InitializeOnLoad]
    static class UrpFieldTestSetup
    {
        const string SettingsFolder = "Assets/Settings";
        const string RendererPath = SettingsFolder + "/Subsistence2DRenderer.asset";
        const string PipelinePath = SettingsFolder + "/SubsistenceURP.asset";

        static UrpFieldTestSetup() { EditorApplication.delayCall += EnsureUrpAssets; }

        static void EnsureUrpAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Type urpAssetType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            Type rendererDataType = Type.GetType("UnityEngine.Rendering.Universal.Renderer2DData, Unity.RenderPipelines.Universal.Runtime");
            if (urpAssetType == null || rendererDataType == null) return;
            string projectRoot=Directory.GetParent(Application.dataPath).FullName;
            string settingsFolderPath=Path.Combine(projectRoot,SettingsFolder);
            if(!Directory.Exists(settingsFolderPath))Directory.CreateDirectory(settingsFolderPath);
            AssetDatabase.Refresh();
            ScriptableObject rendererData=AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererPath);
            if(rendererData==null){rendererData=ScriptableObject.CreateInstance(rendererDataType);AssetDatabase.CreateAsset(rendererData,RendererPath);}
            ScriptableObject pipelineAsset=AssetDatabase.LoadAssetAtPath<ScriptableObject>(PipelinePath);
            if(pipelineAsset==null)
            {
                MethodInfo createMethod=urpAssetType.GetMethod("Create",new[]{typeof(ScriptableObject)});
                if(createMethod!=null){pipelineAsset=createMethod.Invoke(null,new object[]{rendererData}) as ScriptableObject;if(pipelineAsset!=null)AssetDatabase.CreateAsset(pipelineAsset,PipelinePath);}
            }
            if(pipelineAsset!=null)
            {
                SerializedObject so=new SerializedObject(pipelineAsset);
                if(so.FindProperty("m_SupportsHDR")!=null)so.FindProperty("m_SupportsHDR").boolValue=true;
                if(so.FindProperty("m_RenderScale")!=null)so.FindProperty("m_RenderScale").floatValue=1f;
                if(so.FindProperty("m_MSAA")!=null)so.FindProperty("m_MSAA").intValue=2;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pipelineAsset);
                GraphicsSettings.defaultRenderPipeline=(RenderPipelineAsset)pipelineAsset;
                QualitySettings.renderPipeline=(RenderPipelineAsset)pipelineAsset;
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
    }
}
#endif
