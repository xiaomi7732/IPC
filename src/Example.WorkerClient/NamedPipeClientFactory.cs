using CodeWithSaar.IPC;

namespace Example.WorkerClient;

internal class NamedPipeClientFactory
{
  private readonly IServiceProvider _serviceProvider;

  public NamedPipeClientFactory(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
  }

  public INamedPipeClientService CreateClient() => ActivatorUtilities.CreateInstance<DuplexNamedPipeService>(_serviceProvider);
}