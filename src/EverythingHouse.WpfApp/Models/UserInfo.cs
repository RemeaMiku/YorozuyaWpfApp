using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Runtime.Serialization;

namespace EverythingHouse.WpfApp.Models;

/// <summary>
/// 用户信息模型
/// </summary>
public class UserInfo
{
    [JsonPropertyName("createTime")]
    public string CreateTime { get; set; } = null!;

    [JsonPropertyName("field")]
    public string Field { get; set; } = null!;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = null!;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 1是admin
    /// </summary>
    [JsonPropertyName("role")]
    public long Role { get; set; }

    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; } = null!;

    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;
}