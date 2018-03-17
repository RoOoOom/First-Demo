using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombineMeshTestor))]
public class CombineTestorEditor : Editor {
    CombineMeshTestor m_cmt;
    void OnEnable()
    {
        m_cmt = (CombineMeshTestor)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("CreateCombineMeshObj"))
        {
            m_cmt.CombineMeshDysn();
            AssetDatabase.Refresh();
        }

        if(GUILayout.Button("CreateMeshObjOnly"))
        {
            m_cmt.CreateMeshOnly();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/批量移除Animator")]
    public static void RemoveAnimators()
    {
        GameObject go = Selection.activeObject as GameObject;
        if (go == null)
            return;
        int count = 0;
        foreach (Transform tran in go.GetComponentsInChildren<Transform>())
        {
            if (tran.GetComponent<Animator>() != null)
            {
                DestroyImmediate(tran.GetComponent<Animator>());
                count++;
            }
        }

        Debug.Log("移除的Animator组件数量:" + count);
    }
}
