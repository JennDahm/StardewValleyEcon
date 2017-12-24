using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace StardewEcon
{
    class EconEventManager
    {
        #region Private Fields
        private IList<IEconEvent> monthlyEvents;
        private IList<IEconEvent> biweeklyEvents;
        private IList<IEconEvent> weeklyEvents;

        private List<IEconEvent> currentEvents;
        #endregion

        #region Constructors
        public EconEventManager(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.LoadEventLists();
        }
        #endregion

        #region Public Properties
        public IModHelper Helper { get; }

        public IMonitor Monitor { get; }

        public IReadOnlyList<IEconEvent> CurrentEvents => this.currentEvents;
        #endregion

        #region Public Functions
        /**
         * <summary>Attempts to load the events last saved for the user.</summary>
         * 
         * <returns>True if the operation was successful; false if it was not.</returns>
         * <remarks>Failure here indicates that we need to generate entirely new events.</remarks>
         */
        public bool TryLoadPlayerEvents()
        {
            // TODO
            return false;
        }

        /**
         * <summary>Discards current events and generates an entirely new set.</summary>
         * <remarks>
         *  This will automatically apply the new events. See <see cref="ApplyEvents"/>
         * </remarks>
         * 
         * <returns>The new set generated. This is also available via the CurrentEvents property.</returns>
         */
        public IReadOnlyList<IEconEvent> GenerateNewEvents()
        {
            this.currentEvents = new List<IEconEvent>()
            {
                RandomlySelectFromList(this.monthlyEvents),
                RandomlySelectFromList(this.biweeklyEvents),
                RandomlySelectFromList(this.weeklyEvents)
            };

            this.ApplyEvents();

            return this.currentEvents;
        }

        /**
         * <summary>Checks the current date and updates all events if necessary.</summary>
         * <remarks>
         *  If events are to be updated, this function will automatically
         *  unapply old events and apply new events.
         * </remarks>
         * 
         * <returns>True if the events were changed; false if no change occured.</returns>
         */
        public bool UpdateEvents()
        {
            // TODO
            return false;
        }

        /**
         * <summary>Applies pricing changes to all affected items in the game.</summary>
         * <remarks>
         *  This affects items in the player's inventory and chests as
         *  well as newly created items. This does not affect store prices, as
         *  those are hard-coded.
         * </remarks>
         * <seealso cref="UnapplyEvents"/>
         */
        public void ApplyEvents()
        {
            // TODO

            // This is test code for affecting the pricing of objects.
            /*int index = Object.stone;
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
            this.Monitor.Log($"Modified price of stone to 500.", LogLevel.Info);*/
        }

        /**
         * <summary>Unapplies pricing changes to all affected items in the game.</summary>
         * <remarks>
         *  This should be called when the player returns to the title screen so
         *  that events don't carry over between save files by mistake.
         * </remarks>
         */
        public void UnapplyEvents()
        {
            // TODO
        }
        #endregion

        #region Private Functions
        /**
         * <summary>Loads and parses events from the mod configuration folder.</summary>
         */
        private void LoadEventLists()
        {
            this.monthlyEvents = this.LoadEventsFrom(@"config/monthly.txt");
            this.biweeklyEvents = this.LoadEventsFrom(@"config/biweekly.txt");
            this.weeklyEvents = this.LoadEventsFrom(@"config/weekly.txt");
        }

        private IList<IEconEvent> LoadEventsFrom(string filename)
        {
            var list = new List<IEconEvent>();
            var filepath = Path.Combine(this.Helper.DirectoryPath, filename);
            var fileinfo = new FileInfo(filepath);

            // Make sure the file exists before reading from it
            if (fileinfo.Exists)
            {
                foreach (string line in File.ReadLines(filepath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string trimmedLine = line.Trim();
                    list.Add(new EconEvent(trimmedLine, StardewValley.Object.topazIndex, -10, 100));
                }
            }

            // If the file was empty or nonexistant, we need dummy text.
            if (list.Count == 0)
            {
                list.Add(new EconEvent("Nothing to report.", -1, 0, 0));
            }

            return list;
        }

        private T RandomlySelectFromList<T>(IList<T> list)
        {
            if ((list?.Count ?? 0) == 0)
            {
                return default(T);
            }

            return list[new Random().Next(list.Count)];
        }
        #endregion
    }
}
