using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LineDowntime.Common
{
    internal class UDPClient
    {
        public delegate void Callback(string input);
        private static CancellationTokenSource cts = new CancellationTokenSource();
        public static async void ReadData(Callback callback)
        {
            try
            {
                IPAddress localAddr = Settings.UDPAddress.Address;
                int listenPort = Settings.UDPAddress.Port;

                IPEndPoint localEndPoint = new IPEndPoint(localAddr, listenPort);

                using (UdpClient listener = new UdpClient(localEndPoint))
                {
                    try
                    {

                        while (!cts.Token.IsCancellationRequested)
                        {
                            UdpReceiveResult result = await listener.ReceiveAsync();
                            byte[] bytes = result.Buffer;
                            string receivedData = Encoding.UTF8.GetString(bytes);
                            callback(receivedData);
                        }
                    }
                    catch
                    {
                        throw; // Do NOT throw new exception, it will destroy stack trace. Stupid ass C#
                    }
                    finally
                    {
                        listener.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
                throw; // uncomment this or add messagebox if needed
            }
            finally
            {
                await Task.Delay(5000);
                ReadData(callback);
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
