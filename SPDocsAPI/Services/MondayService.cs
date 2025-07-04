using SPDocsAPI.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using SPDocsAPI.Interfaces;
namespace SPDocsAPI.Services
{
    public static class MondayService 
    {
        public static async Task<UserResponse> GetAllusers(IConfiguration configuration)
        {
            HttpClient _httpClient = new HttpClient();
            string baseURL = "https://spmondayapi.azurewebsites.net/api";

            string mondayToken = configuration["MondayToken"];
            var url = $"{baseURL}/users";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", mondayToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (Exception ex)
                {
                    return null;
                    throw;
                }
              
            }

            return null;
        }
        public static async Task<AllBoardsResponse> GetAllBoardsAsync(IConfiguration configuration)
        {
            HttpClient _httpClient = new HttpClient();
            string workspace ="SolidProfessor";
            string mondayToken = configuration["MondayToken"];
            string baseURL = "https://spmondayapi.azurewebsites.net/api";


            var url = $"{baseURL}/allboards";
            if (!string.IsNullOrEmpty(workspace))
                url += $"?workspace={Uri.EscapeDataString(workspace)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",mondayToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AllBoardsResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;

        }
        public static async Task<ActivityLogRoot>GetActivityBoardAsync(IConfiguration configuration,long boardId)
        {
            HttpClient _httpClient = new HttpClient();
            string mondayToken = configuration["MondayToken"];
            string baseURL = "https://spmondayapi.azurewebsites.net/api";
            var url = $"{baseURL}/board/{boardId}/activitylogs";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", mondayToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ActivityLogRoot>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
    }
}
