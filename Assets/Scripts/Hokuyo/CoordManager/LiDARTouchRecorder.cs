using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LiDARTouchRecorder : MonoBehaviour
{

    public GameObject objectPrefab;  // ȭ�鿡 ��� ������Ʈ ������
    public Camera mainCamera;        // �������� ȭ�鿡 �����ϴ� ī�޶�

    public Vector2 lidarCalibPos;
    public float lidarCalibAngle;
    public float lidarCalibDistance;

    private List<Vector2> screenCoordinates = new List<Vector2>();  // ��ġ�� ȭ�� ��ǥ ����Ʈ
    private List<Vector2> lidarData = new List<Vector2>();  // ���̴��� ����, �Ÿ�

    // ������Ʈ�� Ư�� ��ġ�� ���� �Լ�
    public void SpawnObject(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));  // Z���� ī�޶���� �Ÿ�
        Instantiate(objectPrefab, worldPos, Quaternion.identity);
    }



    // ���̴ٷ� ������ ��ġ ���
    public void RecordLidarData(float angle, float distance, Vector3 objectPosition)
    {
        lidarData.Add( new Vector2(angle, distance) );
        screenCoordinates.Add( mainCamera.WorldToScreenPoint(objectPosition) );
        
        Debug.Log($"������ ������ - ����: {angle}, �Ÿ�: {distance}");        
    }
    

    public Vector2 PredictTouchLocation(float lidarAngle, float lidarDistance)
    {
        // ���̴��� ������ �Ÿ��� �������� ������ ȭ�� ��ǥ ���
        // ����: �ܼ��� ��ȯ�� ���� ������ ȭ�� ��ǥ ��ȯ (���� ������ ������ ������� ���)
        float screenX = lidarDistance * Mathf.Cos(lidarAngle * Mathf.Deg2Rad) + lidarCalibPos.x;
        float screenY = lidarDistance * Mathf.Sin(lidarAngle * Mathf.Deg2Rad) + lidarCalibPos.y;

        return new Vector2(screenX, screenY);
    }







    private int touchCount = 0;
    public int maxTouches = 5;  // �ִ� 5�� �ݺ�
    private CoordinateStorage coordinateStorage;
    private LiDARTouchRecorder lidarRecorder;

    void Start()
    {
        coordinateStorage = GetComponent<CoordinateStorage>();
        lidarRecorder = GetComponent<LiDARTouchRecorder>();
    }

      
    

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);  // ù ��° ��ġ �̺�Ʈ
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;  // ȭ����� ��ġ ��ǥ
                touchCoordinates.Add(touchPosition);
                Debug.Log($"��ġ ��ġ: {touchPosition}");

                // ������Ʈ ���� (�̰� ��ġ ��ǥ�� �������� ȭ�鿡 ������Ʈ�� ���� ����)
                // ObjectSpawner.SpawnObject(touchPosition); // �̹� ObjectSpawner�� �ִٰ� ����
            }
        }
    }
}