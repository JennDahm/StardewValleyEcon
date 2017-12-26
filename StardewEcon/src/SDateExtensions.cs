using StardewModdingAPI.Utilities;

namespace StardewEcon
{
    public static class SDateExtensions
    {
        /**
         * <summary>Returns an integer representing the date's season</summary>
         * <returns>An integer 0-3 representing the season, or -1 if the season is invalid.</returns>
         */
        public static int SeasonAsInt(this SDate date)
        {
            if (date == null)
            {
                return -1;
            }

            if (date.Season.Equals("spring"))
            {
                return 0;
            }
            if (date.Season.Equals("summer"))
            {
                return 1;
            }
            if (date.Season.Equals("fall"))
            {
                return 2;
            }
            if (date.Season.Equals("winter"))
            {
                return 3;
            }

            return -1;
        }

        /**
         * <summary>Returns an integer representing the week within the current month.</summary>
         * <returns>An integer 0-3 representing the week.</returns>
         */
        public static int Week(this SDate date)
        {
            return (date.Day - 1) / 7;
        }
    }
}
