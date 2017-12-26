using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static StardewEcon.EconEvent;

namespace StardewEcon
{
    /**
     * <summary>Generates randomized content for HeadlineTemplates.</summary>
     * <remarks>
     *  This class has static content that must be loaded before use using the
     *  static <see cref="LoadContentFiles(string)"/> function.
     *  
     *  
     * </remarks>
     * <example>
     *  <code>
     *      EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);
     *      HeadlineTemplate template;
     *      // ...
     *      var factory = EconEventFactory.Create(EventType.Monthly, SDate.Now());
     *      EconEvent event = template.GenerateNewEvent(factory);
     *  </code>
     * </example>
     */
    public class EconEventFactory : IHeadlineContentProvider
    {
        #region Constant Configuration File Locations
        public const string MonthlyEventsFilePath = @"config/monthly.txt";
        public const string BiweeklyEventsFilePath = @"config/biweekly.txt";
        public const string WeeklyEventsFilePath = @"config/weekly.txt";

        public const string LocationsFilePath = @"config/locations.txt";
        public const string CropsFilePath = @"config/crops.txt";
        #endregion

        #region Static Content Lists
        private static IReadOnlyDictionary<EventType, IReadOnlyList<HeadlineTemplate>> _EventTemplates;

        private static IReadOnlyList<string> _Locations;
        private static IReadOnlyList<int> _Crops;
        #endregion

        #region Instance Fields
        private Random rand;
        private IReadOnlyList<HeadlineTemplate> templates;

        private EventType eventType;
        private SDate dateCreated;
        #endregion

        private static bool _Initialized = false;

        private EconEventFactory(EventType type, SDate date)
        {
            this.eventType = type;
            this.templates = _EventTemplates[type];

            this.ResetToDate(date);
        }

        public EventType EventType => this.eventType;

        public SDate Date => this.dateCreated;

        #region Public Static Functions
        /**
         * <summary>Loads the content files to use when generating headlines.</summary>
         * <remarks>
         *  This must be called before any other public functions are called.
         * </remarks>
         * 
         * <param name="modLocation">The location the mod is installed in.</param>
         */
        public static void LoadContentFiles(string modDir)
        {
            if (!_Initialized)
            {
                var eventTemplates = new Dictionary<EventType, IReadOnlyList<HeadlineTemplate>>();
                eventTemplates.Add(EventType.Monthly, LoadEventTemplateList(Path.Combine(modDir, MonthlyEventsFilePath)));
                eventTemplates.Add(EventType.Biweekly, LoadEventTemplateList(Path.Combine(modDir, BiweeklyEventsFilePath)));
                eventTemplates.Add(EventType.Weekly, LoadEventTemplateList(Path.Combine(modDir, WeeklyEventsFilePath)));
                _EventTemplates = eventTemplates;


                _Locations = LoadStringList(Path.Combine(modDir, LocationsFilePath));
                _Crops = LoadItemList(Path.Combine(modDir, CropsFilePath));

                // Safeguards against missing/empty files:
                if (_Locations.Count == 0)
                {
                    _Locations = new List<string>() { "Zuzu City" };
                }
                if (_Crops.Count == 0)
                {
                    _Crops = new List<int>() { StardewValley.Object.stone };
                }

                _Initialized = true;
            }
        }

        /**
         * <summary>Create a new factory for the given event type on the given date.</summary>
         * <remarks>
         *  For deteminism, a new factory can be created for every headline that
         *  you generate, or you can use the <see cref="ResetRNG()"/> or <see cref="ResetToDate(SDate)"/>
         *  methods to give a level of determinism to the randomness.
         * </remarks>
         * 
         * <param name="type">The type of event to be generated.</param>
         * <param name="date">The game date on which the event is generated. (Default: current date)</param>
         * <returns>An EconEventFactory to be used to generate an event for the given type on the given date.</returns>
         */
        public static EconEventFactory Create(EventType type, SDate date = null)
        {
            AssertInitialized();
            date = date ?? SDate.Now();

            return new EconEventFactory(type, date);
        }
        #endregion

