using System.Threading;
using System.Threading.Tasks;

namespace CodeWithSaar.IPC
{
    internal interface INamedPipeServerService : INamedPipeOperations
    {
        Task WaitForConnectionAsync(CancellationToken cancellationToken);
    }
}
