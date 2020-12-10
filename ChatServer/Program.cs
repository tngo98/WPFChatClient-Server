//
// FILE			:	Program.cs
// PROJECT		:	Windows Programming - PROG2121 - Assignment #5
// PROGRAMMER	:	Julian Lichty, Muhammad Mamooji
// DESCRIPTION	:	This file contains methods for the server side logic for the chat box application
//


//Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

//Main Program
namespace ChatServer
{
    class Program
    {
        private static bool keepListening = true;
        private static List<Thread> clientThreads = new List<Thread>();
        private static List<TcpClient> clients = new List<TcpClient>();
        private static List<NetworkStream> clientStreams = new List<NetworkStream>();

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        static void Main(string[] args)
        {
            //Set Server to NULL
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("192.168.0.10");
                server = new TcpListener(localAddr, port);

                //Start listening for client requests
                server.Start();

                //Enter the listening loop
                while (true)
                {
                    Console.Write("Waiting for a connection... \n");

                    // Perform a blocking call to accept requests.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!\n");

                    clients.Add(client);

                    NetworkStream stream = client.GetStream();
                    clientStreams.Add(stream);

                    //Create a new thread to listen to the client
                    Thread newThread = new Thread(new ParameterizedThreadStart(Worker));

                    //Add the thread to the list and start the thread with the client's stream
                    clientThreads.Add(newThread);
                    newThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}\n", e);
            }
            finally
            {
                //Stop listening for new clients
                server.Stop();
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static void Worker(Object o)
        {
            //Variables
            TcpClient client = (TcpClient)o;
            byte[] bytes = new byte[256];
            string data = string.Empty;
            NetworkStream stream = client.GetStream();
            int i = 0;

            while (keepListening)
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("\nReceived: {0}", data);

                    if (data == "/[+ClientEnd+]/")
                    {
                        for (int s = 0; s < clientStreams.Count; s++)
                        {
                            if (clientStreams[s] == stream)
                            {
                                stream.Close();
                                client.Close();
                                clientThreads[s].Interrupt();
                            }
                        }
                    }
                    else
                    {
                        //Loop through clientStreams list
                        for (int s = 0; s < clientStreams.Count; s++)
                        {
                            //check that the current element in clientStreams isn't the same stream used in this thread
                            if (clientStreams[s] != stream)
                            {
                                //Convert the msg recieved from the client to bytes (with userID, according to the server, attached)
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(("Client " + s + "/[+UserId+]/" + data));

                                //Send the message to the other clients
                                clientStreams[s].Write(msg, 0, msg.Length);
                            }

                        }

                    }

                }
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }
}
