using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataStructure;

namespace LiDARAgentSys
{
    class LidarFilter
    {
        private static List<double> environmentBaseline = new List<double>(); // 환경 기준값 저장
        private static List<List<double>> calibrationBuffer = new List<List<double>>(); // 환경 인식용 버퍼
        private const int calibrationSamples = 100; // 환경 인식 샘플 개수 (100번 측정)
        private const double threshold = 80; // 변화 감지 임계값 (8cm = 80mm)
        private static int calibrationCount = 0; // 현재까지 수집된 샘플 개수
        private static bool isCalibrated = false; // 환경 인식 완료 여부
        private bool running = true;

        public async Task ProcessData()
        {
            while (running)
            {
                if (DataQueue.HasRawData())
                {
                    LiDARAgentData rawData = DataQueue.DequeueRawData();
                    Console.WriteLine("[디버그] 필터 실행 중...");

                    if (!isCalibrated)
                    {
                        Console.WriteLine("환경 인식 중...");
                        CalibrateEnvironment(rawData.measurements);
                        continue;
                    }

                    List<LiDARMeasurement> filteredData = DetectChanges(rawData.measurements);

                    if (filteredData.Count > 0)
                    {
                        Console.WriteLine("필터링된 데이터:");
                        foreach (var data in filteredData)
                        {
                            Console.WriteLine($"각도: {data.angle:F2}도, 거리: {data.distance} mm");
                        }
                        DataQueue.EnqueueFilteredData(new LiDARAgentData(rawData.timestamp, filteredData));
                    }
                    else
                    {
                        Console.WriteLine("[디버그] 필터링된 데이터 없음.");
                    }
                }
                else
                {
                    Console.WriteLine("[디버그] 필터 대기 중...");
                }
                await Task.Delay(100); // 비동기 대기
            }
        }

        // 환경 인식 함수 (초기 100번 샘플 수집 후 평균 계산)
        private static void CalibrateEnvironment(List<LiDARMeasurement> measurements)
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
                if (measurements[i].distance != 65533)
                {
                    calibrationBuffer[i].Add(measurements[i].distance);
                }
            }

            calibrationCount++;

            if (calibrationCount >= calibrationSamples)
            {
                for (int i = 0; i < environmentBaseline.Count; i++)
                {
                    if (calibrationBuffer[i].Count > 0)
                    {
                        environmentBaseline[i] = calibrationBuffer[i].Average();
                    }
                }

                isCalibrated = true;
                Console.WriteLine("환경 인식 완료! (100회 측정)");
            }
        }


        // 변화 감지 함수 (환경과 비교하여 변화된 값만 추출)
        private static List<LiDARMeasurement> DetectChanges(List<LiDARMeasurement> measurements)
        {
            List<LiDARMeasurement> filteredData = new List<LiDARMeasurement>();

            for (int i = 0; i < measurements.Count; i++)
            {
                if (i >= environmentBaseline.Count) continue;

                // 탐지 불가능한 거리값(65533) 무시
                if (measurements[i].distance == 65533) continue;

                double difference = Math.Abs(measurements[i].distance - environmentBaseline[i]);

                if (difference > threshold) // 8cm 이상 변화 감지
                {
                    filteredData.Add(measurements[i]); // 변화 감지된 데이터만 저장
                }
            }
            return filteredData;
        }


        public void Stop()
        {
            running = false;
        }
    }
}
