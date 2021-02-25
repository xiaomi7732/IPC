using System;
using System.Threading.Tasks;

namespace CodeWithSaar.IPC
{
    internal interface INamedPipeOperations
    {
        Task SendMessageAsync(string message);
        Task<string> ReadMessageAsync();
    }
}
