using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

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
         * <summary>A list of event factories generated from _EventTypes.</summary>
         * <remarks>Will never be null or empty so long as <see cref="_EventTypes"/> is not empty.</remarks>
         */
        private IReadOnlyList<EconEventFactory> factories;

        /**
         * <summary>The events currently in effect.</summary>
         * <remarks>
         *  Until <see cref="GenerateAndApplyNewEvents"/> is called, is null. Afterwards,
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
        public EconEventManager(IMonitor monitor)
        {
            this.Monitor = monitor;

            // Note that have to create a date, because this constructor may be
            // called when a new game is created. At that point, SDate.Now()
            // would fail because the "current" date isn't valid.
            var date = new SDate(1, "spring");
            this.factories = _EventTypes.Select(type => EconEventFactory.Create(type, date)).ToList();
        }
        #endregion

        #region Public Properties

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
         * 
         * <param name="helper">The ModHelper to use for loading.</param>
         * <returns>True if the events were successfully loaded. False otherwise.</returns>
         */
        public bool LoadPlayerEvents(IModHelper helper)
        {
            // NOTE: This is called on Day 1 of Spring when gameplay starts.

            var filename = ModEntry.GetPerGameConfigFileName("events");
            var config = SaveConfig.Load(helper, filename);
            if( config != null )
            {
                this.currentEvents = new List<EconEvent>(config.Events);
                return true;
            }
            else
            {
                return false;
            }
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
            // Make sure there actually *are* events
            if( currentEvents == null || currentEvents.Count == 0 )
            {
                this.GenerateAndApplyNewEvents();
                return true;
            }
            else
            {
                // Update each of the existing events when it's time.
                var anyUpdated = false;
                for (int i = 0; i < _EventTypes.Length; i++)
                {
                    if( IsUpdateToday(_EventTypes[i]) )
                    {
                        // Unapply the old one, generate a new one, and apply the new one.
                        this.UnapplyEvent(this.currentEvents[i]);
                        this.currentEvents[i] = this.factories[i].ReseedWithDate().GenerateRandomEvent();
                        this.ApplyEvent(this.currentEvents[i]);

                        anyUpdated = true;
                    }
                }
                return anyUpdated;
            }
        }

        /**
         * <summary>Saves the current events to the player's save file.</summary>
         * <remarks>
         *  This should be called after every player save.
         * </remarks>
         * 
         * <param name="helper">The ModHelper to use for saving.</param>
         */
        public void SaveEvents(IModHelper helper)
        {
            // NOTE: In earlier versions of SMAPI, SaveEvents is called at the
            // end of "Day 0" - the intro date. Luckily, this isn't a big deal.

            var config = new SaveConfig()
            {
                Version = ModEntry.ModVersion,
                Events = this.currentEvents ?? new List<EconEvent>(),
            };

            var filename = ModEntry.GetPerGameConfigFileName("events");
            config.Save(helper, filename);
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
            this.currentEvents.ForEach(evnt => this.UnapplyEvent(evnt));
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
        private IReadOnlyList<EconEvent> GenerateAndApplyNewEvents()
        {
            // In the unlikely event we call this while there are existing events,
            // make sure to unapply them.
            if( this.currentEvents != null )
            {
                this.UnapplyEvents();
            }

            // Generate the events for the current date.
            this.currentEvents = this.factories
                .Select(f => f.ReseedWithDate().GenerateRandomEvent())
                .ToList();

            this.currentEvents.ForEach(this.ApplyEvent);

            return this.currentEvents;
        }

        /**
         * <summary>Applies the given event's pricing changes to all affected items in the game.</summary>
         * <remarks>
         *  This affects items in the player's inventory and chests as
         *  well as newly created items. This does not affect store prices, as
         *  those are hard-coded.
         * </remarks>
         * 
         * <param name="e">The event to apply.</param>
         * <seealso cref="UnapplyEvent(EconEvent)"/>
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

        /**
         * <summary>Unapplies the given event's pricing changes to all affected items in the game.</summary>
         * <remarks>
         *  This affects items in the player's inventory and chests as
         *  well as newly created items. This does not affect store prices, as
         *  those are hard-coded.
         * </remarks>
         * 
         * <param name="e">The event to unapply.</param>
         * <seealso cref="ApplyEvent(EconEvent)"/>
         */
        private void UnapplyEvent(EconEvent e)
        {
            // TODO
        }

        private bool IsUpdateToday(EventType type)
        {
            // Days start from one. We want them to start from 0.
            var dayOfMonth = SDate.Now().Day - 1;

            switch(type)
            {
                case EventType.Monthly:
                    return dayOfMonth == 0;
                case EventType.Biweekly:
                    return (dayOfMonth % 14) == 0;
                case EventType.Weekly:
                    return (dayOfMonth % 7) == 0;
                default:
                    return false;
            }
        }
        #endregion
    }
}
