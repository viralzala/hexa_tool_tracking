using System.Collections.Generic;
namespace Log.Models
{
    public class MqttTopicModel
    {
        public string msg { get; set; }
        public string gmac { get; set; }
        public List<Obj> obj { get; set; }
    }
    public class Obj
    {
        public int type { get; set; }
        public string dmac { get; set; }
        public string uuid { get; set; }
        public int majorID { get; set; }
        public int minorID { get; set; }
        public int refpower { get; set; }
        public int rssi { get; set; }
        public string time { get; set; }
        public int vbatt { get; set; } = 0;
    }
}
