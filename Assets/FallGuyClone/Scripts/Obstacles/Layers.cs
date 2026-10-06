using UnityEngine;

namespace FallGuyClone
{
    public static class Layers
    {
        /// <summary>Moving obstacles. The camera ignores this layer so it does not jitter.</summary>
        public const int Dynamic = 1; // built-in "TransparentFX"
        /// <summary>Player. Ground checks and the camera ignore this layer.</summary>
        public const int Player = 2;  // built-in "Ignore Raycast"
    }
}
