using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

using StardewEcon.Econ;

namespace StardewEcon.View
{
    /**
     * <summary>A pop-up menu to display the current events.</summary>
     * <remarks>
     *  Instances of this class are not meant to be long-lived. It's written
     *  with the expectation that you create an instance when the user opens
     *  the menu and that you abandon that instance when the user closes it.
     *  
     *  <see cref="NewsBulletinObject"/> is an invisible object for users to
     *  interact with to tsee this menu. This mod will place one over the sign
     *  that normally displays Pierre's hours.
     * </remarks>
     * 
     * <seealso cref="NewsBulletinObject"/>
     */
    public class NewsBulletinMenu : IClickableMenu
    {
        #region Dimensions
        /**
         * <summary>The width of the menu inside the borders, in pixels.</summary>
         */
        //private int internalWidth;

        /**
         * <summary>The width of the menu inside the borders, in pixels.</summary>
         */
        //private int internalHeight;

        /**
         * <summary>The X offset from the upper-left corner of the bounding box to the box inside the menu's borders, in pixels.</summary>
         */
        //private int xOffsetToInternal;

        /**
         * <summary>The Y offset from the upper-left corner of the bounding box to the box inside the menu's borders, in pixels.</summary>
         */
        //private int yOffsetToInternal;

        /**
         * <summary>The height of each event section, in pixels.</summary>
         */
        //private int sectionHeight;

        /**
         * <summary>The height of the separators between event sections, in pixels.</summary>
         */
        //private int separatorHeight;

        /**
         * <summary>The dimensions of this menu.</summary>
         */
        private MenuDimensions dims;
        #endregion

        /**
         * <summary>A hover box describing an event, if one is moused-over.</summary>
         */
        private EconEventHoverBox hoverBox;

        /**
         * <summary>The events displayed by this menu.</summary>
         */
        private IList<EconEvent> events;

        /**
         * <summary>Creates a new News Bulletin to show the given events.</summary>
         * <remarks>
         *  This constructor will tell the game to play sound - it's expected
         *  that it is called once per opening of this menu at the time the user
         *  requests to open it.
         * </remarks>
         * 
         * <param name="events">The events to show.</param>
         */
        public NewsBulletinMenu(IEnumerable<EconEvent> events)
            : base(x: 0, y: 0, width: 0, height: 0, showUpperRightCloseButton: true)
        {
            this.events = new List<EconEvent>(events);
            this.hoverBox = null;
            this.dims = new MenuDimensions(this.events.Count, Game1.pixelZoom);

            var outerBox = this.dims.OuterBox();
            this.width = outerBox.Width;
            this.height = outerBox.Height;
            this.xPositionOnScreen = outerBox.X;
            this.yPositionOnScreen = outerBox.Y;

            // Reinitialize close button:
            this.initializeUpperRightCloseButton();

            // Play sound!
            Game1.playSound("bigSelect");
        }

