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
         * <summary>Returns a random location name.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will return an entirely different value.
         * </remarks>
         * 
         * <returns>A random location name.</returns>
         */
        string GetRandomLocation();

        /**
         * <summary>Returns a random crop item ID.</summary>
         * <remarks>
         *  This advances the state of the internal RNG. The next call to
         *  this function will return an entirely different value.
         * </remarks>
         * 
         * <returns>A random crop ID.</returns>
         */
        int GetRandomCrop();
    }
}
