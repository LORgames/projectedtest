using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using MischiefFramework.Networking;

namespace MischiefFramework.Cache {
    internal static class SettingManager {
        internal static bool _yInverted = false;
        internal static float _lookSensitivity = 0.5f;
        internal static float _musicVolume = 0.5f;
        internal static float _soundEffectsVolume = 0.5f;
        internal static bool _musicMute = false;
        internal static bool _soundEffectsMute = false;
        internal static string _language = "en_AU";

        internal static string _playerName = "Player";
        internal static string _lastIP = "127.0.0.1";

        internal static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MischiefFramework\\";
        internal static string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MischiefFramework\\settings.bin";

        public static void LoadFromFile() {
            if (File.Exists(filename)) {
                byte[] data = File.ReadAllBytes(filename);

                NetworkMessage savedData = new NetworkMessage(data, 2);

                //REMEMBER TO ENSURE BACKWARD COMPATIBILITY AND ADD NULLS IF YOU REMOVE SOMETHING
                //   AND TO ADD BOOLS TO THE END OF BOOLS SECTION AND <ANYTHING ELSE> AT THE VERY
                //   END OF THE LIST

                //Read the boolean values (upto 32?)
                int bools = savedData.GetInt();

                _yInverted = ((bools >> 1) & 1) > 0;
                _musicMute = ((bools >> 1) & 2) > 0;
                _soundEffectsMute = ((bools >> 3) & 1) > 0;

                //Read some floats
                _lookSensitivity = savedData.GetFloat();
                _musicVolume = savedData.GetFloat();
                _soundEffectsVolume = savedData.GetFloat();

                //Read some strings
                _language = savedData.GetString();
                _playerName = savedData.GetString();
                _lastIP = savedData.GetString();
            }
        }

        public static void SaveToFile() {
            NetworkMessage savedData = new NetworkMessage(NetworkMessageTypes.GameInformation);

            //Save the boolean values (upto 32?)
            int bools = 0;

            bools += (_yInverted ? 1 : 0) << 0;
            bools += (_musicMute ? 1 : 0) << 1;
            bools += (_soundEffectsMute ? 1 : 0) << 2;

            savedData.AddInt(bools);

            //Read some floats
            savedData.AddFloat(_lookSensitivity);
            savedData.AddFloat(_musicVolume);
            savedData.AddFloat(_soundEffectsVolume);

            //Read some strings
            savedData.AddString(_language);
            savedData.AddString(_playerName);
            savedData.AddString(_lastIP);

            int seven;
            byte[] bytes;
            savedData.Encode(out bytes, out seven);

            if (!Directory.Exists(filepath)) {
                Directory.CreateDirectory(filepath);
            }

            File.WriteAllBytes(filename, bytes);
        }
    }
}
