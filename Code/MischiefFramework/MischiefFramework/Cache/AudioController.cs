using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace MischiefFramework.Cache {
    internal class AudioController {
        private static bool disableAudio = false;
        private static List<SoundEffectInstance> currentlyActiveLoops = new List<SoundEffectInstance>();

        internal static void PlayOnce(string filename, float volumeMultiplier = 1.0f) {
            if (disableAudio) return;

            try {
                ResourceManager.LoadAsset<SoundEffect>("Sounds/"+filename).Play(volumeMultiplier * SettingManager._soundEffectsVolume, 0.0f, 0.0f);
            } catch {
                disableAudio = true;
            }
        }

        internal static void PlayLooped(string filename, float volumeMultiplier = 1.0f) {
            if (disableAudio) return;

            try {
                SoundEffectInstance bg = ResourceManager.LoadAsset<SoundEffect>("Sounds/" + filename).CreateInstance();
                bg.IsLooped = true;
                bg.Volume = volumeMultiplier * SettingManager._musicVolume;
                bg.Play();

                currentlyActiveLoops.Add(bg);
            } catch {
                disableAudio = true;
            }
        }
    }
}
