using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static StardewEcon.EconEvent;

namespace StardewEcon
{
    /**
     * <summary>Generates randomized EconEvents and content for HeadlineTemplates.</summary>
     * <remarks>
     *  Although to high-level code this class is in charge of creating
     *  <see cref="EconEvent"/>s, it doesn't handle the actual construction
     *  of them. That job is assigned to <see cref="HeadlineTemplate"/>,
     *  which constructs EconEvents based on text templates using substitution
     *  content that this class provides. External code doesn't deal directly
     *  with HeadlineTemplates, however - this class manages the lists of
     *  templates itself.
     *  
     *  The RNG used for this factory is seeded based on a combination of the
     *  player's game ID and the in-game date. Monthly, Biweekly, and Weekly
     *  events each use different formulas that ensure that, for example, the
     *  week number doesn't affect the seed for Monthly events, but the month
     *  number does affect the seed for Weekly events.
     *  
     *  This class has static content that must be loaded before use using the
     *  static <see cref="LoadContentFiles(string)"/> method. This static content
     *  includes the list of HeadlineTemplates and lists of all substitutable
     *  content that HeadlineTemplates would need to know in order to construct
     *  an EconEvent.
     *  
     *  All of this static content is located in the `config` folder of this
     *  mod's installation directory. Refer to the constants of this class
     *  like <see cref="MonthlyEventsFilePath"/> for the locations of
     *  individual files.
     *  
     *  Rather than a constructor, please use the static <see cref="Create(EventType, SDate)"/>
     *  method. This method asserts that all static content is set up before any
     *  new space is allocated.
     * </remarks>
     * 
     * <example>
     *  Here is an example of a one-off factory used to generate an event:
     *  <code>
     *      EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);
     *      // ...
     *      var factory = EconEventFactory.Create(EventType.Monthly, SDate.Now());
     *      EconEvent event = factory.GenerateRandomEvent();
     *  </code>
     *  
     *  Here is an example of a factory used several times to make events in
     *  a deterministic and repeatable fashion. Every time this example is run
     *  with the same config files, it will produce the same set of events.
     *  <code>
     *      EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);
     *      factory = EconEventFactory.Create(Event.Weekly, new SDate(1, "spring"));
     *      EconEvent event1 = factory.GenerateRandomEvent();
     *      EconEvent event2 = factory.ReseedWithDate(new SDate(8, "spring")).GenerateRandomEvent()
     *      EconEvent event3 = factory.ReseedWithDate(new SDate(15, "spring")).GenerateRandomEvent()
     *      EconEvent event4 = factory.ReseedWithDate(new SDate(22, "spring")).GenerateRandomEvent()
     *  </code>
     *  
     *  Alternatively, if you reset the factory to its initial date, it will
     *  generate the same event over and over again. All the events produced
     *  running this example will be the same:
     *  <code>
     *      EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);
     *      factory = EconEventFactory.Create(Event.Weekly, new SDate(1, "spring"));
     *      EconEvent event1 = factory.GenerateRandomEvent();
     *      EconEvent event2 = factory.ResetRNG().GenerateRandomEvent()
     *      EconEvent event3 = factory.ResetRNG().GenerateRandomEvent()
     *      EconEvent event4 = factory.ResetRNG().GenerateRandomEvent()
     *  </code>
     * </example>
     * 
     * <seealso cref="EconEventManager"/>
     * <seealso cref="HeadlineTemplate"/>
     */
    public class EconEventFactory : IHeadlineContentProvider
    {
        #region Configuration File Constants
        // Note: The file paths here are relative to the mod's install directory.
        public const string MonthlyEventsFilePath = @"config/Monthly.txt";
        public const string BiweeklyEventsFilePath = @"config/Biweekly.txt";
        public const string WeeklyEventsFilePath = @"config/Weekly.txt";

        // Note: The defaults selected here are arbitrary.
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
        /**
         * <summary>The internal RNG.</summary>
         */
        private Random rand;

        /**
         * <summary>The list of headline templates we can draw from.</summary>
         */
        private readonly IReadOnlyList<HeadlineTemplate> templates;

        /**
         * <summary>The type of event we generate.</summary>
         */
        private readonly EventType eventType;

