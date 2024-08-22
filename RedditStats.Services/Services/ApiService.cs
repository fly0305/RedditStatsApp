using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RedditStats.Dto;
using RedditStats.Services.Interfaces;
using System.Net.Http.Headers;

namespace RedditStats.Services.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _accessToken;
        private readonly IOptions<AppSettings> _applicationSettings;

        public ApiService(HttpClient httpClient, IOptions<AppSettings> applicationSettings)
        {
            _httpClient = httpClient;
            _applicationSettings = applicationSettings;

            _clientId = _applicationSettings.Value.RedditApi.ClientId;
            _clientSecret = _applicationSettings.Value.RedditApi.ClientSecret;

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_applicationSettings.Value.RedditApi.UserAgent);
            _httpClient.BaseAddress = new Uri("https://oauth.reddit.com");

            // Obtain the access token when the service is initialized
            Task.Run(AuthenticateAsync).Wait();
        }

        private async Task AuthenticateAsync()
        {
            var authenticationString = $"{_clientId}:{_clientSecret}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var requestData = new List<KeyValuePair<string, string>> { new("grant_type", "client_credentials") };

            var requestContent = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", requestContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JObject.Parse(content);
            _accessToken = tokenResponse["access_token"].ToString();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<JArray> GetNewPostsAsync(string subreddit)
        {
            var response = await _httpClient.GetAsync($"/r/{subreddit}/new.json?limit=100");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            return (JArray)json["data"]["children"];
        }
    }
}