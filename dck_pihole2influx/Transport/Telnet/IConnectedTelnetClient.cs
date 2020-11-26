using System.Threading.Tasks;

namespace dck_pihole2influx.Transport.Telnet
{
    public interface IConnectedTelnetClient
    {

        void Connect(string telnetHost, int telnetPort);
        bool IsConnected();

        Task<bool> LoginOnTelnet(string userName, string password);

        Task WriteCommand(PiholeCommands command);

        Task<string> ReadResult(string terminator);

        void ClientDispose();
    }
}