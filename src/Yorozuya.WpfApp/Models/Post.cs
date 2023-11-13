using System.Text.Json.Serialization;

namespace Yorozuya.WpfApp.Models;

/// <summary>
/// 问题模型
/// </summary>
public class Post
{
    [JsonPropertyName("askerId")]
    public long AskerId { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    [JsonPropertyName("createTime")]
    public string CreateTime { get; set; } = null!;

    [JsonPropertyName("delTag")]
    public long DelTag { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; } = null!;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; } = null!;

    [JsonPropertyName("views")]
    public long Views { get; set; }
}