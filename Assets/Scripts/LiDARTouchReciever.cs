using MongaLiDAR;
using UnityEngine;

public class LiDARTouchReceiver : MonoBehaviour, ITouchInputHandler
{
    // �̱��� �ν��Ͻ�
    public static LiDARTouchReceiver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� �̱��� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �̱����� �����ϸ� �ٸ� ������Ʈ ����
        }
    }

    // ��ġ ���� �̺�Ʈ
    public void OnTouchStart(Vector2 screenCoord, int lidarId, string objID)
    {
        Debug.Log($"Touch started at: {screenCoord} from LiDAR {lidarId} and ObjID {objID}");
        // ��ġ ���� ���� �ൿ�� ���� (��: ������Ʈ Ŭ��, UI ������Ʈ ��)
    }

    // ��ġ ���� �̺�Ʈ
    public void OnTouchEnd(Vector2 screenCoord, int lidarId, string objID)
    {
        Debug.Log($"Touch ended at: {screenCoord} from LiDAR {lidarId} and ObjID {objID}");
        // ��ġ ���� ���� �ൿ�� ���� (��: ������Ʈ ��ȣ�ۿ� ���� ��)
    }
}