        #region Public Functions
        /**
         * <summary>Generates a new, randomly generated <see cref="EconEvent"/>.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will return an entirely different <see cref="EconEvent"/>.
         * </remarks>
         * 
         * <returns>A fully generated EconEvent.</returns>
         */
        public EconEvent GenerateRandomEvent()
        {
            return RandomlySelectFromList(this.templates)?.GenerateNewEvent(this);
        }

        /**
         * <summary>Resets the internal RNG to its original state when the factory was created.</summary>
         * <remarks>
         *  This can be used to force a factory to generate the same event
         *  multiple times without creating a new one every time.
         * </remarks>
         * 
         * <returns>The same factory itself.</returns>
         */
        public EconEventFactory ResetRNG()
        {
            this.rand = new Random(GetRNGSeed(this.eventType, this.dateCreated));
            return this;
        }

        /**
         * <summary>Resets the factory to its original state as though it were created with the given date.</summary>
         * <remarks>
         *  This can be used to avoid recreating factories every time new events
         *  need to be generated.
         * </remarks>
         * 
         * <param name="date">The date to reset to. (Default: current date)</param>
         * <returns>The same factory itself.</returns>
         */
        public EconEventFactory ResetToDate(SDate date = null)
        {
            this.dateCreated = date ?? SDate.Now();
            return ResetRNG();
        }

        #region IHeadlineContentProvider implementation
        public Random GetRNG()
        {
            return this.rand;
        }

        public string GetRandomLocation()
        {
            return RandomlySelectFromList(_Locations);
        }

        public int GetRandomCrop()
        {
            return RandomlySelectFromList(_Crops);
        }
        #endregion
        #endregion

        #region Private Static Setup Functions
        private static List<HeadlineTemplate> LoadEventTemplateList(string absFilepath)
        {
            var list = new List<HeadlineTemplate>();
            var fileinfo = new FileInfo(absFilepath);
            
            if (fileinfo.Exists)
            {
                // Select all nonempty lines that are not comments.
                list = File.ReadLines(absFilepath)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Where(s => s[0] != '#')
                    .Select(s => new HeadlineTemplate(s))
                    .ToList();
            }

            // If the file was empty or nonexistant, we need dummy text.
            if (list.Count == 0)
            {
                list.Add(new HeadlineTemplate("Nothing to report."));
            }

            return list;
        }

        private static List<string> LoadStringList(string absFilepath)
        {
            var fileinfo = new FileInfo(absFilepath);

            if (!fileinfo.Exists)
            {
                return new List<String>();
            }

            // Select all nonempty lines that are not comments.
            return File.ReadLines(absFilepath)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Where(s => s[0] != '#')
                .ToList();
        }

        private static List<int> LoadItemList(string absFilepath)
        {
            var fileinfo = new FileInfo(absFilepath);

            if (!fileinfo.Exists)
            {
                return new List<int>();
            }

            // Select all nonempty lines that are not comments and can
            // parse to non-negative integers.
            return File.ReadLines(absFilepath)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Where(s => s[0] != '#')
                .Select(s =>
                {
                    int result = -1;
                    int.TryParse(s, out result);
                    return result;
                })
                .Where(i => 0 <= i)
                .ToList();
        }
        #endregion

        #region Private Static Utility Functions
        private static void AssertInitialized()
        {
            if( !EconEventFactory._Initialized )
            {
                throw new InvalidOperationException("The class hasn't been initialized! Call EconEventFactory.LoadContentFiles(string) first.");
            }
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

            switch (type)
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

        private T RandomlySelectFromList<T>(IReadOnlyList<T> list)
        {
            if ((list?.Count ?? 0) == 0)
            {
                return default(T);
            }

            return list[this.rand.Next(list.Count)];
        }
        #endregion
    }
}
