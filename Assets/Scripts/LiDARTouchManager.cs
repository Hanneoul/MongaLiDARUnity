using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord); // ��ġ ����
        void OnTouchEnd(Vector2 screenCoord);   // ��ġ ��
    }


    public class LiDARTouchManager : MonoBehaviour
    {
        private ITouchInputHandler touchInputHandler;  // ��ġ �̺�Ʈ�� ó���� �ڵ鷯

        private void Start()
        {
            // ��ġ �̺�Ʈ�� ó���� �ڵ鷯 (��: ��ġ �̺�Ʈ�� ���� ������Ʈ)
            touchInputHandler = GetComponent<ITouchInputHandler>();
        }

        // LiDAR ������ (����, �Ÿ�) -> ��ġ ��ǥ�� ��ȯ �� �̺�Ʈ ȣ��
        public void ProcessLiDARData(float distance, float angle)
        {
            Vector2 screenCoord = ConvertToScreenPosition(distance, angle);

            // ȭ�� ��ǥ�� ������� ��ġ ����/�� �̺�Ʈ ȣ��
            touchInputHandler.OnTouchStart(screenCoord);

            // ����: 2�� �� ��ġ ����
            Invoke("TriggerTouchEnd", 2f);  // 2�� �� OnTouchEnd ȣ��
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // ���� ��ǥ (���� ó�� �� ���� ��ǥ�� �ٲ�� �մϴ�)
            touchInputHandler.OnTouchEnd(screenCoord);
        }

        // LiDAR ������ (����, �Ÿ�)�� ȭ�� ��ǥ�� ��ȯ
        private Vector2 ConvertToScreenPosition(float distance, float angle)
        {
            Camera cam = Camera.main;
            float orthoSize = cam.orthographicSize;
            float aspectRatio = cam.aspect;

            // ������ �ﰢ���� ����Ͽ� LiDAR���� ���� ������ �Ÿ��� 2D ȭ�� ��ǥ�� ��ȯ
            float x = distance * Mathf.Cos(angle);
            float y = distance * Mathf.Sin(angle);

            // 3D ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            Vector3 worldPosition = new Vector3(x, y, 0);
            Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);

            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
