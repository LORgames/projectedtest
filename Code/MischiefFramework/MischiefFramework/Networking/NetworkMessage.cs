using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MischiefFramework.Networking {
    internal class NetworkMessage {
        internal NetworkMessageTypes Type = NetworkMessageTypes.None;
        private List<Byte> out_data;
        private Byte[] in_data;
        private int seemlessReadIndex = 0;

        internal NetworkMessage(Byte[] received, int ignoreAtStart = 0) {
            in_data = received;
            Type = (NetworkMessageTypes)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(in_data, ignoreAtStart));
            seemlessReadIndex = 2 + ignoreAtStart;
        }

        internal NetworkMessage(NetworkMessageTypes messageType) {
            out_data = new List<byte>();
            out_data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)messageType)));
            Type = messageType;
        }

        internal void Encode(out Byte[] outBytes, out int length) {
            if (out_data != null) {
                length = out_data.Count;

                out_data.InsertRange(0, BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length)));
                length += 2;
                outBytes = out_data.ToArray();
            } else {
                throw new Exception("MISSING OUTDATA!");
            }
        }

        internal void AddInt(int number) {
            out_data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(number)));
        }

        internal int GetInt(int index) {
            seemlessReadIndex += 4;
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(in_data, index));
        }

        internal int GetInt() {
            return GetInt(seemlessReadIndex);
        }

        internal void AddFloat(float number) {
            out_data.AddRange(BitConverter.GetBytes(number));
        }

        internal float GetFloat(int index) {
            seemlessReadIndex += 4;
            return BitConverter.ToSingle(in_data, index);
        }

        internal float GetFloat() {
            return GetFloat(seemlessReadIndex);
        }

        internal void AddString(string Message) {
            Byte[] encoded = Encoding.UTF8.GetBytes(Message);
            out_data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)encoded.Length)));
            out_data.AddRange(encoded);
        }

        internal string GetString(int index) {
            //Get length as NUMBER OF BYTES
            short length = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(in_data, index));
            seemlessReadIndex += 2 + length;

            return Encoding.UTF8.GetString(in_data, index + 2, length);
        }

        internal string GetString() {
            return GetString(seemlessReadIndex);
        }

        public override string ToString() {
            if(in_data != null) {
                return "[NMI: " + Type + " " + in_data.Length + "B]";
            } else {
                return "[NMO: " + Type + " " + out_data.Count + "B]";
            }
        }

        internal void Flip() {
            if (out_data != null) {
                out_data.RemoveRange(0, 4);
                in_data = out_data.ToArray();
                out_data.Clear();
                out_data = null;
                seemlessReadIndex = 0;
            } else if(in_data != null) {
                out_data = new List<byte>();
                out_data.AddRange(in_data);
                in_data = null;
            }
        }
    }
}
