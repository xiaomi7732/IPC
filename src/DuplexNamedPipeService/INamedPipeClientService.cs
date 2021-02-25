using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWithSaar.IPC
{
    internal interface INamedPipeClientService: INamedPipeOperations
    {
        Task ConnectAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
