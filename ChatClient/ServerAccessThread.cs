using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace ChatServerPrototype1.ChatClient
{
    using ChatServerPrototype1.Utilities;
    public class ServerAccessThread
    {
        private Socket socket;
	    private String userName;
        private DataContractSerializer serializer = null;
        private NetworkStream networkStream = null;
        public ServerAccessThread(Socket socket, String userName)
        {
            this.socket = socket;
            this.userName = userName;
        }

        public void Run() 
        {
            Console.WriteLine("Welcome " + userName);
            Console.WriteLine("Client connected to server {0}",
                        socket.RemoteEndPoint.ToString());
            Console.WriteLine("Begin chatting.");

            serializer = new DataContractSerializer(typeof(Message));
            networkStream = new NetworkStream(socket);
            
            while (socket.Connected)
            {
                if (networkStream.DataAvailable)
                {

                    Message? response = receiveIncomingData();
                    if (response is Message valueOfResponse)
                    {
                        Console.WriteLine("Echoed from server: {0}",
                            valueOfResponse.TheMessage);
                    }
                }
            }
                
        }

        private Message? receiveIncomingData()
        {
            Message? newMessage = null;
            var memStream = new MemoryStream();
            try {
                readBytesInto(ref memStream);
                newMessage = deserializeFromFirstXMLelement(ref memStream);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace);
            }
            finally 
            {
                memStream?.Dispose();
            }
            return newMessage;
            
        }

        private void readBytesInto(ref MemoryStream memStream)
        {
            // read in chunks of 2kb
            int buffSize = 2048;
            byte[] buffer1 = new byte[buffSize];
            int bytesRead;
            int offset = 0;
            
            do {
                bytesRead = networkStream.Read(buffer1, 0, buffSize);                    
                memStream.Write(buffer1, offset, bytesRead);
                System.Array.Clear(buffer1, 0, buffSize);
                offset += bytesRead;
            } 
            while (networkStream.DataAvailable);
        }

        private Message? deserializeFromFirstXMLelement(ref MemoryStream memStream)
        {
            Message? msg = null;
            byte[] bMessage = memStream.ToArray();

            XmlDictionaryReader xmlDictReader = XmlDictionaryReader.CreateTextReader(bMessage, 0, bMessage.Length, new XmlDictionaryReaderQuotas());
            while (xmlDictReader.Read())
            {
                switch (xmlDictReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (serializer.IsStartObject(xmlDictReader))
                        {
                            Console.WriteLine("Found start element");
                            try
                            {
                                msg = (Message)serializer.ReadObject(xmlDictReader);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Problem deserializing Message from client \n");
                                Console.WriteLine(e.ToString());
                                Console.WriteLine("Exception message:\n{0}",e.Message);
                                Console.WriteLine("Stack trace:\n{0}", e.StackTrace); 
                                break;                               
                            }
                        }
                        Console.WriteLine("Message deserialized successfully");
                        break;
                }
            }
            return msg;            
        }

    }
}
