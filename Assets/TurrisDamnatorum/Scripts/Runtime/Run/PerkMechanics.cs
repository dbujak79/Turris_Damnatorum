using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Perki Hell Unleashed zmieniające mechanikę: tabele poziomów (Rozszczepienie, Niestabilne pociski), uśpienie perka bez
    /// pasującego wyposażenia (HU <c>StatSystem.IsActive</c> + <c>RequiresAny</c>) i stałe z HU (rozpęd, blok bierny, odbicie).
    /// </summary>
    public static class PerkMechanics
    {
        /// <summary>Rozszczepienie: łączna liczba pocisków na strzał na poziomach 1–4 (HU 2, 3, 5, 8); boczne – 40% obrażeń.</summary>
        static readonly int[] Volley = { 1, 2, 3, 5, 8 };
        public const float VolleySideShare = 0.4f;

        public static int VolleyCount(float level) => Volley[Mathf.Clamp(Mathf.RoundToInt(level), 0, Volley.Length - 1)];

        /// <summary>Niestabilne pociski: część obrażeń w wybuchu na poziomach 1–3 (HU 0,3 / 0,45 / 0,6).</summary>
        static readonly float[] Explosion = { 0f, 0.3f, 0.45f, 0.6f };

        public static float ExplosionShare(float level) => Explosion[Mathf.Clamp(Mathf.RoundToInt(level), 0, Explosion.Length - 1)];

        /// <summary>Wybuch pocisku z perkiem ma co najmniej tyle metrów promienia (HU 1,5 m).</summary>
        public const float MinExplosionRadius = 1.5f;

        /// <summary>Rozpęd: najwięcej warstw i po ilu sekundach bez trafienia znikają.</summary>
        public const int MomentumMaxStacks = 5;
        public const float MomentumTimeout = 1.5f;

        /// <summary>Blok bierny z tarczą: szansa bazowa, przyrost za poziom tarczy powyżej I i odnowienie ładunku na poziomach I–III.</summary>
        public const float BaseShieldBlockChance = 15f, ShieldBlockPerLevel = 2f;
        static readonly float[] BlockCooldown = { 1f, 0.92f, 0.85f };

        public static float ShieldBlockCooldown(int shieldLevel) => BlockCooldown[Mathf.Clamp(shieldLevel, 0, BlockCooldown.Length - 1)];

        /// <summary>Odbity pocisk leci szybciej i dłużej.</summary>
        public const float ReflectSpeed = 1.4f, ReflectExtraLife = 1.5f;

        /// <summary>
        /// Perk HU działa przy obecnym wyposażeniu: wymagane tagi na właściwym slocie (broń główna / druga ręka / którykolwiek
        /// przedmiot) i – gdy perk ma warunki „którykolwiek” – posiadany inny perk albo tag broni głównej. Uśpiony perk zostaje
        /// kupiony, ale nic nie daje, dopóki wyposażenie się nie zmieni.
        /// </summary>
        public static bool Active(HellEconomy.UpgradeRow r, RunState run) =>
            ActiveWith(r, run, run.equipment.Get(EquipSlot.MainHand)?.definition, run.equipment.Get(EquipSlot.OffHand)?.definition);

        /// <summary>Jak <see cref="Active(HellEconomy.UpgradeRow, RunState)"/>, ale z podanymi rękami (HU LoadoutAnalysis: „po zmianie broni”).</summary>
        public static bool ActiveWith(HellEconomy.UpgradeRow r, RunState run, ItemDefinition main, ItemDefinition off)
        {
            if (r == null) return true;
            if (r.RequiredTags.Length > 0 && r.RequiredSlot != 0)
            {
                bool Has(ItemDefinition d) => d != null && r.RequiredTags.All(HellEconomy.TagsOf(d).Contains);
                bool ok = r.RequiredSlot == 1 ? Has(main) : r.RequiredSlot == 2 ? Has(off)
                    : Has(main) || Has(off) || run.equipment.Equipped.Any(kv => kv.Key != EquipSlot.MainHand && kv.Key != EquipSlot.OffHand && kv.Value != null && Has(kv.Value.definition));
                if (!ok) return false;
            }
            if (r.RequiresAny.Length == 0) return true;
            foreach (var c in r.RequiresAny)
            {
                if (c.StartsWith("perk:"))
                {
                    string id = DefaultContent.HuPerkId(new HellEconomy.UpgradeRow { id = c.Substring(5) });
                    if (run.boons.Any(b => b.definition != null && b.definition.id == id && b.stacks > 0)) return true;
                }
                else if (c.StartsWith("tag:") && main != null && HellEconomy.TagsOf(main).Contains(c.Substring(4))) return true;
            }
            return false;
        }

        /// <summary>Wzmocnienie działa (zwykłe wzmocnienia Turris – zawsze; perki HU – gdy nie są uśpione).</summary>
        public static bool Active(BoonDefinition b, RunState run) => b == null || !b.id.StartsWith("hu_perk_") || Active(FloorShop.PerkRow(b), run);
    }
}
