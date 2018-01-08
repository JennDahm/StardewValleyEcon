using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewEcon.Econ;

namespace StardewEcon.View
{
    /**
     * <summary>An interactive object that displays the News Bulletin.</summary>
     * <remarks>
     *  This is an invisible object placed on top of the tile outside of Pierre's
     *  shop that normally shows you his hours. When interacted with, it will
     *  bring up the News Bulletin menu.
     *  
     *  This object is not to be saved with the game. It may cause the game to
     *  panic if it is in the objects list when the game is saved or loaded.
     *  Please call <see cref="RemoveBeforeSaving"/> before the game is saved
     *  and <see cref="ReplaceAfterSaving"/> after the game is saved.
     * </remarks>
     */
    class NewsBulletinObject : StardewValley.Object
    {
        /**
         * <summary>The event manager managing the events to display.</summary>
         */
        public readonly EconEventManager eventManager;

        /**
         * <summary>A reference to the area this object is hosted in.</summary>
         * <remarks>
         *  This is here simply so that we don't need to provide it with each
         *  call to <see cref="RemoveBeforeSaving"/> and <see cref="ReplaceAfterSaving"/>,
         *  which is pretty lazy. Luckily, this doesn't change during gameplay.
         * </remarks>
         */
        private GameLocation area;

        /**
         * <summary>Create a new NewsBulletinObject that draws its events from the given manager.</summary>
         * 
         * <param name="eventManager">The event manager that provides the current events.</param>
         */
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

        /**
         * <summary>Places ourself into the given area.</summary>
         * <remarks>
         *  This tells both ourself and the given area where we are. We save the
         *  area parameter in order to use it later when removing and replacing
         *  ourself in the area around save operations.
         * </remarks>
         * 
         * <param name="area">The game area to place ourself in.</param>
         * <param name="loc">The tile location within the game area to place ourself at.</param>
         * 
         * <seealso cref="RemoveBeforeSaving"/>
         * <seealso cref="ReplaceAfterSaving"/>
         */
        public void SetInTown(GameLocation area, Vector2 loc)
        {
            this.area = area;
            this.TileLocation = loc;
            this.area.Objects.Add(this.TileLocation, this);
            this.boundingBox.X = (int) loc.X * Game1.tileSize;
            this.boundingBox.Y = (int) loc.Y * Game1.tileSize;
        }

        /**
         * <summary>Temporarily removes ourself from our game area.</summary>
         * <remarks>
         *  This should come before a save operation. This exists to prevent any
         *  kind of (de)serialization panic from the game not understanding how
         *  to deal with this class.
         * </remarks>
         * 
         * <seealso cref="ReplaceAfterSaving"/>
         */
        public void RemoveBeforeSaving()
        {
            if( this.area?.Objects?[this.TileLocation] == this )
            {
                this.area.Objects.Remove(this.TileLocation);
            }
        }

        /**
         * <summary>Replaces ourself in our last known game area and location.</summary>
         * <remarks>
         *  This should come after a save operation.
         * </remarks>
         * 
         * <seealso cref="RemoveBeforeSaving"/>
         */
        public void ReplaceAfterSaving()
        {
            this.area?.Objects?.Add(this.TileLocation, this);
        }

        /**
         * <summary>Checks whether this object could perform an action, and optionally performs it.</summary>
         * <remarks>
         *  You'll have to read the game source code to understand how this function works.
         *  It's somehow different from <see cref="isActionable(StardewValley.Farmer)"/>.
         * </remarks>
         * 
         * <param name="who">The Farmer interacting with the object.</param>
         * <param name="justCheckingForActivity">Whether or not to perform any actions.</param>
         * 
         * <seealso cref="isActionable(StardewValley.Farmer)"/>
         */
        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
            {
                return true;
            }

            Game1.activeClickableMenu = new NewsBulletinMenu(eventManager.CurrentEvents);
            return true;
        }

        /**
         * <summary>Checks whether this object could perform an action.</summary>
         * <remarks>
         *  You'll have to read the game source code to understand how this function works.
         *  It's somehow different from <see cref="checkForAction(StardewValley.Farmer, bool)"/>.
         *  
         *  That said, it seems like this code is only used when mousing over an object.
         * </remarks>
         */
        public override bool isActionable(StardewValley.Farmer who)
        {
            // HACK: Force inspectable icon on this and the tile above it.
            if (Game1.currentCursorTile.Equals(this.TileLocation)
                || Game1.currentCursorTile.Equals(this.TileLocation - new Vector2(0, 1))) {
                Game1.isInspectionAtCurrentCursorTile = true;
            }
            return true;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         * <remarks>
         *  Day updates are handled in <see cref="ModEntry"/>.
         * </remarks>
         */
        public override void DayUpdate(GameLocation location)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            return;
        }

        /**
         * <summary>Overrides to force impassibility.</summary>
         */
        public override bool isPassable()
        {
            return false;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         * <remarks>
         *  We don't want tools to do anything to this object.
         * </remarks>
         */
        public override bool performToolAction(Tool t)
        {
            return false;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         * <remarks>
         *  There is no reasonable action to perform here.
         * </remarks>
         */
        public override bool performObjectDropInAction(Object dropIn, bool probe, StardewValley.Farmer who)
        {
            return false;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void updateWhenCurrentLocation(GameTime time)
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         */
        public override void actionOnPlayerEntry()
        {
            return;
        }

        /**
         * <summary>Overrides to do nothing.</summary>
         * <remarks>
         *  We do not want anything to remove this object except ourself.
         * </remarks>
         */
        public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
        {
            return;
        }
    }
}
