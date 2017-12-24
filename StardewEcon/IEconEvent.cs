namespace StardewEcon
{
    interface IEconEvent
    {
        void RandomlyInitialize();

        string Headline { get; }

        string HoverText { get; }
    }
}
