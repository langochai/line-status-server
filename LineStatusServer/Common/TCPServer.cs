using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LineStatusServer.Common
{
    public class TCPServer
    {
        public delegate void Callback(string input);

        #region VARIABLES
        private Socket _serverSocket;
        private List<Socket> _clientSockets;
        int BUFFER_SIZE = 1024;
        private byte[] _buffer;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Địa chỉ cổng kết nối
        /// </summary>
        public int Port { get; set; } = 0;
        public ProtocolType ProtocolType { get; set; }
        /// <summary>
        /// Cờ báo dữ liệu là string hay hexa
        /// </summary>
        public bool IsHex { get; set; } = false;
        /// <summary>
        /// Cờ báo có đang kết nối hay ko?
        /// </summary>
        public bool IsConnected { get; set; } = false;

        public Callback Callback_Server;
        #endregion

        private static CancellationTokenSource cts = new CancellationTokenSource();

        public static async void ReadData(Callback callback)
        {
            try
            {
                //IPAddress localAddr = Settings.UDPAddress.Address;
                //int listenPort = Settings.UDPAddress.Port;

                //IPEndPoint localEndPoint = new IPEndPoint(localAddr, listenPort);

                //using (UdpClient listener = new UdpClient())
                //{
                //    listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //    listener.Client.Bind(localEndPoint);
                //    try
                //    {
                //        while (!cts.Token.IsCancellationRequested)
                //        {
                //            UdpReceiveResult result = await listener.ReceiveAsync();
                //            byte[] bytes = result.Buffer;
                //            string receivedData = Encoding.UTF8.GetString(bytes);
                //            callback(receivedData);
                //        }
                //    }
                //    catch
                //    {
                //        throw; // Do NOT throw new exception, it will destroy stack trace. Stupid ass C#
                //    }
                //    finally
                //    {
                //        listener.Close();
                //    }
                //}
                //IPAddress localAddr = IPAddress.Any;
                //int listenPort = Settings.UDPAddress.Port;
                //ProtocolType protocolType = ProtocolType.Tcp;
                //Socket _serverSocket = new Socket(AddressFamily.InterNetwork, (protocolType == ProtocolType.Tcp ? SocketType.Stream : SocketType.Dgram), protocolType);

                //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                //_serverSocket.Bind(localEndPoint);
                //_serverSocket.Listen(100);
                //_serverSocket.BeginAccept(AcceptCallback, null);


            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
                throw; // uncomment this or add messagebox if needed
            }
            finally
            {
                if ((!cts.Token.IsCancellationRequested))
                {
                    await Task.Delay(5000);
                    ReadData(callback);
                }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;
            if (_serverSocket == null) return;
            try
            {
                socket = _serverSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _clientSockets.Add(socket);
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        bool IsSocketConnected(Socket s)
        {
            try
            {
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm nhận dữ liệu từ client
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket current = (Socket)ar.AsyncState;
            int received;
            //string ip = current.RemoteEndPoint.ToString();
            if (!IsSocketConnected(current))
            {
                current.Close();
                _clientSockets.Remove(current);
                return;
            }
            try
            {
                received = current.EndReceive(ar);
            }
            catch (SocketException)
            {
                current.Close();
                _clientSockets.Remove(current);
                return;
            }
            string text = string.Empty;
            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            text = IsHex ? BitConverter.ToString(recBuf).Replace("-", "") : Encoding.ASCII.GetString(recBuf);

            if (!string.IsNullOrEmpty(text))
                if (Callback_Server != null)
                    Callback_Server(text);

            try
            {
                current.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
            catch (Exception)
            {
                current.Close();
                _clientSockets.Remove(current);
                return;
            }
        }

        public static void StopConnection()
        {
            cts.Cancel();
        }

        public static void ResetConnection(Callback callback)
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            ReadData(callback);
        }
    }
}