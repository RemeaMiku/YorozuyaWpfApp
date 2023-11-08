using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace EverythingHouse.WpfApp.Models;

/// <summary>
/// 问题模型
/// </summary>
public class Post
{
    [JsonPropertyName("asker")]
    public string Asker { get; set; } = null!;

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