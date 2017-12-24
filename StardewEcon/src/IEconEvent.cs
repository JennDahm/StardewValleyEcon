namespace StardewEcon
{
    public interface IEconEvent
    {
        string Headline { get; }

        int AffectedItem { get; }

        int PercentChange { get; }

        int NewPrice { get; }

        int OldPrice { get; }
    }
}
