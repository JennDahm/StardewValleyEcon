using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

using static StardewEcon.EconEvent;
using StardewModdingAPI.Utilities;

namespace StardewEcon
{
    public class EconEventManager
    {
        private static readonly EventType[] _EventTypes = 
        {
            EventType.Monthly,
            EventType.Biweekly,
            EventType.Weekly
        };

        #region Private Fields
        private IReadOnlyDictionary<EventType, EconEventFactory> factories;

        private List<EconEvent> currentEvents;
        #endregion

        #region Constructors
        public EconEventManager(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;

            // TODO: Move this call to ModEntry?
            EconEventFactory.LoadContentFiles(this.Helper.DirectoryPath);

            // Note that we absolutely have to create a date, because this
            // constructor may be called before the game is loaded, and
            // SDate.Now() fails during that time period.
            this.factories = _EventTypes.ToDictionary(t => t, t => EconEventFactory.Create(t, new SDate(1, "spring")));
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
         *  This will automatically apply the new events. See <see cref="ApplyEvents"/>.
         * </remarks>
         * 
         * <returns>The new set generated. This is also available via the CurrentEvents property.</returns>
         */
        public IReadOnlyList<EconEvent> GenerateNewEvents()
        {
            // Generate the events.
            this.currentEvents =
                _EventTypes.Select(t => this.factories[t])
                .Select(f => f.ResetToDate().GenerateRandomEvent())
                .ToList();

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
        #endregion
    }
}
