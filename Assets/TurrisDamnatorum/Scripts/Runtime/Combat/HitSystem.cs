using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Wszystko, co może zostać trafione (gracz i przeciwnicy).</summary>
    public interface IHitReceiver
    {
        Faction Faction { get; }
        bool IsDead { get; }
        Transform Transform { get; }
        /// <summary>Punkt celowania (środek tułowia).</summary>
        Vector3 AimPoint { get; }
        HitResult ReceiveHit(HitData hit);
        /// <summary>Wywoływane u atakującego, gdy jego cios wręcz został sparowany.</summary>
        void OnAttackParried(float poiseDamage);
        /// <summary>Wywoływane u atakującego po rozstrzygnięciu trafienia (np. efekty "za trafienie").</summary>
        void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result);
    }

    /// <summary>
    /// Zastępczy punkt odbierania trafień (Przygwożdżony cień): kolizja należy do innego obiektu, ale trafienie liczy się
    /// u właściciela – w jednym ataku ciało i cień to ta sama tożsamość celu (jedna porcja obrażeń).
    /// </summary>
    public interface IHitAlias
    {
        IHitReceiver AliasOf { get; }
    }

    /// <summary>Cel, który po przełamaniu postawy może przyjąć ripostę.</summary>
    public interface IRiposteTarget
    {
        bool CanBeRiposted { get; }
        void ReceiveRiposte(float damage, IHitReceiver attacker, HitData elements = default);
    }

    /// <summary>Pilnuje, by jeden zamach trafił ten sam cel najwyżej raz.</summary>
    public class HitTracker
    {
        readonly HashSet<IHitReceiver> hit = new HashSet<IHitReceiver>();
        public void Reset() => hit.Clear();
        public bool TryRegister(IHitReceiver r) => hit.Add(r);
        public bool Contains(IHitReceiver r) => hit.Contains(r);
        public int Count => hit.Count;
        /// <summary>Kopia listy trafionych (Taniec bez przerwy: przeniesiony cios nie odświeża celów).</summary>
        public void CopyFrom(HitTracker other) { hit.Clear(); if (other != null) foreach (var r in other.hit) hit.Add(r); }
    }

    public static class HitQuery
    {
        static readonly Collider[] Buffer = new Collider[32];
        /// <summary>Lista celów na każdy poziom zagnieżdżenia: trafienie może wywołać kolejne zapytanie (efekty „przy trafieniu”, wybuchy).</summary>
        static readonly List<List<IHitReceiver>> FoundStack = new List<List<IHitReceiver>>();
        static int depth;

        /// <summary>Kapsuła od <paramref name="from"/> do <paramref name="to"/>; każdy nowy wrogi cel trafiony raz na tracker.</summary>
        public static void Capsule(Vector3 from, Vector3 to, float radius, Faction attacker, HitTracker tracker, Action<IHitReceiver> onHit)
        {
            int n = Physics.OverlapCapsuleNonAlloc(from, to, radius, Buffer, ~0, QueryTriggerInteraction.Ignore);
            Deliver(n, attacker, tracker, onHit);
        }

        public static void Sphere(Vector3 center, float radius, Faction attacker, HitTracker tracker, Action<IHitReceiver> onHit)
        {
            int n = Physics.OverlapSphereNonAlloc(center, radius, Buffer, ~0, QueryTriggerInteraction.Ignore);
            Deliver(n, attacker, tracker, onHit);
        }

        static void Deliver(int n, Faction attacker, HitTracker tracker, Action<IHitReceiver> onHit)
        {
            if (depth == FoundStack.Count) FoundStack.Add(new List<IHitReceiver>());
            var found = FoundStack[depth];
            found.Clear();
            for (int i = 0; i < n; i++)
            {
                var r = Receiver(Buffer[i]);
                if (r == null || r.Faction == attacker || r.IsDead) continue;
                if (found.Contains(r)) continue;
                found.Add(r);
            }
            depth++;
            try
            {
                for (int i = 0; i < found.Count; i++)
                    if (tracker.TryRegister(found[i])) onHit(found[i]);
            }
            finally { depth--; found.Clear(); }
        }

        /// <summary>Odbiorca trafienia kolizji; cień (alias) zamienia się na właściciela.</summary>
        public static IHitReceiver Receiver(Collider c)
        {
            if (c == null) return null;
            var r = c.GetComponentInParent<IHitReceiver>();
            if (r is IHitAlias a) { var owner = a.AliasOf; return owner != null && !owner.IsDead ? owner : null; }
            return r;
        }

        /// <summary>Rozstrzyga trafienie i powiadamia obie strony. Zwraca wynik.</summary>
        public static HitResult Apply(IHitReceiver attacker, IHitReceiver target, HitData hit, bool melee)
        {
            var result = target.ReceiveHit(hit);
            if (result.outcome != HitOutcome.Ignored) CombatEvents.RaiseImpact(target, hit, result);
            if (result.outcome == HitOutcome.Parried && melee && attacker != null && !attacker.IsDead)
            {
                EnemyBrain.ParryDefender = target as PlayerCombat; // jawne pochodzenie parowania (bez zgadywania globalnego gracza)
                try { attacker.OnAttackParried(result.attackerPoiseDamage); }
                finally { EnemyBrain.ParryDefender = null; }
            }
            attacker?.OnHitResolved(target, hit, result);
            return result;
        }
    }

    /// <summary>Lekka magistrala zdarzeń dla interfejsu (liczby obrażeń, komunikaty).</summary>
    public static class CombatEvents
    {
        public static event Action<Vector3, HitResult, bool> HitResolved;   // pozycja, wynik, czy cel to gracz
        public static event Action<string, Color> Message;
        public static event Action<IHitReceiver> Died;
        /// <summary>Napis w świecie (tyknięcia efektów, reakcje, słabości) – bez efektów trafienia.</summary>
        public static event Action<Vector3, string, Color> WorldText;
        /// <summary>Rozstrzygnięte bezpośrednie trafienie (bez tyknięć efektów): cel, dane ciosu, wynik – dla reakcji ciała, hitstopu i warstw dźwięku.</summary>
        public static event Action<IHitReceiver, HitData, HitResult> Impact;
        /// <summary>Tyknięcie efektu (krwawienie, podpalenie) na celu – dla księgi obrażeń.</summary>
        public static event Action<IHitReceiver, float> StatusDamage;
        public static void RaiseStatusDamage(IHitReceiver target, float amount) => StatusDamage?.Invoke(target, amount);

        public static void RaiseHit(Vector3 pos, HitResult r, bool targetIsPlayer) => HitResolved?.Invoke(pos, r, targetIsPlayer);
        public static void RaiseMessage(string msg, Color c) => Message?.Invoke(msg, c);
        public static void RaiseDied(IHitReceiver r) => Died?.Invoke(r);
        public static void RaiseWorldText(Vector3 pos, string text, Color c) => WorldText?.Invoke(pos, text, c);
        public static void RaiseImpact(IHitReceiver target, in HitData hit, in HitResult r) => Impact?.Invoke(target, hit, r);

        public static void ClearAll()
        {
            HitResolved = null; Message = null; Died = null; WorldText = null; Impact = null;
        }
    }
}
