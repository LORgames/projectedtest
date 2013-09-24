using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.Cache;

namespace MischiefFramework.Core.Helpers {
    internal enum Positions {
        TOPLEFT,
        TOPCENTER,
        TOPRIGHT,
        CENTERLEFT,
        CENTER,
        CENTERRIGHT,
        BOTTOMLEFT,
        BOTTOMCENTER,
        BOTTOMRIGHT
    }

    class MenuHelper {
        private abstract class Item {
            internal Type type;
            internal string itemString; // The header in most cases
            internal Vector2 itemStringSize; // The size of the item string
            internal Vector2 itemStringPosition; // The position of the item string
            internal Vector2 position; // Entire Item position
            internal Vector2 size; // Entire Item size
            internal Delegate startMethod; // The method that is used when the player presses start
            internal Delegate startedMethod; // The method that is used to determine if the player has pressed start, mostly just for the bars
            internal Delegate activeMethod; // To determine if the item is active, whether it can be selected or not
            internal SpriteFont font;
            internal Color defaultColor; // colour used when the item is not selected
            internal Color selectedColor; // colour used when the item is selected
            internal Color deactivatedColor; // colour used when the item can't be selected. NOTE: Color.Transparent used for when item shouldn't be shown

            internal Item(Type type) {
                this.type = type;
            }

            internal abstract void UpdateSize(); // Update internal positions and size
            internal abstract void UpdatePosition(Viewport vp, Vector2 menuSize, float yOffset, Positions menuPosition); // Update external position
            internal abstract void UpdateXInput(bool positive); // Not always implemented, since not every item does something when the left or right keys are used.
            internal abstract void UpdateMouseClicked(Vector2 mousePosition, bool getStartHeld); // Only called when the MouseClicked position is over this item.
            internal abstract void Render(SpriteBatch spritebatch, int index, int current_menu_item);
            internal Vector2 GetSize() {
                return size;
            }

            internal Vector2 GetPosition() {
                return position;
            }
        }

        private class TextItem : Item {

            internal TextItem() : base(typeof(TextItem)) {
            }

            internal override void UpdateSize() {
                itemStringSize = font.MeasureString(itemString);
                size = itemStringSize;
            }

            internal override void UpdatePosition(Viewport vp, Vector2 menuSize, float yOffset, Positions menuPosition) {
                // Set the X value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.CENTERLEFT || menuPosition == Positions.BOTTOMLEFT) {
                    itemStringPosition.X = vp.X;
                } else if (menuPosition == Positions.TOPCENTER || menuPosition == Positions.CENTER || menuPosition == Positions.BOTTOMCENTER) {
                    itemStringPosition.X = (vp.Width - itemStringSize.X) / 2.0f;
                } else {
                    itemStringPosition.X = vp.Width - itemStringSize.X;
                }

                // Set the Y value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.TOPCENTER || menuPosition == Positions.TOPRIGHT) {
                    itemStringPosition.Y = vp.Y + yOffset;
                } else if (menuPosition == Positions.CENTERLEFT || menuPosition == Positions.CENTER || menuPosition == Positions.CENTERRIGHT) {
                    itemStringPosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                } else {
                    itemStringPosition.Y = vp.Height - menuSize.Y + yOffset;
                }

                position = itemStringPosition;
            }

            internal override void UpdateXInput(bool positive) {
                // There are no options used by X input
            }

            internal override void UpdateMouseClicked(Vector2 mousePosition, bool getStartHeld) {
                // The event has already been verified, the mouse clicked inside this object.
                if (this.startMethod != null && !getStartHeld) {
                    this.startMethod.DynamicInvoke(null);
                }
            }

