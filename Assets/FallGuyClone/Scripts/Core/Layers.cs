using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Layer numbers used by the game (built-in layers, so no Tags &amp; Layers setup is needed).</summary>
    public static class Layers
    {
        /// <summary>Moving obstacles. The camera ignores this layer so it does not jitter.</summary>
        public const int Dynamic = 1; // built-in "TransparentFX"
        /// <summary>Player. Ground checks and the camera ignore this layer.</summary>
        public const int Player = 2;  // built-in "Ignore Raycast"
    }
}
