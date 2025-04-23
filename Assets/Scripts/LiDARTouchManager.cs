using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord, int lidarId); // 터치 시작
        void OnTouchEnd(Vector2 screenCoord, int lidarId);   // 터치 종료
    }




    public class LiDARManager : MonoBehaviour
    {
        // 카메라 객체 (하나의 카메라만 사용)
        public Camera lidarCamera;  // 카메라 객체 (하나만 사용)

        public int lidarId;           // 각 LiDAR의 고유 ID

        // LiDAR의 각도와 거리
        public float horizontalAngle = 0f;  // 수평 각도 (degree)
        public float verticalAngle = 0f;    // 수직 각도 (degree)
        public float lidarDistance = 10f;   // LiDAR의 거리 (meters)

        private LiDARTouchReceiver touchReceiver;  // LiDAR 리시버 참조

        private void Start()
        {
            // LiDARTouchReceiver가 제대로 연결되었는지 확인
            touchReceiver = LiDARTouchReceiver.Instance;
            if (touchReceiver == null)
            {
                Debug.LogError("LiDARTouchReceiver is not assigned!");
                return;
            }

            // LiDAR 연결 후 데이터 처리 (예시로 LiDAR 데이터를 처리)
            ProcessLiDARData(10f, 45f, 0f); // 예시: 10m 거리, 45도 각도에서 터치 이벤트 처리
        }

        // LiDAR 데이터 처리: 거리와 각도를 화면 좌표로 변환 후, 터치 이벤트 처리
        public void ProcessLiDARData(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR 데이터 (거리, 수평 각도, 수직 각도)를 화면 좌표로 변환
            Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(distance, horizontalAngle, verticalAngle);

            // LiDARTouchReceiver에 터치 시작 이벤트 전달
            touchReceiver.OnTouchStart(screenCoord, lidarId);

            // 예시: 2초 후 터치 종료
            Invoke("TriggerTouchEnd", 2f);  // 2초 후 터치 종료 처리
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // 예시 좌표
            touchReceiver.OnTouchEnd(screenCoord, lidarId);  // 터치 종료 이벤트 처리
        }

        // LiDAR 데이터(각도, 거리)를 3D 좌표로 변환 후, 화면 좌표로 변환
        private Vector2 ConvertLiDARDataToScreenCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR의 수평/수직 각도와 거리로 3D 좌표 계산
            Vector3 worldPosition = ConvertToWorldCoordinates(distance, horizontalAngle, verticalAngle);

            // 하나의 카메라를 사용하여 3D 좌표를 화면 좌표로 변환
            return ConvertToScreenCoordinates(worldPosition);
        }

        // LiDAR 데이터 (거리, 각도)를 3D 월드 좌표로 변환
        private Vector3 ConvertToWorldCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // 각도를 라디안으로 변환
            float radianHorizontal = horizontalAngle * Mathf.Deg2Rad;
            float radianVertical = verticalAngle * Mathf.Deg2Rad;

            // 삼각법을 이용하여 LiDAR 데이터를 3D 공간으로 변환
            float x = distance * Mathf.Cos(radianVertical) * Mathf.Sin(radianHorizontal);
            float y = distance * Mathf.Cos(radianVertical) * Mathf.Cos(radianHorizontal);
            float z = distance * Mathf.Sin(radianVertical);

            return new Vector3(x, y, z);
        }

        // 3D 좌표를 화면 좌표로 변환
        private Vector2 ConvertToScreenCoordinates(Vector3 worldPosition)
        {
            // 하나의 카메라를 사용하여 3D 좌표를 2D 화면 좌표로 변환
            Vector3 screenPosition = lidarCamera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
