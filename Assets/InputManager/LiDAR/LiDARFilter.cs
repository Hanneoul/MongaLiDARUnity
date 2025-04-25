using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongaLiDAR
{
    //class LidarFilter
    //{
    //    private static List<double> environmentBaseline = new List<double>(); // ȯ�� ���ذ� ����
    //    private static List<List<double>> calibrationBuffer = new List<List<double>>(); // ȯ�� �νĿ� ����
    //    private const int calibrationSamples = 100; // ȯ�� �ν� ���� ���� (100�� ����)
    //    private const double threshold = 80; // ��ȭ ���� �Ӱ谪 (8cm = 80mm)
    //    private static int calibrationCount = 0; // ������� ������ ���� ����
    //    private static bool isCalibrated = false; // ȯ�� �ν� �Ϸ� ����
    //    private bool running = true;

    //    public async Task ProcessData()
    //    {
    //        while (running)
    //        {
    //            if (DataQueue.HasRawData())
    //            {
    //                LiDARAgentData rawData = DataQueue.DequeueRawData();
    //                Console.WriteLine("[�����] ���� ���� ��...");

    //                if (!isCalibrated)
    //                {
    //                    Console.WriteLine("ȯ�� �ν� ��...");
    //                    CalibrateEnvironment(rawData.measurements);
    //                    continue;
    //                }

    //                List<LiDARMeasurement> filteredData = DetectChanges(rawData.measurements);

    //                if (filteredData.Count > 0)
    //                {
    //                    Console.WriteLine("���͸��� ������:");
    //                    foreach (var data in filteredData)
    //                    {
    //                        Console.WriteLine($"����: {data.angle:F2}��, �Ÿ�: {data.distance} mm");
    //                    }
    //                    DataQueue.EnqueueFilteredData(new LiDARAgentData(rawData.timestamp, filteredData));
    //                }
    //                else
    //                {
    //                    Console.WriteLine("[�����] ���͸��� ������ ����.");
    //                }
    //            }
    //            else
    //            {
    //                Console.WriteLine("[�����] ���� ��� ��...");
    //            }
    //            await Task.Delay(100); // �񵿱� ���
    //        }
    //    }

    //    // ȯ�� �ν� �Լ� (�ʱ� 100�� ���� ���� �� ��� ���)
    //    private static void CalibrateEnvironment(List<LiDARMeasurement> measurements)
    //    {
    //        if (calibrationCount == 0)
    //        {
    //            environmentBaseline = new List<double>(new double[measurements.Count]);
    //            calibrationBuffer = new List<List<double>>(measurements.Count);

    //            for (int i = 0; i < measurements.Count; i++)
    //            {
    //                calibrationBuffer.Add(new List<double>());
    //            }
    //        }

    //        // 65533 ���� �������� ����
    //        for (int i = 0; i < measurements.Count; i++)
    //        {
    //            if (measurements[i].distance != 65533)
    //            {
    //                calibrationBuffer[i].Add(measurements[i].distance);
    //            }
    //        }

    //        calibrationCount++;

    //        if (calibrationCount >= calibrationSamples)
    //        {
    //            for (int i = 0; i < environmentBaseline.Count; i++)
    //            {
    //                if (calibrationBuffer[i].Count > 0)
    //                {
    //                    environmentBaseline[i] = calibrationBuffer[i].Average();
    //                }
    //            }

    //            isCalibrated = true;
    //            Console.WriteLine("ȯ�� �ν� �Ϸ�! (100ȸ ����)");
    //        }
    //    }


    //    // ��ȭ ���� �Լ� (ȯ��� ���Ͽ� ��ȭ�� ���� ����)
    //    private static List<LiDARMeasurement> DetectChanges(List<LiDARMeasurement> measurements)
    //    {
    //        List<LiDARMeasurement> filteredData = new List<LiDARMeasurement>();

    //        for (int i = 0; i < measurements.Count; i++)
    //        {
    //            if (i >= environmentBaseline.Count) continue;

    //            // Ž�� �Ұ����� �Ÿ���(65533) ����
    //            if (measurements[i].distance == 65533) continue;

    //            double difference = Math.Abs(measurements[i].distance - environmentBaseline[i]);

    //            if (difference > threshold) // 8cm �̻� ��ȭ ����
    //            {
    //                filteredData.Add(measurements[i]); // ��ȭ ������ �����͸� ����
    //            }
    //        }
    //        return filteredData;
    //    }


    //    public void Stop()
    //    {
    //        running = false;
    //    }
    //}
}
