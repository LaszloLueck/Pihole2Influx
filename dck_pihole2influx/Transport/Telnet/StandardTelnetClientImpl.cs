using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using dck_pihole2influx.Logging;
using Optional;

namespace dck_pihole2influx.Transport.Telnet
{
    public interface IStandardTelnetClientFactory
    {
        StandardTcpClientImpl Build();
    }

    public sealed class StandardTelnetClientFactory : IStandardTelnetClientFactory
    {
        public StandardTcpClientImpl Build()
        {
            return new();
        }
    }


    public sealed class StandardTcpClientImpl
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<StandardTcpClientImpl>.GetLogger();

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

        public Option<bool> Connect(string hostOrIp, int port)
        {
            try
            {
                _tcpClient = new TcpClient();

                if(!_tcpClient.ConnectAsync(hostOrIp, port).Wait(500)) throw new TimeoutException("Connection failure");
                _tcpClient.ReceiveTimeout = 5000;
                _tcpClient.SendTimeout = 1000;
                _stream = _tcpClient.GetStream();
                return Option.Some(true);
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Error while connecting to <{hostOrIp}:{port}>");
                return Option.None<bool>();
            }
        }

        public void CloseAndDisposeStream()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public Option<bool> WriteCommand(PiholeCommands command)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(TelnetCommands.GetCommandByName(command) + "\n");
                _stream.Write(data, 0, data.Length);
                return Option.Some(true);
            }
            catch (SocketException exception)
            {
                Log.Error(exception, "Write timeout while sending some data to client");
                return Option.None<bool>();
            }
        }

        public Option<string> ReceiveDataSync(PiholeCommands message, string terminator)
        {
            try
            {
                var retValue = new StringBuilder();
                var received = new byte[256];
                while (_stream.Read(received, 0, received.Length) > 0)
                {
                    var tmp = Encoding.UTF8.GetString(received);
                    received = new byte[256];
                    retValue.Append(tmp.Replace("\0", ""));
                    if (tmp.Contains(terminator)) break;
                }

                return Option.Some(retValue.ToString());
            }
            catch (IOException exception)
            {
                Log.Error(exception, "Read timeout while reading a network stream");
                return Option.None<string>();
            }
        }
    }
}