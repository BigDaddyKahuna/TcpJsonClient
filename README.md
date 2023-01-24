# TcpJsonClient
C# program to test sending json to a TCP client

This was orginally written to prototype the logic for sending JSON, via TCP, to a specific IP:Port.  
Pieces of this were then brought into our framework code to perform that functionality.  
Note that the listen feature is incomplete and does not work.

Notes:
The IP:Port values and send data are currently hard coded.  The data to send is converted from a data bean (POJO) into JSON by (https://www.newtonsoft.com/json).

The receiver used to test the output is PortPeeker.  (https://portpeeker.software.informer.com/).  

Sample Output:

C:\dev\code\TeamProject\DevProjects\TcpJsonClient\TcpJsonClient\bin\Debug>TcpJsonClient send

Args size: 1

Main args: send

Socket connected to 192.168.0.177:18000

Data Sent

Sent 59 bytes to server.

Done 

This shows the help:

C:\dev\code\TeamProject\DevProjects\TcpJsonClient\TcpJsonClient\bin\Debug>TcpJsonClient

Args size: 0

Enter one of the two values:

    'send' to send data.

    'listen' to receive data. Also needs URL and Port.
