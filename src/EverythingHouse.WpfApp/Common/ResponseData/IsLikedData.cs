using System.Text.Json.Serialization;

namespace EverythingHouse.WpfApp.Common.ResponseData;

public class IsLikedData
{
    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }
}