        /**
         * <summary>The in-game date used to seed the internal RNG.</summary>
         */
        private SDate dateSeed;
        #endregion

        /**
         * <summary>Whether or not we've initialized our static content.</summary>
         */
        private static bool _Initialized = false;

        /**
         * <summary>Creates a new factory of the given type and seeds its RNG with the given date.</summary>
         */
        private EconEventFactory(EventType type, SDate date)
        {
            this.eventType = type;
            this.templates = _EventTemplates[type];

            this.ReseedWithDate(date);
        }

        #region Public Properties
        /**
         * <summary>The type of event we generate.</summary>
         */
        public EventType EventType => this.eventType;

        /**
         * <summary>The in-game date used to seed this factory's RNG.</summary>
         */
        public SDate Date => this.dateSeed;
        #endregion

        #region Public Static Functions
        /**
         * <summary>Loads the content files to use when generating headlines.</summary>
         * <remarks>
         *  This must be called before any other public functions are called.
         * </remarks>
         * 
         * <param name="modDir">The directory that the mod is installed in.</param>
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
         * <summary>Create a new factory for the given event type seeded with the given date.</summary>
         * <remarks>
         *  For deteminism, a new factory can be created for every headline that
         *  you generate, or you can use the <see cref="ResetRNG()"/> or
         *  <see cref="ReseedWithDate(SDate)"/> methods to give a level of
         *  determinism to the randomness. See the examples in the class
         *  description.
         * </remarks>
         * 
         * <param name="type">The type of event the factory will to be generated.</param>
         * <param name="date">The game date to use to seed the factory's RNG. (Default: current date)</param>
         * <returns>An EconEventFactory to be used to generate events for the given type.</returns>
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
         *  
         *  If you want more determinism, consider calling one of the reset
         *  functions after calling this function.
         * </remarks>
         * 
         * <returns>A fully generated EconEvent.</returns>
         * 
         * <seealso cref="ResetRNG"/>
         * <seealso cref="ReseedWithDate(SDate)"/>
         */
        public EconEvent GenerateRandomEvent()
        {
            return RandomlySelectFromList(this.templates).GenerateNewEvent(this);
        }

        /**
         * <summary>Resets the internal RNG to its original state when the factory was created.</summary>
         * <remarks>
         *  This can be used to force a factory to generate the same event
         *  multiple times without creating a new one every time. See the
         *  examples in the class description.
         * </remarks>
         * 
         * <returns>The same factory itself.</returns>
         * 
         * <seealso cref="ReseedWithDate(SDate)"/>
         */
        public EconEventFactory ResetRNG()
        {
            this.rand = new Random(GetRNGSeed(this.eventType, this.dateSeed));
            return this;
        }

        /**
         * <summary>Resets the factory to its original state as though it were seeded with the given date.</summary>
         * <remarks>
         *  This can be used to avoid recreating factories every time new events
         *  need to be generated. See the examples in the class description.
         * </remarks>
         * 
         * <param name="date">The date to reseed the RNG with. (Default: current date)</param>
         * <returns>The same factory itself.</returns>
         * 
         * <seealso cref="ResetRNG"/>
         */
        public EconEventFactory ReseedWithDate(SDate date = null)
        {
            this.dateSeed = date ?? SDate.Now();
            return ResetRNG();
        }

