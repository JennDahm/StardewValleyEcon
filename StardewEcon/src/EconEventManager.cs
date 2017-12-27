using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

using static StardewEcon.EconEvent;

namespace StardewEcon
{
    /**
     * <summary>Manages the economic events happening in the game world.</summary>
     * <remarks>
     *  This class is assigned the responsibility of keeping track of which
     *  events are active, determining when it is time to create new events, and
     *  applying/reversing the changes of events as they start and end.
     *  
     *  That last responsibility may eventually be delegated to the
     *  <see cref="EconEvent"/> class itself, but this class will always be the
     *  one to signal the start and end of events.
     *  
     *  Usage of this class is as follows:
     *  <list type="number">
     *      <item>
     *          <term>
     *          <see cref="StardewModdingAPI.Events.SaveEvents.AfterLoad"/>
     *          </term>
     *          <description>
     *          When the player loads a save file, create a new EconEventManager
     *          and call <see cref="LoadPlayerEvents"/>. This will load the
     *          events active when the player last played and apply them. If the
     *          manager fails to load events for the save file,
     *          <see cref="CurrentEvents"/> will be null, but that's alright
     *          because shortly afterwards the
     *          <see cref="StardewModdingAPI.Events.TimeEvents.AfterDayStarted"/>
     *          hook will fire and you'll follow the next step.
     *          </description>
     *      </item>
     *      <item>
     *          <term>
     *          <see cref="StardewModdingAPI.Events.TimeEvents.AfterDayStarted"/>
     *          </term>
     *          <description>
     *          At the start of every day, call <see cref="UpdateEvents"/>. This
     *          function will check for expired events and generate new ones
     *          when necessary. If it generates new events, it will also unapply
     *          the replaced events and apply the new events immediately.
     *          
     *          If in the previous step the manager couldn't load events for the
     *          loaded save file, the first time this is called it will generate
     *          an entire list of new events.
     *          </description>
     *      </item>
     *      <item>
     *          <term>
     *          <see cref="StardewModdingAPI.Events.SaveEvents.AfterSave"/>
     *          </term>
     *          <description>
     *          Whenever the game saves, also save the current events by calling
     *          <see cref="SaveEvents"/>.
     *          </description>
     *      </item>
     *      <item>
     *          <term>
     *          <see cref="StardewModdingAPI.Events.SaveEvents.AfterReturnToTitle"/>
     *          </term>
     *          <description>
     *          When the user returns to the title screen, we need to unapply all
     *          of the current events so that we don't affect other save files
     *          the user starts up afterwards. To do this, call <see cref="UnapplyEvents"/>.
     *          
     *          Note that we don't need to do this when the user closes the game
     *          (nor do we have a hook into that event) because any change we've
     *          made to in-memory structures other than the player's items won't
     *          persist when the player starts the game again.
     *          </description>
     *      </item>
     *  </list>
     * </remarks>
     * 
     * <seealso cref="EconEvent"/>
     * <seealso cref="EconEventManager"/>
     */
    public class EconEventManager
    {
        #region Private Fields
        /**
         * <summary>The event types we generate in the order of display.</summary>
         * <remarks>For convenience purposes.</remarks>
         */
        private static readonly EventType[] _EventTypes = 
        {
            EventType.Monthly,
            EventType.Biweekly,
            EventType.Weekly
        };

        /**
         * <summary>A map from event type to event factory.</summary>
         * <remarks>Will never be null or empty so long as <see cref="_EventTypes"/> is not empty.</remarks>
         */
        private IReadOnlyDictionary<EventType, EconEventFactory> factories;

        /**
         * <summary>The events currently in effect.</summary>
         * <remarks>
         *  Until <see cref="GenerateNewEvents"/> is called, is null. Afterwards,
         *  is never null and always has the same number of elements as
         *  <see cref="_EventTypes"/>.
         * </remarks>
         */
        private List<EconEvent> currentEvents;
        #endregion

        #region Constructors
        /**
         * <summary>Creates a new event manager</summary>
         * <remarks>
         *  You must have set up <see cref="EconEventFactory"/>'s static content
         *  before constructing any event managers.
         * </remarks>
         */
        public EconEventManager(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;

            // Note that we do not have to create a date, because this
            // constructor should not be called before the game is loaded.
            // SDate.Now() would fail during that time period because the
            // "current" date isn't valid.
            this.factories = _EventTypes.ToDictionary(t => t, t => EconEventFactory.Create(t));
        }
        #endregion

        #region Public Properties
        /**
         * <seealso cref="Mod.Helper"/>
         */
        public IModHelper Helper { get; }

        /**
         * <seealso cref="Mod.Monitor"/>
         */
        public IMonitor Monitor { get; }

        /**
         * <summary>The currently active events.</summary>
         * <remarks>
         *  Until <see cref="UpdateEvents"/> is called for the first time, this
         *  is not guaranteed to be non-null. Afterwards, it is guaranteed to be
         *  non-null and not empty.
         * </remarks>
         */
        public IReadOnlyList<EconEvent> CurrentEvents => this.currentEvents;
        #endregion

        #region Public Functions
        /**
         * <summary>Attempts to load and apply the events last saved for the player's save.</summary>
         * <remarks>
         *  This should be called after the player loads the game, shortly after
         *  construction of this manager and before the first call to
         *  <see cref="UpdateEvents"/>.
         * </remarks>
         */
        public void LoadPlayerEvents()
        {
            // TODO: Load player events
            this.GenerateNewEvents();
        }

        /**
         * <summary>Checks the current date and updates all events if necessary.</summary>
         * <remarks>
         *  If events are to be updated, this function will automatically
         *  unapply old events and apply new events.
         *  
         *  If there are no current events (the player started a new game or we
         *  could not load the player's current events for whatever reason), this
         *  will generate all new events and apply them.
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
         * <summary>Saves the current events to the player's save file.</summary>
         * <remarks>
         *  This should be called after every player save.
         * </remarks>
         */
        public void SaveEvents()
        {
            // TODO
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
         * <summary>Discards current events and generates an entirely new set.</summary>
         * <remarks>
         *  This will automatically apply the new events. See <see cref="ApplyEvents"/>.
         * </remarks>
         * 
         * <returns>The new set generated. This is also available via the CurrentEvents property.</returns>
         */
        private IReadOnlyList<EconEvent> GenerateNewEvents()
        {
            // Generate the events.
            this.currentEvents =
                _EventTypes.Select(t => this.factories[t])
                .Select(f => f.ReseedWithDate().GenerateRandomEvent())
                .ToList();

            this.currentEvents.ForEach(this.ApplyEvent);

            return this.currentEvents;
        }

        /**
         * <summary>Applies pricing changes to all affected items in the game.</summary>
         * <remarks>
         *  This affects items in the player's inventory and chests as
         *  well as newly created items. This does not affect store prices, as
         *  those are hard-coded.
         * </remarks>
         * 
         * <param name="e">The event to apply.</param>
         * <seealso cref="UnapplyEvents"/>
         */
        private void ApplyEvent(EconEvent e)
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
        #endregion
    }
}
