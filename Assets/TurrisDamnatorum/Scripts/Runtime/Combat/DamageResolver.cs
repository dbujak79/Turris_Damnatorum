using UnityEngine;

namespace Turris
{
    /// <summary>Jedno trafienie. Obrażenia rozbite na składniki fizyczny i magiczny.</summary>
    public struct HitData
    {
        public float physical;
        public float magic;
        public float guardLoad;
        public float poiseDamage;
        public bool blockable;
        public bool parryable;
        public bool dodgeable;
        public bool heavy;
        public bool isRiposte;
        public Vector3 sourcePosition;
        public object attacker;        // IHitReceiver atakującego (do reakcji na parowanie)
        public bool fromMeleeWeapon;   // dla efektów "trafienie bronią"
        public string attackName;

        public float Total => physical + magic;

        public static HitData FromAttack(AttackDefinition a, float damage, Vector3 source, object attacker)
        {
            return new HitData
            {
                physical = a.damageType == DamageType.Physical ? damage : 0f,
                magic = a.damageType == DamageType.Magic ? damage : 0f,
                guardLoad = a.guardLoad,
                poiseDamage = a.poiseDamage,
                blockable = a.blockable,
                parryable = a.parryable,
                dodgeable = a.dodgeable,
                heavy = a.heavy,
                sourcePosition = source,
                attacker = attacker,
                attackName = a.name,
            };
        }
    }

    /// <summary>Migawka stanu obronnego celu w chwili trafienia.</summary>
    public struct DefenseState
    {
        public Vector3 position;
        public Vector3 forward;
        public bool invulnerable;      // okno niewrażliwości (unik, riposta)
        public bool parryActive;       // aktywne okno parowania
        public ParryData parry;
        public bool guarding;          // podniesiona garda
        public GuardData guard;
        public float stamina;
        public float physicalResist;   // 0..1 z pancerza
        public float magicResist;
        public bool dead;
    }

    public struct HitResult
    {
        public HitOutcome outcome;
        public float healthDamage;
        public float staminaDamage;
        public float poiseDamage;               // postawa odebrana obrońcy
        public float attackerPoiseDamage;       // postawa odebrana atakującemu (parowanie)
    }

    /// <summary>
    /// Czysta, deterministyczna logika rozstrzygania trafień – wspólna dla gracza i przeciwników.
    ///
    /// Kolejność sprawdzeń:
    ///  1. Martwy cel → Ignored.
    ///  2. Niewrażliwość (unik) chroni tylko przed atakami z dodgeable = true.
    ///  3. Aktywne okno parowania + atak parryable + atak z przodu → Parried (0 obrażeń, duże obrażenia postawy atakującego).
    ///     Parowanie NIE przechodzi w blok: jeśli nie wyszło, trafienie jest liczone jak zwykłe.
    ///  4. Podniesiona garda + atak blockable + atak z przodu:
    ///     koszt = guardLoad × stabilityMultiplier.
    ///     • koszt ≤ wytrzymałość → Blocked: obrażenia × (1 − redukcja gardy danego typu), wytrzymałość − koszt.
    ///     • koszt > wytrzymałość → GuardBroken: wytrzymałość → 0, obrażenia = max(po bloku, bez bloku × guardBreakDamageFactor),
    ///       ogłuszenie. (Reguła jawna i identyczna dla tarczy oraz broni.)
    ///  5. W przeciwnym razie → Hit: pełne obrażenia po redukcji pancerza.
    /// Pancerz (resist) działa zawsze, przed redukcją gardy.
    /// </summary>
    public static class DamageResolver
    {
        public static HitResult Resolve(in HitData hit, in DefenseState def, BalanceConfig rules)
        {
            var r = new HitResult();
            if (def.dead) { r.outcome = HitOutcome.Ignored; return r; }

            if (def.invulnerable && hit.dodgeable)
            {
                r.outcome = HitOutcome.Dodged;
                return r;
            }

            float phys = hit.physical * (1f - Mathf.Clamp01(def.physicalResist));
            float mag = hit.magic * (1f - Mathf.Clamp01(def.magicResist));
            float armored = phys + mag;

            if (def.parryActive && def.parry != null && hit.parryable && IsInFront(def, hit.sourcePosition, def.parry.angle))
            {
                r.outcome = HitOutcome.Parried;
                r.attackerPoiseDamage = def.parry.poiseDamage;
                return r;
            }

            if (def.guarding && def.guard != null && hit.blockable && IsInFront(def, hit.sourcePosition, def.guard.blockAngle))
            {
                float cost = hit.guardLoad * def.guard.stabilityMultiplier;
                float leaked = phys * (1f - def.guard.physicalReduction) + mag * (1f - def.guard.magicReduction);
                if (cost > def.stamina)
                {
                    r.outcome = HitOutcome.GuardBroken;
                    r.staminaDamage = def.stamina;
                    r.healthDamage = Mathf.Max(leaked, armored * rules.guardBreakDamageFactor);
                    r.poiseDamage = hit.poiseDamage;
                }
                else
                {
                    r.outcome = HitOutcome.Blocked;
                    r.staminaDamage = cost;
                    r.healthDamage = leaked;
                    r.poiseDamage = 0f;
                }
                return r;
            }

            r.outcome = HitOutcome.Hit;
            r.healthDamage = armored;
            r.poiseDamage = hit.poiseDamage;
            return r;
        }

        public static bool IsInFront(in DefenseState def, Vector3 source, float totalAngle)
        {
            Vector3 to = source - def.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return true;
            Vector3 fwd = def.forward;
            fwd.y = 0f;
            return Vector3.Angle(fwd, to) <= totalAngle * 0.5f;
        }
    }
}
