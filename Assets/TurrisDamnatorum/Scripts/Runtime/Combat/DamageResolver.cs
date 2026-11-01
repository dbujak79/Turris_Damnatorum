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
        /// <summary>Przebicie odporności fizycznej (perki HU, affiksy) – ułamek, odporność celu nie spada poniżej 0.</summary>
        public float physicalPenetration;
        /// <summary>
        /// Przebicie odporności każdego żywiołu osobno (ułamek: 0,1 = 10 p.p.; odporność 50% → 40%). Działa wyłącznie na składnik
        /// swojego żywiołu i nie tworzy podatności poniżej zera; istniejąca podatność zostaje bez zmian.
        /// </summary>
        public float penFire, penFrost, penLightning, penEarth;
        /// <summary>
        /// Kanoniczny podział części magicznej na wszystkie żywioły naraz (affiksy dodanych żywiołów, zaklęcie, konwersje).
        /// Nieaktywny – obowiązuje dawny zapis <see cref="element"/>/<see cref="element2"/>. Aktywny – jedyne źródło udziałów,
        /// a <see cref="element"/>/<see cref="element2"/> są tylko opisem (dominujący i drugi żywioł: barwa, dźwięk, warunki „niesie ogień”).
        /// Wartość (struktura) – kopia trafienia ma własny podział.
        /// </summary>
        public MagicSplit split;

        // ---- Pochodzenie (cechy klasowe, rozdz. 7.2): dziedziczone przez kopie, odpryski, przebicia i strefy (kopiowanie struktury).
        /// <summary>Id ręcznej akcji gracza (atak, rzut, strzał), z której pochodzi trafienie; 0 = brak (wróg, reakcja z zewnątrz).</summary>
        public int originId;
        /// <summary>Zwykły atak bronią (cios albo podstawowy pocisk łuku/różdżki) – nie skill ze slotu.</summary>
        public bool basicAttack;
        /// <summary>Efekt potomny cechy klasowej (fala Łaski, Żar, pęknięcie, płyta, wyładowanie…): bez kradzieży życia,
        /// bez leczenia za zabójstwo i bez wytwarzania zasobów cech.</summary>
        public bool featureEffect;
        /// <summary>Trafienie wtórne mocy unikatu (fala, ślad, wyrok, iskry…): bez kradzieży życia, bez zasobów cech i bez ponownych haków mocy.</summary>
        public bool uniqueEffect;
        /// <summary>Atak sługi (attacker = pan) – nie jest ręczną akcją gracza (Wykorzystaj wyłom).</summary>
        public bool servantHit;
        /// <summary>Kopia albo rzut z wyzwalacza (echo, wachlarz, kaskada, przekaźnik, słup) – nie nowa ręczna akcja.</summary>
        public bool copyHit;
        /// <summary>Strzała odpowiedzi: pocisk niesie ładunek (dobór żywiołu przy kontakcie).</summary>
        public bool answerCharge;
        /// <summary>Strzała zaklęcia: id ładunku czaru (0 = brak); wyzwala się raz, na pierwszym trafieniu.</summary>
        public int spellArrowToken;
        /// <summary>Zderzenie frontów: budżet wyładowania przekazany z odrzutem (0 = brak).</summary>
        public float frontsBudget;
        /// <summary>Gwoździe łowcy: część trafienia odkładana jako grot (0 = brak).</summary>
        public float nailShare;
        /// <summary>Stanowisko strzeleckie: punkt wylotu pocisku inny niż postać (Sokole oko liczy od niego).</summary>
        public Vector3 launchPoint;
        public bool hasLaunchPoint;

        public float Total => physical + magic;

        /// <summary>Udział żywiołu w części magicznej (0–1); <see cref="Element.None"/> – magia bez żywiołu.</summary>
        public float ShareOf(Element e)
        {
            if (split.active) { float sum = split.Sum; return sum > 1e-6f ? split.Get(e) / sum : 0f; }
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            return (element == e ? 1f - s2 : 0f) + (element2 == e && element2 != element ? s2 : 0f);
        }

        /// <summary>Obrażenia danego składnika części magicznej.</summary>
        public float MagicOf(Element e) => magic * ShareOf(e);

        /// <summary>Trafienie niesie ten żywioł (dowolny składnik).</summary>
        public bool Has(Element e) => e != Element.None && ShareOf(e) > 0f;

        /// <summary>Część magiczna ma jakikolwiek składnik żywiołu (nie tylko magię bez żywiołu).</summary>
        public bool Elemental => magic > 0f && (split.active ? split.ElementalSum > 1e-6f
            : element != Element.None || (element2 != Element.None && element2Share > 0f));

        /// <summary>Przebicie odporności dla danego żywiołu (ułamek).</summary>
        public float PenetrationOf(Element e) => e == Element.Fire ? penFire : e == Element.Frost ? penFrost
            : e == Element.Lightning ? penLightning : e == Element.Earth ? penEarth : 0f;

        /// <summary>Mnożnik części magicznej ważony udziałami żywiołów (odporność, premia do żywiołu).</summary>
        public float Weighted(System.Func<Element, float> perElement)
        {
            if (split.active)
            {
                float sum = split.Sum;
                if (sum <= 1e-6f) return perElement(Element.None);
                float w = 0f;
                for (int i = 0; i < MagicSplit.Count; i++) { var e = MagicSplit.ElementAt(i); float s = split.Get(e); if (s > 0f) w += s * perElement(e); }
                return w / sum;
            }
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            return (1f - s2) * perElement(element) + s2 * perElement(element2);
        }

        /// <summary>Mnoży część magiczną osobno dla każdego żywiołu i przelicza udziały (każdy składnik dostaje swój mnożnik raz).</summary>
        public void ScaleMagicByElement(System.Func<Element, float> perElement)
        {
            if (split.active)
            {
                float before = split.Sum;
                if (before <= 1e-6f) { magic *= perElement(Element.None); return; }
                for (int i = 0; i < MagicSplit.Count; i++) { var e = MagicSplit.ElementAt(i); float s = split.Get(e); if (s > 0f) split.Set(e, s * perElement(e)); }
                float after = split.Sum;
                magic *= after / before;
                split.Normalize();
                SyncLegacy();
                return;
            }
            float s2 = element2 != Element.None && element2 != element ? Mathf.Clamp01(element2Share) : 0f;
            float m1 = perElement(element), m2 = s2 > 0f ? perElement(element2) : 0f;
            float w2 = (1f - s2) * m1 + s2 * m2;
            magic *= w2;
            if (s2 > 0f) element2Share = w2 > 1e-6f ? s2 * m2 / w2 : 0f;
        }

        /// <summary>Przepisuje dawny zapis (żywioł główny + drugi) do podziału kanonicznego; bez zmiany sumy.</summary>
        public void Materialize()
        {
            if (split.active) return;
            var s = new MagicSplit { active = true };
            if (magic > 0f) for (int i = 0; i < MagicSplit.Count; i++) { var e = MagicSplit.ElementAt(i); s.Set(e, ShareOf(e)); }
            split = s;
            split.Normalize();
        }

        /// <summary>
        /// Dodaje składnik magiczny danego żywiołu (dodany żywioł broni, zaklęcie). Gdy dawny zapis wystarcza (puste lub ten sam
        /// żywioł), zostaje dawny – bez zmian dla starych ścieżek; w przeciwnym razie trafienie przechodzi na podział kanoniczny.
        /// </summary>
        public void AddMagic(Element e, float amount)
        {
            if (amount <= 0f) return;
            if (!split.active)
            {
                if (magic <= 1e-6f) { magic = amount; element = e; element2 = Element.None; element2Share = 0f; return; }
                if (element == e && (element2 == Element.None || element2 == element || element2Share <= 0f)) { magic += amount; return; }
                Materialize();
            }
            float total = magic + amount, sum = split.Sum;
            for (int i = 0; i < MagicSplit.Count; i++)
            {
                var x = MagicSplit.ElementAt(i);
                float a = sum > 1e-6f ? magic * split.Get(x) / sum : 0f;
                if (x == e) a += amount;
                split.Set(x, total > 1e-6f ? a / total : 0f);
            }
            magic = total;
            SyncLegacy();
        }

        /// <summary>Konwersja żywiołu (część <paramref name="fraction"/>): suma bez zmian, bez kopii źródła.</summary>
        public void ConvertElement(Element from, Element to, float fraction = 1f)
        {
            fraction = Mathf.Clamp01(fraction);
            if (from == to || fraction <= 0f) return;
            if (!split.active && fraction >= 1f)
            {
                if (element == from) element = to;
                if (element2 == from) element2 = to;
                return;
            }
            Materialize();
            float moved = split.Get(from) * fraction;
            split.Set(from, split.Get(from) - moved);
            split.Set(to, split.Get(to) + moved);
            SyncLegacy();
        }

        /// <summary>Cała część magiczna staje się jednym żywiołem (moce unikatów nadpisujące żywioł).</summary>
        public void SetSingleElement(Element e)
        {
            split = default;
            element = e; element2 = Element.None; element2Share = 0f;
        }

        /// <summary>Opis dawnych pól z podziału: dominujący żywioł i drugi (udział drugiego w całej części magicznej).</summary>
        void SyncLegacy()
        {
            if (!split.active) return;
            Element best = Element.None, next = Element.None; float b = 0f, n = 0f;
            for (int i = 0; i < MagicSplit.Count; i++)
            {
                var e = MagicSplit.ElementAt(i);
                if (e == Element.None) continue;
                float s = split.Get(e);
                if (s > b) { next = best; n = b; best = e; b = s; }
                else if (s > n) { next = e; n = s; }
            }
            element = best; element2 = next;
            float sum = split.Sum;
            element2Share = next != Element.None && sum > 1e-6f ? n / sum : 0f;
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

    /// <summary>
    /// Udziały części magicznej trafienia: magia bez żywiołu i cztery żywioły naraz (bez alokacji – struktura wartościowa).
    /// </summary>
    public struct MagicSplit
    {
        public const int Count = 5;
        public bool active;
        public float none, fire, frost, lightning, earth;

        public static Element ElementAt(int i) => i == 0 ? Element.None : i == 1 ? Element.Fire : i == 2 ? Element.Frost : i == 3 ? Element.Lightning : Element.Earth;

        public float Get(Element e) => e == Element.Fire ? fire : e == Element.Frost ? frost : e == Element.Lightning ? lightning : e == Element.Earth ? earth : none;

        public void Set(Element e, float v)
        {
            v = Mathf.Max(0f, v);
            switch (e)
            {
                case Element.Fire: fire = v; break;
                case Element.Frost: frost = v; break;
                case Element.Lightning: lightning = v; break;
                case Element.Earth: earth = v; break;
                default: none = v; break;
            }
        }

        public float Sum => none + fire + frost + lightning + earth;
        public float ElementalSum => fire + frost + lightning + earth;

        public void Normalize()
        {
            float s = Sum;
            if (s <= 1e-6f) return;
            none /= s; fire /= s; frost /= s; lightning /= s; earth /= s;
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
        /// <summary>Wielki pożar: rodzina podpalenia (źródło) – 0 = zwykłe, jedno podpalenie.</summary>
        public int burnFamily;
        /// <summary>Wielki pożar: najwyżej tyle niezależnych rodzin na celu (0 – cecha nieaktywna).</summary>
        public int burnFamilies;
        /// <summary>Szata odroczonego pogorzeliska: podpalenie nie tyka, tylko gromadzi budżet (limit w sekundach palenia, termin).</summary>
        public bool deferBurn;
        public float deferCapSeconds;
        /// <summary>Zbroja runożercy: pęknięcie pancerza obniża odporności żywiołów (ułamek za warstwę) zamiast fizycznej.</summary>
        public float sunderMagicPerStack;

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

        /// <summary>
        /// Słabości i odporności celu na żywioły, osobno dla każdego składnika części magicznej: mnożnik żywiołu celu + osłabienie
        /// (<paramref name="cut"/>), potem przebicie tego żywiołu (odporność nie spada poniżej 0, podatność bez zmian). Magia bez
        /// żywiołu – bez zmian. Zwraca mnożnik ważony (do napisu „SŁABOŚĆ”).
        /// </summary>
        public static float ApplyElementResistances(ref HitData hit, System.Func<Element, float> baseMultiplier, float cut)
        {
            if (!hit.Elemental) return 1f;
            var h = hit;
            float Mult(Element e) => e == Element.None ? 1f : AffixMath.ResistMultiplier(baseMultiplier(e) + cut, h.PenetrationOf(e));
            float m = hit.Weighted(Mult);
            hit.ScaleMagicByElement(Mult);
            return m;
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
