using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MischiefFramework.World.Information;
using System.Net;
using MischiefFramework.Cache;

namespace MischiefFramework.Networking {
    internal class GameClient : INetworkInterface {
        private TcpClient tcpClient;
        private NetworkStream clientStream;

        private Thread listenThread;

        private NetworkListenerController listeners = new NetworkListenerController();
        private List<NetworkMessage> outBox = new List<NetworkMessage>();

        public GameClient(string hostname) {
            try {
                //TODO: Don't hardcode port numbers
                tcpClient = new TcpClient(hostname, 3584);
                clientStream = tcpClient.GetStream();
                listenThread = new Thread(new ThreadStart(HandleCommunications));
                listenThread.Start();

                NetworkMessage nmo = new NetworkMessage(NetworkMessageTypes.ClientConnect);
                nmo.AddString(Player.Name);
                SendMessage(nmo);
            } catch (ArgumentNullException e) {
                System.Diagnostics.Debug.WriteLine("ArgumentNullException: {0}", e);
            } catch (SocketException e) {
                System.Diagnostics.Debug.WriteLine("SocketException: {0}", e);
            }
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

        public void Shutdown() {
            // Close everything.
            if(clientStream != null)
                clientStream.Close();

            if(tcpClient != null)
                tcpClient.Close();

            System.Diagnostics.Debug.Write("Waiting...");
            listenThread.Join();
            System.Diagnostics.Debug.WriteLine("Done");
        }

        public void HandleCommunications() {
            byte[] thisMessage = new byte[512];
            int bytesRead;

            int errors = 0;

            try {
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

                            lock (clientStream) {
                                if (clientStream.CanWrite) {
                                    clientStream.Write(outBytes, 0, length);
                                }
                            }
                        }

                        outBox.RemoveRange(0, totalMessages);
                    }

                    // Buffer to store the response bytes.
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
                                bytesRead = clientStream.Read(thisMessage, 0, 2);
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

                            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(thisMessage, 0));

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
                            System.Diagnostics.Debug.WriteLine("Recv: {0}", nm);

                            listeners.DeployMessage(nm);
                        }
                    }

                    Thread.Yield();
                }
            } catch {
                Shutdown();
            }
        }
    }
}
