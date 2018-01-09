using UnityEditor;
using UnityEngine;

namespace Game.NavMesh
{
    [CustomEditor(typeof(ChooseMap))]
    public class ChooseMapEditor : Editor
    {
        private ChooseMap _choosmMap;

        void OnEnable()
        {
            if (null == _choosmMap)
            {
                _choosmMap = (ChooseMap)target;
                _choosmMap.Init();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Load Map"))
            {
                _choosmMap.LoadMap();
            }
            GUILayout.EndVertical();
        }
    }
}
