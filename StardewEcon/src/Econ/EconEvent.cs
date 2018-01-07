using System;
using System.Runtime.Serialization;

namespace StardewEcon.Econ
{
    /**
     * <summary>Represents an economic event happening in the game world.</summary>
     * <remarks>
     *  This class is POD. It simply represents the necessary information to
     *  describe the event, apply necessary changes, and reverse those changes.
     *  
     *  This may change in the future - I may assign this class the responsibility
     *  of applying and reversing its changes. It will not likely be assigned the
     *  responsibility of determining when that happens, however.
     * </remarks>
     * 
     * <seealso cref="EconEventManager"/>
     * <seealso cref="EconEventFactory"/>
     */
    [Serializable]
    public class EconEvent : ISerializable
    {
        /**
         * <summary>Creates a new event.</summary>
         * 
         * <param name="headline">The headline description of the event to display to the user.</param>
         * <param name="item">The ID of the item affected by this event.</param>
         * <param name="oldPrice">The old price of the item.</param>
         * <param name="percent">How much the price will change under this event. See <see cref="PercentChange"/>.</param>
         */
        public EconEvent(string headline, int item, int percent, int oldPrice)
        {
            this.Headline = headline;

            this.AffectedItem = item;
            this.PercentChange = percent;
            this.OriginalPrice = oldPrice;
        }

        /**
         * <summary>Special constructor used only for deserialization.</summary>
         * 
         * <param name="info">The information to load.</param>
         * <param name="context">The context of the deserialization.</param>
         * 
         * <seealso cref="ISerializable"/>
         */
        public EconEvent(SerializationInfo info, StreamingContext context)
        {
            this.Headline = info.GetString(nameof(this.Headline));
            this.AffectedItem = info.GetInt32(nameof(this.AffectedItem));
            this.OriginalPrice = info.GetInt32(nameof(this.OriginalPrice));
            this.PercentChange = info.GetInt32(nameof(this.PercentChange));
        }

        /**
         * <summary>Fills the serialization info with our properties.</summary>
         * 
         * <seealso cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/>
         */
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Headline), this.Headline);
            info.AddValue(nameof(this.AffectedItem), this.AffectedItem);
            info.AddValue(nameof(this.OriginalPrice), this.OriginalPrice);
            info.AddValue(nameof(this.PercentChange), this.PercentChange);
        }

        /**
         * <summary>The headline description of the event.</summary>
         * <remarks>This should be localized.</remarks>
         */
        public string Headline { get; }

        /**
         * <summary>The ID of the item affected by this event.</summary>
         */
        public int AffectedItem { get; }

        /**
         * <summary>
         *  The amount that the item's price changed as a percentage of its
         *  original price.
         * </summary>
         * <remarks>
         *  A value of 1 here indicates a 1% increase in price; -5 represents
         *  a 5% decrease in price.
         * </remarks>
         */
        public int PercentChange { get; }

        /**
         * <summary>
         *  The original price of the item before the event.
         * </summary>
         */
        public int OriginalPrice { get; }

        /**
         * <summary>The new price of the item during the event.</summary>
         * <remarks>
         *  This is a calculated field.
         * </remarks>
         */
        public int NewPrice {
            get
            {
                return OriginalPrice + (OriginalPrice * PercentChange) / 100;
            }
        }

        /**
         * <summary>A string representation of the event, for debugging purposes.</summary>
         * <remarks>
         *  The format of this string should not be relied upon for programmatic
         *  purposes. It's strictly for human consumption.
         * </remarks>
         * 
         * <returns>A string describing the event.</returns>
         */
        public override string ToString()
        {
            return $"'{this.Headline}': Item {this.AffectedItem} price is {this.OriginalPrice} {this.PercentChange.ToString("+ #;- #;+ 0")}% = {this.NewPrice}";
        }

        /**
         * The EconEvent doesn't itself need to know what type it is, but other
         * classes will find this helpful.
         */
        public enum EventType
        {
            /**
             * <summary>Events that start on the first of a season and last for the entire season.</summary>
             */
            Monthly,

            /**
             * <summary>Events that start on either the first or the fifteenth of a season and last for two weeks.</summary>
             */
            Biweekly,

            /**
             * <summary>Events that start at the beginning of a week and last for one week.</summary>
             */
            Weekly
        }
    }
}
