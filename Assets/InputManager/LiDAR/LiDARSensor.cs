using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SCIP_library;
using static DataStructure;

namespace LiDARAgentSys
{
    class LiDARSensor
    {
        private bool running = true;
        private string ip;
        private int port;
        private int startIndex;
        private int endIndex;
        private const int maxIndex = 1080;
        private const double scanAngle = 270.0;

        public LiDARSensor()
        {
            GetConnectionInfo();
            GetIndexRange();
        }

        private void GetConnectionInfo()
        {
            ip = "192.168.0.10";
            port = 10940;

            Console.WriteLine($"IP 주소를 입력하세요. [기본값: {ip}]");
            string inputIp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputIp)) ip = inputIp;

            Console.WriteLine($"포트 번호를 입력하세요. [기본값: {port}]");
            string inputPort = Console.ReadLine();
            if (int.TryParse(inputPort, out int tempPort)) port = tempPort;

            Console.WriteLine($"라이다 연결 정보: {ip}:{port}");
        }

        private void GetIndexRange()
        {
            Console.WriteLine($"저장할 데이터의 시작 인덱스를 입력하세요 (0 ~ {maxIndex}, 기본값: 200):");
            string inputStart = Console.ReadLine();
            startIndex = int.TryParse(inputStart, out int tempStart) && tempStart >= 0 && tempStart <= maxIndex ? tempStart : 200;

            Console.WriteLine($"저장할 데이터의 끝 인덱스를 입력하세요 ({startIndex} ~ {maxIndex}, 기본값: 400):");
            string inputEnd = Console.ReadLine();
            endIndex = int.TryParse(inputEnd, out int tempEnd) && tempEnd >= startIndex && tempEnd <= maxIndex ? tempEnd : 400;

            Console.WriteLine($"설정된 인덱스 범위: {startIndex} ~ {endIndex}");
        }

        public async Task CollectData()
        {
            try
            {
                using (TcpClient urg = new TcpClient(ip, port))
                using (NetworkStream stream = urg.GetStream())
                {
                    await WriteCommand(stream, SCIP_Writer.SCIP2());
                    string receiveData = await ReadLine(stream);


                    while (running)
                    {
                        await WriteCommand(stream, SCIP_Writer.MD(0, maxIndex));
                        receiveData = await ReadLine(stream);

                        List<long> distances = new List<long>();
                        long unusedTimeStamp = 0;

                        receiveData = await ReadLine(stream);
                        if (string.IsNullOrEmpty(receiveData) || !SCIP_Reader.MD(receiveData, ref unusedTimeStamp, ref distances))
                        {
                            Console.WriteLine("데이터 수신 실패 (패킷 오류 또는 데이터 없음)");
                            await Task.Delay(50);
                            continue;
                        }

                        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        Console.WriteLine($"[디버그] 데이터 수신됨, 거리값 개수: {distances.Count}");

                        if (distances.Count > 0)
                        {
                            int adjustedEndIndex = Math.Min(endIndex, distances.Count - 1);
                            List<LiDARMeasurement> measurements = new List<LiDARMeasurement>();

                            for (int i = startIndex; i <= adjustedEndIndex; i++)
                            {
                                double angle = (i / (double)maxIndex) * scanAngle;
                                measurements.Add(new LiDARMeasurement(angle, distances[i]));
                            }

                            Console.WriteLine($"[디버그] 원본 데이터 필터에 전달됨, 타임스탬프: {timeStamp}, 데이터 개수: {measurements.Count}");
                            DataQueue.EnqueueRawData(new LiDARAgentData(long.Parse(timeStamp), measurements));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
        }

        public void Stop()
        {
            running = false;
        }

        private async Task WriteCommand(NetworkStream stream, string data)
        {
            if (!stream.CanWrite) return;
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        static async Task<string> ReadLine(NetworkStream stream)
        {
            if (!stream.CanRead) return null;

            StringBuilder sb = new StringBuilder();
            bool isNL2 = false;
            bool isNL = false;
            byte[] buffer = new byte[1];

            try
            {
                do
                {
                    int byteRead = await stream.ReadAsync(buffer, 0, 1);
                    if (byteRead == 0) break; // 연결이 끊어진 경우

                    char receivedChar = (char)buffer[0];

                    if (receivedChar == '\n')
                    {
                        if (isNL) isNL2 = true;
                        else isNL = true;
                    }
                    else isNL = false;

                    sb.Append(receivedChar);
                } while (!isNL2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"데이터 읽기 오류: {ex.Message}");
                return null;
            }

            return sb.ToString();
        }
    }
}
