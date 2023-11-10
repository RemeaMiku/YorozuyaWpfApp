using System.Text.Json.Serialization;

namespace EverythingHouse.WpfApp.Common;

/// <summary>
/// API 响应
/// </summary>
/// <typeparam name="TData">数据类型</typeparam>
public class Response<TData>
{
    [JsonPropertyName("code")]
    public long Code { get; set; }

    [JsonPropertyName("data")]
    public TData? Data { get; set; } = default;

    [JsonPropertyName("msg")]
    public string Message { get; set; } = null!;
}