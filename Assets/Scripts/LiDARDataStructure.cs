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
        // 필터링 수치들
        public int filter_calibrationSamples;  // 환경 인식 샘플 개수
        public double filter_threshold;         // 변화 감지 임계값

        // 환경 인식 관련 데이터
        public List<double> environmentBaseline; // 환경 기준값 저장
        public List<List<double>> calibrationBuffer; // 환경 인식용 버퍼
        public int calibrationCount; // 현재까지 수집된 샘플 개수
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

    public class DataQueue
    {
        private ConcurrentQueue<LiDARAgentData> rawDataQueue;
        private ConcurrentQueue<LiDARAgentData> filteredDataQueue;

        // 생성자에서 큐를 초기화
        public DataQueue()
        {
            rawDataQueue = new ConcurrentQueue<LiDARAgentData>();
            filteredDataQueue = new ConcurrentQueue<LiDARAgentData>();
        }

        // Raw 데이터 큐에 데이터 추가
        public void EnqueueRawData(LiDARAgentData data)
        {
            try
            {
                rawDataQueue.Enqueue(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Raw 데이터 큐에 추가 중 오류 발생: {ex.Message}");
                // 예외 처리 로직 추가 (예: 로깅)
            }
        }

        // Raw 데이터 큐에 데이터가 있는지 확인
        public bool HasRawData()
        {
            try
            {
                return !rawDataQueue.IsEmpty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Raw 데이터 큐 확인 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        // Raw 데이터 큐에서 데이터 꺼내기
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
                Debug.LogError($"Raw 데이터 큐에서 꺼내는 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        // 필터링된 데이터 큐에 데이터 추가
        public void EnqueueFilteredData(LiDARAgentData data)
        {
            try
            {
                filteredDataQueue.Enqueue(data);
                Debug.Log($"필터링된 데이터 추가됨 (타임스탬프: {data.timestamp})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"필터링된 데이터 큐에 추가 중 오류 발생: {ex.Message}");
                // 예외 처리 로직 추가 (예: 로깅)
            }
        }

        // 필터링된 데이터 큐에 데이터가 있는지 확인
        public bool HasFilteredData()
        {
            try
            {
                return !filteredDataQueue.IsEmpty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"필터링된 데이터 큐 확인 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        // 필터링된 데이터 큐에서 데이터 꺼내기
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
                Debug.LogError($"필터링된 데이터 큐에서 꺼내는 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        // 큐를 초기화 (초기화 시점에 필요)
        public void Stop()
        {
           
            try
            {
                rawDataQueue.Clear();
                filteredDataQueue.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"큐 초기화 중 오류 발생: {ex.Message}");
                // 예외 처리 로직 추가 (예: 로깅)
            }
        }

        // 생성자에서 큐를 초기화하고, 필요 시 메모리 할당을 안전하게 처리합니다.
        ~DataQueue()
        {
            try
            {
                rawDataQueue = null;
                filteredDataQueue = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DataQueue 소멸자에서 오류 발생: {ex.Message}");
                // 예외 처리 로직 추가 (예: 로깅)
            }
        }
    }
}