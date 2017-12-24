using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace StardewEcon
{
    class NewsBulletinMenu : IClickableMenu
    {
        private int internalWidth;
        private int internalHeight;
        private int xOffsetToInternal;
        private int yOffsetToInternal;
        private int itemHeight;
        private int separatorHeight;

        private string hoverText;

        private IList<IEconEvent> events;

        public NewsBulletinMenu(IEnumerable<IEconEvent> events)
            : base(x: 0, y: 0, width: 0, height: 0, showUpperRightCloseButton: true)
        {
            this.events = new List<IEconEvent>(events);

            // Calculate some numbers
            int numItems = this.events.Count;
            this.separatorHeight = 4 * Game1.pixelZoom;
            this.itemHeight = 50 * Game1.pixelZoom;

            this.internalWidth = 100 * Game1.pixelZoom;
            this.internalHeight = separatorHeight * (numItems-1) + itemHeight * (numItems);
            this.xOffsetToInternal = IClickableMenu.borderWidth;
            this.yOffsetToInternal = IClickableMenu.borderWidth + 7 * Game1.pixelZoom;

            // Calculate width, height, position on screen:
            this.width = this.internalWidth + 1 * Game1.pixelZoom + IClickableMenu.borderWidth * 2;
            this.height = this.internalHeight + 5 * Game1.pixelZoom + IClickableMenu.borderWidth * 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2;

            // Reinitialize close button:
            this.initializeUpperRightCloseButton();

            // Play sound!
            Game1.playSound("bigSelect");
        }

        public override void draw(SpriteBatch b)
        {
            var content = getContent().ToList();

            // Draw box and background fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(
                x: this.xPositionOnScreen,
                y: this.yPositionOnScreen - Game1.tileSize + Game1.tileSize / 8,
                width: this.width,
                height: this.height + Game1.tileSize,
                speaker: false,
                drawOnlyBox: true,
                message: null,
                objectDialogueWithPortrait: false);
            base.drawBorderLabel(b, "News Bulletin", Game1.smallFont, this.xPositionOnScreen + Game1.tileSize/2, this.yPositionOnScreen - Game1.tileSize + Game1.tileSize / 8);

            // Draw contents
            for( int i = 0; i < content.Count; i++ )
            {
                IEconEvent e = content[i].Item1;
                Rectangle rect = content[i].Item2;
                if (i != content.Count - 1)
                {
                    this.drawHorizontalPartition(b, rect.Bottom - (Game1.tileSize - this.separatorHeight) / 2, true);
                }
                
                Utility.DrawWrappedString(b, Game1.dialogueFont, e.Headline, rect, Color.Black);
            }

            // Draw close box and mouse
            base.draw(b);
            this.drawMouse(b);

            // Draw hover text!
            if (!string.IsNullOrEmpty(this.hoverText))
            {
                new EconEventHoverBox(StardewValley.Object.stone, 20).draw(b);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            //throw new NotImplementedException();
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverText = "";
            int i = 0;
            foreach (var content in getContent())
            {
                Rectangle rect = content.Item2;
                i++;
                if ( rect.Contains(x, y) )
                {
                    this.hoverText = content.Item1.HoverText;
                    break;
                }
            }
        }

        private IEnumerable<Tuple<IEconEvent, Rectangle>> getContent()
        {
            int x = this.xPositionOnScreen + this.xOffsetToInternal;
            int y = this.yPositionOnScreen + this.yOffsetToInternal;
            int dy = this.itemHeight + this.separatorHeight;
            return this.events.Select((e, i) => Tuple.Create(e, new Rectangle(x, y + i * dy, this.internalWidth, this.itemHeight)));
        }
    }
}
