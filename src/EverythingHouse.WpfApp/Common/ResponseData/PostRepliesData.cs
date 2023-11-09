using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Common.ResponseData;

public class PostRepliesData
{
    [JsonPropertyName("replyList")]
    public IEnumerable<Reply> ReplyList { get; set; } = null!;
}
