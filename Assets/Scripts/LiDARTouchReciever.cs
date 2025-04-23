using UnityEngine;
using MongaLiDAR;

public class LiDARTouchReciever : MonoBehaviour, ITouchInputHandler
{
    // 터치 시작 이벤트
    public void OnTouchStart(Vector2 screenCoord)
    {
        Debug.Log("Touch started at: " + screenCoord);
        // 여기서 터치 시작 시의 행동을 정의
    }

    // 터치 종료 이벤트
    public void OnTouchEnd(Vector2 screenCoord)
    {
        Debug.Log("Touch ended at: " + screenCoord);
        // 여기서 터치 종료 시의 행동을 정의
    }
}