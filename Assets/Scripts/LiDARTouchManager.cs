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
        // LiDAR ��ġ�� ���ο� �ʿ��� ������Ʈ 
        public Camera lidarCamera;  // ī�޶� ��ü (�ϳ��� ���)
        public ITouchInputHandler touchReceiver;  // LiDAR ���ù� ����


        public string settingFilename = "lidar_data01.json";
        //LiDAR ����
        public int lidarId = 0;           // �� LiDAR�� ���� ID (����ڰ� ����)
        public string lidarIP = "192.168.0.10";
        public int lidarPort = 10940;

        public int startIndex = 200;                     //LiDAR ���� ���� ���� (�迭�ε���)
        private int endIndex = 400;               //LiDAR ���� �� ���� (�迭�ε���)
        private int maxIndex = 1080;              //�ִ밢
        public double scanAngle = 270.0;

        
        

        private bool isRunning = false;               // ���̴� ���ۿ���

        // LiDAR�� ��ġ�� ����


        //                 ^ Yaw
        //                 |
        //                 | / 
        //                 |/
        //    -------------+--------------> pitch
        //                /|
        //               / |
        //       Roll   V  

        
        //��Ʈ�� ����
        private TcpClient tcpClient;
        private NetworkStream stream;
        private CancellationTokenSource cancellationTokenSource;        //���� ��ū

        //������ť
        private DataQueue dataQueue;

        public string filterFilename = "lidar_filter_data01.json";

        void Stop()
        {
            isRunning = false;
        }


        void Start()
        {
            dataQueue = new DataQueue(); // LiDARTouchManager�� �ڽŸ��� DataQueue�� ����

            LoadData(); // ������ �ҷ�����

            // LiDARTouchReceiver�� ����� ����Ǿ����� Ȯ��
            touchReceiver = LiDARTouchReceiver.Instance;
            if (touchReceiver == null)
            {
                Debug.LogError("LiDARTouchReceiver is not assigned!");
                return;
            }

            //���� ����ó��
            startIndex = startIndex >= 0 && startIndex <= maxIndex ? startIndex : 200;
            endIndex = endIndex >= startIndex && endIndex <= maxIndex ? endIndex : 400;

            Debug.Log($"������ �ε��� ����: " + startIndex.ToString() + " ~ " + endIndex.ToString());

            // TCP ������ �񵿱�� �õ�
            StartConnectionAsync();
            isRunning = true;

            FilteringProcess();
        }
        private void Update()
        {
            // filteredData�� �ִ��� Ȯ��
            if (dataQueue.HasFilteredData())
            {
                // filteredData�� ������
                LiDARAgentData filteredData = dataQueue.DequeueFilteredData();

                // filteredData�� ���� ������ LiDARTouchReceiver�� ��ġ �̺�Ʈ ȣ��
                foreach (var measurement in filteredData.measurements)
                {
                    // ��ġ ���� �̺�Ʈ ȣ��
                    LiDARTouchReceiver.Instance.OnTouchStart(new Vector2((float)measurement.angle, (float)measurement.distance), lidarId);

                    // ��ġ ���� �̺�Ʈ ȣ�� (���÷� �ٷ� ���� ó��, �����δ� ��Ȳ�� �°� ����)
                    //LiDARTouchReceiver.Instance.OnTouchEnd(new Vector2((float)measurement.angle, (float)measurement.distance), lidarId);
                }
            }
        }



        // JSON���� �����ϱ�
        public void SaveData()
        {
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

        // JSON���� ������ �ҷ�����
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

        // �񵿱� ������� LiDAR�� ���� �õ�
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
                        Debug.Log("LiDAR ���� ����!");
                        await ReceiveDataLoopAsync(token);
                        break; // ���� �� ������ ���� ���� ����
                    }
                    else
                    {
                        Debug.Log("LiDAR ���� ����, 5�� �� ��õ�...");
                        await Task.Delay(5000, token); // ���� ���� �� 5�� ��� �� ��õ�
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"���� �߻�: {ex.Message}");
                    await Task.Delay(5000, token); // ���� �߻� �� ��õ�
                }
            }
        }

        // �񵿱� TCP ���� �õ�
        private async Task<bool> TryConnectAsync(CancellationToken token)
        {
            try
            {
                tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(lidarIP, lidarPort);
                if (await Task.WhenAny(connectTask, Task.Delay(10000, token)) == connectTask) // 10�� Ÿ�Ӿƿ�
                {
                    if (tcpClient.Connected)
                    {
                        stream = tcpClient.GetStream();
                        return true;
                    }
                }
                Debug.LogError("���� �ð� �ʰ�");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"���� ����: {ex.Message}");
                return false;
            }
            
        }


        // LiDAR ������ ���� ����
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
                        Debug.LogError("������ ���� ����");
                        await Task.Delay(50, token); // ������ ���� ���� �� 50ms ��� �� ��õ�
                        continue;
                    }

                    List<long> distances = new List<long>();
                    long unusedTimeStamp = 0;

                    receiveData = await ReadLineAsync();
                    if (string.IsNullOrEmpty(receiveData) || !SCIP_Reader.MD(receiveData, ref unusedTimeStamp, ref distances))
                    {
                        Debug.LogError("������ ��Ŷ ����");
                        await Task.Delay(50, token); // ��Ŷ ���� �� ��õ�
                        continue;
                    }

                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    Debug.Log($"[�����] ������ ���ŵ�, �Ÿ��� ����: {distances.Count}");

                    if (distances.Count > 0)
                    {
                        int adjustedEndIndex = Math.Min(endIndex, distances.Count - 1);
                        List<LiDARMeasurement> measurements = new List<LiDARMeasurement>();

                        for (int i = startIndex; i <= adjustedEndIndex; i++)
                        {
                            double angle = (i / (double)maxIndex) * scanAngle;
                            measurements.Add(new LiDARMeasurement(angle, distances[i]));
                        }

                        Debug.Log($"[�����] ���� ������ ���Ϳ� ���޵�, Ÿ�ӽ�����: {timeStamp}, ������ ����: {measurements.Count}");
                        dataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"������ ���� ����: {ex.Message}");
            }
            finally
            {
                // ���� ���� �� �ڿ� ����
                CleanupConnection();
            }
        }

        // TCP ��ɾ �񵿱������� ��Ʈ���� ����
        private async Task WriteCommandAsync(string data)
        {
            if (stream?.CanWrite == true)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        // ��Ʈ��ũ ��Ʈ������ �� ���� �񵿱������� �б�
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
                        if (byteRead == 0) break; // ���� ������

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
                    Debug.LogError($"������ �б� ����: {ex.Message}");
                    return null;
                }

                return sb.ToString();
            }
            return null;
        }

        // ���� ���� �� ���ҽ� ����
        private void CleanupConnection()
        {
            stream?.Close();
            tcpClient?.Close();
            stream?.Dispose();
            tcpClient?.Dispose();
            isRunning = false;
        }

        // ���ø����̼� ���� �� ���� ����
        private void OnApplicationQuit()
        {
            cancellationTokenSource?.Cancel(); // �۾� ���
            CleanupConnection();
        }




        //-------------------������� ����

        //-������� ����--------------------------

        //���͸� ��ġ
        public int filter_calibrationSamples = 100; // ȯ�� �ν� ���� ���� (100�� ����)
        public double filter_threshold = 80; // ��ȭ ���� �Ӱ谪 (8cm = 80mm)

        private List<double> environmentBaseline = new List<double>(); // ȯ�� ���ذ� ����
        private List<List<double>> calibrationBuffer = new List<List<double>>(); // ȯ�� �νĿ� ����
        private int calibrationCount = 0; // ������� ������ ���� ����
        private bool isCalibrated = false; // ȯ�� �ν� �Ϸ� ����
        


        // JSON���� �����ϱ�
        public void SaveFilterData()
        {
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

        // JSON���� ������ �ҷ�����
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
                        Debug.Log("���� ���� ��...");

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
                            Debug.Log("���͸��� ������:");
                            foreach (var data in filteredData)
                            {
                                Debug.Log($"����: {data.angle:F2}��, �Ÿ�: {data.distance} mm");
                            }
                            dataQueue.EnqueueFilteredData(new LiDARAgentData(rawData.timestamp, filteredData));
                        }
                        else
                        {
                            Debug.Log("���͸��� ������ ����.");
                        }
                    }
                    else
                    {
                        Debug.Log("���� ��� ��...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"���� ó�� �� ���� �߻�: {ex.Message}");
                }

                await Task.Delay(100); // �񵿱� ���
            }
        }

        // ȯ�� �ν� �Լ� (�ʱ� filter_calibrationSamples�� ���� ���� �� ��� ���)
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

                // 65533 ���� �������� ����
                for (int i = 0; i < measurements.Count; i++)
                {
                    if (measurements[i].distance != 65533) // ���͸��� ���� ó��
                    {
                        calibrationBuffer[i].Add(measurements[i].distance);
                    }
                }

                calibrationCount++;

                // 100�� ���� ���� �� ȯ�� �ν� �Ϸ�
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
                    Debug.Log("ȯ�� �ν� �Ϸ�! (" + filter_calibrationSamples + "ȸ ����)");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ȯ�� �ν� �� ���� �߻�: {ex.Message}");
            }
        }

        // ��ȭ ���� �Լ� (ȯ��� ���Ͽ� ��ȭ�� ���� ����)
        private List<LiDARMeasurement> DetectChanges(List<LiDARMeasurement> measurements)
        {
            List<LiDARMeasurement> filteredData = new List<LiDARMeasurement>();

            try
            {
                for (int i = 0; i < measurements.Count; i++)
                {
                    if (i >= environmentBaseline.Count) continue;

                    // Ž�� �Ұ����� �Ÿ���(65533) ����
                    if (measurements[i].distance == 65533) continue;

                    double difference = Math.Abs(measurements[i].distance - environmentBaseline[i]);

                    if (difference > filter_threshold) // threshold �̻� ��ȭ ����
                    {
                        filteredData.Add(measurements[i]); // ��ȭ ������ �����͸� ����
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"��ȭ ���� �� ���� �߻�: {ex.Message}");
            }

            return filteredData;
        }









        //------------------- ������� ����

















        //private async void Start()
        //{
        //    //// LiDAR ���� �� ������ ó�� (���÷� LiDAR �����͸� ó��)
        //    //Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(10f, 45f, 0f); // ����: 10m �Ÿ�, 45�� �������� ��ġ �̺�Ʈ ó��





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
        //                    Debug.LogError("������ ���� ���� (��Ŷ ���� �Ǵ� ������ ����)");
        //                    await Task.Delay(50);
        //                    continue;
        //                }

        //                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //                Debug.Log($"[�����] ������ ���ŵ�, �Ÿ��� ����: {distances.Count}");

        //                if (distances.Count > 0)
        //                {
        //                    int adjustedEndIndex = Math.Min(endIndex, distances.Count - 1);
        //                    List<LiDARMeasurement> measurements = new List<LiDARMeasurement>();

        //                    for (int i = startIndex; i <= adjustedEndIndex; i++)
        //                    {
        //                        double angle = (i / (double)maxIndex) * scanAngle;
        //                        measurements.Add(new LiDARMeasurement(angle, distances[i]));
        //                    }

        //                    Debug.Log($"[�����] ���� ������ ���Ϳ� ���޵�, Ÿ�ӽ�����: {timeStamp}, ������ ����: {measurements.Count}");
        //                    DataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError($"���� �߻�: {ex.Message}");
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
        //            if (byteRead == 0) break; // ������ ������ ���

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
        //        Console.WriteLine($"������ �б� ����: {ex.Message}");
        //        return null;
        //    }

        //    return sb.ToString();
        //}



















        // LiDAR ������ ó��: �Ÿ��� ������ ȭ�� ��ǥ�� ��ȯ ��, ��ġ �̺�Ʈ ó��
        public void ProcessLiDARData(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR ������ (�Ÿ�, ���� ����, ���� ����)�� ȭ�� ��ǥ�� ��ȯ
            Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(distance, horizontalAngle, verticalAngle);

            // LiDARTouchReceiver�� ��ġ ���� �̺�Ʈ ����
            touchReceiver.OnTouchStart(screenCoord, lidarId);

            // ����: 2�� �� ��ġ ����
            Invoke("TriggerTouchEnd", 2f);  // 2�� �� ��ġ ���� ó��
        }

        private void TriggerTouchEnd()
        {
            Vector2 screenCoord = new Vector2(100, 100);  // ���� ��ǥ
            touchReceiver.OnTouchEnd(screenCoord, lidarId);  // ��ġ ���� �̺�Ʈ ó��
        }

        // LiDAR ������(����, �Ÿ�)�� 3D ��ǥ�� ��ȯ ��, ȭ�� ��ǥ�� ��ȯ
        private Vector2 ConvertLiDARDataToScreenCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // LiDAR�� ����/���� ������ �Ÿ��� 3D ��ǥ ���
            Vector3 worldPosition = ConvertToWorldCoordinates(distance, horizontalAngle, verticalAngle);

            // �ϳ��� ī�޶� ����Ͽ� 3D ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            return ConvertToScreenCoordinates(worldPosition);
        }

        // LiDAR ������ (�Ÿ�, ����)�� 3D ���� ��ǥ�� ��ȯ
        private Vector3 ConvertToWorldCoordinates(float distance, float horizontalAngle, float verticalAngle)
        {
            // ������ �������� ��ȯ
            float radianHorizontal = horizontalAngle * Mathf.Deg2Rad;
            float radianVertical = verticalAngle * Mathf.Deg2Rad;

            // �ﰢ���� �̿��Ͽ� LiDAR �����͸� 3D �������� ��ȯ
            float x = distance * Mathf.Cos(radianVertical) * Mathf.Sin(radianHorizontal);
            float y = distance * Mathf.Cos(radianVertical) * Mathf.Cos(radianHorizontal);
            float z = distance * Mathf.Sin(radianVertical);

            return new Vector3(x, y, z);
        }

        // 3D ��ǥ�� ȭ�� ��ǥ�� ��ȯ
        private Vector2 ConvertToScreenCoordinates(Vector3 worldPosition)
        {
            // �ϳ��� ī�޶� ����Ͽ� 3D ��ǥ�� 2D ȭ�� ��ǥ�� ��ȯ
            Vector3 screenPosition = lidarCamera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPosition.x, screenPosition.y);
        }
    }
}
