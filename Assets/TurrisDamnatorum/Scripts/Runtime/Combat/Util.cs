using UnityEngine;

namespace Turris
{
    public static class Util
    {
        /// <summary>Destroy w trybie gry, DestroyImmediate w edytorze (np. w testach EditMode).</summary>
        public static void DestroySafe(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o); else Object.DestroyImmediate(o);
        }
    }
}
