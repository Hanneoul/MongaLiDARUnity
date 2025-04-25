using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SCIP_library;
using UnityEngine;
using System.Linq;

namespace MongaLiDAR
{

    public class LiDARTouchManager : MonoBehaviour
    {
        //LiDAR 정보
        public int lidarId = 0;           // 각 LiDAR의 고유 ID (사용자가 정함)
        public string lidarIP = "192.168.0.10";
        public int lidarPort = 10940;

        public int startIndex = 200;                     //LiDAR 감지 시작 각도 (배열인덱스)
        private int endIndex = 400;               //LiDAR 감지 끝 각도 (배열인덱스)
        private int maxIndex = 1080;              //최대각
        public double scanAngle = 270.0;

        public string settingFilename = "lidar_data01.json";
        public string filterFilename = "lidar_filter_data01.json";

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
        private TcpClient tcpClient;
        private NetworkStream stream;
        private CancellationTokenSource cancellationTokenSource;        //중지 토큰

        //데이터큐
        private DataQueue dataQueue;



        void Stop()
        {
            isRunning = false;
        }


        void Start()
        {
            LoadData(); // 데이터 불러오기

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

            // TCP 연결을 비동기로 시도
            StartConnectionAsync();
            isRunning = true;

            FilteringProcess();
        }
        private void Update()
        {
            // filteredData가 있는지 확인
            if (dataQueue.HasFilteredData())
            {
                // filteredData를 꺼내옴
                LiDARAgentData filteredData = dataQueue.DequeueFilteredData();

                // filteredData에 값이 있으면 LiDARTouchReceiver의 터치 이벤트 호출
                foreach (var measurement in filteredData.measurements)
                {
                    // 터치 시작 이벤트 호출
                    LiDARTouchReceiver.Instance.OnTouchStart(new Vector2((float)measurement.angle, (float)measurement.distance), lidarId);

                    // 터치 종료 이벤트 호출 (예시로 바로 종료 처리, 실제로는 상황에 맞게 변경)
                    //LiDARTouchReceiver.Instance.OnTouchEnd(new Vector2((float)measurement.angle, (float)measurement.distance), lidarId);
                }
            }
        }



        // JSON으로 저장하기
        public void SaveData()
        {
            dataQueue = new DataQueue(); // LiDARTouchManager는 자신만의 DataQueue를 가짐

            LiDARInitData lidarData = new LiDARInitData();

            lidarData.Lidar_id = lidarId;
            lidarData.lidar_ip = lidarIP;
            lidarData.lidar_port = lidarPort;

            lidarData.startIndex = startIndex;
            lidarData.scanAngle = scanAngle;

            string json = JsonUtility.ToJson(lidarData);
            System.IO.File.WriteAllText(Application.dataPath + "/" + settingFilename, json);
            Debug.Log("Data saved to JSON: " + json);
        }

        // JSON에서 데이터 불러오기
        public void LoadData()
        {
            if (System.IO.File.Exists(Application.dataPath + "/" + settingFilename))
            {
                LiDARInitData lidarData = new LiDARInitData();

                string json = System.IO.File.ReadAllText(Application.dataPath + "/" + settingFilename);
                lidarData = JsonUtility.FromJson<LiDARInitData>(json);
                Debug.Log("Data loaded from JSON: " + json);

                lidarId = lidarData.Lidar_id;
                lidarIP = lidarData.lidar_ip;
                lidarPort = lidarData.lidar_port;

                startIndex = lidarData.startIndex;
                scanAngle = lidarData.scanAngle;
            }
            else
            {
                Debug.Log("No saved data found!");
            }
        }

        // 비동기 방식으로 LiDAR에 접속 시도
        private async void StartConnectionAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            while (!token.IsCancellationRequested)
            {

                try
                {
                    if (await TryConnectAsync(token))
                    {
                        Debug.Log("LiDAR 접속 성공!");
                        await ReceiveDataLoopAsync(token);
                        break; // 연결 후 데이터 수신 루프 시작
                    }
                    else
                    {
                        Debug.Log("LiDAR 접속 실패, 5초 후 재시도...");
                        await Task.Delay(5000, token); // 접속 실패 시 5초 대기 후 재시도
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"오류 발생: {ex.Message}");
                    await Task.Delay(5000, token); // 오류 발생 시 재시도
                }
            }
        }

