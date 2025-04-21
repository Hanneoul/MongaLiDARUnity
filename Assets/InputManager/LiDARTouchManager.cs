using LiDARAgentSys;
using System;
using UnityEngine;

public class LiDARTouchManager : MonoBehaviour
{
    private static LiDARSensor sensor;
    private static LidarFilter filter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         

    
        Debug.Log("LiDAR �ý��� ����...");

        sensor = new LiDARSensor();
        filter = new LidarFilter();

        // �񵿱� ������� ����
        var sensorTask = sensor.CollectData();
        //var filterTask = filter.ProcessData();
        var senderTask = DataSender.SendDataAsync();

        Debug.Log("�����Ϸ��� ESC�� ��������.");
        while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
        {
            await Task.Delay(100);
        }

        Debug.Log("�ý��� ���� ��...");
        sensor.Stop();
        filter.Stop();
        DataQueue.Stop();

        // ��� �񵿱� �۾� �Ϸ� ���
        await Task.WhenAll(sensorTask, senderTask);

        Debug.Log("LiDAR �ý��� ���� �Ϸ�.");
    }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
