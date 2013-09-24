using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MischiefFramework.Networking {
    internal enum NetworkMessageTypes {
        None,           // Error Packet 
        Unknown,        // Send this packet when an unknown happens
        GameInformation,// The game information [playerCount]
        ClientConnect,  // Connect to server [ClientName]
        ClientLeave,    // A client is leaving
        Chat,           // A chat message
    }
}
