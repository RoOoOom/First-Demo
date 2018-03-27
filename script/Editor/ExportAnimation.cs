using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ExportAnimation {  

    [MenuItem("Tools/Export Animation Clip")]
    static void ExportClip()
    {
        foreach ( Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets) )
        {
            if (!Directory.Exists(DirectoryPath()))
            {
                Directory.CreateDirectory(DirectoryPath());
                AssetDatabase.Refresh();
            }

            string filePath = AssetDatabase.GetAssetPath(obj);
            Debug.Log(filePath);
            Object[] objArray = AssetDatabase.LoadAllAssetsAtPath(filePath);
            Debug.Log(objArray.Length);

            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i].GetType() == typeof(AnimationClip))
                {
                    Debug.Log(objArray[i].name);
                    AnimationClip srcClip = objArray[i] as AnimationClip;
                    AnimationClip newClip = new AnimationClip();
                    newClip.name = srcClip.name;

                    string animPath = "Assets/Copy Animation/" + newClip.name + ".anim";
                    AnimationClipSettings setting = AnimationUtility.GetAnimationClipSettings(srcClip);
                    AnimationUtility.SetAnimationClipSettings(newClip, setting);
                    newClip.frameRate = srcClip.frameRate;//设置帧率

                    EditorCurveBinding[] curveArray = AnimationUtility.GetCurveBindings(srcClip);

                    for (int j = 0; j < curveArray.Length; j++)
                    {
                        AnimationUtility.SetEditorCurve(newClip, curveArray[j], AnimationUtility.GetEditorCurve(srcClip, curveArray[j]));
                    }
                    Debug.Log(animPath);
                    AssetDatabase.CreateAsset(newClip, animPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }                              
        }
    }

    static private string DirectoryPath()
    {
        return Application.dataPath + "/Copy Animation";
    }
}
