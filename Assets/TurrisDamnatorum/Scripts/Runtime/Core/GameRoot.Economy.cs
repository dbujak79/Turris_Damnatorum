using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Etap 6 Hell Unleashed w przepływie gry: sklep po każdym piętrze i łupy z przedmiotami.</summary>
    public partial class GameRoot
    {
        /// <summary>Oferty sklepu po ukończonym piętrze (bronie, perki, przedmioty) – <see cref="FloorShop"/>.</summary>
        public List<FloorOffer> FloorStock { get; private set; } = new List<FloorOffer>();

        public void BuyFloorOffer(FloorOffer offer)
        {
            if (Run == null || !FloorShop.Buy(offer, Run)) return;
            Sfx.Ui(SoundId.UiBuy);
            CombatEvents.RaiseMessage(offer.kind == FloorOffer.Kind.WeaponLevel ? $"{offer.item.Name} – nowy poziom" : $"Kupiono: {offer.Title}",
                offer.item != null ? HellEconomy.RarityColor(offer.item.rarity) : new Color(0.7f, 0.85f, 1f));
            Run.SyncGrantedSkills();
            Player.RefreshBuild();
        }

        /// <summary>
        /// Łup z przedmiotem (HU <c>LootTable</c>): zwykły wróg 4%, elita 60% (jakość 0–1), boss 100% (jakość 1–2). 30% łupów to broń,
        /// reszta zbroja i biżuteria (treść innej klasy ×0,35); rzut na unikat daje unikat dostępny na tym piętrze. Przedmiot leży na
        /// ziemi (<see cref="LootPickup"/>), po podniesieniu trafia do pustego slotu albo do plecaka.
        /// </summary>
        void ItemDrop(EnemyBrain e)
        {
            if (Run == null || IsEnemyTrial || e == null || e.Def == null || e.Def.isMinion) return;
            var source = e.Def.isBoss ? LootSource.Boss : e.IsElite ? LootSource.Elite : LootSource.Normal;
            float chance = source == LootSource.Boss ? 1f : source == LootSource.Elite ? 0.6f : 0.04f;
            if (rng.NextDouble() >= chance) return;
            int wave = Run.floorIndex + 1;
            var rarity = HellEconomy.RollRarity(source, wave, rng);
            string family = FloorShop.Family(Run.startingClass);
            float Affinity(string classId) => string.IsNullOrEmpty(classId) || classId.StartsWith(family) ? 1f : FloorShop.OffClassWeight;
            var pool = config.baseItemPool.Where(d => d != null && d.minFloor <= Run.floorsCleared && !d.IsShield && d.kind != ItemKind.OffHand).ToList();
            List<ItemDefinition> candidates;
            if (rarity == ItemRarity.Unique)
            {
                // Unikaty HU i natywne unikaty Turris (UniqueLoot: preferencja klasy i poszukiwanego przedmiotu).
                var picked = UniqueLoot.Pick(config, Run, source, Meta?.Profile.soughtUnique, rng);
                candidates = picked != null ? new List<ItemDefinition> { picked } : new List<ItemDefinition>();
            }
            else if (rng.NextDouble() < 0.3)
                candidates = pool.Where(d => d.IsWeapon && !string.IsNullOrEmpty(d.huSourceId) && !d.huSourceId.Contains(".unique.")).ToList();
            else
                candidates = pool.Where(d => HellEconomy.ItemRowOf(d) is HellEconomy.ItemRow r && r.Rarity != 4).ToList();
            if (candidates.Count == 0)
            {
                if (rarity != ItemRarity.Unique) return;
                rarity = wave < HellEconomy.LegendaryMinWave ? ItemRarity.Rare : ItemRarity.Legendary;
                candidates = pool.Where(d => HellEconomy.ItemRowOf(d) is HellEconomy.ItemRow r && r.Rarity != 4).ToList();
                if (candidates.Count == 0) return;
            }
            float Weight(ItemDefinition d) => d.offerWeight * Affinity(HellEconomy.ItemRowOf(d)?.ClassId ?? HellWeaponCatalog.Find(d.huSourceId ?? "")?.ClassId);
            float total = candidates.Sum(Weight), roll = (float)rng.NextDouble() * total;
            var def = candidates.FirstOrDefault(d => (roll -= Weight(d)) <= 0f) ?? candidates[candidates.Count - 1];
            var item = HellEconomy.Roll(def, source, wave, rng, rarity == ItemRarity.Unique ? (ItemRarity?)null : rarity);
            var c = HellEconomy.RarityColor(item.rarity);
            CombatEvents.RaiseWorldText(e.AimPoint + Vector3.up * 1.2f, item.rarity >= ItemRarity.Rare ? HellEconomy.RarityName(item.rarity).ToUpperInvariant() + "!" : "ŁUP", c);
            var run = Run;
            LootPickup.Spawn(Player, e.transform.position, c, item.Name, () =>
            {
                if (Run != run) return;
                if (!run.CanReceive(item)) { CombatEvents.RaiseMessage($"Plecak pełny: {item.Name} przepada", new Color(0.9f, 0.5f, 0.4f)); return; }
                run.AddItem(item, true);
                if (item.definition.IsNativeUnique && Meta != null) Meta.DiscoverUnique(item.definition.id);
                run.SyncGrantedSkills();
                Player.RefreshBuild();
                CombatEvents.RaiseMessage($"Łup: {item.Name} ({HellEconomy.RarityName(item.rarity)})", c);
            }, item.rarity >= ItemRarity.Legendary);
        }
    }
}