        // 비동기 TCP 접속 시도
        private async Task<bool> TryConnectAsync(CancellationToken token)
        {
            try
            {
                tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(lidarIP, lidarPort);
                if (await Task.WhenAny(connectTask, Task.Delay(10000, token)) == connectTask) // 10초 타임아웃
                {
                    if (tcpClient.Connected)
                    {
                        stream = tcpClient.GetStream();
                        return true;
                    }
                }
                Debug.LogError("접속 시간 초과");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"접속 오류: {ex.Message}");
                return false;
            }
            
        }


        // LiDAR 데이터 수신 루프
        private async Task ReceiveDataLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && isRunning)
                {
                    await WriteCommandAsync(SCIP_Writer.SCIP2());

                    string receiveData = await ReadLineAsync();
                    if (string.IsNullOrEmpty(receiveData))
                    {
                        Debug.LogError("데이터 수신 실패");
                        await Task.Delay(50, token); // 데이터 수신 실패 시 50ms 대기 후 재시도
                        continue;
                    }

                    List<long> distances = new List<long>();
                    long unusedTimeStamp = 0;

                    receiveData = await ReadLineAsync();
                    if (string.IsNullOrEmpty(receiveData) || !SCIP_Reader.MD(receiveData, ref unusedTimeStamp, ref distances))
                    {
                        Debug.LogError("데이터 패킷 오류");
                        await Task.Delay(50, token); // 패킷 오류 시 재시도
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
                        dataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"데이터 수신 오류: {ex.Message}");
            }
            finally
            {
                // 연결 종료 시 자원 해제
                CleanupConnection();
            }
        }

        // TCP 명령어를 비동기적으로 스트림에 쓰기
        private async Task WriteCommandAsync(string data)
        {
            if (stream?.CanWrite == true)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        // 네트워크 스트림에서 한 줄을 비동기적으로 읽기
        private async Task<string> ReadLineAsync()
        {
            if (stream?.CanRead == true)
            {
                StringBuilder sb = new StringBuilder();
                bool isNL2 = false;
                bool isNL = false;
                byte[] buffer = new byte[1];

                try
                {
                    do
                    {
                        int byteRead = await stream.ReadAsync(buffer, 0, 1);
                        if (byteRead == 0) break; // 연결 끊어짐

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
                    Debug.LogError($"데이터 읽기 오류: {ex.Message}");
                    return null;
                }

                return sb.ToString();
            }
            return null;
        }

        // 연결 종료 및 리소스 정리
        private void CleanupConnection()
        {
            stream?.Close();
            tcpClient?.Close();
            stream?.Dispose();
            tcpClient?.Dispose();
            isRunning = false;
        }

        // 애플리케이션 종료 시 연결 종료
        private void OnApplicationQuit()
        {
            cancellationTokenSource?.Cancel(); // 작업 취소
            CleanupConnection();
        }




        //-------------------여기까지 센서

        //-여기부터 필터--------------------------

        //필터링 수치
        public int filter_calibrationSamples = 100; // 환경 인식 샘플 개수 (100번 측정)
        public double filter_threshold = 80; // 변화 감지 임계값 (8cm = 80mm)

        private List<double> environmentBaseline = new List<double>(); // 환경 기준값 저장
        private List<List<double>> calibrationBuffer = new List<List<double>>(); // 환경 인식용 버퍼
        private int calibrationCount = 0; // 현재까지 수집된 샘플 개수
        private bool isCalibrated = false; // 환경 인식 완료 여부
        


        // JSON으로 저장하기
        public void SaveFilterData()
        {
            dataQueue = new DataQueue(); // LiDARTouchManager는 자신만의 DataQueue를 가짐

            LiDARFilterData lidarFilterData = new LiDARFilterData();

            lidarFilterData.filter_calibrationSamples = filter_calibrationSamples;
            lidarFilterData.filter_threshold = filter_threshold;
            lidarFilterData.environmentBaseline = environmentBaseline;
            lidarFilterData.calibrationBuffer = calibrationBuffer;
            lidarFilterData.calibrationCount = calibrationCount;


            string json = JsonUtility.ToJson(lidarFilterData);
            System.IO.File.WriteAllText(Application.dataPath + "/" + filterFilename, json);
            Debug.Log("Data saved to JSON: " + json);
        }

        // JSON에서 데이터 불러오기
        public bool LoadFilterData()
        {
            if (System.IO.File.Exists(Application.dataPath + "/" + filterFilename))
            {
                LiDARFilterData lidarFilterData = new LiDARFilterData();

                string json = System.IO.File.ReadAllText(Application.dataPath + "/" + filterFilename);
                lidarFilterData = JsonUtility.FromJson<LiDARFilterData>(json);
                Debug.Log("Calibration Filtering Data loaded from JSON: " + json);

                filter_calibrationSamples = lidarFilterData.filter_calibrationSamples;
                filter_threshold = lidarFilterData.filter_threshold;
                environmentBaseline = lidarFilterData.environmentBaseline;
                calibrationBuffer = lidarFilterData.calibrationBuffer;
                calibrationCount = lidarFilterData.calibrationCount;

                return true;                
            }
            else
            {
                Debug.Log("No Calibration Filtering Data found!");
                return false;
            }
        }

        public async void FilteringProcess()
        {
            while (isRunning)
            {
                try
                {
                    if (dataQueue.HasRawData())
                    {
                        LiDARAgentData rawData = dataQueue.DequeueRawData();
                        Debug.Log("필터 실행 중...");

                        if (!isCalibrated)
                        {
                            if(!LoadFilterData())
                            {
                                Debug.Log("Generating Calibration Filtering Dataset. Plz wait..");
                                CalibrateEnvironment(rawData.measurements);
                                SaveFilterData();                                
                            }                            
                            continue;
                        }
                      

                        List<LiDARMeasurement> filteredData = DetectChanges(rawData.measurements);

                        if (filteredData.Count > 0)
                        {
                            Debug.Log("필터링된 데이터:");
                            foreach (var data in filteredData)
                            {
                                Debug.Log($"각도: {data.angle:F2}도, 거리: {data.distance} mm");
                            }
                            dataQueue.EnqueueFilteredData(new LiDARAgentData(rawData.timestamp, filteredData));
                        }
                        else
                        {
                            Debug.Log("필터링된 데이터 없음.");
                        }
                    }
                    else
                    {
                        Debug.Log("필터 대기 중...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"필터 처리 중 오류 발생: {ex.Message}");
                }

                await Task.Delay(100); // 비동기 대기
            }
        }

        // 환경 인식 함수 (초기 filter_calibrationSamples번 샘플 수집 후 평균 계산)
        private void CalibrateEnvironment(List<LiDARMeasurement> measurements)
        {
            try
            {
                if (calibrationCount == 0)
                {
                    environmentBaseline = new List<double>(new double[measurements.Count]);
                    calibrationBuffer = new List<List<double>>(measurements.Count);

                    for (int i = 0; i < measurements.Count; i++)
                    {
                        calibrationBuffer.Add(new List<double>());
                    }
                }

                // 65533 값은 저장하지 않음
                for (int i = 0; i < measurements.Count; i++)
                {
                    if (measurements[i].distance != 65533) // 필터링된 값만 처리
                    {
                        calibrationBuffer[i].Add(measurements[i].distance);
                    }
                }

                calibrationCount++;

                // 100번 샘플 수집 후 환경 인식 완료
                if (calibrationCount >= filter_calibrationSamples)
                {
                    for (int i = 0; i < environmentBaseline.Count; i++)
                    {
                        if (calibrationBuffer[i].Count > 0)
                        {
                            environmentBaseline[i] = calibrationBuffer[i].Average();
                        }
                    }

                    isCalibrated = true;
                    Debug.Log("환경 인식 완료! (" + filter_calibrationSamples + "회 측정)");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"환경 인식 중 오류 발생: {ex.Message}");
            }
        }

        // 변화 감지 함수 (환경과 비교하여 변화된 값만 추출)
        private List<LiDARMeasurement> DetectChanges(List<LiDARMeasurement> measurements)
        {
            List<LiDARMeasurement> filteredData = new List<LiDARMeasurement>();

            try
            {
                for (int i = 0; i < measurements.Count; i++)
                {
                    if (i >= environmentBaseline.Count) continue;

                    // 탐지 불가능한 거리값(65533) 무시
                    if (measurements[i].distance == 65533) continue;

                    double difference = Math.Abs(measurements[i].distance - environmentBaseline[i]);

                    if (difference > filter_threshold) // threshold 이상 변화 감지
                    {
                        filteredData.Add(measurements[i]); // 변화 감지된 데이터만 저장
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"변화 감지 중 오류 발생: {ex.Message}");
            }

            return filteredData;
        }









        //------------------- 여기까지 필터

















        //private async void Start()
        //{
        //    //// LiDAR 연결 후 데이터 처리 (예시로 LiDAR 데이터를 처리)
        //    //Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(10f, 45f, 0f); // 예시: 10m 거리, 45도 각도에서 터치 이벤트 처리





        //    try
        //    {
        //        using (TcpClient urg = new TcpClient(lidarIP, lidarPort))
        //        using (NetworkStream stream = urg.GetStream())
        //        {
        //            isRunning = true;
        //            await WriteCommand(stream, SCIP_Writer.SCIP2());
        //            string receiveData = await ReadLine(stream);


        //            while (isRunning)
        //            {
        //                await WriteCommand(stream, SCIP_Writer.MD(0, maxIndex));
        //                receiveData = await ReadLine(stream);

        //                List<long> distances = new List<long>();
        //                long unusedTimeStamp = 0;

        //                receiveData = await ReadLine(stream);
        //                if (string.IsNullOrEmpty(receiveData) || !SCIP_Reader.MD(receiveData, ref unusedTimeStamp, ref distances))
        //                {
        //                    Debug.LogError("데이터 수신 실패 (패킷 오류 또는 데이터 없음)");
        //                    await Task.Delay(50);
        //                    continue;
        //                }

        //                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //                Debug.Log($"[디버그] 데이터 수신됨, 거리값 개수: {distances.Count}");

        //                if (distances.Count > 0)
        //                {
        //                    int adjustedEndIndex = Math.Min(endIndex, distances.Count - 1);
        //                    List<LiDARMeasurement> measurements = new List<LiDARMeasurement>();

        //                    for (int i = startIndex; i <= adjustedEndIndex; i++)
        //                    {
        //                        double angle = (i / (double)maxIndex) * scanAngle;
        //                        measurements.Add(new LiDARMeasurement(angle, distances[i]));
        //                    }

        //                    Debug.Log($"[디버그] 원본 데이터 필터에 전달됨, 타임스탬프: {timeStamp}, 데이터 개수: {measurements.Count}");
        //                    DataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError($"오류 발생: {ex.Message}");
        //    }




        //}

        //private async Task WriteCommand(NetworkStream stream, string data)
        //{
        //    if (!stream.CanWrite) return;
        //    byte[] buffer = Encoding.ASCII.GetBytes(data);
        //    await stream.WriteAsync(buffer, 0, buffer.Length);
        //}

        //static async Task<string> ReadLine(NetworkStream stream)
        //{
        //    if (!stream.CanRead) return null;

        //    StringBuilder sb = new StringBuilder();
        //    bool isNL2 = false;
        //    bool isNL = false;
        //    byte[] buffer = new byte[1];

        //    try
        //    {
        //        do
        //        {
        //            int byteRead = await stream.ReadAsync(buffer, 0, 1);
        //            if (byteRead == 0) break; // 연결이 끊어진 경우

        //            char receivedChar = (char)buffer[0];

        //            if (receivedChar == '\n')
        //            {
        //                if (isNL) isNL2 = true;
        //                else isNL = true;
        //            }
        //            else isNL = false;

        //            sb.Append(receivedChar);
        //        } while (!isNL2);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"데이터 읽기 오류: {ex.Message}");
        //        return null;
        //    }

        //    return sb.ToString();
        //}



















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
