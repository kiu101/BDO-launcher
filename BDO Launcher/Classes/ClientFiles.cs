using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BDO_Launcher
{

   

    public class ClientFile
    {
        [JsonProperty("dlpath")]
        public string dlpath { get; set; }
        [JsonProperty("installpath")]
        public string installpath { get; set; }
        [JsonProperty("filename")]
        public string filename { get; set; }
        [JsonProperty("md5")]
        public string md5 { get; set; }
    }
}
