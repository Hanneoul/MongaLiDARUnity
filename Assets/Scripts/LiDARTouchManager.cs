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
        //LiDAR ����
        public int lidarId = 0;           // �� LiDAR�� ���� ID (����ڰ� ����)
        public string lidarIP = "192.168.0.10";
        public int lidarPort = 10940;

        int startIndex = 200;                     //LiDAR ���� ���� ���� (�迭�ε���)
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

        // LiDAR ��ġ�� ���ο� �ʿ��� ������Ʈ 
        public Camera lidarCamera;  // ī�޶� ��ü (�ϳ��� ���)
        public LiDARTouchReceiver touchReceiver;  // LiDAR ���ù� ����

        
        //��Ʈ�� ����
        public TcpClient client;
        public NetworkStream stream;

        private async void Start()
        {
            //// LiDAR ���� �� ������ ó�� (���÷� LiDAR �����͸� ó��)
            //Vector2 screenCoord = ConvertLiDARDataToScreenCoordinates(10f, 45f, 0f); // ����: 10m �Ÿ�, 45�� �������� ��ġ �̺�Ʈ ó��


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
                            Debug.LogError("������ ���� ���� (��Ŷ ���� �Ǵ� ������ ����)");
                            await Task.Delay(50);
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
                            DataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"���� �߻�: {ex.Message}");
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
                    if (byteRead == 0) break; // ������ ������ ���

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
                Console.WriteLine($"������ �б� ����: {ex.Message}");
                return null;
            }

            return sb.ToString();
        }



















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
