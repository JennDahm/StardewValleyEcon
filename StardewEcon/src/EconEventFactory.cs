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
    public class EconEventFactory
    {
        #region Constant Configuration File Locations
        public const string LocationsFilePath = "config/locations.txt";
        public const string CropsFilePath = "config/crops.txt";
        #endregion

        #region Static Content Lists
        private static IReadOnlyList<string> _Locations;
        private static IReadOnlyList<int> _Crops;
        #endregion

        #region Instance Fields
        private Random rand;
        #endregion

        private static bool _Initialized = false;

        private EconEventFactory(EventType type, SDate date)
        {
            this.rand = new Random(GetRNGSeed(type, date));
        }

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
                EconEventFactory._Locations = LoadStringList(Path.Combine(modDir, LocationsFilePath));
                EconEventFactory._Crops = LoadItemList(Path.Combine(modDir, CropsFilePath));

                _Initialized = true;
            }
        }

        /**
         * <summary>Create a new factory for the given event type on the given date.</summary>
         * <remarks>
         *  A new factory should be created for every headline you generate.
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

        /**
         * <summary>From the given list of templates, pick one and generate content for it.</summary>
         * <remarks>
         *  DEPRECATED. This is only in use as a bridge from the EconEventManager
         *  generating RNG seeds and loading templates to this class handling
         *  those operations.
         * </remarks>
         * 
         * <param name="templates">The list of templates to select from.</param>
         * <returns>A fully generated EconEvent.</returns>
         */
        public EconEvent GenerateEventFromTemplates(IReadOnlyList<HeadlineTemplate> templates)
        {
            if( templates == null )
            {
                return null;
            }

            return RandomlySelectFromList(templates, this.rand)?.GenerateNewEvent(this.rand);
        }

        #region Private Static Setup Functions
        public static void LoadGenerationLists(string modLocation)
        {
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

        private T RandomlySelectFromList<T>(IReadOnlyList<T> list, Random rand = null)
        {
            if ((list?.Count ?? 0) == 0)
            {
                return default(T);
            }
            rand = rand ?? new Random();

            return list[rand.Next(list.Count)];
        }
        #endregion
    }
}
