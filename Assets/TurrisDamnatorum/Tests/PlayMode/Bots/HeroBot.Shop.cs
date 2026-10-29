using System.Linq;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Sklep po piętrze (etap 6 Hell Unleashed): gracz kupuje kolejny poziom broni, potem perki (obrażenia i życie, chętniej
    /// podnosi posiadane), potem przedmioty poprawiające build; nowych broni nie kupuje (rodzaj broni decyduje o skillach).
    /// Tester kupuje poziom broni i jedną losową ofertę – sprawdza pokrycie sklepu.
    /// </summary>
    public abstract partial class HeroBot
    {
        static bool OffensivePerk(BoonDefinition b) => b.modifiers.Any(m => m.stat == StatType.PhysicalDamage || m.stat == StatType.GlobalDamage
            || m.stat == StatType.AttackSpeed || m.stat == StatType.ExecuteDamage || m.stat == StatType.CooldownReduction);

        protected virtual float PerkScore(BoonDefinition b)
        {
            float s = OffensivePerk(b) ? 3f : b.modifiers.Any(m => m.stat == StatType.MaxHealth || m.stat == StatType.PhysicalDefense || m.stat == StatType.DamageReduction) ? 2.5f : 1f;
            return s + Root.Run.BoonStacks(b) * 0.5f; // podnoszenie posiadanego perka
        }

        protected void ManageFloorShop()
        {
            var run = Root.Run;
            var stock = Root.FloorStock;
            if (stock == null || stock.Count == 0) return;
            void Buy(FloorOffer o)
            {
                if (o == null || o.sold || run.souls < o.price) return;
                Root.BuyFloorOffer(o);
                if (o.sold) Rec.R.purchases.Add($"sklep: {o.Title}");
            }
            Buy(stock.FirstOrDefault(o => o.kind == FloorOffer.Kind.WeaponLevel));
            if (P.persona == BotPersona.Tester)
            {
                var any = stock.Where(o => !o.sold && run.souls >= o.price && o.kind != FloorOffer.Kind.Weapon).ToList();
                if (any.Count > 0) Buy(any[rng.Next(any.Count)]);
                return;
            }
            int perks = 0;
            foreach (var o in stock.Where(o => o.kind == FloorOffer.Kind.Perk).OrderByDescending(o => PerkScore(o.perk)).ToList())
                if (perks < 2 && PerkScore(o.perk) >= 2.5f && run.souls >= o.price) { Buy(o); perks++; }
            foreach (var o in stock.Where(o => o.kind == FloorOffer.Kind.Item && !o.sold).OrderByDescending(o => o.item.rarity).ToList())
                if (run.souls >= o.price && GearWouldHelp(o.item)) { Buy(o); ManageGear(); }
        }
    }
}
