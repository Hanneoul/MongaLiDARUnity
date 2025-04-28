using System.Collections.Generic;
using UnityEngine;

//���� 
//Shift + c : ��� on/off
//�����̽��� : Ķ���극�̼� ������Ʈ ����

public class LiDARTouchRecorder : MonoBehaviour
{
    public Camera mainCamera;        // �������� ȭ�鿡 �����ϴ� ī�޶�
    public GameObject spawnedObject;  // ȭ�鿡 ��� ������Ʈ ������

    
    //������ġ 
    public List<Vector2> lidarCalibPos;
    public List<float> lidarCalibAngle;
    public List<float> lidarCalibDistanceRatio;

    private List<Vector2> screenCoordinates = new List<Vector2>();  // ��ġ�� ȭ�� ��ǥ ����Ʈ
    private List<Vector2> lidarData = new List<Vector2>();  // ���̴��� ����, �Ÿ�

    private int touchCount = 0;
    public int maxTouches = 5;  // �ִ� 5�� �ݺ�


    public int maxLidarSample = 300;  // �ִ� 5�� �ݺ�

    //Ķ���극�̼� ��� ���
    private bool isSpawningEnabled = false;

    private bool isCalibrating = false;
    private bool isRunning = false;


    void Update()
    {
        // Shift + C �� ������ ���� ��� ���
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSpawningEnabled = !isSpawningEnabled;
            Debug.Log("Object Spawning: " + (isSpawningEnabled ? "Enabled" : "Disabled"));
        }

        // ���� ��尡 Ȱ��ȭ�� ��쿡�� ������Ʈ ����
        if (isSpawningEnabled && Input.GetKeyDown(KeyCode.Space))                               
        {
            Run();
        }
    }

    void SpawnObject()
    {
        // ȭ���� ��ǥ ���� ��� (ī�޶��� orthographicSize ���)
        float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float screenHeight = mainCamera.orthographicSize;

        // ȭ�� ������ ���� ��ġ ���� (x, y ���� ������)
        float randomX = Random.Range(-screenWidth, screenWidth) * 0.7f;
        float randomY = Random.Range(-screenHeight, screenHeight) * 0.7f;

        Vector3 movePos = new Vector3(randomX, randomY, 0f);

        // ī�޶��� nearClipPlane ���� �ణ �ָ� ����
        Vector3 spawnPosition = new Vector3(0f, 0f, 10f);

        // ������Ʈ ����
        spawnedObject.SetActive(true);
        spawnedObject.transform.position = spawnPosition;

        // ������Ʈ�� ī�޶� ���ϰ� ȸ�� (���� ����)
        spawnedObject.transform.rotation = mainCamera.transform.rotation;

        // ������ǥ �ݿ�
        spawnedObject.transform.position += mainCamera.transform.rotation * movePos;

        // ī�޶� ��ǥ��ŭ ������Ʈ�� �̵���Ű�� (ī�޶���� ��� ��ġ ���)
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
                if (touchList[i].ObjID == objId)   //������ ������Ʈ���
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
                    spawnedObject.SetActive(false);//�� ������ ��
                    Debug.Log($"������ ������ - ����: {ang}, �Ÿ�: {dist}");

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
                

                // ������ �ð��� 3�� �̻��̸�
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
            spawnedObject.SetActive(false);//�� ������ ��
        }
    }












    // ���̴ٷ� ������ ��ġ ���
    public void RecordLidarData(float angle, float distance, Vector3 objectPosition)
    {
        lidarData.Add( new Vector2(angle, distance) );
        screenCoordinates.Add( mainCamera.WorldToScreenPoint(objectPosition) );
        
            
    }
    

    public Vector2 PredictTouchLocation(float lidarAngle, float lidarDistance)
    {
        // ���̴��� ������ �Ÿ��� �������� ������ ȭ�� ��ǥ ���
        // ����: �ܼ��� ��ȯ�� ���� ������ ȭ�� ��ǥ ��ȯ (���� ������ ������ ������� ���)
        float screenX = lidarDistance * Mathf.Cos(lidarAngle * Mathf.Deg2Rad);
        float screenY = lidarDistance * Mathf.Sin(lidarAngle * Mathf.Deg2Rad);
        return new Vector2(screenX, screenY);
    }

   
    
}