        #region IHeadlineContentProvider implementation
        /**
         * <seealso cref="IHeadlineContentProvider.GetRNG"/>
         */
        public Random GetRNG()
        {
            return this.rand;
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomLocation"/>
         * <seealso cref="LocationsFilePath"/>
         */
        public string GetRandomLocation()
        {
            return RandomlySelectFromList(_Locations);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomCrop"/>
         * <seealso cref="CropsFilePath"/>
         */
        public int GetRandomCrop()
        {
            return RandomlySelectFromList(_Crops);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomMineral"/>
         * <seealso cref="MineralsFilePath"/>
         */
        public int GetRandomMineral()
        {
            return RandomlySelectFromList(_Minerals);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomForagedItem"/>
         * <seealso cref="ForagedGoodsFilePath"/>
         */
        public int GetRandomForagedItem()
        {
            return RandomlySelectFromList(_ForagedGoods);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomRiverFish"/>
         * <seealso cref="RiverFishFilePath"/>
         */
        public int GetRandomRiverFish()
        {
            return RandomlySelectFromList(_RiverFish);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomOceanFish"/>
         * <seealso cref="OceanFishFilePath"/>
         */
        public int GetRandomOceanFish()
        {
            return RandomlySelectFromList(_OceanFish);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomArtisanGood"/>
         * <seealso cref="ArtisanGoodsFilePath"/>
         */
        public int GetRandomArtisanGood()
        {
            return RandomlySelectFromList(_ArtisanGoods);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomCookedItem"/>
         * <seealso cref="CookedItemsFilePath"/>
         */
        public int GetRandomCookedItem()
        {
            return RandomlySelectFromList(_CookedItems);
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomEarthquake"/>
         */
        public string GetRandomEarthquake()
        {
            // For now, return a random one-fraction-place number in the range [4, 8)
            var scale = this.rand.Next(40, 70) / 10.0;
            return scale.ToString("0.0");
        }

        /**
         * <seealso cref="IHeadlineContentProvider.GetRandomFatalities"/>
         */
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
         * <returns>The parsed lines of the file.</returns>
         * 
         * <seealso cref="LoadSimpleList{T}(string, T, Func{string, T}, Func{T, bool})"/>
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
         * <returns>The parsed lines of the file.</returns>
         * 
         * <seealso cref="LoadSimpleList{T}(string, T, Func{string, T}, Func{T, bool})"/>
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
         * <returns>The parsed lines of the file.</returns>
         * 
         * <seealso cref="LoadSimpleList{T}(string, T, Func{string, T}, Func{T, bool})"/>
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
         * <returns>The parsed lines of the file.</returns>
         * 
         * <seealso cref="LoadEventTemplateList(string)"/>
         * <seealso cref="LoadStringList(string, string)"/>
         * <seealso cref="LoadItemList(string, int)"/>
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
        /**
         * <summary>Ensures that the static content of this class has been initialized before any factories are constructed.</summary>
         * <exception cref="InvalidOperationException">Thrown if the class has not been initialized.</exception>
         */
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
         *  In order to keep the seeds as deterministic as possible, each type
         *  has a separately generated RNG seed. Each seed is created by left
         *  shifting the year number by a small amount, bitwise ORing another
         *  number (A), and then bitwise XORing the entire result by the player's
         *  game ID, giving a unique seed for each month/biweek/week on each
         *  playthrough.
         *  
         *  For monthly events, the number (A) is a number 0-3 representing
         *  the season number. For biweekly events, the number (A) is a number
         *  0-8 representing which half of which month we're in. For weekly events,
         *  the number (A) is a number 0-16 representing which week of which month
         *  we're in.
         *  
         *  Note that while the seed for monthly events does not include the week
         *  number, the seed for weekly events includes the month. The first is
         *  to ensure that the monthly seed remains consistent throughout the
         *  month. The second is to prevent the same four weekly events from
         *  occuring throughout the entire year. The biweekly seed includes the
         *  month for the same reason.
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
            int month = date.SeasonAsInt(); // range: 0-3 (2 bits)
            int biweek = date.Week() / 2; // range: 0-1 (1 bit)
            int week = date.Week(); // range: 0-3 (2 bits)

            switch (type)
            {
                case EventType.Monthly:
                    return ((year << 2) | month) ^ gameID;

                case EventType.Biweekly:
                    return ((year << 3) | (month << 1) | biweek) ^ gameID;

                case EventType.Weekly:
                    return ((year << 4) | (month << 2) | week) ^ gameID;

                default:
                    return 0; // TODO: Some kind of error?
            }
        }

        /**
         * <summary>Using the factory's internal RNG, randomly selects an element from the given list.</summary>
         * <remarks>
         *  Each element in the list has an equal chance of being chosen.
         *  
         *  Note that this advances the RNG's state.
         * </remarks>
         * 
         * <param name="list">The list to choose from.</param>
         * <returns>A randomly chosen item from the list.</returns>
         */
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
