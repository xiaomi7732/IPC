using CodeWithSaar.IPC;
using Example.DataContracts;
using Microsoft.Extensions.Options;

namespace Example.WorkerServer;

internal class IPCServer : BackgroundService
{
    private readonly NamedPipeOptions _options;
    private readonly NamedPipeServerFactory _namedPipeServerFactory;
    private readonly ILogger<IPCServer> _logger;

    public IPCServer(
        IOptions<NamedPipeOptions> options,
        NamedPipeServerFactory namedPipeServerFactory,
        ILogger<IPCServer> logger)
    {
        _namedPipeServerFactory = namedPipeServerFactory ?? throw new ArgumentNullException(nameof(namedPipeServerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (INamedPipeServerService namedPipeServer = _namedPipeServerFactory.CreateNamedPipeServerService())
            {
                // Send messages back and forth
                _logger.LogInformation("[SERVER] Waiting for connection.");
                await namedPipeServer.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("[SERVER] Connected.");

                _logger.LogInformation("[SERVER] Sending greeting...");
                await namedPipeServer.SendMessageAsync("Hello~from server").ConfigureAwait(false);

                _logger.LogWarning("[SERVER] Taking a break. Why is this needed?");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);

                await namedPipeServer.SendMessageAsync("Hello again from server").ConfigureAwait(false);
                _logger.LogInformation("[SERVER] Greeting sent.");

                _logger.LogInformation("[SERVER] Guessing what will the client say...");
                string whatTheClientSay = await namedPipeServer.ReadMessageAsync().ConfigureAwait(false);
                _logger.LogInformation("[SERVER] The client says: {0}", whatTheClientSay);

                // Send objects back and forth
                Customer serverCustomer = new Customer()
                {
                    Name = "Server Customer",
                };
                await namedPipeServer.SendAsync(serverCustomer).ConfigureAwait(false);
                Customer clientCustomer = await namedPipeServer.ReadAsync<Customer>().ConfigureAwait(false);
                _logger.LogInformation("[SERVER] Client customer name: {customerName}", clientCustomer.Name);
            }
            _logger.LogInformation("Done an iteration. Move on to wait for the next connection.");
        }
    }
}
