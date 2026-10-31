using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Jedno trafienie. Obrażenia rozbite na składniki fizyczny i magiczny.</summary>
    public struct HitData
    {
        /// <summary>Opcjonalna rodzina cząsteczek HU; nie bierze udziału w obliczaniu obrażeń.</summary>
        public HellFxElement? visualElement;
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
        public Element element;        // żywioł części magicznej
        /// <summary>Drugi żywioł części magicznej (HU: składowe mieszane) i jego udział w niej (0–1); reszta to <see cref="element"/>.</summary>
        public Element element2;
        public float element2Share;
        public List<StatusApplication> statuses; // efekty nakładane po trafieniu (może być null)
        public float explosionRadius;  // pocisk: wybuch przy trafieniu
        public float leaveZoneDuration;// pocisk: wybuch zostawia płonącą ziemię na tyle sekund (0 = nie)
        public bool isStatusTick;      // tyknięcie efektu – bez reakcji, postawy i drgnięcia
        public bool isReaction;        // obrażenia reakcji (roztrzaskanie, przeskok przewodzenia) – nie wywołują kolejnych reakcji
        public bool isGroundHazard;
        public float accuracy;         // celność atakującego (ułamek) – zmniejsza unik demona    // tyknięcie strefy na ziemi albo aury (HU GroundHazard) – nakłada efekty, ale nie wywołuje reakcji
        public float bonusVsFrozen, bonusVsBleeding, executeBonus;
        /// <summary>Premia przeciw wychłodzonym i przemoczonym (HU: przeciw przemoczonym – Przewodnictwo, więź Burza).</summary>
        public float bonusVsChilled;
        /// <summary>Trafienie skilla odpalonego wyzwalaczem pieczęci – to, co zabije, nie odpala kolejnego wyzwalacza.</summary>
        public bool huTriggered;
        public StatusModifiers mods;   // modyfikatory efektów z wyposażenia atakującego
        public WeaponModel weapon;     // zwykły cios bronią (Auto = nie broń: czar, technika, pocisk) – barwa trafienia i hitstop atakującego
        public float knockback;        // odrzut trafionego wroga (m); ujemny = przyciągnięcie do źródła
        public float knockdown;        // powalenie trafionego wroga (s; bossy odporne, potem odporność na ogłuszenie)
        public float knockdownChance;  // szansa powalenia (0 – zawsze); losowana osobno dla każdego trafionego
        public bool isProjectile;      // trafienie pociskiem (Sokole oko)
        /// <summary>Przebicie odporności (perki HU): część żywiołu i fizyczna – w ułamku, odporność celu nie spada poniżej 0.</summary>
        public float penetration, physicalPenetration;

        public float Total => physical + magic;

        /// <summary>Udział żywiołu w części magicznej (0–1).</summary>
        public float ShareOf(Element e)
        {
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            return (element == e ? 1f - s2 : 0f) + (element2 == e && element2 != element ? s2 : 0f);
        }

        /// <summary>Trafienie niesie ten żywioł (główny albo drugi).</summary>
        public bool Has(Element e) => e != Element.None && ShareOf(e) > 0f;

        /// <summary>Mnożnik części magicznej ważony udziałami żywiołów (odporność, premia do żywiołu).</summary>
        public float Weighted(System.Func<Element, float> perElement)
        {
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            return (1f - s2) * perElement(element) + s2 * perElement(element2);
        }

        /// <summary>Mnoży część magiczną osobno dla każdego żywiołu i przelicza udział drugiego.</summary>
        public void ScaleMagicByElement(System.Func<Element, float> perElement)
        {
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            float m1 = perElement(element), m2 = s2 > 0f ? perElement(element2) : 0f;
            float w = (1f - s2) * m1 + s2 * m2;
            magic *= w;
            if (s2 > 0f) element2Share = w > 1e-6f ? s2 * m2 / w : 0f;
        }

        public static HitData FromAttack(AttackDefinition a, float damage, Vector3 source, object attacker)
        {
            return new HitData
            {
                // Broń z żywiołem: część obrażeń fizycznych staje się obrażeniami żywiołu (magicznymi).
                physical = a.damageType == DamageType.Physical ? damage * (1f - (a.element != Element.None ? a.elementShare : 0f)) : 0f,
                magic = a.damageType == DamageType.Magic ? damage : (a.element != Element.None ? damage * a.elementShare : 0f),
                guardLoad = a.guardLoad,
                poiseDamage = a.poiseDamage,
                blockable = a.blockable,
                parryable = a.parryable,
                dodgeable = a.dodgeable,
                heavy = a.heavy,
                sourcePosition = source,
                attacker = attacker,
                attackName = a.name,
                element = a.element,
                element2 = a.element2, element2Share = a.element2Share,
                statuses = a.statuses,
                explosionRadius = a.explosionRadius,
                bonusVsFrozen = a.bonusVsFrozen,
                bonusVsBleeding = a.bonusVsBleeding,
                executeBonus = a.executeBonus,
                mods = StatusModifiers.None,
            };
        }
    }

    /// <summary>Modyfikatory efektów pochodzące z wyposażenia atakującego (przenoszone w trafieniu).</summary>
    public struct StatusModifiers
    {
        public float burnDurationBonus;   // + s podpalenia
        public int bleedStackBonus;       // + maks. warstw krwawienia
        public float bleedMovingBonus;    // + do mnożnika krwawienia w ruchu
        public float bleedDamageMult;     // mnożnik siły krwawienia
        public int freezeReduction;       // mniej warstw chłodu do zamrożenia
        public float conductionMult;      // mnożnik przewodzenia
        public bool conductionJump;       // przewodzenie przeskakuje na kolejnego wroga

        public static StatusModifiers None => new StatusModifiers { bleedDamageMult = 1f, conductionMult = 1f };
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

            float phys = hit.physical * (1f - Mathf.Clamp(def.physicalResist, -1f, 1f)); // ujemna: pęknięcie pancerza poniżej zera (HU)
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
