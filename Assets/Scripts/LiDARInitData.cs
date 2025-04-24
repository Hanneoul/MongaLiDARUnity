using UnityEngine;

namespace MongaLiDAR
{
    [System.Serializable]
    public class LiDARInitData
    {
        public int Lidar_id;
        public string lidar_ip;
        public int lidar_port;

        public int startIndex;
        public double scanAngle;
    }
}