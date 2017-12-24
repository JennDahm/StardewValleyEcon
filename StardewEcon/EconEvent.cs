using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewEcon
{
    class EconEvent : IEconEvent
    {
        public EconEvent(string headline, string hover)
        {
            this.Headline = headline;
            this.HoverText = hover;
        }

        public void RandomlyInitialize()
        {
        }

        public string Headline { get; }

        public string HoverText { get; }
    }
}
