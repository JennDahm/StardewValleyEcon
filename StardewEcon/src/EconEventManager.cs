using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace StardewEcon
{
    public class EconEventManager
    {
        #region Private Fields
        private IList<HeadlineTemplate> monthlyEvents;
        private IList<HeadlineTemplate> biweeklyEvents;
        private IList<HeadlineTemplate> weeklyEvents;

        private List<EconEvent> currentEvents;
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

        public IReadOnlyList<EconEvent> CurrentEvents => this.currentEvents;
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
            // TODO: Load player events
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
        public IReadOnlyList<EconEvent> GenerateNewEvents()
        {
            // Generate the RNG seeds
            Random monthRNG  = new Random(GetRNGSeed(EventType.Monthly, SDate.Now()));
            Random biweekRNG = new Random(GetRNGSeed(EventType.Biweekly, SDate.Now()));
            Random weekRNG   = new Random(GetRNGSeed(EventType.Weekly, SDate.Now()));

            // Generate the events.
            this.currentEvents = new List<EconEvent>()
            {
                RandomlySelectFromList(this.monthlyEvents, monthRNG).GenerateNewEvent(monthRNG),
                RandomlySelectFromList(this.biweeklyEvents, biweekRNG).GenerateNewEvent(biweekRNG),
                RandomlySelectFromList(this.weeklyEvents, weekRNG).GenerateNewEvent(weekRNG)
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

        private IList<HeadlineTemplate> LoadEventsFrom(string filename)
        {
            var list = new List<HeadlineTemplate>();
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

                    // Trim; Skip comments
                    string trimmedLine = line.Trim();
                    if(line[0] == '#')
                    {
                        continue;
                    }

                    list.Add(new HeadlineTemplate(trimmedLine));
                }
            }

            // If the file was empty or nonexistant, we need dummy text.
            if (list.Count == 0)
            {
                list.Add(new HeadlineTemplate("Nothing to report."));
            }

            return list;
        }

        private T RandomlySelectFromList<T>(IList<T> list, Random rand = null)
        {
            if ((list?.Count ?? 0) == 0)
            {
                return default(T);
            }
            rand = rand ?? new Random();

            return list[rand.Next(list.Count)];
        }

        /**
         * <summary>Generates a deterministic RNG seed based on the game date and ID.</summary>
         * <remarks>
         *  In order to keep the event types as independent as possible, each
         *  type has a separately generated RNG seed. Each seed is created by
         *  left shifting the year number by a small amount, bitwise ORing
         *  another number (A), and then bitwise XORing the entire result by
         *  the player's game ID, giving a unique seed for each week on each
         *  playthrough.
         *  
         *  For monthly events, the number (A) is a number 0-3 representing
         *  the season number. For biweekly events, the number (A) is a number
         *  0-1 representing which half of the month we're in. For weekly events,
         *  the number (A) is a number 0-3 representing which week of the month
         *  we're in.
         *  
         *  The game ID is retrieved via the expression
         *  <code>StardewValley.Game1.uniqueIDForThisGame</code>
         * </remarks>
         * 
         * <param name="type">The type of event being generated.</param>
         * <param name="date">The date of generation.</param>
         * <returns>A deterministicly generated RNG seed.</returns>
         */
        private int GetRNGSeed(EventType type, SDate date)
        {
            int year = date.Year;
            int gameID = (int)StardewValley.Game1.uniqueIDForThisGame;

            switch(type)
            {
                case EventType.Monthly:
                    int month = date.SeasonAsInt(); // range: 0-3 (2 bits)
                    return ((year << 2) | month) ^ gameID;

                case EventType.Biweekly:
                    int biweek = date.Week() / 2; // range: 0-1 (1 bit)
                    return ((year << 1) | biweek) ^ gameID;

                case EventType.Weekly:
                    int week = date.Week(); // range: 0-3 (2 bits)
                    return ((year << 2) | week) ^ gameID;

                default:
                    return 0; // TODO: Some kind of error?
            }
        }

        private enum EventType
        {
            Monthly,
            Biweekly,
            Weekly
        }
        #endregion
    }
}
