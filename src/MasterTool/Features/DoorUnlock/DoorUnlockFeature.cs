using System;
using EFT.Interactive;
using UnityEngine;

namespace MasterTool.Features.DoorUnlock
{
    /// <summary>
    /// Finds all locked doors in the scene and changes their state to unlocked (shut).
    /// </summary>
    public static class DoorUnlockFeature
    {
        /// <summary>
        /// Unlocks every <see cref="Door"/> in the scene that is currently in the Locked state.
        /// </summary>
        /// <returns>The number of doors unlocked, or -1 if an error occurred.</returns>
        public static int UnlockAll()
        {
            try
            {
                var doors = UnityEngine.Object.FindObjectsOfType<Door>();
                int count = 0;
                foreach (var door in doors)
                {
                    if (door.DoorState == EDoorState.Locked)
                    {
                        door.DoorState = EDoorState.Shut;
                        count++;
                    }
                }
                return count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
