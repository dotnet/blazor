using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestServer.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SlowController : Controller
    {
        [HttpGet]
        public async Task<string> SayHello(int delay)
        {
            await Task.Delay(delay);
            return "Hello";
        }
    }
}
