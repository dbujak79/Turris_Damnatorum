using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Mina/pułapka gracza, którą moce mogą przenosić (Fala przypływu) albo odpalać zdalnie (sygnał noża).</summary>
    public interface IUniqueMine : IHuAnchor
    {
        PlayerCombat Owner { get; }
        bool Armed { get; }
        float Remaining { get; }
        void MoveTo(Vector3 p);
    }

    /// <summary>Rejestr żywych min gracza (HU i mocy unikatów) i wstrzymanie wyzwalania kontaktem (Medalion sygnału z otchłani).</summary>
    public static class UniqueMines
    {
        static readonly List<IUniqueMine> all = new List<IUniqueMine>();
        /// <summary>Wejście wroga nie odpala miny (zdalny zapalnik) – null: zwykłe zasady.</summary>
        public static Func<IUniqueMine, bool> HoldContact;

        public static void Register(IUniqueMine m) { if (m != null && !all.Contains(m)) all.Add(m); }

        public static List<IUniqueMine> Live(PlayerCombat owner)
        {
            all.RemoveAll(m => m == null || !m.AnchorLive);
            var list = new List<IUniqueMine>();
            foreach (var m in all) if (owner == null || m.Owner == owner) list.Add(m);
            return list;
        }

        public static bool Held(IUniqueMine m) => HoldContact != null && HoldContact(m);
        public static void Clear() => all.Clear();
    }
}
