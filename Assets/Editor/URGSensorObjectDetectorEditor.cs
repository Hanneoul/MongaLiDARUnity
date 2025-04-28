using HKY;
using UnityEditor;
using UnityEngine;

namespace HKY
{

    [CustomEditor(typeof(URGSensorObjectDetector))]
    public class URGSensorObjectDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // 기본 Inspector UI 그리기

            URGSensorObjectDetector lidarManager = (URGSensorObjectDetector)target;

            // Generate 버튼 추가
            if (GUILayout.Button("Save as JSON"))
            {
                lidarManager.SaveOptionFile();
            }

            // Clear Children 버튼 추가
            if (GUILayout.Button("Load from JSON"))
            {
                lidarManager.LoadOptionFile();
            }
        }
    }
}
