using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Common;

namespace Yorozuya.WpfApp.Servcies.Http;

public abstract class BaseHttpService(HttpClient httpClient)
{
    protected readonly HttpClient _httpClient = httpClient;

    protected static void AddAuthorization(HttpRequestMessage message, string token)
        => message.Headers.Authorization = new("Bearer", token);

}
