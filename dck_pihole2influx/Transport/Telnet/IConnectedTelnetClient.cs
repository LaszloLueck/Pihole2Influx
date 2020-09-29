using System.Threading.Tasks;

namespace dck_pihole2influx.Transport.Telnet
{
    public interface IConnectedTelnetClient
    {
        bool IsConnected();

        Task<bool> LoginOnTelnet(string userName, string password);

        Task WriteCommand(string command);

        Task WriteCommand(PiholeCommands command);

        Task<string> ReadResult(string terminator);

        void DisposeClient();

        void Dispose();
    }
}