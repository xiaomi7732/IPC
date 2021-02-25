using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeWithSaar.IPC
{
    internal sealed class DuplexNamedPipeService : INamedPipeServerService, INamedPipeClientService, IDisposable
    {
        private SemaphoreSlim _threadSafeLock = new SemaphoreSlim(1, 1);
        private PipeStream _pipeStream;
        private readonly NamedPipeOptions _options;
        private readonly ILogger _logger;
        private NamedPipeRole _currentMode = NamedPipeRole.NotSpecified;

        public DuplexNamedPipeService(IOptions<NamedPipeOptions> namedPipeOptions, ILogger<DuplexNamedPipeService> logger)
        {
            _options = namedPipeOptions?.Value ?? throw new ArgumentNullException(nameof(namedPipeOptions));
            _logger = logger;
        }

        public async Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _threadSafeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                switch (_currentMode)
                {
                    case NamedPipeRole.Client:
                        throw new InvalidOperationException("Can't wait for connection on a client.");
                    case NamedPipeRole.Server:
                        throw new InvalidOperationException("Can't setup a server for a second time.");
                    case NamedPipeRole.NotSpecified:
                    default:
                        break;
                }

                _currentMode = NamedPipeRole.Server;
                NamedPipeServerStream serverStream = new NamedPipeServerStream(_options.PipeName, PipeDirection.InOut, maxNumberOfServerInstances: 1, transmissionMode: PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                _pipeStream = serverStream;
                await serverStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _threadSafeLock.Release();
            }
        }


        public async Task ConnectAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (timeout.TotalMilliseconds > Int32.MaxValue)
            {
                throw new InvalidOperationException($"Specified timeout of {timeout.TotalMilliseconds}ms exceeded the maximum allowed: {Int32.MaxValue}ms.");
            }
            try
            {
                await _threadSafeLock.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
                switch (_currentMode)
                {
                    case NamedPipeRole.Client:
                        throw new InvalidOperationException("A connection is already established.");
                    case NamedPipeRole.Server:
                        throw new InvalidOperationException("Can't connect to another server from a server.");
                    case NamedPipeRole.NotSpecified:
                    default:
                        break;
                }

                _currentMode = NamedPipeRole.Client;
                NamedPipeClientStream clientStream = new NamedPipeClientStream(serverName: ".", _options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                _pipeStream = clientStream;

                await clientStream.ConnectAsync((int)timeout.TotalMilliseconds, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _threadSafeLock.Release();
            }
        }

        public async Task<string> ReadMessageAsync()
        {
            VerifyModeIsSpecified();
            string line;
            using (StreamReader reader = new StreamReader(_pipeStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: -1, leaveOpen: true))
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
            }
            return line;
        }

        public async Task SendMessageAsync(string message)
        {
            VerifyModeIsSpecified();

            using (StreamWriter writer = new StreamWriter(_pipeStream, encoding: Encoding.UTF8, bufferSize: -1, leaveOpen: true))
            {
                await writer.WriteLineAsync(message).ConfigureAwait(false);
            }
        }

        private void VerifyModeIsSpecified()
        {
            if (_currentMode is NamedPipeRole.NotSpecified)
            {
                throw new InvalidOperationException("Pipe mode requires to be set before operations. Either wait for connection as a server or connect to a server as a client before reading or writing messages.");
            }
        }

        public void Dispose()
        {
            _threadSafeLock?.Dispose();
            _threadSafeLock = null;

            _pipeStream?.Dispose();
            _pipeStream = null;

        }
    }
}
