using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.World.Information;

namespace MischiefFramework.Networking {
    internal interface INetworkInterface {
        void AddListener(INetworkListen obj);
        void RemoveListener(INetworkListen obj);

        void SendMessage(NetworkMessage message);
        void Shutdown();
    }
}
