using System.Collections.Generic;
using UnityEngine;

//사용법 
//Shift + c : 모드 on/off
//스페이스바 : 캘리브레이션 오브젝트 생성

public class LiDARTouchRecorder : MonoBehaviour
{
    public Camera mainCamera;        // 프로젝터 화면에 대응하는 카메라
    public GameObject spawnedObject;  // 화면에 띄울 오브젝트 프리팹

    
    //보정수치 
    public List<Vector2> lidarCalibPos;
    public List<float> lidarCalibAngle;
    public List<float> lidarCalibDistanceRatio;

    private List<Vector2> screenCoordinates = new List<Vector2>();  // 터치된 화면 좌표 리스트
    private List<Vector2> lidarData = new List<Vector2>();  // 라이다의 각도, 거리

    private int touchCount = 0;
    public int maxTouches = 5;  // 최대 5번 반복


    public int maxLidarSample = 300;  // 최대 5번 반복

    //캘리브레이션 기능 토글
    private bool isSpawningEnabled = false;

    private bool isCalibrating = false;
    private bool isRunning = false;


    void Update()
    {
        // Shift + C 를 눌러서 생성 모드 토글
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSpawningEnabled = !isSpawningEnabled;
            Debug.Log("Object Spawning: " + (isSpawningEnabled ? "Enabled" : "Disabled"));
        }

        // 생성 모드가 활성화된 경우에만 오브젝트 생성
        if (isSpawningEnabled && Input.GetKeyDown(KeyCode.Space))                               
        {
            Run();
        }
    }

    void SpawnObject()
    {
        // 화면의 좌표 범위 계산 (카메라의 orthographicSize 사용)
        float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float screenHeight = mainCamera.orthographicSize;

        // 화면 내에서 랜덤 위치 생성 (x, y 범위 내에서)
        float randomX = Random.Range(-screenWidth, screenWidth) * 0.7f;
        float randomY = Random.Range(-screenHeight, screenHeight) * 0.7f;

        Vector3 movePos = new Vector3(randomX, randomY, 0f);

        // 카메라의 nearClipPlane 보다 약간 멀리 생성
        Vector3 spawnPosition = new Vector3(0f, 0f, 10f);

        // 오브젝트 생성
        spawnedObject.SetActive(true);
        spawnedObject.transform.position = spawnPosition;

        // 오브젝트를 카메라를 향하게 회전 (수평 유지)
        spawnedObject.transform.rotation = mainCamera.transform.rotation;

        // 랜덤좌표 반영
        spawnedObject.transform.position += mainCamera.transform.rotation * movePos;

        // 카메라 좌표만큼 오브젝트를 이동시키기 (카메라와의 상대 위치 계산)
        spawnedObject.transform.position += mainCamera.transform.position;

        spawnedObject.transform.position += mainCamera.transform.forward * (mainCamera.nearClipPlane+0.05f);
    }

    public List<Touch> touchList = new List<Touch>();

    public class Touch
    {
        public string ObjID = "";
        public float distance = 0;
        public float angle = 0;
        public int count = 0;

        public Touch(long dist, long ang, string objId)
        {
            angle = (float)ang;
            distance = (float)dist;
            ObjID = objId;
            count = 0;
        }
    }

    void InitCount()
    {
        touchList.Clear();
    }

    void InputData(long distance, long angle, string objId)
    {
        if (isCalibrating)
        {
            bool isInList = false;
            for (int i = 0; i < touchList.Count; i++)
            {
                if (touchList[i].ObjID == objId)   //동일한 오브젝트라면
                {
                    touchList[i].distance += (float)distance;
                    touchList[i].angle += (float)angle;
                    touchList[i].count++;
                    isInList = true;
                }
                if (touchList[i].count > maxLidarSample)
                {
                    isCalibrating = false;
                    screenCoordinates.Add(new Vector2(spawnedObject.transform.position.x, spawnedObject.transform.position.y));
                    lidarData.Add(new Vector2(touchList[i].distance / (float)touchList[i].count, touchList[i].angle / (float)touchList[i].count));
                    float dist = lidarData[lidarData.Count - 1].x;
                    float ang = lidarData[lidarData.Count - 1].y;
                    spawnedObject.SetActive(false);//다 됐으면 끔
                    Debug.Log($"감지된 데이터 - 각도: {ang}, 거리: {dist}");

                    break;
                }
            }
            if (!isInList)
            {
                touchList.Add(new Touch(distance, angle, objId));
            }
        }
    }

    private float elapsedTime = 0f;

    void Run()
    {
        if (touchCount < maxTouches)
        {
            if (!isCalibrating)
            {
                

                // 누적된 시간이 3초 이상이면
                if (elapsedTime >= 3f)
                {
                    elapsedTime = 0f;
                    InitCount();
                    SpawnObject();
                    isCalibrating = true;
                }
                else
                { 
                    elapsedTime += Time.deltaTime; 
                }
            }            
        }
        else
        {
            spawnedObject.SetActive(false);//다 됐으면 끔
        }
    }












    // 라이다로 감지된 위치 기록
    public void RecordLidarData(float angle, float distance, Vector3 objectPosition)
    {
        lidarData.Add( new Vector2(angle, distance) );
        screenCoordinates.Add( mainCamera.WorldToScreenPoint(objectPosition) );
        
            
    }
    

    public Vector2 PredictTouchLocation(float lidarAngle, float lidarDistance)
    {
        // 라이다의 각도와 거리를 바탕으로 예측된 화면 좌표 계산
        // 예시: 단순한 변환을 통해 예측된 화면 좌표 반환 (실제 구현은 데이터 기반으로 계산)
        float screenX = lidarDistance * Mathf.Cos(lidarAngle * Mathf.Deg2Rad);
        float screenY = lidarDistance * Mathf.Sin(lidarAngle * Mathf.Deg2Rad);
        return new Vector2(screenX, screenY);
    }

   
    
}