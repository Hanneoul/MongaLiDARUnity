using UnityEngine;
using MongaLiDAR;

public class LiDARTouchReciever : MonoBehaviour, ITouchInputHandler
{
    // ��ġ ���� �̺�Ʈ
    public void OnTouchStart(Vector2 screenCoord)
    {
        Debug.Log("Touch started at: " + screenCoord);
        // ���⼭ ��ġ ���� ���� �ൿ�� ����
    }

    // ��ġ ���� �̺�Ʈ
    public void OnTouchEnd(Vector2 screenCoord)
    {
        Debug.Log("Touch ended at: " + screenCoord);
        // ���⼭ ��ġ ���� ���� �ൿ�� ����
    }
}