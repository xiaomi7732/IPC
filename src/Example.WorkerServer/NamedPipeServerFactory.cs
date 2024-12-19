using CodeWithSaar.IPC;

namespace Example.WorkerServer;

internal class NamedPipeServerFactory
{
  private readonly IServiceProvider _serviceProvider;

  public NamedPipeServerFactory(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
  }

  public INamedPipeServerService CreateNamedPipeServerService()
  {
    return ActivatorUtilities.CreateInstance<DuplexNamedPipeService>(_serviceProvider);
  }
}