using System;
using TcpJsonClient;

public class Program
{
    public static int Main(String[] args)
    {
        Console.WriteLine("Args size: " + args.Length);
        if (null == args || args.Length == 0)
        {
            printHelp();
        }
        else
        {
            Console.WriteLine("Main args: " + args[0]);
            string mode = args[0].ToLower();
            if (mode.Equals("send"))
            {               

                    // The IP for the remote device
                    string host = "int.base";

                    // The port number for the remote device.  
                    int port = 18000;

                    BoatClass boat = new BoatClass("BoatName", "BoatOwner", "1092");

                    AsynchronousClient client = new AsynchronousClient();
                    client.StartClient(host, port, boat);
            }
            else if (mode.Equals("listen"))
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide a URL and a port value.");
                }
                else
                {
                    string url = args[1];
                    int port = Convert.ToInt32(args[2]);
                    ServiceProvider provider = new ServiceProvider();
                    TcpServer tcpServer = new TcpServer(provider, url, port);
                    tcpServer.Start();
                }

            }
            else
            {
                printHelp();
            }
        }
        return 0;
    }

    private static void printHelp()
    {
        Console.WriteLine("Enter one of the two values: ");
        Console.WriteLine("    'send' to send data.");
        Console.WriteLine("    'listen' to receive data. Also needs URL and Port.");
    }




}