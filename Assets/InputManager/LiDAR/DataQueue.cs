//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System;


//namespace MongaLiDAR
//{
//    public static class DataQueue
//    {
//        private static ConcurrentQueue<LiDARAgentData> rawDataQueue = new ConcurrentQueue<LiDARAgentData>();
//        private static ConcurrentQueue<LiDARAgentData> filteredDataQueue = new ConcurrentQueue<LiDARAgentData>();

//        public static void EnqueueRawData(LiDARAgentData data) => rawDataQueue.Enqueue(data);
//        public static bool HasRawData() => !rawDataQueue.IsEmpty;
//        public static LiDARAgentData DequeueRawData() => rawDataQueue.TryDequeue(out var result) ? result : null;

//        public static void EnqueueFilteredData(LiDARAgentData data)
//        {
//            filteredDataQueue.Enqueue(data);
//            Console.WriteLine($"���͸��� ������ �߰��� (Ÿ�ӽ�����: {data.timestamp})");
//        }

//        public static bool HasFilteredData() => !filteredDataQueue.IsEmpty;
//        public static LiDARAgentData DequeueFilteredData() => filteredDataQueue.TryDequeue(out var result) ? result : null;

//        public static void Stop()
//        {
//            rawDataQueue = new ConcurrentQueue<LiDARAgentData>();
//            filteredDataQueue = new ConcurrentQueue<LiDARAgentData>();
//        }
//    }
//}
