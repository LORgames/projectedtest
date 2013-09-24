using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MischiefFramework.World.Information {
    interface IInfoPanel {
        string GetHeader();
        int GetHeaderIcon();
        List<string> GetStats();
        List<int> GetUpgrades();
    }
}
