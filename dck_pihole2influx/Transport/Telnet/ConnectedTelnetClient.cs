using System.Threading;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using PrimS.Telnet;
using Serilog;

namespace dck_pihole2influx.Transport.Telnet
{
    public class ConnectedTelnetClient : IConnectedTelnetClient
    {
        private static readonly ILogger Log = LoggingFactory<ConnectedTelnetClient>.CreateLogging();
        private readonly Client _client;

        public ConnectedTelnetClient(string telnetHost, int telnetPort)
        {
            Log.Information($"Connect to Telnet-Host at {telnetHost}:{telnetPort}");
            _client = new Client(telnetHost, telnetPort, new CancellationToken());
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public async Task WriteCommand(string command)
        {
            await _client.WriteLine(command);
        }

        public async Task WriteCommand(PiholeCommands command)
        {
            await _client.WriteLine(TelnetCommands.GetCommandByName(command));
        }

        public async Task<string> ReadResult(string terminator)
        {
            return await _client.TerminatedReadAsync(terminator);
        }

        public async Task<bool> LoginOnTelnet(string userName, string password)
        {
            return await _client.TryLoginAsync(userName, password,100).ConfigureAwait(false);
        }

        public void DisposeClient()
        {
            _client.Dispose();
        }

        public void ClientDispose()
        {
            DisposeClient();
        }
    }
}