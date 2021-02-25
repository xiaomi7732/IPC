using System;
using System.Threading.Tasks;
using CodeWithSaar.IPC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeWithSaar.Example.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            NamedPipeOptions namedPipeOptions = new NamedPipeOptions()
            {
                PipeName = "hello_namedpipe_service",
            };
            IOptions<NamedPipeOptions> options = Options.Create<NamedPipeOptions>(namedPipeOptions);
            ILogger<DuplexNamedPipeService> logger = LoggerFactory.Create(config => { }).CreateLogger<DuplexNamedPipeService>();
            using (INamedPipeServerService namedPipeServer = new DuplexNamedPipeService(options, logger))
            {
                Console.WriteLine("[SERVER] Waiting for connection.");
                await namedPipeServer.WaitForConnectionAsync(default).ConfigureAwait(false);
                Console.WriteLine("[SERVER] Connected.");

                Console.WriteLine("[SERVER] Sending greeting...");
                await namedPipeServer.SendMessageAsync("Hello~from server").ConfigureAwait(false);
                Console.WriteLine("[SERVER] Greeting sent.");

                Console.WriteLine("[SERVER] Guessing what will the client say...");
                string whatTheClientSay = await namedPipeServer.ReadMessageAsync().ConfigureAwait(false);
                Console.WriteLine("[SERVER] The client says: {0}", whatTheClientSay);
            }
        }
    }
}
