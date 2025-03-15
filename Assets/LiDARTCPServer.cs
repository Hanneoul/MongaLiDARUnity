using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class LiDARTCPServer : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private SynchronizationContext unityContext;
    public int port = 8080; // 사용할 포트 번호

    void Start()
    {
        unityContext = SynchronizationContext.Current;
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Debug.Log($"TCP 서버 시작됨 (포트 {port})");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                Debug.Log($"클라이언트 연결됨! IP: {clientEndPoint.Address}, 포트: {clientEndPoint.Port}");

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        
                        if (string.IsNullOrEmpty(message))
                        {
                            Debug.LogWarning("수신된 데이터가 비어 있음.");
                            continue;
                        }

                        try
                        {
                            LiDARData data = JsonUtility.FromJson<LiDARData>(message);
                            if (data == null)
                            {
                                Debug.LogWarning($"잘못된 JSON 데이터: {message}");
                                continue;
                            }

                            unityContext.Post(_ => HandleLiDARData(data), null);
                        }
                        catch (Exception jsonEx)
                        {
                            Debug.LogError($"JSON 변환 오류: {jsonEx.Message}, 받은 데이터: {message}");
                        }
                    }
                }

                client.Close();
                Debug.Log("클라이언트 연결 종료");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"TCP 서버 오류: {ex.Message}");
        }
    }

    void HandleLiDARData(LiDARData data)
    {
        Debug.Log($"좌표 수신 - X: {data.x}, Y: {data.y}, Z: {data.z}");

        GameObject obj = GameObject.Find("TargetObject");
        if (obj != null)
        {
            obj.transform.position = new Vector3(data.x, data.y, data.z);
        }
    }

    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
        Debug.Log("TCP 서버 종료됨.");
    }
}

[Serializable]
public class LiDARData
{
    public float x, y, z;
}
