using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using static DataStructure;

namespace LiDARAgentSys
{
    public class DataSender // 데이터 즉시 전송
    {
        private static readonly string serverIp = "172.16.82.184"; // 서버 IP 설정
        private static readonly int serverPort = 6700; // 서버 포트 설정
        private static volatile bool running = true; // 스레드 종료 여부

        public static async Task SendDataAsync()
        {
            while (running)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        Console.WriteLine($"[디버그] 서버 {serverIp}:{serverPort} 연결 시도...");
                        await client.ConnectAsync(serverIp, serverPort);

                        using (NetworkStream stream = client.GetStream())
                        {
                            Console.WriteLine($"✅ 서버 {serverIp}:{serverPort} 연결 완료!");

                            while (running && client.Connected)
                            {
                                if (DataQueue.HasFilteredData())
                                {
                                    // 필터링된 데이터 가져오기
                                    var filteredData = DataQueue.DequeueFilteredData();

                                    // `LiDARAgentMessage`로 변환하여 서버로 전송
                                    var message = new LiDARAgentMessage(
                                        "Agent_001",
                                        filteredData.measurements.Count,
                                        new List<LiDARAgentData> { filteredData }
                                    );

                                    string jsonMessage = "";//JsonSerializer.Serialize(message);
                                    byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage + "\n");

                                    try
                                    {
                                        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                                        Console.WriteLine($"✅ 서버로 전송됨: {jsonMessage}");
                                    }
                                    catch (Exception sendEx)
                                    {
                                        Console.WriteLine($"⚠️ 데이터 전송 오류: {sendEx.Message}");
                                        break; // 전송 실패 시 루프 종료 후 재연결
                                    }
                                }

                                await Task.Delay(50); // 50ms 대기 후 재확인
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ 서버 연결 오류: {ex.Message}");
                    await Task.Delay(3000); // 3초 후 재연결 시도
                }
            }
        }

        public static void Stop()
        {
            running = false;
        }
    }
}
