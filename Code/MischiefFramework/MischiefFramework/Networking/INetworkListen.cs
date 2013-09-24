using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MischiefFramework.Networking {
    internal interface INetworkListen {
        bool OnData(NetworkMessage data);
    }
}
