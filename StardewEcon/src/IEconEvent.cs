namespace StardewEcon
{
    interface IEconEvent
    {
        string Headline { get; }

        string HoverText { get; }

        int AffectedItem { get; }

        int NewPrice { get; }

        int OldPrice { get; }
    }
}
