using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LiDARTouchRecorder : MonoBehaviour
{

    public GameObject objectPrefab;  // 화면에 띄울 오브젝트 프리팹
    public Camera mainCamera;        // 프로젝터 화면에 대응하는 카메라

    public Vector2 lidarCalibPos;
    public float lidarCalibAngle;
    public float lidarCalibDistance;

    private List<Vector2> screenCoordinates = new List<Vector2>();  // 터치된 화면 좌표 리스트
    private List<Vector2> lidarData = new List<Vector2>();  // 라이다의 각도, 거리

    // 오브젝트를 특정 위치에 띄우는 함수
    public void SpawnObject(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));  // Z값은 카메라와의 거리
        Instantiate(objectPrefab, worldPos, Quaternion.identity);
    }



    // 라이다로 감지된 위치 기록
    public void RecordLidarData(float angle, float distance, Vector3 objectPosition)
    {
        lidarData.Add( new Vector2(angle, distance) );
        screenCoordinates.Add( mainCamera.WorldToScreenPoint(objectPosition) );
        
        Debug.Log($"감지된 데이터 - 각도: {angle}, 거리: {distance}");        
    }
    

    public Vector2 PredictTouchLocation(float lidarAngle, float lidarDistance)
    {
        // 라이다의 각도와 거리를 바탕으로 예측된 화면 좌표 계산
        // 예시: 단순한 변환을 통해 예측된 화면 좌표 반환 (실제 구현은 데이터 기반으로 계산)
        float screenX = lidarDistance * Mathf.Cos(lidarAngle * Mathf.Deg2Rad) + lidarCalibPos.x;
        float screenY = lidarDistance * Mathf.Sin(lidarAngle * Mathf.Deg2Rad) + lidarCalibPos.y;

        return new Vector2(screenX, screenY);
    }







    private int touchCount = 0;
    public int maxTouches = 5;  // 최대 5번 반복
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
            Touch touch = Input.GetTouch(0);  // 첫 번째 터치 이벤트
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;  // 화면상의 터치 좌표
                touchCoordinates.Add(touchPosition);
                Debug.Log($"터치 위치: {touchPosition}");

                // 오브젝트 띄우기 (이건 터치 좌표를 기준으로 화면에 오브젝트를 띄우는 예시)
                // ObjectSpawner.SpawnObject(touchPosition); // 이미 ObjectSpawner가 있다고 가정
            }
        }
    }
}