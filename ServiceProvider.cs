using System;
using System.Text;


namespace TcpJsonClient
{
    /// <SUMMARY>
    /// EchoServiceProvider. Just replies messages
    /// received from the clients.
    /// </SUMMARY>
    public class ServiceProvider : TcpServiceProvider
    {
        private string _receivedStr;

        public override object Clone()
        {
            return new ServiceProvider();
        }

        public override void
               OnAcceptConnection(ConnectionState state)
        {
            Console.WriteLine("OnAcceptConnection");
            _receivedStr = "";
            if (!state.Write(Encoding.UTF8.GetBytes(
                            "Hello World!\r\n"), 0, 14))
                state.EndConnection();
            //if write fails... then close connection
        }


        public override void OnReceiveData(ConnectionState state)
        {
            Console.WriteLine("OnReceiveData");
            byte[] buffer = new byte[1024];
            while (state.AvailableData > 0)
            {
                int readBytes = state.Read(buffer, 0, 1024);
                if (readBytes > 0)
                {
                    _receivedStr +=
                      Encoding.UTF8.GetString(buffer, 0, readBytes);
                    if (_receivedStr.IndexOf("<EOF>") >= 0)
                    {
                        state.Write(Encoding.UTF8.GetBytes(_receivedStr), 0,
                        _receivedStr.Length);
                        _receivedStr = "";
                    }
                }
                else
                {
                    Console.WriteLine("Read failed, closing connection.");
                    state.EndConnection();
                }
                //If read fails then close connection
            }
        }


        public override void OnDropConnection(ConnectionState state)
        {
            Console.WriteLine("OnDropConnection");
            //Nothing to clean here
        }
    }
}