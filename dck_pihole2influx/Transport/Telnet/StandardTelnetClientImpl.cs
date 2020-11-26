using System.Net.Sockets;
using System.Text;

namespace dck_pihole2influx.Transport.Telnet
{
    public class StandardTelnetClientFactory
    {
        public StandardTcpClientImpl Build()
        {
            return new StandardTcpClientImpl();
        }
    }
    
    
    public sealed class StandardTcpClientImpl
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private bool IsTcpClientConnected()
        {
            return _tcpClient.Connected;
        }

        private void DisconnectTcpClient()
        {
            _tcpClient.Close();
        }

        public void CloseAndDisposeTcpClient()
        {
            if (IsTcpClientConnected())
                DisconnectTcpClient();

            _tcpClient.Dispose();
        }

        public void Connect(string hostOrIp, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(hostOrIp, port);
            _stream = _tcpClient.GetStream();
        }

        public void CloseAndDisposeStream()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public void WriteCommand(PiholeCommands command)
        {
            var data = Encoding.UTF8.GetBytes(TelnetCommands.GetCommandByName(command) + "\n");
            _stream.Write(data, 0, data.Length);
        }
        
        public string ReceiveDataSync(PiholeCommands message, string terminator)
        {
            
            var retValue = new StringBuilder();
            var received = new byte[256];
            while (_stream.Read(received, 0, received.Length) > 0)
            {
                var tmp = Encoding.UTF8.GetString(received);
                received = new byte[256];
                retValue.Append(tmp.Replace("\0",""));
                if (tmp.Contains(terminator)) break;
            }
            return retValue.ToString();
        }
    }
}