using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MischiefFramework.Core.Interfaces;
using MischiefFramework.Cache;
using ZDataTypes;

namespace MischiefFramework.World.Information {
    internal class InfoPanel : IHeadsUpDisplay {
        internal static InfoPanel instance;
        string header;
        int headerIcon;
        List<string> stats;
        List<int> upgradeIcons;
        SpriteFont headerFont;
        SpriteFont mainStatFont;
        SpriteFont statsFont;
        float DESC_COLUMN_WIDTH = 150.0f;
        SpritesheetPosition[] positions;
        Texture2D ninjaSpritesheet;

        Color brownText = new Color(60, 43, 16);

        const int MAX_INDEX_OF_MAIN_STATS = 10;

        bool visible = false;

        public InfoPanel() {
            instance = this;
            this.headerFont = ResourceManager.LoadAsset<SpriteFont>("Fonts/InfoPanelHeader");
            this.mainStatFont = ResourceManager.LoadAsset<SpriteFont>("Fonts/InfoPanelMainStat");
            this.statsFont = ResourceManager.LoadAsset<SpriteFont>("Fonts/InfoPanelStats");
            //ninjaSpritesheet = ResourceManager.LoadAsset<Texture2D>("HUD/Ninja/SpriteSheet");
            //positions = ResourceManager.LoadAsset<SpritesheetPosition[]>("HUD/Ninja/positions");
        }

        public void ChangeInformation(string header, int headerIcon, List<string> stats, List<int> upgradeIcons) {
            this.header = header;
            this.headerIcon = headerIcon;
            this.stats = stats;
            this.upgradeIcons = upgradeIcons;
            this.visible = true;
        }

        public void Hide() {
            visible = false;
        }

        public void Show() {
            visible = true;
        }

        public bool IsVisible() {
            return visible;
        }

        private Rectangle FindSourcePosition(string name) {
            Rectangle sourcePos = Rectangle.Empty;
            for (int i = 0; i < positions.Length; i++) {
                if (positions[i].Name == name) {
                    sourcePos.X = positions[i].X;
                    sourcePos.Y = positions[i].Y;
                    sourcePos.Width = positions[i].Width;
                    sourcePos.Height = positions[i].Height;
                }
            }
            return sourcePos;
        }

        public void RenderHeadsUpDisplay(SpriteBatch drawtome) {
            if (visible) {
                // Draw background
                Rectangle backgroundSourcePos = FindSourcePosition("InfoPanel");
                Vector2 backgroundDestPos = Vector2.Zero;
                backgroundDestPos.X = (Game.device.Viewport.Width - backgroundSourcePos.Width) / 2.0f;
                backgroundDestPos.Y = (Game.device.Viewport.Height - backgroundSourcePos.Height) / 2.0f;
                drawtome.Draw(ninjaSpritesheet, backgroundDestPos, backgroundSourcePos, Color.White);

                // Draw header icon
                Rectangle headerIconSourcePos = FindSourcePosition("Icon" + headerIcon.ToString());
                Vector2 headerIconDestPos = backgroundDestPos;
                headerIconDestPos.X += 48;
                headerIconDestPos.Y += 45;
                drawtome.Draw(ninjaSpritesheet, headerIconDestPos, headerIconSourcePos, Color.White);

                // Draw header text
                Vector2 headerDestPos = backgroundDestPos;
                headerDestPos.X += 130f;
                headerDestPos.Y += 60f;
                drawtome.DrawString(headerFont, header, headerDestPos, brownText);

                // Draw stats
                for (int i = 0; i < Math.Min(MAX_INDEX_OF_MAIN_STATS, stats.Count); i += 2) {
                    Vector2 statsDestPos = headerIconDestPos;
                    statsDestPos.Y += i == 0 ? 100f : 110f + mainStatFont.MeasureString(stats[0]).Y + (statsFont.MeasureString(stats[i]).Y - 4f) * (i / 2 - 1);
                    statsDestPos.X += 10f;
                    drawtome.DrawString(i == 0 ? mainStatFont : statsFont, stats[i], statsDestPos, brownText);
                    statsDestPos.X += DESC_COLUMN_WIDTH;
                    drawtome.DrawString(i == 0 ? mainStatFont : statsFont, stats[i + 1], statsDestPos, Color.White);
                }

                // Draw upgrade icons
                Vector2 upgradeIconsDestPos = backgroundDestPos + new Vector2(57, 394);
                for (int i = 0; i < upgradeIcons.Count; i++) {
                    Rectangle upgradeIconsSourcePos = FindSourcePosition("Icon" + upgradeIcons[i].ToString());
                    drawtome.Draw(ninjaSpritesheet, upgradeIconsDestPos, upgradeIconsSourcePos, Color.White);
                    upgradeIconsDestPos.X += 72;
                }

                // Draw upgrade icon (next to the upgrade text)
                Vector2 upgradeIconDestPos = backgroundDestPos + new Vector2(305f, 335f);
                Rectangle upgradeIconSourcePos = FindSourcePosition("InfoPanelUpgradeIcon");
                drawtome.Draw(ninjaSpritesheet, upgradeIconDestPos, upgradeIconSourcePos, Color.White);
            }
        }
    }
}
