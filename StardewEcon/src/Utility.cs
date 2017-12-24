using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewValley.Menus;

namespace StardewEcon
{
    static class Utility
    {
        // Returns true if the string was truncated.
        public static bool DrawWrappedString(SpriteBatch batch, SpriteFont font, string text, Rectangle bounds, Color color)
        {
            // This code is not terribly sturdy. Handle with care.
            string[] fullParagraphs = text.Split(new[]{'\n'}, StringSplitOptions.None);
            IEnumerable<string[]> paragraphs = fullParagraphs.Select(p => p.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries));

            bool truncate = false;
            bool firstParagraph = true;
            StringBuilder outputText = new StringBuilder();
            foreach(string[] paragraph in paragraphs)
            {
                if( !firstParagraph )
                {
                    outputText.Append("\n");
                }

                // Check height
                Vector2 textSize;
                if (firstParagraph) {
                    textSize = font.MeasureString("J");
                }
                else {
                    textSize = font.MeasureString(outputText);
                }
                if( textSize.Y > bounds.Height )
                {
                    // If we're out of height, we're done.
                    truncate = true;
                    if( outputText.Length > 0 )
                    {
                        outputText.Remove(outputText.Length - 1, 1);
                    }
                    break;
                }

                // Append words of paragraph, wrapping when necessary
                bool firstWordInParagraph = true;
                foreach(string word in paragraph)
                {
                    // Attempt to append:
                    if (!firstWordInParagraph)
                    {
                        outputText.Append(" ");
                    }

                    outputText.Append(word);

                    // Check width:
                    textSize = font.MeasureString(outputText);
                    if( textSize.X > bounds.Width )
                    {
                        int spaceIndex = outputText.Length - 1 - word.Length;

                        // If this is the first word in the paragraph and we've
                        // already broken the width limit, we're kinda screwed.
                        if( firstWordInParagraph )
                        {
                            truncate = true;
                            outputText.Remove(spaceIndex, word.Length + 1);
                            break;
                        }

                        // Fix by wrapping
                        outputText.Replace(' ', '\n', spaceIndex, 1);

                        // Check width and height:
                        // If we've broken either limit, we have to truncate.
                        textSize = font.MeasureString(outputText);
                        if( (textSize.X > bounds.Width) || (textSize.Y > bounds.Height) )
                        {
                            truncate = true;
                            outputText.Remove(spaceIndex, word.Length + 1);
                            break;
                        }
                    }

                    firstWordInParagraph = false;
                }

                firstParagraph = false;

                if ( truncate )
                {
                    break;
                }
            }

            StardewValley.Utility.drawTextWithShadow(batch, outputText.ToString(), font, new Vector2(bounds.X, bounds.Y), color);
            return truncate;
        }

        public static void DrawHoverTextWithItem(SpriteBatch batch, string text, SpriteFont font, int itemIndex = -1, float alpha = 1f)
        {
            if( batch == null || font == null )
            {
                return;
            }

            text = text ?? "";

            Rectangle menuTextureSourceRect = new Rectangle(0, 256, 60, 60);

            int margin = Game1.tileSize / 4;
            int dividerWidth = Game1.tileSize / 4;

            Vector2 textSize;
            Rectangle hoverBoxBounds;
            Rectangle spriteSourceRect = new Rectangle(0, 0, 0, 0);
            Vector2 spriteSize = new Vector2(0, 0);

            // Bounds generation
            {
                // Width/Height generation
                textSize = font.MeasureString(text);
                int width = Math.Max(0, (int)textSize.X);
                int height = Math.Max(20 * 3 - margin * 2, (int)textSize.Y);

                // If we're dealing with an item, we need to modify the width/height:
                if (itemIndex > -1)
                {
                    spriteSourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, itemIndex, 16, 16);
                    spriteSize = new Vector2(spriteSourceRect.Width, spriteSourceRect.Height) * Game1.pixelZoom;
                    width += (int)spriteSize.X + dividerWidth;
                    height = Math.Max(height, (int)spriteSize.Y);
                }

                width += margin * 2;
                height += margin * 2;

                // X and Y generation
                int x = Game1.getOldMouseX() + Game1.tileSize / 2;
                int y = Game1.getOldMouseY() + Game1.tileSize / 2;

                hoverBoxBounds = new Rectangle(x, y, width, height);

                // Screen bounds checking:
                Rectangle safeArea = StardewValley.Utility.getSafeArea();
                CoerceInto(hoverBoxBounds, safeArea);
            }

