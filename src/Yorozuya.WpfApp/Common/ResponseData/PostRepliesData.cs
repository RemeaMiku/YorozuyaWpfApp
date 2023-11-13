using System.Collections.Generic;
using System.Text.Json.Serialization;
using Yorozuya.WpfApp.Models;

namespace Yorozuya.WpfApp.Common.ResponseData;

public class PostRepliesData
{
    [JsonPropertyName("replyList")]
    public IEnumerable<Reply> ReplyList { get; set; } = null!;
}
