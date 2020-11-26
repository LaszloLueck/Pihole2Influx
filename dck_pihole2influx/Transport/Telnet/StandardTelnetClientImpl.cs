using System;
using System.Net.Sockets;
using System.Text;

namespace dck_pihole2influx.Transport.Telnet
{
    public class StandardTcpClientImpl
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _stream;


        public StandardTcpClientImpl()
        {
            _tcpClient = new TcpClient();
        }

        private bool IsTcpClientConnected()
        {
            return _tcpClient.Connected;
        }

        public void DisconnectTcpClient()
        {
            _tcpClient.Close();
        }

        public void DisposeTcpClient()
        {
            if (IsTcpClientConnected())
                DisconnectTcpClient();

            _tcpClient.Dispose();
        }

        public void Connect(string hostOrIp, int port)
        {
            _tcpClient.Connect(hostOrIp, port);
            _stream = _tcpClient.GetStream();
        }

        public void CloseAndDisposeStream()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public string ReceiveDataSync(PiholeCommands message)
        {
            byte[] data = Encoding.UTF8.GetBytes(TelnetCommands.GetCommandByName(message) + "\n");
            
            _stream.Write(data, 0, data.Length);
            var retValue = new StringBuilder();
            bool i = true;
            while(i)
            {
                if (!_stream.DataAvailable) continue;
                byte[] rcvd = new byte[256];
                while (_stream.Read(rcvd, 0, rcvd.Length) > 0)
                {
                    var tmp = Encoding.UTF8.GetString(rcvd);
                    Array.Clear(rcvd, 0 ,rcvd.Length);
                    retValue.Append(tmp.Replace("\0",""));

                    if (!tmp.Contains("---EOM---")) continue;
                    i = false;
                    break;
                }
            }
            byte[] quit = Encoding.UTF8.GetBytes(TelnetCommands.GetCommandByName(PiholeCommands.Quit) + "\n");
            _stream.Write(quit, 0, quit.Length);
            return retValue.ToString();
        }
    }
}