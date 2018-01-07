using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewEcon.Econ;

namespace StardewEcon.View
{
    class NewsBulletinObject : StardewValley.Object
    {
        public readonly EconEventManager eventManager;

        private GameLocation area;

        public NewsBulletinObject(EconEventManager eventManager)
        {
            this.eventManager = eventManager;

            // Set up some properties:
            this.name = null;
            this.displayName = null;
            this.type = "interactive";
            this.fragility = 2; // Prevent it from breaking
            this.boundingBox = new Rectangle(0, 0, Game1.tileSize, Game1.tileSize);

            this.canBeGrabbed = false;
            this.isRecipe = false;
            this.isLamp = false;
            this.questItem = false;
        }

        public void SetInTown(GameLocation area, Vector2 loc)
        {
            this.area = area;
            this.TileLocation = loc;
            this.area.Objects.Add(this.TileLocation, this);
            this.boundingBox.X = (int) loc.X * Game1.tileSize;
            this.boundingBox.Y = (int) loc.Y * Game1.tileSize;
        }

        public void RemoveBeforeSaving()
        {
            if( this.area?.Objects?[this.TileLocation] == this )
            {
                this.area.Objects.Remove(this.TileLocation);
            }
        }

        public void ReplaceAfterSaving()
        {
            this.area?.Objects?.Add(this.TileLocation, this);
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
            {
                return true;
            }

            Game1.activeClickableMenu = new NewsBulletinMenu(eventManager.CurrentEvents);
            return true;
        }

        public override bool isActionable(StardewValley.Farmer who)
        {
            // Force inspectable icon
            if (Game1.currentCursorTile.Equals(this.TileLocation)
                || Game1.currentCursorTile.Equals(this.TileLocation - new Vector2(0, 1))) {
                Game1.isInspectionAtCurrentCursorTile = true;
            }
            return true;
        }

        public override void DayUpdate(GameLocation location)
        {
            // TODO: Maybe handle telling the manager what to do here? Dunno.
            return;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            return;
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            return;
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            return;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            return;
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            return;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            return;
        }

        public override bool isPassable()
        {
            return false;
        }

        public override bool performToolAction(Tool t)
        {
            return false;
        }

        public override bool performObjectDropInAction(Object dropIn, bool probe, StardewValley.Farmer who)
        {
            return false;
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            return;
        }

        public override void actionOnPlayerEntry()
        {
            return;
        }

        public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
        {
            return;
        }
    }
}
