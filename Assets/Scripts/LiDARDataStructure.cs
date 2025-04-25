using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MongaLiDAR
{

    [Serializable]
    public class LiDARInitData
    {
        public int Lidar_id;
        public string lidar_ip;
        public int lidar_port;

        public int startIndex;
        public double scanAngle;
    }

    [Serializable]
    public class LiDARFilterData
    {
        // ���͸� ��ġ��
        public int filter_calibrationSamples;  // ȯ�� �ν� ���� ����
        public double filter_threshold;         // ��ȭ ���� �Ӱ谪

        // ȯ�� �ν� ���� ������
        public List<double> environmentBaseline; // ȯ�� ���ذ� ����
        public List<List<double>> calibrationBuffer; // ȯ�� �νĿ� ����
        public int calibrationCount; // ������� ������ ���� ����
    }


    // ���̴� �������� ���� ���� �����͸� �����ϴ� Ŭ����
    public class LiDARMeasurement
    {
        public double angle { get; set; }  // ���� (��)
        public long distance { get; set; } // �Ÿ� (mm)

        public LiDARMeasurement(double angle, long distance)
        {
            this.angle = angle;
            this.distance = distance;
        }
    }

    // �ϳ��� �������� ������ �����͸� ���� Ŭ����
    public class LiDARAgentData
    {
        public long timestamp { get; set; } // ���� �ð� (ms ����)
        public List<LiDARMeasurement> measurements { get; set; } // ������ �Ÿ�-���� ������ ���

        // Vector2 ���·� ��ȯ�� ������ ����
        public List<Vector2> DistanceAngleData
        {
            get
            {
                List<Vector2> vectorData = new List<Vector2>();
                foreach (var measurement in measurements)
                {
                    vectorData.Add(new Vector2((float)measurement.angle, measurement.distance));
                }
                return vectorData;
            }
        }

        public LiDARAgentData(long timestamp, List<LiDARMeasurement> measurements)
        {
            this.timestamp = timestamp;
            this.measurements = measurements;
        }
    }

    // ������ ������ ���� �޽��� ������ ����
    public class LiDARAgentMessage
    {
        public string agentId { get; set; }  // ������Ʈ ���� ID
        public List<LiDARAgentData> scanData { get; set; }  // ������ ������ ����Ʈ
        public int dataAmount { get; set; }

        public LiDARAgentMessage(string agentId, int dataAmount, List<LiDARAgentData> scanData)
        {
            this.agentId = agentId;
            this.dataAmount = dataAmount;
            this.scanData = scanData;

        }
    }

    public class DataQueue
    {
        private ConcurrentQueue<LiDARAgentData> rawDataQueue;
        private ConcurrentQueue<LiDARAgentData> filteredDataQueue;

        // �����ڿ��� ť�� �ʱ�ȭ
        public DataQueue()
        {
            rawDataQueue = new ConcurrentQueue<LiDARAgentData>();
            filteredDataQueue = new ConcurrentQueue<LiDARAgentData>();
        }

        // Raw ������ ť�� ������ �߰�
        public void EnqueueRawData(LiDARAgentData data)
        {
            try
            {
                rawDataQueue.Enqueue(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Raw ������ ť�� �߰� �� ���� �߻�: {ex.Message}");
                // ���� ó�� ���� �߰� (��: �α�)
            }
        }

        // Raw ������ ť�� �����Ͱ� �ִ��� Ȯ��
        public bool HasRawData()
        {
            try
            {
                return !rawDataQueue.IsEmpty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Raw ������ ť Ȯ�� �� ���� �߻�: {ex.Message}");
                return false;
            }
        }

        // Raw ������ ť���� ������ ������
        public LiDARAgentData DequeueRawData()
        {
            try
            {
                if (rawDataQueue.TryDequeue(out var result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Raw ������ ť���� ������ �� ���� �߻�: {ex.Message}");
                return null;
            }
        }

        // ���͸��� ������ ť�� ������ �߰�
        public void EnqueueFilteredData(LiDARAgentData data)
        {
            try
            {
                filteredDataQueue.Enqueue(data);
                Debug.Log($"���͸��� ������ �߰��� (Ÿ�ӽ�����: {data.timestamp})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"���͸��� ������ ť�� �߰� �� ���� �߻�: {ex.Message}");
                // ���� ó�� ���� �߰� (��: �α�)
            }
        }

        // ���͸��� ������ ť�� �����Ͱ� �ִ��� Ȯ��
        public bool HasFilteredData()
        {
            try
            {
                return !filteredDataQueue.IsEmpty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"���͸��� ������ ť Ȯ�� �� ���� �߻�: {ex.Message}");
                return false;
            }
        }

        // ���͸��� ������ ť���� ������ ������
        public LiDARAgentData DequeueFilteredData()
        {
            try
            {
                if (filteredDataQueue.TryDequeue(out var result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"���͸��� ������ ť���� ������ �� ���� �߻�: {ex.Message}");
                return null;
            }
        }

        // ť�� �ʱ�ȭ (�ʱ�ȭ ������ �ʿ�)
        public void Stop()
        {
           
            try
            {
                rawDataQueue.Clear();
                filteredDataQueue.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ť �ʱ�ȭ �� ���� �߻�: {ex.Message}");
                // ���� ó�� ���� �߰� (��: �α�)
            }
        }

        // �����ڿ��� ť�� �ʱ�ȭ�ϰ�, �ʿ� �� �޸� �Ҵ��� �����ϰ� ó���մϴ�.
        ~DataQueue()
        {
            try
            {
                rawDataQueue = null;
                filteredDataQueue = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DataQueue �Ҹ��ڿ��� ���� �߻�: {ex.Message}");
                // ���� ó�� ���� �߰� (��: �α�)
            }
        }
    }
}