using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;

namespace CammeraControlWFP.Businesslogic
{
    public class CameraHandler
    {
        private static TcpClient client;
        private Action<string> _callback;
        private static NetworkStream clientStream;

        public CameraHandler( Action<string> callback)
        {
            _callback = callback;
        }

        public void startPoling()
        {
            Connect();
            clientStream = client.GetStream();
            Int32 bytes;
            String responseData = String.Empty;
            Byte[] data = new Byte[256];

            while (true)
            {
                bytes = clientStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                // Read the second batch of the TcpServer response bytes. 
                bytes = clientStream.Read(data, 0, data.Length);
                responseData = responseData + System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                //response data sampel: 2 codes with content "ABC-abc-1234"
                //1234L000000043{0D}{0A}1234star;1;ABC-abc-1234;ABC-abc-1234;stop
                //ticket + length + /r/n + ticket + data + /r/n
                //extract data from receive string
                //extract length information
                int lengthData = int.Parse(responseData.Substring(5, 9));
                //extract data from string
                string stringData = responseData.Substring(20, (lengthData - 6));
                if (stringData.Equals("star;0;;stop"))
                {
                    _callback("No data available");
                }
                else
                {
                    _callback(stringData);
                }

            }
        }
        private static void Connect()
        {
            //Open connection to O2I
            client = new TcpClient();
            //connection data O2I50x
            client.Connect("10.151.1.130", 50010);
        }
    }
}
