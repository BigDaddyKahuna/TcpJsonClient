using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;

namespace TcpJsonClient
{
    public class TcpServer
    {
        private int _port;
        private Socket _listener;
        private TcpServiceProvider _provider;
        private ArrayList _connections;
        private int _maxConnections = 100;
        private string _url;

        private AsyncCallback ConnectionReady;
        private WaitCallback AcceptConnection;
        private AsyncCallback ReceivedDataReady;

        /// <SUMMARY>
        /// Initializes server. To start accepting
        /// connections call Start method.
        /// </SUMMARY>
        public TcpServer(TcpServiceProvider provider, string url, int port)
        {
            _url = url;
            _provider = provider;
            _port = port;
            _listener = new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.Tcp);
            _connections = new ArrayList();
            ConnectionReady = new AsyncCallback(ConnectionReady_Handler);
            //AcceptConnection = new WaitCallback(AcceptConnection_Handler);
            //ReceivedDataReady = new AsyncCallback(ReceivedDataReady_Handler);
        }


        /// <SUMMARY>
        /// Start accepting connections.
        /// A false return value tell you that the port is not available.
        /// </SUMMARY>
        public bool Start()
        {
            try
            {

                IPHostEntry host = Dns.GetHostEntry(_url);
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

                Console.WriteLine("IP: " + _url);
                Console.WriteLine("Port: " + _port);

                bool IsRunning = true;
                while (IsRunning)
                {
                    try
                    {
                        _listener = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        //_listener.Bind(new IPEndPoint(IPAddress.Parse(_url), _port));
                        _listener.Bind(localEndPoint);
                        _listener.Listen(10);
                        _listener.ReceiveBufferSize = 1024;//SocketState.BUFFER_SIZE;
                        _listener.BeginAccept(new AsyncCallback(ConnectionReady), null);
                        new System.Threading.AutoResetEvent(false).WaitOne();
                        _listener.Close();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(string.Format("Exception in TcpServerConnection({0}).start(), error is {1} ", _listener, exc.Message));
                        IsRunning = false;
                    }
                }





/*

                _listener.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), _port));
                _listener.Listen(100);
                //_listener.BeginAccept(ConnectionReady, null);
                Console.WriteLine("Server started.");

                // Incoming data from the client.
                Socket handler = _listener.Accept();

                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }
                */

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Start failed: " + e.Message);
                Console.WriteLine(new System.Diagnostics.StackTrace().ToString());
                return false;
            }
        }


        /// <SUMMARY>
        /// Callback function: A new connection is waiting.
        /// </SUMMARY>
        private void ConnectionReady_Handler(IAsyncResult ar)
        {
            lock (this)
            {
                if (_listener == null)
                {
                    Console.WriteLine("_listener is null");
                    return;
                }

                if (_listener.Connected)
                {
                    Socket conn = _listener.EndAccept(ar);

                    if (_connections.Count >= _maxConnections)
                    {
                        Console.WriteLine("Max connections reached.");
                        //Max number of connections reached.
                        string msg = "SE001: Server busy";
                        conn.Send(Encoding.UTF8.GetBytes(msg), 0,
                                  msg.Length, SocketFlags.None);
                        conn.Shutdown(SocketShutdown.Both);
                        conn.Close();
                    }

                    else
                    {
                        Console.WriteLine("Start servicing a new connection");
                        //Start servicing a new connection
                        ConnectionState st = new ConnectionState();
                        st._conn = conn;
                        st._server = this;
                        st._provider = (TcpServiceProvider)_provider.Clone();
                        st._buffer = new byte[4];
                        _connections.Add(st);
                        //Queue the rest of the job to be executed latter
                        ThreadPool.QueueUserWorkItem(AcceptConnection, st);
                    }
                    //Resume the listening callback loop
                    _listener.BeginAccept(ConnectionReady, null);
                }
                


            }
        }


        /// <SUMMARY>
        /// Executes OnAcceptConnection method from the service provider.
        /// </SUMMARY>
        private void AcceptConnection_Handler(object state)
        {
            ConnectionState st = state as ConnectionState;
            try { st._provider.OnAcceptConnection(st); }
            catch
            {
                Console.WriteLine("_listener is null");//report error in provider... Probably to the EventLog
            }
            //Starts the ReceiveData callback loop
            if (st._conn.Connected)
                st._conn.BeginReceive(st._buffer, 0, 0, SocketFlags.None,
                  ReceivedDataReady, st);
        }


        /// <SUMMARY>
        /// Executes OnReceiveData method from the service provider.
        /// </SUMMARY>
        private void ReceivedDataReady_Handler(IAsyncResult ar)
        {
            ConnectionState st = ar.AsyncState as ConnectionState;
            st._conn.EndReceive(ar);
            //Im considering the following condition as a signal that the
            //remote host droped the connection.
            if (st._conn.Available == 0) DropConnection(st);
            else
            {
                try { st._provider.OnReceiveData(st); }
                catch
                {
                    Console.WriteLine("Error occurred.");
                }
                //Resume ReceivedData callback loop
                if (st._conn.Connected)
                    st._conn.BeginReceive(st._buffer, 0, 0, SocketFlags.None,
                      ReceivedDataReady, st);
            }
        }


        /// <SUMMARY>
        /// Shutsdown the server
        /// </SUMMARY>
        public void Stop()
        {
            lock (this)
            {
                _listener.Close();
                _listener = null;
                //Close all active connections
                foreach (object obj in _connections)
                {
                    ConnectionState st = obj as ConnectionState;
                    try { st._provider.OnDropConnection(st); }
                    catch
                    {
                        //some error in the provider
                    }
                    st._conn.Shutdown(SocketShutdown.Both);
                    st._conn.Close();
                }
                _connections.Clear();
            }
        }


        /// <SUMMARY>
        /// Removes a connection from the list
        /// </SUMMARY>
        internal void DropConnection(ConnectionState st)
        {
            lock (this)
            {
                st._conn.Shutdown(SocketShutdown.Both);
                st._conn.Close();
                if (_connections.Contains(st))
                    _connections.Remove(st);
            }
        }


        public int MaxConnections
        {
            get
            {
                return _maxConnections;
            }
            set
            {
                _maxConnections = value;
            }
        }


        public int CurrentConnections
        {
            get
            {
                lock (this) { return _connections.Count; }
            }
        }
    }
}