using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrerenderingApp.Client;

namespace PrerenderingApp.Server.Controller
{
    [Route("api/[controller]")]
    public class SampleDataController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IWeatherService _weatherService;

        public SampleDataController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<WeatherForecast>> WeatherForecasts()
        {
            return await _weatherService.GetForecast();
        }
    }
}