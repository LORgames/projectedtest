using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Core;

namespace MischiefFramework.Cache {
    internal class Player {
        private static PlayerInput input;
        internal static string Name = "";

        internal static PlayerInput Input {
            get { return input; }
            set { if (input != null) throw new Exception("Game Already Started..."); input = value; }
        }
    }
}
