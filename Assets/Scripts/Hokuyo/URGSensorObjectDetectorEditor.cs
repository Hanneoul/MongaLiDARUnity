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
            DrawDefaultInspector(); // �⺻ Inspector UI �׸���

            URGSensorObjectDetector lidarManager = (URGSensorObjectDetector)target;

            // Generate ��ư �߰�
            if (GUILayout.Button("Save as JSON"))
            {
                lidarManager.SaveOptionFile();
            }

            // Clear Children ��ư �߰�
            if (GUILayout.Button("Load from JSON"))
            {
                lidarManager.LoadOptionFile();
            }
        }
    }
}
