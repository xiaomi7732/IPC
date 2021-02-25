﻿using System;
using System.Threading.Tasks;
using CodeWithSaar.IPC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeWithSaar.Example.Client
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

            using (INamedPipeClientService namedPipeClient = new DuplexNamedPipeService(options, logger))
            {
                Console.WriteLine("[CLIENT] Connecting to named pipe server...");
                await namedPipeClient.ConnectAsync(TimeSpan.FromSeconds(2), default).ConfigureAwait(false);
                Console.WriteLine("[CLIENT] Connected to named pipe server.");
                
                string whatTheServerSay = await namedPipeClient.ReadMessageAsync().ConfigureAwait(false);
                Console.WriteLine("[CLIENT] The server says: {0}", whatTheServerSay);
                string reply = "Hey from the client";
                Console.WriteLine("[CLIENT] Telling the server: {0}", reply);
                await namedPipeClient.SendMessageAsync(reply);
            }
        }
    }
}
