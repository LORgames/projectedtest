using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Globalization;
using System.IO;

namespace MischiefFramework.Cache {
    internal class ResourceManager {
        internal static ContentManager Content;

        internal static void SetContent(ContentManager nContent) {
            Content = nContent;
        }

        internal static T LoadLocalizedAsset<T>(string assetName) {
            string[] cultureNames = {
                CultureInfo.CurrentCulture.Name,                        // eg. "en-AU"
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName     // eg. "en"
            };

            // Look first for a specialized language-country version of the asset,
            // then if that fails, loop back around to see if we can find one that
            // specifies just the language without the country part.
            foreach (string cultureName in cultureNames) {
                string localizedAssetName = assetName + '.' + cultureName;

                try {
                    return Content.Load<T>(localizedAssetName);
                } catch (ContentLoadException) { } catch (System.IO.FileNotFoundException) { }
            }

            // If we didn't find any localized asset, fall back to the default name.
            return LoadAsset<T>(assetName);
        }

        internal static T LoadAsset<T>(string assetName) {
            // If we didn't find any asset, return default.
            try {
                return Content.Load<T>(assetName);
            } catch (ContentLoadException) { } catch (System.IO.FileNotFoundException) { }

            return default(T);
        }

        internal static void Flush() {
            Content.Unload();
        }
    }
}
