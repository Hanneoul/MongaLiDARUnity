using UnityEngine;

namespace MongaLiDAR
{
    public interface ITouchInputHandler
    {
        void OnTouchStart(Vector2 screenCoord, int lidarId, string objID); // ��ġ ����
        void OnTouchEnd(Vector2 screenCoord, int lidarId, string objID);   // ��ġ ����
    }
}
