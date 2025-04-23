using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord); // 터치 시작
        void OnTouchEnd(Vector2 screenCoord);   // 터치 끝
    }


    public class LiDARTouchManager : MonoBehaviour
    {
        private ITouchInputHandler touchInputHandler;  // 터치 이벤트를 처리할 핸들러

        private void Start()
        {
            // 터치 이벤트를 처리할 핸들러 (예: 터치 이벤트를 받을 컴포넌트)
            touchInputHandler = GetComponent<ITouchInputHandler>();
        }

        // LiDAR 데이터 (각도, 거리) -> 터치 좌표로 변환 후 이벤트 호출
        public void ProcessLiDARData(float distance, float angle)
        {
            Vector2 screenCoord = ConvertToScreenPosition(distance, angle);

            // 화면 좌표를 기반으로 터치 시작/끝 이벤트 호출
            touchInputHandler.OnTouchStart(screenCoord);

            // 예시: 2초 후 터치 종료
            Invoke("TriggerTouchEnd", 2f);  // 2초 후 OnTouchEnd 호출
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // 예시 좌표 (실제 처리 시 사용될 좌표로 바꿔야 합니다)
            touchInputHandler.OnTouchEnd(screenCoord);
        }

        // LiDAR 데이터 (각도, 거리)를 화면 좌표로 변환
        private Vector2 ConvertToScreenPosition(float distance, float angle)
        {
            Camera cam = Camera.main;
            float orthoSize = cam.orthographicSize;
            float aspectRatio = cam.aspect;

            // 간단한 삼각법을 사용하여 LiDAR에서 받은 각도와 거리를 2D 화면 좌표로 변환
            float x = distance * Mathf.Cos(angle);
            float y = distance * Mathf.Sin(angle);

            // 3D 좌표를 화면 좌표로 변환
            Vector3 worldPosition = new Vector3(x, y, 0);
            Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);

            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
