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

    //static async Task Run()
    //{
        //Debug.Log("LiDAR �ý��� ����...");

        //sensor = new LiDARSensor();
        //filter = new LidarFilter();

        //// �񵿱� ������� ����
        //var sensorTask = sensor.CollectData();
        //var filterTask = filter.ProcessData();
        ////var senderTask = DataSender.SendDataAsync();

        //Debug.Log("�����Ϸ��� ESC�� ��������.");
        //while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
        //{
        //    await Task.Delay(100);
        //}

        //Debug.Log("�ý��� ���� ��...");
        //sensor.Stop();
        //filter.Stop();
        //DataQueue.Stop();

        //// ��� �񵿱� �۾� �Ϸ� ���
        //await Task.WhenAll(sensorTask, filterTask, senderTask);

        //Debug.Log("LiDAR �ý��� ���� �Ϸ�.");
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
