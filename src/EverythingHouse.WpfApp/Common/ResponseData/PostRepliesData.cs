using System.Collections.Generic;
using System.Text.Json.Serialization;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Common.ResponseData;

public class PostRepliesData
{
    [JsonPropertyName("replyList")]
    public IEnumerable<Reply> ReplyList { get; set; } = null!;
}
