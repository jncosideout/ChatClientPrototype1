using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace ChatServerPrototype1.ChatClient
{
    using ChatServerPrototype1.Utilities;
    public class Client
    {
        private static readonly string host = "localhost";
        private static readonly int portNumber = 4444;

        private string userName;
        private string serverHost;
        private int serverPort;
        private DataContractSerializer serializer = null;
        private NetworkStream networkStream = null;

        public static void Main(String[] args)
        {
            string readName = null;
            Console.WriteLine("Please input username:");
            readName = "alex";//Console.ReadLine();

            while (String.IsNullOrEmpty(readName)) 
            {
               readName = Console.ReadLine();
               if (readName.Trim().Equals(""))
               {
                   Console.WriteLine("Invalid username. Please try again:");
               }
            }
            Client client = new Client(readName, host, portNumber);
            client.StartClient();
        }

        private Client(string userName, string serverHost, int serverPort)
        {
            this.userName = userName;
            this.serverHost = serverHost;
            this.serverPort = serverPort;
        }

        private void StartClient()
        {

            try 
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addre
                IPHostEntry host = Dns.GetHostEntry(Client.host);
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEP = new IPEndPoint(ipAddress, portNumber);

                // Create a TCP/IP socket
                TcpClient tcpClient = new TcpClient();

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to remote endpoint
                    tcpClient.Connect(localEP);
                    ServerAccessThread serverAccessThread = new ServerAccessThread(tcpClient.Client, userName);
                    Thread workThread = new Thread(new ThreadStart(serverAccessThread.Run));
                    workThread.Start();
                    serializer = new DataContractSerializer(typeof(Message));
                    networkStream = tcpClient.GetStream();
                    
                    serializeOutgoingData("auto-message from client 1");//testASB
                    while (workThread.IsAlive) 
                    {
                        String nextSend = Console.ReadLine();
                        if (nextSend == "end")
                        {
                            break;
                        }
                        serializeOutgoingData(nextSend);   
                    }
                    // Release the socket

                    networkStream.Close();
                    tcpClient.Close();                      
                } 
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("Argument Null Exception: {0}",
                        ane.ToString());
                    Console.WriteLine("Exception message:\n{0}",ane.Message);
                    Console.WriteLine("Stack trace:\n{0}",ane.StackTrace);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Socket Exception: {0}",
                        se.ToString());
                    Console.WriteLine("Exception message:\n{0}",se.Message);
                    Console.WriteLine("Stack trace:\n{0}", se.StackTrace);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception {0}",
                        e.ToString());
                    Console.WriteLine("Exception message:\n{0}",e.Message);
                    Console.WriteLine("Stack trace:\n{0}", e.StackTrace);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in first try: {0}",
                    e.ToString());
                Console.WriteLine("Exception message:\n{0}", e.Message);
                Console.WriteLine("Stack trace:\n{0}", e.StackTrace);
            }
        }
        private void serializeOutgoingData(String nextSend) 
        {
            Message message = new Message(nextSend); 
            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.WriteObject(memStream, message); 
                string xmlOutput = toXML(message); 
                Console.WriteLine(xmlOutput);                 
                byte[] bMessage = memStream.ToArray();
                networkStream.Write(bMessage, 0, bMessage.Length);
            }
        }


        private string toXML(Message message)
        {
            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) {Formatting = Formatting.Indented})
            {
                serializer.WriteObject(writer, message);
                return output.GetStringBuilder().ToString();
            }
        }       


    }// EoC


}// EoNamespace