            // Draw the hover text box
            Utility.drawTextureBox(
                batch,
                texture: Game1.menuTexture,
                sourceRect: menuTextureSourceRect,
                destinationRect: hoverBoxBounds,
                color: Color.White * alpha);
            
            // Draw the hovertext and its shadow
            if (!string.IsNullOrWhiteSpace(text))
            {
                float x = hoverBoxBounds.X + margin;
                float y = hoverBoxBounds.Y + (hoverBoxBounds.Height - textSize.Y) / 2;
                batch.DrawString(font, text, new Vector2(x, y) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, new Vector2(x, y) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, new Vector2(x, y) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, new Vector2(x, y), Game1.textColor * 0.9f * alpha);
            }

            // Draw the item sprite
            if (itemIndex > -1)
            {
                int x = hoverBoxBounds.X + margin + (int)textSize.X + dividerWidth;
                int y = hoverBoxBounds.Y + (hoverBoxBounds.Height - (int)spriteSize.Y) / 2;
                batch.Draw(
                    texture: Game1.objectSpriteSheet,
                    sourceRectangle: spriteSourceRect,
                    destinationRectangle: new Rectangle(x, y, (int)spriteSize.X, (int)spriteSize.Y),
                    color: Color.White);
            }
        }

        /**
         * <summary>Coerces the first rectangle into the second by modifying position, not size</summary>
         * <remarks>Only actually checks the right and bottom bounds.</remarks>
         * <returns>The coerced rectangle.</returns>
         */
        private static Rectangle CoerceInto(Rectangle toCoerce, Rectangle bounds)
        {
            // Make a copy so we don't modify the ones passed to us.
            Rectangle coerced = new Rectangle(toCoerce.X, toCoerce.Y, toCoerce.Width, toCoerce.Height);

            // Check right bound
            if (coerced.Right > bounds.Right)
            {
                coerced.X = bounds.Right - coerced.Width;
                // Why are we modifying Y? I don't know...
                // I copied this from the decompiled game.
                coerced.Y += Game1.tileSize / 4;
            }
            
            // Check bottom bound
            if (coerced.Bottom > bounds.Bottom)
            {
                // Why are we modifying X? I don't know...
                // I copied this from the decompiled game.
                coerced.X += Game1.tileSize / 4;
                if (coerced.Right > bounds.Right)
                {
                    coerced.X = bounds.Right - coerced.Width;
                }
                coerced.Y = bounds.Bottom - coerced.Height;
            }

            return coerced;
        }

        /**
         * <summary>Wrapper around IClickableMenu.drawTextureBox that lets us
         *  provide a destination rectangle rather than each parameter.</summary>
         */
        private static void drawTextureBox(SpriteBatch batch, Rectangle destinationRect, Color color)
        {
            Rectangle d = destinationRect;
            IClickableMenu.drawTextureBox(batch, d.X, d.Y, d.Width, d.Height, color);
        }

        /**
         * <summary>Wrapper around IClickableMenu.drawTextureBox that lets us
         *  provide a destination rectangle rather than each parameter.</summary>
         */
        private static void drawTextureBox(
            SpriteBatch batch,
            Texture2D texture,
            Rectangle sourceRect,
            Rectangle destinationRect,
            Color color,
            float scale = 1f,
            bool drawShadow = true)
        {
            Rectangle d = destinationRect;
            IClickableMenu.drawTextureBox(batch, texture, sourceRect, d.X, d.Y, d.Width, d.Height, color, scale, drawShadow);
        }
    }
}
