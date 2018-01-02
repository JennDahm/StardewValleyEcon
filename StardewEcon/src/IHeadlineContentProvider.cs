using System;

namespace StardewEcon
{
    /**
     * <summary>A black box that provides random content for headlines.</summary>
     */
    public interface IHeadlineContentProvider
    {
        /**
         * <summary>Returns the internal RNG of this provider.</summary>
         * <remarks>
         *  This allows users of this class to make random decisions outside the
         *  scope of this provider without impeding determinism.
         * </remarks>
         * 
         * <returns>The internal RNG.</returns>
         */
        Random GetRNG();

        /**
         * <summary>Returns canonical item information, disregarding current events.</summary>
         * <remarks>
         *  This must not affect the state of the internal RNG, and it must
         *  return the same result every time it is called with the same input.
         * </remarks>
         * 
         * <param name="itemID">The ID of the item to query.</param>
         * <returns>The canonical information about the given item.</returns>
         */
        ItemInformation GetItemInformation(int itemID);

        /**
         * <summary>Returns a earthquake magnitude as a string.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random earthquake magnitude.</returns>
         */
        string GetRandomEarthquake();

        /**
         * <summary>Returns a random location name.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random location name.</returns>
         */
        string GetRandomLocation();

        /**
         * <summary>Returns a random crop item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random crop ID.</returns>
         */
        int GetRandomCrop();

        /**
         * <summary>Returns a random mineral item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random mineral ID.</returns>
         */
        int GetRandomMineral();

        /**
         * <summary>Returns a random number of fatalities as a string.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random number of fatalities.</returns>
         */
        string GetRandomFatalities();

        /**
         * <summary>Returns a random foraged item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random foraged item ID.</returns>
         */
        int GetRandomForagedItem();

        /**
         * <summary>Returns a random river fish item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random river fish ID.</returns>
         */
        int GetRandomRiverFish();

        /**
         * <summary>Returns a random ocean fish item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random ocean fish ID.</returns>
         */
        int GetRandomOceanFish();

        /**
         * <summary>Returns a random artisan good item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random artisan good ID.</returns>
         */
        int GetRandomArtisanGood();

        /**
         * <summary>Returns a random cooked item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will likely return an entirely different value.
         * </remarks>
         * 
         * <returns>A random cooked item ID.</returns>
         */
        int GetRandomCookedItem();
    }

    public struct ItemInformation
    {
        public string name;
        public int price;

        public ItemInformation(string name, int price)
        {
            this.name = name;
            this.price = price;
        }
    }
}
