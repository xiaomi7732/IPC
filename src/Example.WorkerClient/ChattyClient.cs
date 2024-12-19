using CodeWithSaar.IPC;
using Example.DataContracts;

namespace Example.WorkerClient;

internal class ChattyClient : BackgroundService
{
    private readonly NamedPipeClientFactory _namedPipeClientFactory;
    private readonly ILogger<ChattyClient> _logger;

    public ChattyClient(
        NamedPipeClientFactory namedPipeClientFactory,
        ILogger<ChattyClient> logger)
    {
        _namedPipeClientFactory = namedPipeClientFactory ?? throw new ArgumentNullException(nameof(namedPipeClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (INamedPipeClientService namedPipeClient = _namedPipeClientFactory.CreateClient())
            {
                _logger.LogInformation("[CLIENT] Connecting to named pipe server...");
                try
                {

                    await namedPipeClient.ConnectAsync(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
                    _logger.LogInformation("[CLIENT] Connected to named pipe server.");
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Timeout connecting to server. Is the server up?");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
                    continue;
                }

                string whatTheServerSay = await namedPipeClient.ReadMessageAsync().ConfigureAwait(false);
                _logger.LogInformation("[CLIENT] The server says: {content}", whatTheServerSay);
                whatTheServerSay = await namedPipeClient.ReadMessageAsync().ConfigureAwait(false);
                _logger.LogInformation("[CLIENT] The server says(2): {content}", whatTheServerSay);

                string reply = "Hey from the client";
                _logger.LogInformation("[CLIENT] Telling the server: {content}", reply);
                await namedPipeClient.SendMessageAsync(reply);
                Customer serverCustomer = await namedPipeClient.ReadAsync<Customer>().ConfigureAwait(false);
                _logger.LogInformation("[CLIENT] Server customer name: {content}", serverCustomer.Name);

                // Send objects back and forth
                Customer clientCustomer = new()
                {
                    Name = "Client Customer",
                };
                await namedPipeClient.SendAsync(clientCustomer).ConfigureAwait(false);
            }

            // Wait for 5 sends before chat with the server again
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
        }
    }
}