        /**
         * <summary>Draws the menu.</summary>
         */
        public override void draw(SpriteBatch b)
        {
            this.dims.SetZoom(Game1.pixelZoom);

            // Draw box and background fade
            var dialogueBox = this.dims.DialogueBox();
            var labelPt = this.dims.MenuLabelPoint();
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(
                x: dialogueBox.X,
                y: dialogueBox.Y,
                width: dialogueBox.Width,
                height: dialogueBox.Height,
                speaker: false,
                drawOnlyBox: true,
                message: null,
                objectDialogueWithPortrait: false);
            base.drawBorderLabel(b, "News Bulletin", Game1.smallFont, labelPt.X, labelPt.Y);

            // Draw contents
            if (true)
            {
                for (int i = 0; i < this.events.Count; i++)
                {
                    EconEvent e = this.events[i];
                    if (i != this.events.Count - 1)
                    {
                        this.drawHorizontalPartition(b, this.dims.SeparatorYPosition(i), true);
                    }

                    var sectionBox = this.dims.SectionBox(i);
                    Utility.DrawWrappedString(b, Game1.dialogueFont, e.Headline, sectionBox, Color.Black);
                }
            }
            else
            {
                // Test code to make sure icons display correctly
                //this.drawIconGrid(b, EconEventFactory._Crops);
            }

            // Test code to test dimensions:
            //var flatColor = Game1.fadeToBlackRect;
            //b.Draw(flatColor, this.dims.OuterBox(),    Color.Blue * 0.5f);
            //b.Draw(flatColor, this.dims.DialogueBox(), Color.Red * 0.5f);
            //b.Draw(flatColor, this.dims.InnerBox(),    Color.Green * 0.5f);
            //
            //for (int i = 0; i < this.events.Count; i++)
            //{
            //    var box = this.dims.SectionBox(i);
            //    b.Draw(flatColor, box, Color.Green * 0.5f);

            //    var lineY = this.dims.SeparatorYPosition(i);
            //    var line = new Rectangle
            //    {
            //        X = box.X,
            //        Y = lineY,
            //        Width = box.Width,
            //        Height = 1 * Game1.pixelZoom,
            //    };
            //    b.Draw(flatColor, line, Color.Red * 0.5f);
            //}

            // Draw close box and mouse
            base.draw(b);
            this.drawMouse(b);

            // Draw hover text
            this.hoverBox?.Draw(b);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            //throw new NotImplementedException();
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverBox = null;
            foreach (var content in this.events.Zip(this.dims.SectionBoxes(), Tuple.Create))
            {
                EconEvent e = content.Item1;
                Rectangle rect = content.Item2;
                if ( rect.Contains(x, y) )
                {
                    if (e.AffectedItem > -1)
                    {
                        this.hoverBox = new EconEventHoverBox(e);
                    }
                    break;
                }
            }
        }

        private void DrawIconGrid(SpriteBatch b, IReadOnlyList<int> items)
        {
            int iconWidth = 16 * Game1.pixelZoom;
            int iconHeight = 16 * Game1.pixelZoom;
            int colSep = 0 * Game1.pixelZoom;
            int rowSep = 0 * Game1.pixelZoom;

            var innerBox = this.dims.InnerBox();
            int startX = innerBox.X;
            int startY = innerBox.Y;
            int dx = iconWidth + colSep;
            int dy = iconHeight + rowSep;

            int maxIconsPerRow = (innerBox.Width + colSep) / dx;

            int x = 0;
            int y = 0;
            foreach (var item in items)
            {
                int drawX = startX + x * dx;
                int drawY = startY + y * dy;

                var spriteSourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item, 16, 16);
                var spriteBox = new Rectangle(drawX, drawY, iconWidth, iconHeight);

                b.Draw(
                    texture: Game1.objectSpriteSheet,
                    sourceRectangle: spriteSourceRect,
                    destinationRectangle: spriteBox,
                    color: Color.White);

                x++;
                if (x >= maxIconsPerRow)
                {
                    x = 0;
                    y++;
                }
            }
        }

        /**
         * <summary>Private class to help us manage the dimensions of the menu.</summary>
         */
        private class MenuDimensions
        {
            // Height of horizontal separator, in game pixels
            private const int separatorHeight = 4;

            // Width of section, in game pixels
            private int sectionWidth = 100;
            
            // Height of section, in game pixels
            private int sectionHeight = 50;

            // Number of items (sections)
            private int numItems;

            // Number of screen pixels wide a game pixel is
            private int pixelZoom;

            public MenuDimensions(int numItems, int pixelZoom)
            {
                this.numItems = numItems;
                this.SetZoom(pixelZoom);
            }

            public void SetZoom(int pixelZoom)
            {
                this.pixelZoom = pixelZoom;
            }

            /**
             * <summary>The dimensions of the menu, in screen pixels.</summary>
             * <remarks>
             *  This is used to set our internal parameters, which do things like
             *  position the close button.
             * </remarks>
             */
            public Rectangle OuterBox()
            {
                var innerHeight = this.InnerHeight();
                var extraVertical = this.ExtraVerticalSpace();

                var width  = this.sectionWidth * this.pixelZoom + IClickableMenu.borderWidth * 2;
                var height = innerHeight * this.pixelZoom + IClickableMenu.borderWidth * 2 + extraVertical;
                return new Rectangle
                {
                    X = (Game1.viewport.Width - width) / 2,
                    Y = (Game1.viewport.Height - height) / 2,
                    Width = width,
                    Height = height
                };
            }

