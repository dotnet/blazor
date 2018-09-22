using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestServer.Controllers
{
    [Route("api/[controller]/[action]")]
    public class StreamingController : Controller
    {
        [HttpGet]
        public async Task TimeStream(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/plain";
            using (var writer = new StreamWriter(Response.Body))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await writer.WriteLineAsync(DateTime.UtcNow.ToString("o"));
                    await writer.FlushAsync();
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
    }
}
