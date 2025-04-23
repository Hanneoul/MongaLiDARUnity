using System.Collections.Generic;
using UnityEngine;

public class DataStructure
{


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
}
