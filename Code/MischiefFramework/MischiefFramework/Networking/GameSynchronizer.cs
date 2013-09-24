using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MischiefFramework.World.Information;

namespace MischiefFramework.Networking {
    internal class GameSynchronizer : INetworkListen {
        private Thread internalUpdateThread_1s;
        private bool isServer = false;

        public GameSynchronizer(INetworkInterface ini) {
            ini.AddListener(this);

            isServer = ini is GameServer;

            if (isServer) {
                internalUpdateThread_1s = new Thread(new ThreadStart(ServerPerSecond));
            } else {
                internalUpdateThread_1s = new Thread(new ThreadStart(ClientPerSecond));
            }
        }

        public void ServerPerSecond() {
            //TODO: Actually sync something here :)
            //Ping All
            Thread.Sleep(1000);
        }

        public void ClientPerSecond() {
            //TODO: Actually sync something here :)
            //Ping
            Thread.Sleep(1000);
        }

        public bool OnData(NetworkMessage data) {
            //Look for a ping thing

            return false;
        }

        public void Stop() {
            internalUpdateThread_1s.Abort();
        }
    }
}
