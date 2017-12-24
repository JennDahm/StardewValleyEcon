using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace StardewEcon
{
    public class EconEventHoverBox
    {
        public EconEventHoverBox(EconEvent e)
        {
            this.ItemAffected = e.AffectedItem;
            this.PercentChange = e.PercentChange;
        }

        public int ItemAffected { get; }

        public int PercentChange { get; }

        public void draw(SpriteBatch batch)
        {
            // Constants
            SpriteFont font = Game1.smallFont;
            float alpha = 1f;

            // Generate text and bounds
            string text = $"{this.PercentChange.ToString("+#;-#;0")}%";
            BoxBounds bounds = new BoxBounds(text, this.ItemAffected, font);
            
            // Draw the hover text box
            Utility.drawTextureBox(
                batch,
                texture: Game1.menuTexture,
                sourceRect: new Rectangle(0, 256, 60, 60),
                destinationRect: bounds.hoverBox,
                color: Color.White * alpha);

            // Draw the hovertext and its shadow
            if (!string.IsNullOrWhiteSpace(text))
            {
                batch.DrawString(font, text, bounds.textPos + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, bounds.textPos + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, bounds.textPos + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
                batch.DrawString(font, text, bounds.textPos, Game1.textColor * 0.9f * alpha);
            }

            // Draw the item sprite
            if (this.ItemAffected > -1)
            {
                batch.Draw(
                    texture: Game1.objectSpriteSheet,
                    sourceRectangle: bounds.spriteSourceRect,
                    destinationRectangle: bounds.spriteBox,
                    color: Color.White);
            }
        }
        
        /**
         * <summary>Struct for calculating necessary bounding boxes for the hover box.</summary>
         */
        private struct BoxBounds
        {
            public Rectangle hoverBox;
            public Rectangle spriteBox;
            public Vector2 textPos;
            public Rectangle spriteSourceRect;

            public BoxBounds(string text, int iconIndex, SpriteFont font)
            {
                // Constants
                int margin = Game1.tileSize / 4;
                int dividerWidth = Game1.tileSize / 4;

                // Width/Height generation
                Vector2 textSize = font.MeasureString(text);
                int width = (int)Math.Max(0, textSize.X);
                int height = (int)Math.Max(20 * 3 - margin * 2, textSize.Y);

                // If we're dealing with an item, we need to modify the width/height:
                Vector2 spriteSize;
                if (iconIndex > -1)
                {
                    this.spriteSourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, iconIndex, 16, 16);
                    spriteSize = new Vector2(this.spriteSourceRect.Width, this.spriteSourceRect.Height) * Game1.pixelZoom;
                    width += (int)spriteSize.X + dividerWidth;
                    height = Math.Max(height, (int)spriteSize.Y);
                }
                else
                {
                    this.spriteSourceRect = new Rectangle(0, 0, 0, 0);
                    spriteSize = new Vector2(0, 0);
                }

                width += margin * 2;
                height += margin * 2;

                // X and Y generation
                int x = Game1.getOldMouseX() + Game1.tileSize / 2;
                int y = Game1.getOldMouseY() + Game1.tileSize / 2;

                this.hoverBox = new Rectangle(x, y, width, height);

                // Screen bounds checking:
                Rectangle safeArea = StardewValley.Utility.getSafeArea();
                this.hoverBox = CoerceInto(this.hoverBox, safeArea);

                // Place things:
                this.textPos = new Vector2(
                    this.hoverBox.X + margin,
                    this.hoverBox.Y + (this.hoverBox.Height - textSize.Y) / 2
                    );
                this.spriteBox = new Rectangle(
                    this.hoverBox.X + margin + (int)textSize.X + dividerWidth,
                    this.hoverBox.Y + (this.hoverBox.Height - (int)spriteSize.Y) / 2,
                    (int)spriteSize.X,
                    (int)spriteSize.Y
                    );
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
        }
    }
}
