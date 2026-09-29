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

    /// <summary>Cel, który po przełamaniu postawy może przyjąć ripostę.</summary>
    public interface IRiposteTarget
    {
        bool CanBeRiposted { get; }
        void ReceiveRiposte(float damage, IHitReceiver attacker);
    }

    /// <summary>Pilnuje, by jeden zamach trafił ten sam cel najwyżej raz.</summary>
    public class HitTracker
    {
        readonly HashSet<IHitReceiver> hit = new HashSet<IHitReceiver>();
        public void Reset() => hit.Clear();
        public bool TryRegister(IHitReceiver r) => hit.Add(r);
        public int Count => hit.Count;
    }

    public static class HitQuery
    {
        static readonly Collider[] Buffer = new Collider[32];
        static readonly List<IHitReceiver> Found = new List<IHitReceiver>();

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
            Found.Clear();
            for (int i = 0; i < n; i++)
            {
                var r = Buffer[i].GetComponentInParent<IHitReceiver>();
                if (r == null || r.Faction == attacker || r.IsDead) continue;
                if (Found.Contains(r)) continue;
                Found.Add(r);
            }
            foreach (var r in Found)
                if (tracker.TryRegister(r)) onHit(r);
        }

        /// <summary>Rozstrzyga trafienie i powiadamia obie strony. Zwraca wynik.</summary>
        public static HitResult Apply(IHitReceiver attacker, IHitReceiver target, HitData hit, bool melee)
        {
            var result = target.ReceiveHit(hit);
            if (result.outcome == HitOutcome.Parried && melee && attacker != null && !attacker.IsDead)
                attacker.OnAttackParried(result.attackerPoiseDamage);
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

        public static void RaiseHit(Vector3 pos, HitResult r, bool targetIsPlayer) => HitResolved?.Invoke(pos, r, targetIsPlayer);
        public static void RaiseMessage(string msg, Color c) => Message?.Invoke(msg, c);
        public static void RaiseDied(IHitReceiver r) => Died?.Invoke(r);
        public static void RaiseWorldText(Vector3 pos, string text, Color c) => WorldText?.Invoke(pos, text, c);

        public static void ClearAll()
        {
            HitResolved = null; Message = null; Died = null; WorldText = null;
        }
    }
}
