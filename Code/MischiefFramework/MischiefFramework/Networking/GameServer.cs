using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MischiefFramework.World.Information;
using MischiefFramework.Cache;

namespace MischiefFramework.Networking {
    internal class GameServer : INetworkInterface {
        private TcpListener tcpListener;

        private Thread listenThread;

        private List<NetworkStream> clients = new List<NetworkStream>();
        private NetworkListenerController listeners = new NetworkListenerController();

        private List<NetworkMessage> outBox = new List<NetworkMessage>();

        public GameServer() {
            GameInformation.myPlayerID = 0;

            //TODO: Get port number from settings or something?
            this.tcpListener = new TcpListener(IPAddress.Any, 3584);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            //this.listenThread = new Thread(new ThreadStart(ListenForClients));
            //this.listenThread.Start();
        }

        public void AddListener(INetworkListen obj) {
            listeners.AddListener(obj);
        }

        public void RemoveListener(INetworkListen obj) {
            listeners.RemoveListener(obj);
        }

        public void SendMessage(NetworkMessage message) {
            lock (outBox) {
                outBox.Add(message);
            }
        }

        public void SendMessageTo(NetworkStream clientStream, NetworkMessage message) {
            lock (clientStream) {
                if (clientStream.CanWrite) {
                    byte[] outBytes;
                    int length;
                    message.Encode(out outBytes, out length);

                    clientStream.Write(outBytes, 0, length);
                    System.Diagnostics.Debug.WriteLine("Sent: {0}", message);
                }
            }
        }

        public void Shutdown() {
            if (tcpListener != null) {
                tcpListener.Stop();
                tcpListener = null;
            }

            lock (clients) {
                foreach (NetworkStream ns in clients) {
                    ns.Close();
                }

                clients.Clear();
            }

            listenThread.Join();
        }

        private void ListenForClients() {
            this.tcpListener.Start(2);

            while (this.tcpListener != null) {
                if (tcpListener.Pending()) {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //TODO: single thread that checks all the clients rather than 1 client per thread
                    //create a thread to handle communication with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }

                Thread.Sleep(1000);
            }
        }

        private void HandleClientComm(object client) {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] messageSize = new byte[2];

            lock (clients) {
                clients.Add(clientStream);
            }

            int bytesRead;

            int errors = 0;

            while (errors == 0) {
                lock (outBox) {
                    int totalMessages = outBox.Count;

                    for (int i = 0; i < totalMessages; i++) {
                        NetworkMessage message = outBox[i];

                        byte[] outBytes;

                        // Send the message to the connected TcpServer.
                        int length;

                        message.Encode(out outBytes, out length);
                        System.Diagnostics.Debug.WriteLine("Sent: {0}", message);

                        lock (clients) {
                            foreach (NetworkStream _cli in clients) {
                                lock (_cli) {
                                    if (_cli.CanWrite) {
                                        _cli.Write(outBytes, 0, length);
                                    }
                                }
                            }
                        }

                        lock (listeners) {
                            message.Flip();
                            listeners.DeployMessage(message);
                        }
                    }

                    if (totalMessages > 0) {
                        outBox.RemoveRange(0, totalMessages);
                    }
                }

                bytesRead = 0;

                bool hasReading;

                lock (clientStream) {
                    hasReading = clientStream.DataAvailable;
                }

                //Nothing to read, so just continue
                if (!hasReading) {
                    Thread.Yield();
                    continue;
                }

                lock (clientStream) {
                    while (clientStream.DataAvailable) {
                        try {
                            //blocks until a client sends a message
                            bytesRead = clientStream.Read(messageSize, 0, 2);
                        } catch {
                            //a socket error has occured
                            errors++;
                            break;
                        }

                        if (bytesRead == 0) {
                            //the client has disconnected from the server
                            errors++;
                            break;
                        }

                        int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(messageSize, 0));
                        byte[] thisMessage = new byte[length];

                        try {
                            bytesRead = clientStream.Read(thisMessage, 0, length);
                        } catch {
                            //a socket error has occured
                            errors++;
                            break;
                        }

                        if (bytesRead == 0) {
                            errors++;
                            break;
                        }

                        NetworkMessage nm = new NetworkMessage(thisMessage);
                        nm.Flip();
                        SendMessage(nm);
                    }
                }

                Thread.Yield();
            }

            clientStream.Close();
            tcpClient.Close();
        }
    }
}
