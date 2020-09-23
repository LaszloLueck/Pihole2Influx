using System;
using System.Threading;
using System.Threading.Tasks;
using PrimS.Telnet;

namespace dck_pihole2influx.Transport.Telnet
{
    public class ConnectedTelnetClient : IConnectedTelnetClient
    {
        private readonly Client _client;

        public ConnectedTelnetClient(string telnetHost, int telnetPort)
        {
            _client = new Client(telnetHost, telnetPort, new CancellationToken());
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public async void WriteCommand(string command)
        {
            await _client.WriteLine(command);
        }

        public async Task<string> ReadResult(string terminator)
        {
            return await _client.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(100));
        }

        public async Task<bool> LoginOnTelnet(string userName, string password)
        {
            return await _client.TryLoginAsync(userName, password, 100);
        }

        public void DisposeClient()
        {
            _client.Dispose();
        }
    }
}