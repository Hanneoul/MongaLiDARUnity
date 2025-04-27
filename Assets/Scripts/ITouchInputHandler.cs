using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord, int lidarId, string objID); // 터치 시작
        void OnTouchEnd(Vector2 screenCoord, int lidarId, string objID);   // 터치 종료
    }
}
