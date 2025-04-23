using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord, int lidarId); // ��ġ ����
        void OnTouchEnd(Vector2 screenCoord, int lidarId);   // ��ġ ����
    }




    public class LiDARManager : MonoBehaviour
    {
        // ī�޶� ��ü (�ϳ��� ī�޶� ���)
        public Camera lidarCamera;  // ī�޶� ��ü (�ϳ��� ���)

        public int lidarId;           // �� LiDAR�� ���� ID

        // LiDAR�� ������ �Ÿ�
        public float horizontalAngle = 0f;  // ���� ���� (degree)
        public float verticalAngle = 0f;    // ���� ���� (degree)
        public float lidarDistance = 10f;   // LiDAR�� �Ÿ� (meters)

        private LiDARTouchReceiver touchReceiver;  // LiDAR ���ù� ����

        private void Start()
        {
            // LiDARTouchReceiver�� ����� ����Ǿ����� Ȯ��
            touchReceiver = LiDARTouchReceiver.Instance;
            if (touchReceiver == null)
            {
                Debug.LogError("LiDARTouchReceiver is not assigned!");
                return;
            }

            // LiDAR ���� �� ������ ó�� (���÷� LiDAR �����͸� ó��)
            ProcessLiDARData(10f, 45f, 0f); // ����: 10m �Ÿ�, 45�� �������� ��ġ �̺�Ʈ ó��
        }

        // LiDAR ������ ó��: �Ÿ��� ������ ȭ�� ��ǥ�� ��ȯ ��, ��ġ �̺�Ʈ ó��
        public void ProcessLiDARData(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR ������ (�Ÿ�, ���� ����, ���� ����)�� ȭ�� ��ǥ�� ��ȯ
            Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(distance, horizontalAngle, verticalAngle);

            // LiDARTouchReceiver�� ��ġ ���� �̺�Ʈ ����
            touchReceiver.OnTouchStart(screenCoord, lidarId);

            // ����: 2�� �� ��ġ ����
            Invoke("TriggerTouchEnd", 2f);  // 2�� �� ��ġ ���� ó��
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // ���� ��ǥ
            touchReceiver.OnTouchEnd(screenCoord, lidarId);  // ��ġ ���� �̺�Ʈ ó��
        }

        // LiDAR ������(����, �Ÿ�)�� 3D ��ǥ�� ��ȯ ��, ȭ�� ��ǥ�� ��ȯ
        private Vector2 ConvertLiDARDataToScreenCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR�� ����/���� ������ �Ÿ��� 3D ��ǥ ���
            Vector3 worldPosition = ConvertToWorldCoordinates(distance, horizontalAngle, verticalAngle);

            // �ϳ��� ī�޶� ����Ͽ� 3D ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            return ConvertToScreenCoordinates(worldPosition);
        }

        // LiDAR ������ (�Ÿ�, ����)�� 3D ���� ��ǥ�� ��ȯ
        private Vector3 ConvertToWorldCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // ������ �������� ��ȯ
            float radianHorizontal = horizontalAngle * Mathf.Deg2Rad;
            float radianVertical = verticalAngle * Mathf.Deg2Rad;

            // �ﰢ���� �̿��Ͽ� LiDAR �����͸� 3D �������� ��ȯ
            float x = distance * Mathf.Cos(radianVertical) * Mathf.Sin(radianHorizontal);
            float y = distance * Mathf.Cos(radianVertical) * Mathf.Cos(radianHorizontal);
            float z = distance * Mathf.Sin(radianVertical);

            return new Vector3(x, y, z);
        }

        // 3D ��ǥ�� ȭ�� ��ǥ�� ��ȯ
        private Vector2 ConvertToScreenCoordinates(Vector3 worldPosition)
        {
            // �ϳ��� ī�޶� ����Ͽ� 3D ��ǥ�� 2D ȭ�� ��ǥ�� ��ȯ
            Vector3 screenPosition = lidarCamera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
