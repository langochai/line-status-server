using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NB_TestTruyenThong.Uc
{
    public delegate void ReceiveDataEvents_Server(string data);
    
    public class CSocketServer
    {
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

        public ReceiveDataEvents_Server OnReceiveDataEvents_Server;
        #endregion

        #region FUNCTIONS
        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        public CSocketServer()
        {
            _buffer = new byte[BUFFER_SIZE];
            _clientSockets = new List<Socket>();
        }

        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        /// <param name="portNumber">Địa chỉ Cổng kết nối</param>
        /// <param name="protocolType">Kiểu kết nối</param>
        /// <param name="isHex">Dữ liệu có phải kiểu Hexa hay ko?</param>
        public CSocketServer(int portNumber, ProtocolType protocolType = ProtocolType.Tcp, bool isHex = false)
        {
            Port = portNumber;
            IsHex = isHex;
            ProtocolType = protocolType;
            _buffer = new byte[BUFFER_SIZE];
            _clientSockets = new List<Socket>();
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
        /// Gửi dữ liệu cho toàn bộ client
        /// </summary>
        /// <param name="data">Dữ liệu gửi</param>
        public void SendAll(byte[] data)
        {
            try
            {
                foreach (Socket socket in _clientSockets)
                    socket.Send(data);
            }
            catch { }
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
                if (OnReceiveDataEvents_Server != null)
                    OnReceiveDataEvents_Server(text);

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
        /// <summary>
        /// Đóng lại toàn bộ kết nối với client
        /// </summary>
        private void CloseAllSockets()
        {
            try
            {
                foreach (Socket socket in _clientSockets)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                _clientSockets.Clear();
                _serverSocket.Close();
            }
            catch
            {
            }
        }

        #region PUBLIC FUNCTIONS        
        /// <summary>
        /// Khởi chạy server
        /// </summary>
        ///<returns>
        ///<para>True: Thành công</para>
        ///<para>False: Thất bại</para>
        ///</returns>
        public bool Start()
        {
            try
            {
                if (_serverSocket == null) _serverSocket = new Socket(AddressFamily.InterNetwork,
                    (ProtocolType == ProtocolType.Tcp ? SocketType.Stream : SocketType.Dgram), ProtocolType);

                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _serverSocket.Listen(100);
                _serverSocket.BeginAccept(AcceptCallback, null);
                IsConnected = true;
                return true;
            }
            catch
            {
                IsConnected = false;
                return false;
            }
        }
        /// <summary>
        /// Đóng server
        /// </summary>
        public void Stop()
        {
            CloseAllSockets();
            _serverSocket = null;
            IsConnected = false;
        }
        #endregion

        #endregion
    }
}
