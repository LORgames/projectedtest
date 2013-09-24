using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Networking;

namespace MischiefFramework.World.Information {
    internal class GameInformation {
        internal static int myPlayerID = 0;

        internal static bool isNetworkGame = false;
        internal static INetworkInterface networkInterface = null;
        internal static int TotalPlayers = 2;
    }
}
