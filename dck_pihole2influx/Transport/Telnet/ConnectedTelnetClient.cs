using System.Threading;
using System.Threading.Tasks;
using PrimS.Telnet;

namespace dck_pihole2influx.Transport.Telnet
{

    public class TelnetClientFactory
    {
        public IConnectedTelnetClient GetClient()
        {
            return new ConnectedTelnetClient();
        }
    }
    
    public class ConnectedTelnetClient : IConnectedTelnetClient
    {
        private Client _client;

        public void Connect(string telnetHost, int telnetPort)
        {
            _client = new Client(telnetHost, telnetPort, new CancellationToken());
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public Task WriteCommand(PiholeCommands command)
        {
            return _client.WriteLine(TelnetCommands.GetCommandByName(command));
        }

        public async Task<string> ReadResult(string terminator)
        {
            return await _client.TerminatedReadAsync(terminator);
        }

        public async Task<bool> LoginOnTelnet(string userName, string password)
        {
            return await _client.TryLoginAsync(userName, password,100).ConfigureAwait(false);
        }
        public void ClientDispose()
        {
            _client.Dispose();
        }
    }
}