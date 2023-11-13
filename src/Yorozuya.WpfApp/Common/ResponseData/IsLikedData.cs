using System.Text.Json.Serialization;

namespace Yorozuya.WpfApp.Common.ResponseData;

public class IsLikedData
{
    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }
}
