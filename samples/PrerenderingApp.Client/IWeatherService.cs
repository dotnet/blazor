using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrerenderingApp.Client
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetForecast();
    }
}