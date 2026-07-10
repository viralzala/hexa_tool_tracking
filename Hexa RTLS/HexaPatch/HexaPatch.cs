using System;
using System.Collections.Generic;

namespace HexaPatch
{
    public class HexaPatchInclude
    {
        // EncryptionDecryption Sec = new EncryptionDecryption();

        public _period Period()
        {
            _period Obj = new _period();

            Obj.FromDate = Convert.ToDateTime("01-Jan-2017");
            Obj.ToDate = Convert.ToDateTime("01-Jan-2020");
            return Obj;
        }
        public _Device Device()
        {
            _Device Obj = new _Device();
            Obj.Reader = 5;
            Obj.Tags = 2000;

            return Obj;
        }
        public List<_Platforms> Platforms()
        {

            List<_Platforms> objList = new List<_Platforms>();

            objList.Add(new _Platforms { Mac = "C4:00:AD:D9:B0:80" });

            //Hexahash
            objList.Add(new _Platforms { Mac = "D0:7E:35:AC:FD:F1" });
            objList.Add(new _Platforms { Mac = "00:E0:4A:37:01:4C" });
            objList.Add(new _Platforms { Mac = "D0:7E:35:AC:FD:F0" });
            objList.Add(new _Platforms { Mac = "D0:7E:35:AC:FD:F4" });
            objList.Add(new _Platforms { Mac = "0A:15:2C:08:C7:81" });

            objList.Add(new _Platforms { Mac = "84:A6:C8:46:AF:B9" });
            objList.Add(new _Platforms { Mac = "5C:F9:DD:48:9B:2C" });
            //AWS EC2 VM.
            objList.Add(new _Platforms { Mac = "02:DF:36:F5:B6:62" });
            objList.Add(new _Platforms { Mac = "00:00:00:00:00:00:00:E0" });
            //Azure
            objList.Add(new _Platforms { Mac = "00:0D:3A:F2:4A:7B" });


            //Ashish PC
            objList.Add(new _Platforms { Mac = "00:26:B9:F4:F7:EE" });
            objList.Add(new _Platforms { Mac = "00:23:14:9C:B7:95" });
            objList.Add(new _Platforms { Mac = "00:23:14:9C:B7:94" });
            objList.Add(new _Platforms { Mac = "5C:AC:4C:F4:AC:C5" });

            return objList;
        }

        public partial class _period
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
        }

        public partial class _Device
        {
            public int Reader { get; set; }
            public int Tags { get; set; }
        }

        public partial class _Platforms
        {
            public string Mac { get; set; }
        }
    }
}
