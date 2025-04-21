using System.Collections.Generic;
using UnityEngine;

public class DataStructure
{

    public class Vector2
    {
        public float x { get; set; }
        public float y { get; set; }

        public Vector2(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public override string ToString()
        {
            return $"x: {x}, y: {y}";
        }
    }

    // 라이다 센서에서 개별 측정 데이터를 저장하는 클래스
    public class LiDARMeasurement
    {
        public double angle { get; set; }  // 각도 (도)
        public long distance { get; set; } // 거리 (mm)

        public LiDARMeasurement(double angle, long distance)
        {
            this.angle = angle;
            this.distance = distance;
        }
    }

    // 하나의 센서에서 감지한 데이터를 묶는 클래스
    public class LiDARAgentData
    {
        public long timestamp { get; set; } // 측정 시간 (ms 단위)
        public List<LiDARMeasurement> measurements { get; set; } // 측정된 거리-각도 데이터 목록

        // Vector2 형태로 변환된 데이터 제공
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

    // 서버로 전송할 최종 메시지 데이터 구조
    public class LiDARAgentMessage
    {
        public string agentId { get; set; }  // 에이전트 고유 ID
        public List<LiDARAgentData> scanData { get; set; }  // 감지된 데이터 리스트
        public int dataAmount { get; set; }

        public LiDARAgentMessage(string agentId, int dataAmount, List<LiDARAgentData> scanData)
        {
            this.agentId = agentId;
            this.dataAmount = dataAmount;
            this.scanData = scanData;

        }
    }
}
