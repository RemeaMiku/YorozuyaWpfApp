using System.Text.Json.Serialization;
using Yorozuya.WpfApp.Common.Exceptions;

namespace Yorozuya.WpfApp.Common;

/// <summary>
/// API 响应类
/// </summary>
/// <typeparam name="TData">数据类型，在 Data 包含多个数据时使用Dictionary<string,JsonElement>以进一步反序列化 </typeparam>
public class ApiResponse<TData>
{
    #region Public Properties

    [JsonPropertyName("code")]
    public long Code { get; set; }

    [JsonPropertyName("data")]
    public TData? Data { get; set; } = default;

    [JsonPropertyName("msg")]
    public string Message { get; set; } = null!;

    public bool IsSuccessStatusCode => Code >= 200 && Code <= 299;

    #endregion Public Properties

    #region Public Methods

    public void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
            throw new ApiResponseException(Message);
    }

    #endregion Public Methods
}