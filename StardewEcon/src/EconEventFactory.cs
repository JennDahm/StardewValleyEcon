using System;

namespace StardewEcon
{
    public class EconEventFactory
    {
        private string rawHeadline;

        public EconEventFactory(string rawHeadline)
        {
            // TODO: Parse the headline into something quickly manageable.
            this.rawHeadline = rawHeadline;
        }

        public EconEvent GenerateNewEvent(Random rand)
        {
            // TODO: Actually create this function.
            return new EconEvent(this.rawHeadline, StardewValley.Object.stone, 13, 20);
        }
    }
}