            internal override void Render(SpriteBatch spritebatch, int index, int current_menu_item) {
                if ((bool)this.activeMethod.DynamicInvoke(null) || (!(bool)this.activeMethod.DynamicInvoke(null) && this.deactivatedColor != Color.Transparent)) {
                    Color color = (bool)this.activeMethod.DynamicInvoke(null) ? current_menu_item == index ? selectedColor : defaultColor : deactivatedColor;
                    spritebatch.DrawString(font, itemString, itemStringPosition, color);
                }
            }
        }

        private class BasicBarItem : Item {
            internal Texture2D empty;
            internal Texture2D full;
            internal Vector2 barSize;
            internal Rectangle emptyRect;
            internal Rectangle fullRect;
            internal float MAX_VALUE;
            internal float MIN_VALUE;
            internal float INCREMENT;
            internal float currentValue;
            internal Delegate currentValueMethod;
            internal Delegate updateValueMethod;

            internal BasicBarItem() : base(typeof(BasicBarItem)) {
            }

            internal void UpdateValue() {
                currentValue = (float)this.currentValueMethod.DynamicInvoke(null);
                fullRect.Width = (int)((currentValue / (MAX_VALUE - MIN_VALUE)) * barSize.X);
            }

            internal override void UpdateSize() {
                itemStringSize = font.MeasureString(itemString);
                this.size.X = itemStringSize.X > barSize.X ? itemStringSize.X : barSize.X;
                this.size.Y = itemStringSize.Y + barSize.Y;
            }

            internal override void UpdatePosition(Viewport vp, Vector2 menuSize, float yOffset, Positions menuPosition) {
                // Set the X value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.CENTERLEFT || menuPosition == Positions.BOTTOMLEFT) {
                    itemStringPosition.X = vp.X;
                    emptyRect.X = vp.X;
                    fullRect.X = vp.X;
                } else if (menuPosition == Positions.TOPCENTER || menuPosition == Positions.CENTER || menuPosition == Positions.BOTTOMCENTER) {
                    itemStringPosition.X = (vp.Width - itemStringSize.X) / 2.0f;
                    emptyRect.X = (int)((vp.Width - barSize.X) / 2.0f);
                    fullRect.X = (int)((vp.Width - barSize.X) / 2.0f);
                } else {
                    itemStringPosition.X = vp.Width - itemStringSize.X;
                    emptyRect.X = (int)(vp.Width - barSize.X);
                    fullRect.X = (int)(vp.Width - barSize.X);
                }

                // Set the Y value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.TOPCENTER || menuPosition == Positions.TOPRIGHT) {
                    itemStringPosition.Y = vp.Y + yOffset;
                    emptyRect.Y = (int)(vp.Y + yOffset + itemStringSize.Y);
                    fullRect.Y = (int)(vp.Y + yOffset + itemStringSize.Y);
                } else if (menuPosition == Positions.CENTERLEFT || menuPosition == Positions.CENTER || menuPosition == Positions.CENTERRIGHT) {
                    itemStringPosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                    emptyRect.Y = (int)((vp.Height - menuSize.Y) / 2.0f + yOffset + itemStringSize.Y);
                    fullRect.Y = (int)((vp.Height - menuSize.Y) / 2.0f + yOffset + itemStringSize.Y);
                } else {
                    itemStringPosition.Y = vp.Height - menuSize.Y + yOffset;
                    emptyRect.Y = (int)(vp.Height - menuSize.Y + yOffset + itemStringSize.Y);
                    fullRect.Y = (int)(vp.Height - menuSize.Y + yOffset + itemStringSize.Y);
                }

                position.X = itemStringPosition.X < emptyRect.X ? itemStringPosition.X : emptyRect.X;
                position.Y = itemStringPosition.Y < emptyRect.Y ? itemStringPosition.Y : emptyRect.Y;
            }

            internal override void UpdateXInput(bool positive) {
                if (startedMethod != null) {
                    if ((bool)startedMethod.DynamicInvoke(null)) {
                        this.startMethod.DynamicInvoke(null);
                    }
                }

                currentValue = (float)this.currentValueMethod.DynamicInvoke(null);

                if (positive) {
                    currentValue += INCREMENT;
                } else {
                    currentValue -= INCREMENT;
                }
                this.updateValueMethod.DynamicInvoke(new object[] { currentValue });
                UpdateValue();
            }

            internal override void UpdateMouseClicked(Vector2 mousePosition, bool getStartHeld) {
                float xMin = this.emptyRect.X;
                float yMin = this.emptyRect.Y;
                float xMax = this.emptyRect.X + this.emptyRect.Width;
                float yMax = this.emptyRect.Y + this.emptyRect.Height;

                if (mousePosition.X >= xMin && mousePosition.X <= xMax && mousePosition.Y >= yMin && mousePosition.Y <= yMax) {
                    currentValue = (float)Math.Round((double)((mousePosition.X - emptyRect.X) / barSize.X * (MAX_VALUE - MIN_VALUE) * (MAX_VALUE / INCREMENT))) / (MAX_VALUE / INCREMENT);
                    this.updateValueMethod.DynamicInvoke(new object[] { currentValue });
                    UpdateValue();
                } else {
                    if (startMethod != null && !getStartHeld) {
                        startMethod.DynamicInvoke(null);
                        UpdateValue();
                    }
                }
            }

            internal override void Render(SpriteBatch spritebatch, int index, int current_menu_item) {
                if ((bool)this.activeMethod.DynamicInvoke(null) || (!(bool)this.activeMethod.DynamicInvoke(null) && this.deactivatedColor != Color.Transparent)) {
                    Color color = (bool)this.activeMethod.DynamicInvoke(null) ? current_menu_item == index ? selectedColor : defaultColor : deactivatedColor;
                    spritebatch.DrawString(font, itemString, itemStringPosition, color);
                    spritebatch.Draw(empty, emptyRect, Color.White);
                    spritebatch.Draw(full, fullRect, Color.White);
                }
            }
        }

        private class SubListItem : Item {
            internal List<string> items;
            internal List<Vector2> itemSizes;
            internal List<Vector2> itemPositions;
            internal Color defaultItemColor;
            internal Color selectedItemColor;
            internal int currentValue;
            internal Delegate currentValueMethod;
            internal Delegate updateValueMethod;

            internal SubListItem() : base(typeof(SubListItem)) {
                items = new List<string>();
                itemSizes = new List<Vector2>();
                itemPositions = new List<Vector2>();
            }

            internal void UpdateValue() {
                currentValue = (int)this.currentValueMethod.DynamicInvoke(null);
            }

            internal override void UpdateSize() {
                itemStringSize = font.MeasureString(itemString);

                size.X = 0;
                for (int i = 0; i < items.Count; i++) {
                    itemSizes[i] = font.MeasureString(items[i]);
                    size.X += itemSizes[i].X;
                }
                size.X += (items.Count - 1) * 50;

                if (itemStringSize.X > size.X) {
                    size.X = itemStringSize.X;
                }
                
                this.size.Y = itemStringSize.Y + itemSizes[0].Y;
            }

            internal override void UpdatePosition(Viewport vp, Vector2 menuSize, float yOffset, Positions menuPosition) {
                // Set the X value
                float listWidth = 0.0f;
                for (int i = 0; i < items.Count; i++) {
                    if (i == 0) {
                        listWidth += itemSizes[i].X;
                    } else {
                        listWidth += 50 + itemSizes[i].X;
                    }
                }

                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.CENTERLEFT || menuPosition == Positions.BOTTOMLEFT) {
                    itemStringPosition.X = vp.X;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        if (i > 0) {
                            temp.X = itemPositions[i - 1].X + itemSizes[i - 1].X + 50f;
                        } else {
                            temp.X = vp.X;
                        }
                        itemPositions[i] = temp;
                    }
                } else if (menuPosition == Positions.TOPCENTER || menuPosition == Positions.CENTER || menuPosition == Positions.BOTTOMCENTER) {
                    itemStringPosition.X = (vp.Width - itemStringSize.X) / 2.0f;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        if (i > 0) {
                            temp.X = itemPositions[i - 1].X + itemSizes[i - 1].X + 50f;
                        } else {
                            temp.X = (vp.Width - listWidth) / 2.0f;
                        }
                        itemPositions[i] = temp;
                    }
                } else {
                    itemStringPosition.X = vp.Width - itemStringSize.X;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        if (i > 0) {
                            temp.X = itemPositions[i - 1].X + itemSizes[i - 1].X + 50f;
                        } else {
                            temp.X = vp.Width - listWidth;
                        }
                        itemPositions[i] = temp;
                    }
                }

                // Set the Y value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.TOPCENTER || menuPosition == Positions.TOPRIGHT) {
                    itemStringPosition.Y = vp.Y + yOffset;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        temp.Y = vp.Y + yOffset + itemStringSize.Y;
                        itemPositions[i] = temp;
                    }
                } else if (menuPosition == Positions.CENTERLEFT || menuPosition == Positions.CENTER || menuPosition == Positions.CENTERRIGHT) {
                    itemStringPosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        temp.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset + itemStringSize.Y;
                        itemPositions[i] = temp;
                    }
                } else {
                    itemStringPosition.Y = vp.Height - menuSize.Y + yOffset;
                    for (int i = 0; i < items.Count; i++) {
                        Vector2 temp = itemPositions[i];
                        temp.Y = vp.Height - menuSize.Y + yOffset + itemStringSize.Y;
                        itemPositions[i] = temp;
                    }
                }

                position.X = itemStringPosition.X < itemPositions[0].X ? itemStringPosition.X : itemPositions[0].X;
                position.Y = itemStringPosition.Y < itemPositions[0].Y ? itemStringPosition.Y : itemPositions[0].Y;
            }

            internal override void UpdateXInput(bool positive) {
                if (positive) {
                    currentValue += 1;
                } else {
                    currentValue -= 1;
                }
                this.updateValueMethod.DynamicInvoke(new object[] { currentValue });
                UpdateValue();
            }

            internal override void UpdateMouseClicked(Vector2 mousePosition, bool getStartHeld) {
                for (int i = 0; i < items.Count; i++) {
                    float xMin = itemPositions[i].X;
                    float yMin = itemPositions[i].Y;
                    float xMax = itemPositions[i].X + itemSizes[i].X;
                    float yMax = itemPositions[i].Y + itemSizes[i].Y;

                    if (mousePosition.X >= xMin && mousePosition.X <= xMax && mousePosition.Y >= yMin && mousePosition.Y <= yMax) {
                        currentValue = i;
                        this.updateValueMethod.DynamicInvoke(new object[] { currentValue });
                        UpdateValue();
                        break;
                    }
                }
            }

            internal override void Render(SpriteBatch spritebatch, int index, int current_menu_item) {
                if ((bool)this.activeMethod.DynamicInvoke(null) || (!(bool)this.activeMethod.DynamicInvoke(null) && this.deactivatedColor != Color.Transparent)) {
                    Color color = (bool)this.activeMethod.DynamicInvoke(null) ? current_menu_item == index ? selectedColor : defaultColor : deactivatedColor;
                    spritebatch.DrawString(font, itemString, itemStringPosition, color);

                    for (int i = 0; i < items.Count; i++) {
                        spritebatch.DrawString(font, items[i], itemPositions[i], currentValue == i ? selectedItemColor : defaultItemColor);
                    }
                }
            }
        }

        private class KeybindingItem : Item {
            const float COLUMN_WIDTH = 300.0f;

            internal string leftText;
            internal string middleText;
            internal string rightText;

            internal Vector2 leftPosition;
            internal Vector2 middlePosition;
            internal Vector2 rightPosition;

            internal Vector2 leftSize;
            internal Vector2 middleSize;
            internal Vector2 rightSize;

            internal Delegate updateLeftText;
            internal Delegate updateMiddleText;
            internal Delegate updateRightText;

            internal KeybindingItem() : base(typeof(KeybindingItem)) {
            }

            internal override void UpdateSize() {
                if (updateLeftText != null) {
                    leftText = (string)updateLeftText.DynamicInvoke(null);
                }
                if (updateMiddleText != null) {
                    middleText = (string)updateMiddleText.DynamicInvoke(null);
                }
                if (updateRightText != null) {
                    rightText = (string)updateRightText.DynamicInvoke(null);
                }

                leftSize = font.MeasureString(leftText);
                middleSize = font.MeasureString(middleText);
                rightSize = font.MeasureString(rightText);

                size.X = COLUMN_WIDTH * 3;
                size.Y = leftSize.Y;
            }

            internal override void UpdatePosition(Viewport vp, Vector2 menuSize, float yOffset, Positions menuPosition) {
                // Set the X value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.CENTERLEFT || menuPosition == Positions.BOTTOMLEFT) {
                    leftPosition.X = vp.X;
                    middlePosition.X = vp.X + COLUMN_WIDTH;
                    rightPosition.X = vp.X + COLUMN_WIDTH * 2;

                    // Set position.X
                    position.X = vp.X;
                } else if (menuPosition == Positions.TOPCENTER || menuPosition == Positions.CENTER || menuPosition == Positions.BOTTOMCENTER) {
                    leftPosition.X = (vp.Width - size.X + COLUMN_WIDTH - leftSize.X) / 2.0f;
                    middlePosition.X = (vp.Width - size.X + COLUMN_WIDTH - middleSize.X) / 2.0f + COLUMN_WIDTH;
                    rightPosition.X = (vp.Width - size.X + COLUMN_WIDTH - rightSize.X) / 2.0f + COLUMN_WIDTH * 2;

                    // Set position.X
                    position.X = (vp.Width - size.X) / 2.0f;
                } else {
                    leftPosition.X = vp.Width - size.X + COLUMN_WIDTH - leftSize.X;
                    middlePosition.X = vp.Width - size.X + COLUMN_WIDTH * 2 - middleSize.X;
                    rightPosition.X = vp.Width - rightSize.X;

                    // Set position.X
                    position.X = vp.Width - size.X;
                }

                // Set the Y value
                if (menuPosition == Positions.TOPLEFT || menuPosition == Positions.TOPCENTER || menuPosition == Positions.TOPRIGHT) {
                    leftPosition.Y = vp.Y + yOffset;
                    middlePosition.Y = vp.Y + yOffset;
                    rightPosition.Y = vp.Y + yOffset;
                } else if (menuPosition == Positions.CENTERLEFT || menuPosition == Positions.CENTER || menuPosition == Positions.CENTERRIGHT) {
                    leftPosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                    middlePosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                    rightPosition.Y = (vp.Height - menuSize.Y) / 2.0f + yOffset;
                } else {
                    leftPosition.Y = vp.Height - menuSize.Y + yOffset;
                    middlePosition.Y = vp.Height - menuSize.Y + yOffset;
                    rightPosition.Y = vp.Height - menuSize.Y + yOffset;
                }

                // Set position.Y
                position.Y = leftPosition.Y;
            }

            internal override void UpdateXInput(bool positive) {
                // Shouldn't be used.
            }

            internal override void UpdateMouseClicked(Vector2 mousePosition, bool getStartHeld) {
                if (this.startMethod != null && !getStartHeld) {
                    this.startMethod.DynamicInvoke(null);
                }
            }

            internal override void Render(SpriteBatch spritebatch, int index, int current_menu_item) {
                spritebatch.DrawString(font, leftText, leftPosition, current_menu_item == index ? selectedColor : defaultColor);
                spritebatch.DrawString(font, middleText, middlePosition, current_menu_item == index ? selectedColor : defaultColor);
                spritebatch.DrawString(font, rightText, rightPosition, current_menu_item == index ? selectedColor : defaultColor);
            }
        }

        private List<Item> menuItems = new List<Item>();

        private Viewport vp;
        private Positions menuPosition;
        private int current_menu_item = 0;

        private Delegate backMethod;

        private const float MAX_HELD_TIME = 0.5f;
        private float getXHeldTime = MAX_HELD_TIME;
        private float getYHeldTime = MAX_HELD_TIME;
        private bool getStartHeld = true;
        private bool getBackHeld = true;

        private bool mouseMoved = true;
        private Vector2 lastMousePosition = Vector2.Zero;

        private delegate bool ActiveDelegate();

        private bool AlwaysTrue() {
            return true;
        }

        internal MenuHelper(Viewport vp, Positions menuPosition, Delegate backMethod) {
            this.vp = vp;
            this.menuPosition = menuPosition;
            this.backMethod = backMethod;
        }

        private TextItem CreateTextMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod) {
            TextItem temp = new TextItem();
            temp.startMethod = startMethod;
            temp.itemString = itemString;
            temp.defaultColor = defaultColor;
            temp.selectedColor = selectedColor;
            temp.font = font;
            temp.itemStringSize = temp.font.MeasureString(temp.itemString);
            temp.size = temp.itemStringSize;
            return temp;
        }

        internal void AddTextMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod) {
            TextItem temp = CreateTextMenuItem(itemString, ref font, defaultColor, selectedColor, startMethod);
            temp.activeMethod = new ActiveDelegate(AlwaysTrue);
            temp.deactivatedColor = Color.Black;
            menuItems.Add(temp);
        }

        internal void AddTextMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod, Delegate activeMethod, Color deactivatedColor) {
            TextItem temp = CreateTextMenuItem(itemString, ref font, defaultColor, selectedColor, startMethod);
            temp.activeMethod = activeMethod;
            temp.deactivatedColor = deactivatedColor;
            menuItems.Add(temp);
        }

        private BasicBarItem CreateBarMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod, Delegate startedMethod, ref Texture2D empty, ref Texture2D full, ref Vector2 barSize, float maxValue, float minValue, float incrementValue, Delegate currentValue, Delegate updateValueMethod) {
            BasicBarItem temp = new BasicBarItem();
            temp.startMethod = startMethod;
            temp.startedMethod = startedMethod;
            temp.itemString = itemString;
            temp.defaultColor = defaultColor;
            temp.selectedColor = selectedColor;
            temp.font = font;
            temp.itemStringSize = temp.font.MeasureString(temp.itemString);
            temp.empty = empty;
            temp.full = full;
            temp.fullRect.Width = (int)barSize.X;
            temp.fullRect.Height = (int)barSize.Y;
            temp.emptyRect.Width = (int)barSize.X;
            temp.emptyRect.Height = (int)barSize.Y;
            temp.barSize = barSize;
            temp.MAX_VALUE = maxValue;
            temp.MIN_VALUE = minValue;
            temp.INCREMENT = incrementValue;
            temp.currentValueMethod = currentValue;
            temp.updateValueMethod = updateValueMethod;

            temp.UpdateValue();

            temp.size.X = temp.itemStringSize.X > temp.barSize.X ? temp.itemStringSize.X : temp.barSize.X;
            temp.size.Y = temp.itemStringSize.Y + temp.barSize.Y;

            return temp;
        }

        internal void AddBarMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod, Delegate startedMethod, ref Texture2D empty, ref Texture2D full, ref Vector2 barSize, float maxValue, float minValue, float incrementValue, Delegate currentValue, Delegate updateValueMethod) {
            BasicBarItem temp = CreateBarMenuItem(itemString, ref font, defaultColor, selectedColor, startMethod, startedMethod, ref empty, ref full, ref barSize, maxValue, minValue, incrementValue, currentValue, updateValueMethod);
            temp.activeMethod = new ActiveDelegate(AlwaysTrue);
            temp.deactivatedColor = Color.Black;
            menuItems.Add(temp);
        }

        internal void AddBarMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Delegate startMethod, Delegate startedMethod, ref Texture2D empty, ref Texture2D full, ref Vector2 barSize, float maxValue, float minValue, float incrementValue, Delegate currentValue, Delegate updateValueMethod, Delegate activeMethod, Color deactivatedColor) {
            BasicBarItem temp = CreateBarMenuItem(itemString, ref font, defaultColor, selectedColor, startMethod, startedMethod, ref empty, ref full, ref barSize, maxValue, minValue, incrementValue, currentValue, updateValueMethod);
            temp.activeMethod = activeMethod;
            temp.deactivatedColor = deactivatedColor;
            menuItems.Add(temp);
        }

        private SubListItem CreateSublistMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Color defaultItemColor, Color selectedItemColor, List<String> items, Delegate currentValueMethod, Delegate updateValueMethod) {
            SubListItem temp = new SubListItem();
            temp.itemString = itemString;
            temp.font = font;
            temp.defaultColor = defaultColor;
            temp.selectedColor = selectedColor;
            temp.defaultItemColor = defaultItemColor;
            temp.selectedItemColor = selectedItemColor;
            temp.items = items;
            foreach (string item in items) {
                temp.itemPositions.Add(Vector2.Zero);
                temp.itemSizes.Add(temp.font.MeasureString(item));
                temp.size.X += temp.font.MeasureString(item).X;
            }
            temp.size.X += (temp.items.Count - 1) * 50;
            if (temp.itemStringSize.X > temp.size.X) {
                temp.size.X = temp.itemStringSize.X;
            }
            temp.size.Y = temp.itemStringSize.Y + temp.itemSizes[0].Y;
            temp.currentValueMethod = currentValueMethod;
            temp.updateValueMethod = updateValueMethod;

            return temp;
        }

        internal void AddSublistMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Color defaultItemColor, Color selectedItemColor, List<String> items, Delegate currentValueMethod, Delegate updateValueMethod) {
            SubListItem temp = CreateSublistMenuItem(itemString, ref font, defaultColor, selectedColor, defaultItemColor, selectedItemColor, items, currentValueMethod, updateValueMethod);
            temp.activeMethod = new ActiveDelegate(AlwaysTrue);
            temp.deactivatedColor = Color.Black;
            menuItems.Add(temp);
        }

        internal void AddSublistMenuItem(string itemString, ref SpriteFont font, Color defaultColor, Color selectedColor, Color defaultItemColor, Color selectedItemColor, List<String> items, Delegate currentValueMethod, Delegate updateValueMethod, Delegate activeMethod, Color deactivatedColor) {
            SubListItem temp = CreateSublistMenuItem(itemString, ref font, defaultColor, selectedColor, defaultItemColor, selectedItemColor, items, currentValueMethod, updateValueMethod);
            temp.activeMethod = activeMethod;
            temp.deactivatedColor = deactivatedColor;
            menuItems.Add(temp);
        }

        private KeybindingItem CreateKeybindingMenuItem(ref SpriteFont font, Color defaultColor, Color selectedColor, string leftText, string middleText, string rightText, Delegate updateLeftText, Delegate updateMiddleText, Delegate updateRightText, Delegate startMethod) {
            KeybindingItem temp = new KeybindingItem();
            temp.font = font;
            temp.defaultColor = defaultColor;
            temp.selectedColor = selectedColor;
            temp.leftText = leftText;
            temp.middleText = middleText;
            temp.rightText = rightText;
            temp.updateLeftText = updateLeftText;
            temp.updateMiddleText = updateMiddleText;
            temp.updateRightText = updateRightText;
            temp.startMethod = startMethod;

            return temp;
        }

        internal void AddKeybindingMenuItem(ref SpriteFont font, Color defaultColor, Color selectedColor, string leftText, string middleText, string rightText, Delegate updateLeftText, Delegate updateMiddleText, Delegate updateRightText, Delegate startMethod) {
            KeybindingItem temp = CreateKeybindingMenuItem(ref font, defaultColor, selectedColor, leftText, middleText, rightText, updateLeftText, updateMiddleText, updateRightText, startMethod);
            temp.activeMethod = new ActiveDelegate(AlwaysTrue);
            temp.deactivatedColor = Color.Black;

            menuItems.Add(temp);
        }

        internal void AddKeybindingMenuItem(ref SpriteFont font, Color defaultColor, Color selectedColor, string leftText, string middleText, string rightText, Delegate updateLeftText, Delegate updateMiddleText, Delegate updateRightText, Delegate startMethod, Delegate activeMethod, Color deactivatedColor) {
            KeybindingItem temp = CreateKeybindingMenuItem(ref font, defaultColor, selectedColor, leftText, middleText, rightText, updateLeftText, updateMiddleText, updateRightText, startMethod);
            temp.activeMethod = activeMethod;
            temp.deactivatedColor = deactivatedColor;

            menuItems.Add(temp);
        }

        internal void Update(float dt) {
            vp = Game.device.Viewport;
            // Update Input
            // Keyboard
            bool backHeld = Player.Input.GetMenuBack();
            bool startHeld = Player.Input.GetStart();
            float xHeld = Math.Abs(Player.Input.GetX()) > 0.5 ? Player.Input.GetX() : 0.0f;
            float yHeld = Math.Abs(Player.Input.GetY()) > 0.5 ? Player.Input.GetY() : 0.0f;

            // Mouse
            bool leftMouseHeld = Player.Input.GetFireLeft();

            if (lastMousePosition != Player.Input.GetCursorPosition()) {
                mouseMoved = true;
                lastMousePosition = Player.Input.GetCursorPosition();
            } else {
                mouseMoved = false;
            }

            // Input logic
            if (!backHeld)
                getBackHeld = false;
            if (!startHeld && !leftMouseHeld)
                getStartHeld = false;
            if (Math.Abs(yHeld) <= float.Epsilon || getYHeldTime >= MAX_HELD_TIME)
                getYHeldTime = 0.0f;
            if (Math.Abs(xHeld) <= float.Epsilon || getXHeldTime >= MAX_HELD_TIME)
                getXHeldTime = 0.0f;

            Vector2 menuSize = Vector2.Zero;
            // Perform multiple operations on all the items.
            for (int i = 0; i < menuItems.Count; i++) {
                // Ensure the menu is on the correct item
                if (!(bool)menuItems[i].activeMethod.DynamicInvoke(null) && current_menu_item == i) {
                    current_menu_item++;
                }

                // Update the items size
                menuItems[i].UpdateSize();

                // Calculate the menu size
                if ((bool)menuItems[i].activeMethod.DynamicInvoke(null) || (!(bool)menuItems[i].activeMethod.DynamicInvoke(null) && menuItems[i].deactivatedColor != Color.Transparent)) {
                    menuSize.X = menuSize.X > menuItems[i].GetSize().X ? menuSize.X : menuItems[i].GetSize().X;
                    menuSize.Y += menuItems[i].GetSize().Y;
                }
            }

            // Update all items external positions now that menu size has been calculated
            float yOffset = 0.0f;
            for (int i = 0; i < menuItems.Count; i++) {
                menuItems[i].UpdatePosition(vp, menuSize, yOffset, menuPosition);
                yOffset += menuItems[i].GetSize().Y;
            }

            // More input Logic
            if (mouseMoved) {
                for (int i = 0; i < menuItems.Count; i++) {
                    if ((bool)menuItems[i].activeMethod.DynamicInvoke(null)) {
                        float xMin = menuItems[i].GetPosition().X;
                        float yMin = menuItems[i].GetPosition().Y;
                        float xMax = menuItems[i].GetSize().X + menuItems[i].GetPosition().X;
                        float yMax = menuItems[i].GetSize().Y + menuItems[i].GetPosition().Y;

                        // If mouse is on the ith menuItem
                        if (lastMousePosition.X >= xMin && lastMousePosition.X <= xMax && lastMousePosition.Y >= yMin && lastMousePosition.Y <= yMax) {
                            current_menu_item = i;
                        }
                    }
                }
            } else {
                if (yHeld != 0) {
                    if (getYHeldTime == 0f) {
                        if (yHeld > 0.5) {
                            current_menu_item += menuItems.Count - 1;
                            current_menu_item %= menuItems.Count;

                            // iterate over item if it's not a selectable item
                            while (!(bool)menuItems[current_menu_item].activeMethod.DynamicInvoke(null)) {
                                current_menu_item += menuItems.Count - 1;
                                current_menu_item %= menuItems.Count;
                            }
                        } else if (yHeld < -0.5) {
                            current_menu_item += 1;
                            current_menu_item %= menuItems.Count;

                            // iterate over item if it's not a selectable item
                            while (!(bool)menuItems[current_menu_item].activeMethod.DynamicInvoke(null)) {
                                current_menu_item += 1;
                                current_menu_item %= menuItems.Count;
                            }
                        }
                    }
                    getYHeldTime += dt;
                }

                if (xHeld != 0) {
                    if (getXHeldTime == 0f) {
                        if (xHeld > 0.5) {
                            menuItems[current_menu_item].UpdateXInput(true);
                        } else if (xHeld < -0.5) {
                            menuItems[current_menu_item].UpdateXInput(false);
                        }
                    }
                    getXHeldTime += dt;
                }
            }

            // start held
            if (startHeld && !getStartHeld) {
                if (menuItems[current_menu_item].startMethod != null) {
                    menuItems[current_menu_item].startMethod.DynamicInvoke(null);
                }
                getStartHeld = true;
                getBackHeld = true;
                if (menuItems[current_menu_item].type == typeof(TextItem)) {
                    current_menu_item = 0;
                    while (!(bool)menuItems[current_menu_item].activeMethod.DynamicInvoke(null)) {
                        current_menu_item++;
                    }
                }
            }

            // mouse click
            if (leftMouseHeld) {
                for (int i = 0; i < menuItems.Count; i++) {
                    // Item can't be clicked if it's not active
                    if ((bool)menuItems[i].activeMethod.DynamicInvoke(null)) {
                        // determine if the mouse clicked the item, then let the item take care of the rest of the logic
                        if (menuItems[i].GetPosition().X < lastMousePosition.X && menuItems[i].GetPosition().X + menuItems[i].GetSize().X > lastMousePosition.X &&
                                menuItems[i].GetPosition().Y < lastMousePosition.Y && menuItems[i].GetPosition().Y + menuItems[i].GetSize().Y > lastMousePosition.Y) {

                            if ((!getStartHeld && menuItems[i].type == typeof(TextItem)) || menuItems[i].type != typeof(TextItem)) {
                                current_menu_item = i;
                                menuItems[i].UpdateMouseClicked(lastMousePosition, getStartHeld);

                                if (menuItems[current_menu_item].type == typeof(TextItem)) {
                                    current_menu_item = 0;
                                    while (!(bool)menuItems[current_menu_item].activeMethod.DynamicInvoke(null)) {
                                        current_menu_item++;
                                    }
                                }
                            }
                        }
                    }
                }
                getStartHeld = true;
                getBackHeld = true;
            }

            if (backHeld && !getBackHeld) {
                if (backMethod != null) {
                    backMethod.DynamicInvoke(null);
                }
                getStartHeld = true;
                getBackHeld = true;
                current_menu_item = 0;
            }
        }

        internal void Render(SpriteBatch spritebatch) {
            spritebatch.Begin();

            for (int i = 0; i < menuItems.Count; i++) {
                menuItems[i].Render(spritebatch, i, current_menu_item);
            }

            spritebatch.End();
        }
    }
}
