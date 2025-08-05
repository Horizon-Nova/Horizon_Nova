using HNB.Areas.HNB_WEB.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HNB.Areas.HNB_WEB.Models;

public class ZtreeInfo
{
    [JsonConverter(typeof(StringJsonConverter))]
    public long? id { get; set; }

    [JsonConverter(typeof(StringJsonConverter))]
    public long? pId { get; set; }

    public string name { get; set; }
}