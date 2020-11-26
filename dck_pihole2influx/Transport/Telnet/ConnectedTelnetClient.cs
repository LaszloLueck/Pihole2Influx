using System;
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
        private Lazy<Client> _client;

        public void Connect(string telnetHost, int telnetPort)
        {
            _client = new Lazy<Client>(() => new Client(telnetHost, telnetPort, new CancellationToken()));
        }

        public bool ValueIsCreated()
        {
            return _client.IsValueCreated;
        }

        public bool IsConnected()
        {
            return _client.Value.IsConnected;
        }

        public Task WriteCommand(PiholeCommands command)
        {
            return _client.Value.WriteLine(TelnetCommands.GetCommandByName(command));
        }

        public Task<string> ReadResult(string terminator)
        {
            return _client.Value.TerminatedReadAsync(terminator);
        }

        public async Task<bool> LoginOnTelnet(string userName, string password)
        {
            return await _client.Value.TryLoginAsync(userName, password,100).ConfigureAwait(false);
        }
        public void ClientDispose()
        {
            _client.Value.Dispose();
        }
    }
}