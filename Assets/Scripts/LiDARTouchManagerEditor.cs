using UnityEditor;
using UnityEngine;

namespace MongaLiDAR
{

    [CustomEditor(typeof(LiDARTouchManager))]
    public class LiDARManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // �⺻ Inspector UI �׸���

            LiDARTouchManager lidarManager = (LiDARTouchManager)target;

            // Generate ��ư �߰�
            if (GUILayout.Button("Save as JSON"))
            {
                lidarManager.SaveData();
            }

            // Clear Children ��ư �߰�
            if (GUILayout.Button("Load from JSON"))
            {
                lidarManager.LoadData();
            }
        }
    }
}
