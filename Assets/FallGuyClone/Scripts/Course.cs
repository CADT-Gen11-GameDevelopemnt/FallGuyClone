using System.Collections.Generic;
using UnityEngine;

namespace FallGuyClone
{
    /// <summary>
    /// Data about the built course, stored on the "Course" root object so the
    /// course can be baked into the scene and edited by hand.
    /// </summary>
    public class Course : MonoBehaviour
    {
        public Vector3 spawnPoint = new Vector3(0f, 0.05f, -3f);
        public float startZ = -3f;
        public float finishZ = 210f;
        [Tooltip("Falling below this height respawns the player.")]
        public float killY = -4f;
        public List<Checkpoint> checkpoints = new List<Checkpoint>();
    }
}
