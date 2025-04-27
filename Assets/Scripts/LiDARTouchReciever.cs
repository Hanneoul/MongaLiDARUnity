using MongaLiDAR;
using UnityEngine;

public class LiDARTouchReceiver : MonoBehaviour, ITouchInputHandler
{
    // 싱글턴 인스턴스
    public static LiDARTouchReceiver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 싱글턴 유지
        }
        else
        {
            Destroy(gameObject); // 이미 싱글턴이 존재하면 다른 오브젝트 삭제
        }
    }

    // 터치 시작 이벤트
    public void OnTouchStart(Vector2 screenCoord, int lidarId, string objID)
    {
        Debug.Log($"Touch started at: {screenCoord} from LiDAR {lidarId} and ObjID {objID}");
        // 터치 시작 시의 행동을 정의 (예: 오브젝트 클릭, UI 업데이트 등)
    }

    // 터치 종료 이벤트
    public void OnTouchEnd(Vector2 screenCoord, int lidarId, string objID)
    {
        Debug.Log($"Touch ended at: {screenCoord} from LiDAR {lidarId} and ObjID {objID}");
        // 터치 종료 시의 행동을 정의 (예: 오브젝트 상호작용 종료 등)
    }
}