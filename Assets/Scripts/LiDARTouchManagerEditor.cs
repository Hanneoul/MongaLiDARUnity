using UnityEditor;
using UnityEngine;

namespace MongaLiDAR
{

    [CustomEditor(typeof(LiDARTouchManager))]
    public class LiDARManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // 기본 Inspector UI 그리기

            LiDARTouchManager lidarManager = (LiDARTouchManager)target;

            // Generate 버튼 추가
            if (GUILayout.Button("Save as JSON"))
            {
                lidarManager.SaveData();
            }

            // Clear Children 버튼 추가
            if (GUILayout.Button("Load from JSON"))
            {
                lidarManager.LoadData();
            }
        }
    }
}
