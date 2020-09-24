using System;
using System.Threading;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using Optional;
using PrimS.Telnet;
using Serilog;

namespace dck_pihole2influx.Transport.Telnet
{
    public class TelnetClient
    {
        private static readonly ILogger Log = LoggingFactory<TelnetClient>.CreateLogging();
        private readonly string _telnetHost;
        private readonly int _telnetPort;
        private readonly string _piholeUser;
        private readonly string _piholePassword;


        public TelnetClient(string telnetHost, int telnetPort, string piholeUser, string piholePassword)
        {
            _telnetHost = telnetHost;
            _telnetPort = telnetPort;
            _piholeUser = piholeUser;
            _piholePassword = piholePassword;
        }

        public async Task<Option<string>> ConnectAndReceiveData(TelnetCommands.PiholeCommands key)
        {
            Serilog.Log.Information($"Connect to Telnet-Host at {_telnetHost}:{_telnetPort}");
            var client = new Client(_telnetHost, _telnetPort, new CancellationToken());
            if (client.IsConnected)
            {
                var t = Task.Run(async () =>
                {
                    if (_piholeUser.Length != 0 && _piholePassword.Length != 0)
                    {
                        await client.TryLoginAsync(_piholeUser, _piholePassword, 100);
                    }

                    await client.WriteLine(TelnetCommands.GetCommandByName(key));

                    var s = await client.TerminatedReadAsync("---EOM---\n", TimeSpan.FromMilliseconds(100));

                    await client.WriteLine(TelnetCommands.GetCommandByName(TelnetCommands.PiholeCommands.Quit));
                    
                    client.Dispose();
                    
                    return Option.Some(s);
                });
                return await t;
            }
            else
            {
                return await Task.Run(Option.None<string>);
            }
        }
    }
}