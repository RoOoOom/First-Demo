using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportSceneAsset : Editor {
    
    [MenuItem("Tools/ExportAsset")]
    public static void ExportAsset()
    {
        
        Object[] objs = Selection.GetFiltered(typeof(Object),SelectionMode.DeepAssets);
        foreach (Object obj in objs)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            objPath = objPath.Replace(Application.dataPath, "Assets");
            string savePath = Application.dataPath + "/StreamAsset/" + obj.name  + ".unity3d";
            string[] levels = new string[] {objPath};
            BuildPipeline.BuildPlayer(levels, savePath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);
            AssetDatabase.Refresh();
        }
    }
}
