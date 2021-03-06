﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StardewEcon.Econ
{
    public class HeadlineTemplate
    {
        static private Dictionary<int, Tuple<HeadlineTokenType, HeadlineTokenSubtype>> numToTypeMap;

        private readonly HeadlineToken[] headlineTemplate;

        public HeadlineTemplate(string headlineTemplate)
        {
            this.headlineTemplate = this.ParseHeadlineTemplate(headlineTemplate);
        }

        // Static initializer
        static HeadlineTemplate()
        {
            // Set up our escape-number-to-type map.
            numToTypeMap = new Dictionary<int, Tuple<HeadlineTokenType, HeadlineTokenSubtype>>
            {
                { 0, Tuple.Create(HeadlineTokenType.Other, HeadlineTokenSubtype.Earthquake) },
                { 1, Tuple.Create(HeadlineTokenType.Other, HeadlineTokenSubtype.Location) },
                { 2, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.Crop) },
                { 3, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.Mineral) },
                { 4, Tuple.Create(HeadlineTokenType.Other, HeadlineTokenSubtype.Fatalities) },
                { 5, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.Foraged) },
                { 6, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.RiverFish) },
                { 7, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.OceanFish) },
                { 8, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.Artisan) },
                { 9, Tuple.Create(HeadlineTokenType.Item,  HeadlineTokenSubtype.Cooked) }
            };
        }

        /**
         * <summary>Generates a new event based on the headline template</summary>
         * <remarks>
         *  This method doesn't apply the event; it just generates it.
         * </remarks>
         * 
         * <param name="rand">The RNG to use to generate this event. (Default: a brand new one)</param>
         * <returns>An event based on this template.</returns>
         */
        public EconEvent GenerateNewEvent(IHeadlineContentProvider rng)
        {
            StringBuilder builder = new StringBuilder();
            int affectedItem = -1;
            int oldPrice = 0;
            foreach(HeadlineToken token in this.headlineTemplate)
            {
                // Get the subtype:
                var subtype = HeadlineTemplate.GetSubType(token, rng);
                switch(token.type)
                {
                    case HeadlineTokenType.Item:
                    {
                        // Generate an item, whatever kind.
                        // If we haven't yet generated an item, select that to
                        // be the item affected by this event.
                        int item = GenerateItem(subtype, rng);
                        var itemInfo = rng.GetItemInformation(item);
                        if(affectedItem == -1)
                        {
                            affectedItem = item;
                            oldPrice = itemInfo.price;
                        }
                        
                        builder.Append(itemInfo.name);
                        break;
                    }
                    case HeadlineTokenType.Other:
                    {
                        builder.Append(GenerateOther(subtype, rng));
                        break;
                    }
                    case HeadlineTokenType.String:
                    {
                        builder.Append(token.str);
                        break;
                    }
                }
            }

            int percent = GeneratePercentChange(affectedItem, rng.GetRNG());
            
            return new EconEvent(builder.ToString(), affectedItem, percent, oldPrice);
        }

        #region Private Parsing Functions
        /**
         * <summary>Parses a headline template into a list of tokens for generating events.</summary>
         * 
         * <param name="template">The headline template to parse.</param>
         * <returns>A list of tokens representing the parsed template.</returns>
         * 
         * <seealso cref="HeadlineToken"/>
         * <seealso cref="HeadlineTokenType"/>
         * <seealso cref="HeadlineTokenSubtype"/>
         */
        private HeadlineToken[] ParseHeadlineTemplate(string template)
        {
            List<HeadlineToken> tokens = new List<HeadlineToken>();

            // Using Regex, split the string into a list of tokens in string form.
            // The regex has two parts: the first captures "%%" tokens; the second
            // captures "%0", "%1", "%2", etc. tokens. All other "tokens" will
            // fall in between these tokens, and are really just literal strings
            // to be mapped straight to the output.
            // Note that the parenthesis around each section of the regex is
            // **important**. Using them forces the split function to include what
            // it captured in the output. Without them, we'd lose those tokens.
            // The nested set of parenthesis starts with "?:", which indicates
            // that it is non-capturing, which is also **important.**
            // This only works in .NET 2.0 and up.
            string escapeRegex = @"(%%)|(%\d+(?:\+\d+)*)";
            foreach (string split in Regex.Split(template, escapeRegex))
            {
                // We may get some empty strings in the output of Split if two
                // escape sequences are back-to-back or appear at the beginning
                // or end of the headline. Ignore these - they're unnecessary.
                if (string.IsNullOrEmpty(split))
                {
                    continue;
                }

                // Non-escaped strings:
                if (!Regex.IsMatch(split, escapeRegex))
                {
                    tokens.Add(new HeadlineToken
                    {
                        type = HeadlineTokenType.String,
                        str = split
                    });
                    continue;
                }

                // Escaped strings:
                tokens.Add(ParseEscapeSequence(split));
            }
            return tokens.ToArray();
        }

        /**
         * <summary>Parses an escaped section of a headline template.</summary>
         * <remarks>
         *  The given string must start with a '%' followed by one of two things:
         *  <list type="bullet">
         *      <item>another '%', or</item>
         *      <item>a '+' separated list of positive integers.</item>
         *  </list>
         *  
         *  Examples include:
         *  <list type="bullet">
         *      <item>"%%"</item>
         *      <item>"%3"</item>
         *      <item>"%1+2+4</item>
         *  </list>
         *  
         *  Examples do not include:
         *  <list type="bullet">
         *      <item>"%"</item>
         *      <item>"3"</item>
         *      <item>"%+"</item>
         *      <item>"%+2"</item>
         *      <item>"%1+"</item>
         *      <item>"%1 + 2 + 3"</item>
         *      <item>"%1,2,3"</item>
         *  </list>
         * </remarks>
         * 
         * <param name="escape">The escape sequence to parse.</param>
         * <returns>A HeadlineToken representing this escape sequence.</returns>
         */
        private HeadlineToken ParseEscapeSequence(string escape)
        {
            // Note that we're assured that there is at least one character
            // in escape. The escape regex only matches strings with at least
            // two characters (among other features).
            // We also know that escape is one of two things: a %, or a list of
            // integers separated by +.
            string subEscape = escape.Substring(1);

            if (subEscape[0] == '%')
            {
                // Escaping a %
                return new HeadlineToken
                {
                    type = HeadlineTokenType.String,
                    str = "%"
                };
            }

            // Now we have a list of integers. We need to parse them.
            var ints = subEscape.Split('+').Select(Int32.Parse).ToList();

            // First, make sure that all of them can be mapped.
            // If not all of them can, we should just put the entire sequence
            // into the token stream as it is unmodified.
            if (!ints.All(numToTypeMap.ContainsKey))
            {
                return new HeadlineToken
                {
                    type = HeadlineTokenType.String,
                    str = escape
                };
            }
            var superAndSubtypes = ints.Select(i => numToTypeMap[i]).ToList();

            // Next, make sure that all of them map to the same super type.
            // If they don't all match, handle them the same way as above.
            var distinctSupertypes = superAndSubtypes
                .Select(t => t.Item1)
                .Distinct()
                .ToList();
            if (distinctSupertypes.Count != 1)
            {
                return new HeadlineToken
                {
                    type = HeadlineTokenType.String,
                    str = escape
                };
            }

            var type = distinctSupertypes.First();
            var distinctSubtypes = superAndSubtypes
                .Select(t => t.Item2)
                .Distinct()
                .ToList();
            return new HeadlineToken
            {
                type = type,
                subtypes = distinctSubtypes,
            };
        }
        #endregion

        #region Private Generation Functions
        /**
         * <summary>Returns the subtype of the token, randomly selecting one if necessary.</summary>
         * 
         * <param name="token">The token whose subtype to get.</param>
         * <param name="rng">The RNG provider to use if necessary.</param>
         * 
         * <returns>The subtype of the token.</returns>
         */
        private static HeadlineTokenSubtype GetSubType(HeadlineToken token, IHeadlineContentProvider rng)
        {
            if (token.subtypes == null || token.subtypes.Count == 0)
            {
                return default(HeadlineTokenSubtype);
            }
            else if (token.subtypes.Count == 1)
            {
                return token.subtypes[0];
            }
            else
            {
                var rand = rng.GetRNG();
                var index = rand.Next(token.subtypes.Count);
                return token.subtypes[index];
            }
        }

        /**
         * <summary>Generates a random item of the given type.</summary>
         * 
         * <param name="type">The type of item to generate. See <see cref="HeadlineTokenType.Item"/>.</param>
         * <param name="rand">The RNG to use to generate this item.</param>
         * <returns>The ID of the item generated.</returns>
         * 
         * <seealso cref="HeadlineTokenType"/>
         * <seealso cref="HeadlineTokenSubtype"/>
         */
        private int GenerateItem(HeadlineTokenSubtype type, IHeadlineContentProvider rng)
        {
            switch(type)
            {
                case HeadlineTokenSubtype.Crop: return rng.GetRandomCrop();
                case HeadlineTokenSubtype.Mineral: return rng.GetRandomMineral();
                case HeadlineTokenSubtype.Foraged: return rng.GetRandomForagedItem();
                case HeadlineTokenSubtype.RiverFish: return rng.GetRandomRiverFish();
                case HeadlineTokenSubtype.OceanFish: return rng.GetRandomOceanFish();
                case HeadlineTokenSubtype.Artisan: return rng.GetRandomArtisanGood();
                case HeadlineTokenSubtype.Cooked: return rng.GetRandomCookedItem();
                default: return StardewValley.Object.stone;
            }
        }

        /**
         * <summary>Generates a random string of the given type.</summary>
         * 
         * <param name="type">The type of parameter to generate. See <see cref="HeadlineTokenType.Other"/>.</param>
         * <param name="rand">The RNG to use to generate this parameter.</param>
         * <returns>The parameter generated as a string.</returns>
         * 
         * <seealso cref="HeadlineTokenType"/>
         * <seealso cref="HeadlineTokenSubtype"/>
         */
        private string GenerateOther(HeadlineTokenSubtype type, IHeadlineContentProvider rng)
        {
            switch(type)
            {
                case HeadlineTokenSubtype.Earthquake: return rng.GetRandomEarthquake();
                case HeadlineTokenSubtype.Location: return rng.GetRandomLocation();
                case HeadlineTokenSubtype.Fatalities: return rng.GetRandomFatalities();
                default: return "Zuzu City";
            }
        }

        /**
         * <summary>Generates a percent change for the given item's price.</summary>
         * <remarks>
         *  This does not actually change any prices - it just generates an
         *  integer representing a percent change to the given item's price.
         * </remarks>
         * 
         * <param name="affectedItem">The item whose price to change.</param>
         * <param name="rand">The RNG used to generate the percent change.</param>
         * <returns>The percent change to the item's price, as an integer from -100 to 100.</returns>
         */
        private int GeneratePercentChange(int affectedItem, Random rand)
        {
            // TODO: Consider varying the probability distribution based on the
            //       item?

            // Let's generate percent changes that are multiples of 5 between
            // -25% and +25%, inclusive.
            // This translates to an integer range between -5 and +5, inclusive.
            return rand.Next(-5, 5 + 1) * 5;
        }
        #endregion

        #region Private Types
        private struct HeadlineToken
        {
            public HeadlineTokenType type;
            public List<HeadlineTokenSubtype> subtypes;
            public string str;
        }

        /**
         * <summary>Describes the type of headline token.</summary>
         * <seealso cref="HeadlineToken"/>
         * <seealso cref="HeadlineTokenSubtype"/>
         */
        private enum HeadlineTokenType
        {
            /**
             * <summary>A raw string, to be copied directly into the output.</summary>
             * <remarks>
             *  The subtype parameter of the HeadlineToken struct doesn't matter
             *  when the type is set to String - it's simply ignored.
             * </remarks>
             */
            String,

            /**
             * <summary>An item to be generated before being output.</summary>
             * <remarks>
             *  The subtype parameter of the HeadlineToken struct describes
             *  specifically what type of item. Acceptable values are as follows:
             *  <list type="bullet">
             *      <item><see cref="HeadlineTokenSubtype.Crop"/></item>
             *      <item><see cref="HeadlineTokenSubtype.Mineral"/></item>
             *      <item><see cref="HeadlineTokenSubtype.Foraged"/></item>
             *      <item><see cref="HeadlineTokenSubtype.RiverFish"/></item>
             *      <item><see cref="HeadlineTokenSubtype.OceanFish"/></item> 
             *      <item><see cref="HeadlineTokenSubtype.Artisan"/></item>  
             *      <item><see cref="HeadlineTokenSubtype.Cooked"/></item> 
             *  </list>
             * </remarks>
             */
            Item,

            /**
             * <summary>Some other parameter to be generated before being output.</summary>
             * <remarks>
             *  The subtype parameter of the HeadlineToken struct describes
             *  specifically what type of other parameter. Acceptable values are
             *  as follows:
             *  <list type="bullet">
             *      <item><see cref="HeadlineTokenSubtype.Earthquake"/></item>
             *      <item><see cref="HeadlineTokenSubtype.Location"/></item>
             *      <item><see cref="HeadlineTokenSubtype.Fatalities"/></item>
             *  </list>
             * </remarks>
             */
            Other
        }

        /**
         * <summary>Further narrows down a headline token type.</summary>
         * <remarks>
         *  See <see cref="HeadlineTokenType"/> for which subtype is appropriate with which type.
         * </remarks>
         * <seealso cref="HeadlineToken"/>
         * <seealso cref="HeadlineTokenType"/>
         */
        private enum HeadlineTokenSubtype
        {
            Earthquake,
            Location,
            Crop,
            Mineral,
            Fatalities,
            Foraged,
            RiverFish,
            OceanFish,
            Artisan,
            Cooked,
        }
        #endregion
    }
}
