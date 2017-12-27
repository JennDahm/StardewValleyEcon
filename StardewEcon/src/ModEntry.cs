using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

using Object = StardewValley.Object;
using System.Collections.Generic;

namespace StardewEcon
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private EconEventManager eventManager;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Load static content
            EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);

            // Set up hooks
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        /*********
        ** Private methods
        *********/

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            // TEST CODE
            // This code is to test the news bulletin while I work on a way to
            // integrate it into the game more naturally.
            if( !Context.IsWorldReady )
            {
                // Ignore key presses while the world is 
                return;
            }

            if (!Context.IsPlayerFree)
            {
                if (Game1.activeClickableMenu is NewsBulletinMenu)
                {
                    // TODO: Close the menu?
                }
            }
            else
            {
                if (e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.P)
                {
                    //Game1.activeClickableMenu = new Billboard(false);
                    Game1.activeClickableMenu = new NewsBulletinMenu(this.eventManager.CurrentEvents);
                }
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            this.Monitor.Log($"Loaded save for {Game1.player.name}.", LogLevel.Info);

            // Create the event manager and load events for the player.
            this.eventManager = new EconEventManager(this.Helper, this.Monitor);
            this.eventManager.LoadPlayerEvents();
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            // Unapply events so that they don't carry over into other game saves
            this.eventManager.UnapplyEvents();
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // We want to check for shop menus so that we can modify prices there.
            // Sometimes prices are hard-coded in shops, so we need to get around that.
            ShopMenu menu = e.NewMenu as ShopMenu;
            if(menu == null)
            {
                return;
            }

            // TEST CODE
            // This code simply sets the first item in the shop to have a price
            // of 10,000g to show that we can modify prices.
            this.Monitor.Log($"Player opened shop menu!", LogLevel.Debug);
            Dictionary<Item, int[]> items = this.Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");
            items.FirstOrDefault().Value[0] = 10000;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            // TEST CODE
            // This code monitors the price of an item of interest to ensure
            // that newly created objects of that type have the correct price.
            var stone = new Object(Object.stone, 1);
            var price = stone.price;
            string time = Game1.getTimeOfDayString(Game1.timeOfDay);
            this.Monitor.Log($"Price of stone at {time} : {price}");
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // Update events if necessary.
            if( this.eventManager.UpdateEvents() )
            {
                this.Monitor.Log("Events have changed at the beginning of the day.");
            }

            // TEST CODE
            // This code monitors the price of an item of interest to ensure
            // that newly created objects of that type have the correct price.
            var stone = new Object(Object.stone, 1);
            var price = stone.price;
            this.Monitor.Log($"Price of stone at beginning of day: {price}");
        }
    }
}
