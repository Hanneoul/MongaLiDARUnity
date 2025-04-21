using System;
using System.Threading.Tasks;
using LiDARAgentSys;
using UnityEngine;
using UnityEngine.InputSystem;

public class LiDARManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private static LiDARSensor sensor;
    private static LidarFilter filter;

    void Start()
    {
        
    }

    static async Task Run()
    {
        Debug.Log("LiDAR 시스템 시작...");

        sensor = new LiDARSensor();
        filter = new LidarFilter();

        // 비동기 방식으로 실행
        var sensorTask = sensor.CollectData();
        var filterTask = filter.ProcessData();
        //var senderTask = DataSender.SendDataAsync();

        Debug.Log("종료하려면 ESC를 누르세요.");
        while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
        {
            await Task.Delay(100);
        }

        Debug.Log("시스템 종료 중...");
        sensor.Stop();
        filter.Stop();
        DataQueue.Stop();

        // 모든 비동기 작업 완료 대기
        await Task.WhenAll(sensorTask, filterTask, senderTask);

        Debug.Log("LiDAR 시스템 종료 완료.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
