using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

using Object = StardewValley.Object;
using System.Collections.Generic;

namespace StardewEcon
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private IEnumerable<EconEvent> events;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.events = new List<EconEvent>()
            {
                new EconEvent("This is the first headline.", "+20%"),
                new EconEvent("This is the second headline.", "-10%"),
                new EconEvent("This is the third headline.", "+5%")
            };

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
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
            if(e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.P)
            {
                //Game1.activeClickableMenu = new Billboard(false);
                Game1.activeClickableMenu = new NewsBulletinMenu(this.events);
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            this.Monitor.Log($"Loaded save for {Game1.player.name}.", LogLevel.Info);

            int index = Object.stone;
            string str;
            Game1.objectInformation.TryGetValue(index, out str);
            
            string[] fields = str.Split('/');
            fields[1] = 500.ToString();
            str = string.Join("/", fields);

            var chests = Game1.locations.SelectMany(l => l.objects.Values.OfType<StardewValley.Objects.Chest>());
            var stones = chests.SelectMany(c => c.items.OfType<Object>()).Where(o => o.parentSheetIndex == index);
            foreach(var stone in stones)
            {
                stone.price = 500;
            }

            Game1.objectInformation[index] = str;
            this.Monitor.Log($"Modified price of stone to 500.", LogLevel.Info);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            ShopMenu menu = e.NewMenu as ShopMenu;
            if(menu == null)
            {
                return;
            }

            this.Monitor.Log($"Player opened shop menu!", LogLevel.Debug);
            Dictionary<Item, int[]> items = this.Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");
            items.FirstOrDefault().Value[0] = 10000;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            var stone = new Object(Object.stone, 1);
            var price = stone.price;
            string time = Game1.getTimeOfDayString(Game1.timeOfDay);
            this.Monitor.Log($"Price of stone at {time} : {price}", LogLevel.Debug);
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            var stone = new Object(Object.stone, 1);
            var price = stone.price;
            this.Monitor.Log($"Price of stone at beginning of day: {price}", LogLevel.Debug);
        }
    }
}
