using CodeWithSaar.IPC;
using Example.WorkerClient;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleConsole(simpleConsole =>
{
  simpleConsole.SingleLine = true;
});

builder.Services.AddOptions().Configure<NamedPipeOptions>(opt =>
{
  // Demo hardcoded namedpipe name:
  opt.PipeName = "hello_namedpipe_service";
});

builder.Services.AddSingleton<NamedPipeClientFactory>();
builder.Services.AddHostedService<ChattyClient>();

var host = builder.Build();
host.Run();
