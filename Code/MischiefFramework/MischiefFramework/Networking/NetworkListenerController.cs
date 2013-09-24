using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MischiefFramework.Networking {
    internal class NetworkListenerController {
        internal List<INetworkListen> listeners = new List<INetworkListen>();
        private List<INetworkListen> addListeners = new List<INetworkListen>();
        private List<INetworkListen> removeListeners = new List<INetworkListen>();

        private object lockVar = new object();

        internal void DeployMessage(NetworkMessage message) {
            lock (lockVar) {
                foreach (INetworkListen listener in addListeners) {
                    listeners.Add(listener);
                }

                foreach (INetworkListen listener in removeListeners) {
                    listeners.Remove(listener);
                }

                addListeners.Clear();
                removeListeners.Clear();

                int size = listeners.Count;

                foreach (INetworkListen listener in listeners) {
                    listener.OnData(message);
                }
            }
        }

        internal void AddListener(INetworkListen asset) {
            lock (lockVar) {
                addListeners.Add(asset);
            }
        }

        internal void RemoveListener(INetworkListen asset) {
            lock (lockVar) {
                removeListeners.Add(asset);
            }
        }
    }
}
