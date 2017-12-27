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
        #region Configuration File Constants
        public const string MonthlyEventsFilePath = @"config/Monthly.txt";
        public const string BiweeklyEventsFilePath = @"config/Biweekly.txt";
        public const string WeeklyEventsFilePath = @"config/Weekly.txt";

        public const string LocationsFilePath = @"config/Locations.txt";
        public const string DefaultLocation = "Zuzu City";

        public const string CropsFilePath = @"config/Crops.txt";
        public const int DefaultCrop = 400; // Strawberry

        public const string MineralsFilePath = @"config/Minerals.txt";
        public const int DefaultMineral = 62; // Aquamarine

        public const string ForagedGoodsFilePath = @"config/ForagedGoods.txt";
        public const int DefaultForagedGood = 78; // Cave Carrot

        public const string RiverFishFilePath = @"config/RiverFish.txt";
        public const int DefaultRiverFish = 702; // Chub

        public const string OceanFishFilePath = @"config/OceanFish.txt";
        public const int DefaultOceanFish = 151; // Squid

        public const string ArtisanGoodsFilePath = @"config/ArtisanGoods.txt";
        public const int DefaultArtisanGood = 340; // Honey

        public const string CookedItemsFilePath = @"config/CookedItems.txt";
        public const int DefaultCookedItem = 220; // Chocolate Cake
        #endregion

        #region Static Content Lists
        private static IReadOnlyDictionary<EventType, IReadOnlyList<HeadlineTemplate>> _EventTemplates;

        private static IReadOnlyList<string> _Locations;
        private static IReadOnlyList<int> _Crops;
        private static IReadOnlyList<int> _Minerals;
        private static IReadOnlyList<int> _ForagedGoods;
        private static IReadOnlyList<int> _RiverFish;
        private static IReadOnlyList<int> _OceanFish;
        private static IReadOnlyList<int> _ArtisanGoods;
        private static IReadOnlyList<int> _CookedItems;
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
                var eventTemplates = new Dictionary<EventType, IReadOnlyList<HeadlineTemplate>>
                {
                    { EventType.Monthly, LoadEventTemplateList(Path.Combine(modDir, MonthlyEventsFilePath)) },
                    { EventType.Biweekly, LoadEventTemplateList(Path.Combine(modDir, BiweeklyEventsFilePath)) },
                    { EventType.Weekly, LoadEventTemplateList(Path.Combine(modDir, WeeklyEventsFilePath)) }
                };
                _EventTemplates = eventTemplates;


                _Locations    = LoadStringList(Path.Combine(modDir, LocationsFilePath), DefaultLocation);
                _Crops        = LoadItemList(Path.Combine(modDir, CropsFilePath), DefaultCrop);
                _Minerals     = LoadItemList(Path.Combine(modDir, MineralsFilePath), DefaultMineral);
                _ForagedGoods = LoadItemList(Path.Combine(modDir, ForagedGoodsFilePath), DefaultForagedGood);
                _RiverFish    = LoadItemList(Path.Combine(modDir, RiverFishFilePath), DefaultRiverFish);
                _OceanFish    = LoadItemList(Path.Combine(modDir, OceanFishFilePath), DefaultOceanFish);
                _ArtisanGoods = LoadItemList(Path.Combine(modDir, ArtisanGoodsFilePath), DefaultArtisanGood);
                _CookedItems  = LoadItemList(Path.Combine(modDir, CookedItemsFilePath), DefaultCookedItem);

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

        public int GetRandomMineral()
        {
            return RandomlySelectFromList(_Minerals);
        }

        public int GetRandomForagedItem()
        {
            return RandomlySelectFromList(_ForagedGoods);
        }

        public int GetRandomRiverFish()
        {
            return RandomlySelectFromList(_RiverFish);
        }

        public int GetRandomOceanFish()
        {
            return RandomlySelectFromList(_OceanFish);
        }

        public int GetRandomArtisanGood()
        {
            return RandomlySelectFromList(_ArtisanGoods);
        }

        public int GetRandomCookedItem()
        {
            return RandomlySelectFromList(_CookedItems);
        }

        public string GetRandomEarthquake()
        {
            // For now, return a random one-fraction-place number in the range [4, 8)
            var scale = this.rand.Next(40, 70) / 10.0;
            return scale.ToString("0.0");
        }

        public string GetRandomFatalities()
        {
            // For now, return a random int between 1 and 10, inclusively
            return this.rand.Next(1, 10 + 1).ToString();
        }
        #endregion
        #endregion

        #region Private Static Setup Functions
        /**
         * <summary>Loads a list of event templates from the given file.</summary>
         * <param name="absFilepath">The absolute filepath to the file to load.</param>
         */
        private static List<HeadlineTemplate> LoadEventTemplateList(string absFilepath)
        {
            return LoadSimpleList(
                absFilepath,
                new HeadlineTemplate("Nothing to report."),
                s => new HeadlineTemplate(s),
                t => true);
        }

        /**
         * <summary>Loads a list of basic strings from the given file.</summary>
         * <param name="absFilepath">The absolute filepath to the file to load.</param>
         * <param name="defaultIfEmpty">The string to use if the file is empty/nonexistant.</param>
         */
        private static List<string> LoadStringList(string absFilepath, string defaultIfEmpty)
        {
            return LoadSimpleList(
                absFilepath,
                defaultIfEmpty,
                s => s,
                s => true);
        }

        /**
         * <summary>Loads a list of item IDs from the given file.</summary>
         * <param name="absFilepath">The absolute filepath to the file to load.</param>
         * <param name="defaultIfEmpty">The item ID to use if the file is empty/nonexistant.</param>
         */
        private static List<int> LoadItemList(string absFilepath, int defaultIfEmpty)
        {
            return LoadSimpleList(
                absFilepath,
                defaultIfEmpty,
                s =>
                {
                    int result = -1;
                    int.TryParse(s, out result);
                    return result;
                },
                i => 0 <= i);
        }

        /**
         * <summary>Loads a simple line-by-line list from a file.</summary>
         * <remarks>
         *  Several of our files can be reduced to this same, simple line-by-line
         *  format. Rather than duplicating code, we can centralize it here.
         *  
         *  The filter can be used with the translator when it's possible to
         *  have invalid values.
         * </remarks>
         * 
         * <param name="absFilepath">The absolute filepath to the file to load.</param>
         * <param name="defaultIfEmpty">The default value to put into the list if the file does not exist or is empty.</param>
         * <param name="translator">A translator from string format to the real value.</param>
         * <param name="filter">A filter on the translated values - true to keep, false to skip.</param>
         */
        private static List<T> LoadSimpleList<T>(string absFilepath, T defaultIfEmpty, Func<string, T> translator, Func<T, bool> filter)
        {
            var fileinfo = new FileInfo(absFilepath);

            List<T> list = new List<T>();
            if (fileinfo.Exists)
            {

                // Select all nonempty lines that are not comments and pass the
                // given filter.
                list = File.ReadLines(absFilepath)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Where(s => s[0] != '#')
                    .Select(translator)
                    .Where(filter)
                    .ToList();
            }

            // Safeguard against missing/empty files:
            if (list.Count == 0)
            {
                list.Add(defaultIfEmpty);
            }
            return list;
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
