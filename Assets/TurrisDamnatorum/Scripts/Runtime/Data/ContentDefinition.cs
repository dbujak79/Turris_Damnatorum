using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Wspólne pola identyfikacyjne treści. Jedyny poziom dziedziczenia definicji.</summary>
    public abstract class ContentDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public BuildTag tags;
    }
}
