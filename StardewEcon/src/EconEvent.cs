namespace StardewEcon
{
    class EconEvent : IEconEvent
    {
        public EconEvent(string headline, string hover)
        {
            this.Headline = headline;
            this.HoverText = hover;

            this.AffectedItem = StardewValley.Object.stone;
        }

        public string Headline { get; }

        public string HoverText { get; }

        public int AffectedItem { get; }

        public int NewPrice { get; }

        public int OldPrice { get; }
    }
}
