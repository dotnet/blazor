using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;

namespace PrerenderingApp.Client
{
    public class ClientWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public ClientWeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecast()
        {
            return await _httpClient.GetJsonAsync<WeatherForecast[]>("/api/SampleData/WeatherForecasts");
        }
    }
}