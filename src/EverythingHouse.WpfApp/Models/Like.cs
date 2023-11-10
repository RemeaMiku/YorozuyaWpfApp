using System.Text.Json.Serialization;

namespace EverythingHouse.WpfApp.Models;

/// <summary>
/// 点赞模型
/// </summary>
public class Like
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("replyId")]
    public long ReplyId { get; set; }

    [JsonPropertyName("userId")]
    public long UserId { get; set; }
}