using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LiDARAgentSys;
using SCIP_library;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static DataStructure;
//using static Unity.VisualScripting.Round<TInput, TOutput>;

namespace MongaLiDAR
{
   

    public class LiDARManager : MonoBehaviour
    {
        //LiDAR 정보
        public int lidarId = 0;           // 각 LiDAR의 고유 ID (사용자가 정함)
        public string lidarIP = "192.168.0.10";
        public int lidarPort = 10940;

        int startIndex = 200;                     //LiDAR 감지 시작 각도 (배열인덱스)
        private int endIndex = 400;               //LiDAR 감지 끝 각도 (배열인덱스)
        private int maxIndex = 1080;              //최대각
        public double scanAngle = 270.0;

        private bool isRunning = false;               // 라이다 동작여부

        // LiDAR가 설치된 각도


        //                 ^ Yaw
        //                 |
        //                 | / 
        //                 |/
        //    -------------+--------------> pitch
        //                /|
        //               / |
        //       Roll   V  

        // LiDAR 터치와 매핑에 필요한 오브젝트 
        public Camera lidarCamera;  // 카메라 객체 (하나만 사용)
        public LiDARTouchReceiver touchReceiver;  // LiDAR 리시버 참조

        
        //네트웍 전용
        public TcpClient client;
        public NetworkStream stream;

        private async void Start()
        {
            //// LiDAR 연결 후 데이터 처리 (예시로 LiDAR 데이터를 처리)
            //Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(10f, 45f, 0f); // 예시: 10m 거리, 45도 각도에서 터치 이벤트 처리


            // LiDARTouchReceiver가 제대로 연결되었는지 확인
            touchReceiver = LiDARTouchReceiver.Instance;
            if (touchReceiver == null)
            {
                Debug.LogError("LiDARTouchReceiver is not assigned!");
                return;
            }

            //범위 예외처리
            startIndex = startIndex >= 0 && startIndex <= maxIndex ? startIndex : 200;
            endIndex = endIndex >= startIndex && endIndex <= maxIndex ? endIndex : 400;

            Debug.Log($"설정된 인덱스 범위: " + startIndex.ToString() + " ~ " + endIndex.ToString());


            try
            {
                using (TcpClient urg = new TcpClient(lidarIP, lidarPort))
                using (NetworkStream stream = urg.GetStream())
                {
                    isRunning = true;
                    await WriteCommand(stream, SCIP_Writer.SCIP2());
                    string receiveData = await ReadLine(stream);


                    while (isRunning)
                    {
                        await WriteCommand(stream, SCIP_Writer.MD(0, maxIndex));
                        receiveData = await ReadLine(stream);

                        List<long> distances = new List<long>();
                        long unusedTimeStamp = 0;

                        receiveData = await ReadLine(stream);
                        if (string.IsNullOrEmpty(receiveData) || !SCIP_Reader.MD(receiveData, ref unusedTimeStamp, ref distances))
                        {
                            Debug.LogError("데이터 수신 실패 (패킷 오류 또는 데이터 없음)");
                            await Task.Delay(50);
                            continue;
                        }

                        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        Debug.Log($"[디버그] 데이터 수신됨, 거리값 개수: {distances.Count}");

                        if (distances.Count > 0)
                        {
                            int adjustedEndIndex = Math.Min(endIndex, distances.Count - 1);
                            List<LiDARMeasurement> measurements = new List<LiDARMeasurement>();

                            for (int i = startIndex; i <= adjustedEndIndex; i++)
                            {
                                double angle = (i / (double)maxIndex) * scanAngle;
                                measurements.Add(new LiDARMeasurement(angle, distances[i]));
                            }

                            Debug.Log($"[디버그] 원본 데이터 필터에 전달됨, 타임스탬프: {timeStamp}, 데이터 개수: {measurements.Count}");
                            DataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"오류 발생: {ex.Message}");
            }















            

            
        }

        private async Task WriteCommand(NetworkStream stream, string data)
        {
            if (!stream.CanWrite) return;
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        static async Task<string> ReadLine(NetworkStream stream)
        {
            if (!stream.CanRead) return null;

            StringBuilder sb = new StringBuilder();
            bool isNL2 = false;
            bool isNL = false;
            byte[] buffer = new byte[1];

            try
            {
                do
                {
                    int byteRead = await stream.ReadAsync(buffer, 0, 1);
                    if (byteRead == 0) break; // 연결이 끊어진 경우

                    char receivedChar = (char)buffer[0];

                    if (receivedChar == '\n')
                    {
                        if (isNL) isNL2 = true;
                        else isNL = true;
                    }
                    else isNL = false;

                    sb.Append(receivedChar);
                } while (!isNL2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"데이터 읽기 오류: {ex.Message}");
                return null;
            }

            return sb.ToString();
        }



















        // LiDAR 데이터 처리: 거리와 각도를 화면 좌표로 변환 후, 터치 이벤트 처리
        public void ProcessLiDARData(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR 데이터 (거리, 수평 각도, 수직 각도)를 화면 좌표로 변환
            Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(distance, horizontalAngle, verticalAngle);

            // LiDARTouchReceiver에 터치 시작 이벤트 전달
            touchReceiver.OnTouchStart(screenCoord, lidarId);

            // 예시: 2초 후 터치 종료
            Invoke("TriggerTouchEnd", 2f);  // 2초 후 터치 종료 처리
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // 예시 좌표
            touchReceiver.OnTouchEnd(screenCoord, lidarId);  // 터치 종료 이벤트 처리
        }

        // LiDAR 데이터(각도, 거리)를 3D 좌표로 변환 후, 화면 좌표로 변환
        private Vector2 ConvertLiDARDataToScreenCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR의 수평/수직 각도와 거리로 3D 좌표 계산
            Vector3 worldPosition = ConvertToWorldCoordinates(distance, horizontalAngle, verticalAngle);

            // 하나의 카메라를 사용하여 3D 좌표를 화면 좌표로 변환
            return ConvertToScreenCoordinates(worldPosition);
        }

        // LiDAR 데이터 (거리, 각도)를 3D 월드 좌표로 변환
        private Vector3 ConvertToWorldCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // 각도를 라디안으로 변환
            float radianHorizontal = horizontalAngle * Mathf.Deg2Rad;
            float radianVertical = verticalAngle * Mathf.Deg2Rad;

            // 삼각법을 이용하여 LiDAR 데이터를 3D 공간으로 변환
            float x = distance * Mathf.Cos(radianVertical) * Mathf.Sin(radianHorizontal);
            float y = distance * Mathf.Cos(radianVertical) * Mathf.Cos(radianHorizontal);
            float z = distance * Mathf.Sin(radianVertical);

            return new Vector3(x, y, z);
        }

        // 3D 좌표를 화면 좌표로 변환
        private Vector2 ConvertToScreenCoordinates(Vector3 worldPosition)
        {
            // 하나의 카메라를 사용하여 3D 좌표를 2D 화면 좌표로 변환
            Vector3 screenPosition = lidarCamera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