            /**
             * <summary>Dimensions of the dialogue box, in screen pixels.</summary>
             * <remarks>
             *  We use this when actually drawing the dialogue box.
             * </remarks>
             */
            public Rectangle DialogueBox()
            {
                var outerBox = this.OuterBox();
                var upperMargin = this.DialogueUpperMargin();
                return new Rectangle
                {
                    X = outerBox.X,
                    // For whatever reason, the dialogue box has a HUGE upper margin.
                    Y = outerBox.Y - upperMargin,
                    Width = outerBox.Width,
                    Height = outerBox.Height + upperMargin,
                };
            }

            /**
             * <summary>Dimensions of the space inside the menu, in screen pixels.</summary>
             * <remarks>
             *  Content to appear in the menu must be within this rectangle.
             * </remarks>
             */
            public Rectangle InnerBox()
            {
                var outerBox = this.OuterBox();
                var extraVertical = this.ExtraVerticalSpace();
                return new Rectangle()
                {
                    X = outerBox.X + IClickableMenu.borderWidth,
                    Y = outerBox.Y + IClickableMenu.borderWidth + extraVertical,
                    Width = this.sectionWidth * this.pixelZoom,
                    Height = this.InnerHeight() * this.pixelZoom,
                };
            }

            /**
             * <summary>Dimensions of a section's box, in screen pixels</summary>
             * 
             * <param name="index">Which box to return.</param>
             */
            public Rectangle SectionBox(int index)
            {
                var innerBox = this.InnerBox();
                var secHeight = this.SectionHeight();
                int dy = secHeight + this.SeparatorHeight();

                return new Rectangle
                {
                    X = innerBox.X,
                    Y = innerBox.Y + index * dy,
                    Width = innerBox.Width,
                    Height = secHeight,
                };
            }

            /**
             * <summary>Enumerates over all section boxes</summary>
             */
            public IEnumerable<Rectangle> SectionBoxes()
            {
                // Used nowhere but GetContent(), which is used nowhere but the hover check.
                for (int i = 0; i < this.numItems; i++)
                {
                    yield return this.SectionBox(i);
                }
            }

            /**
             * <summary>Y position to draw a specific section separator at, in screen pixels.</summary>
             * 
             * <param name="index">Which separator; appears below SectionBox(index).</param>
             */
            public int SeparatorYPosition(int index)
            {
                return SectionBox(index).Bottom - this.DividerUpperMargin();
            }

            /**
             * <summary>Where to place the menu label, in screen pixels.</summary>
             */
            public Point MenuLabelPoint()
            {
                var dialogueBox = this.DialogueBox();
                return new Point
                {
                    X = dialogueBox.X + (8 * this.pixelZoom),
                    Y = dialogueBox.Y,
                };
            }

            /**
             * <summary>How tall a section is, in screen pixels.</summary>
             */
            private int SectionHeight()
            {
                return this.sectionHeight * this.pixelZoom;
            }

            /**
             * <summary>How tall a separator is, in screen pixels.</summary>
             */
            private int SeparatorHeight()
            {
                return MenuDimensions.separatorHeight * this.pixelZoom;
            }

            /**
             * <summary>How tall the inner menu space is, in *game* pixels.</summary>
             */
            private int InnerHeight()
            {
                return (numItems) * this.sectionHeight + (numItems - 1) * MenuDimensions.separatorHeight;
            }

            /**
             * <summary>The margin above the dialogue we need to add for it to appear where we expect, in screen pixels.</summary>
             */
            private int DialogueUpperMargin()
            {
                return 16 * this.pixelZoom;
            }

            /**
             * <summary>The margin above the divider we need to account for for it to appear where we expect, in screen pixels.</summary>
             */
            private int DividerUpperMargin()
            {
                // TODO: Make this a function of divider height?
                return 6 * this.pixelZoom;
            }

            /**
             * <summary>The extra vertical space we have to add to clear the bottom of the menu label, in screen pixels.</summary>
             */
            private int ExtraVerticalSpace()
            {
                return 4 * this.pixelZoom;
            }
        }
    }
}
