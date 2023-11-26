using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yorozuya.WpfApp.Common.Helpers;

public static class ApiResponseMessageHandler
{
    public static async Task HandleNoDataApiResponseMessage(HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<object>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
    }

    public static async Task<TModel> HandleModelDataApiResponseMessage<TModel>(HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<TModel>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data;
    }

    public static async Task<IEnumerable<TModel>?> HandleIEnumerbleModelDataApiResponseMessage<TModel>(string key, HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data[key].Deserialize<IEnumerable<TModel>>();
    }
}
