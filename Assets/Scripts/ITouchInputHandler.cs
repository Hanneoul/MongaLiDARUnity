using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord, int lidarId); // ��ġ ����
        void OnTouchEnd(Vector2 screenCoord, int lidarId);   // ��ġ ����
    }
}
