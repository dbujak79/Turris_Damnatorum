using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Etap 6 Hell Unleashed: przedmioty z rzadkością i affixami, perki, poziomy broni i sklep po piętrze.</summary>
    public class EconomyTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        [Test]
        public void Content_HasHuItemsAndSupportedPerks()
        {
            Assert.AreEqual(71, fx.Cfg.baseItemPool.Count(i => i != null && i.id.StartsWith("hu_item_")), "71 przedmiotów HU w puli");
            int perks = fx.Cfg.baseBoonPool.Count(b => b != null && b.id.StartsWith("hu_perk_"));
            Assert.AreEqual(HellEconomy.Upgrades.Count(DefaultContent.PerkSupported), perks);
            Assert.GreaterOrEqual(perks, 20, "perki ze statystykami z odpowiednikiem");
            var bone = fx.Get<ItemDefinition>("hu_item_amulet_bone");
            Assert.AreEqual(24f, bone.modifiers.Single(m => m.stat == StatType.MaxHealth).value, 1e-3f, "życie HU ×2");
        }

        [Test]
        public void StatMap_ConvertsHuUnits()
        {
            Assert.AreEqual(10f, HellEconomy.Map("FireResistance|Flat|0.1").Value.value, 1e-3f, "ułamek → %");
            Assert.AreEqual(StatType.FrostResist, HellEconomy.Map("WaterResistance|Flat|0.1").Value.stat);
            Assert.AreEqual(ModifierMode.Percent, HellEconomy.Map("MaxHealth|PercentAdditive|0.1").Value.mode);
            Assert.IsFalse(HellEconomy.Map("ProjectilePierce|Flat|1").HasValue, "pociski – bez odpowiednika");
        }

        [Test]
        public void Roll_RarityDecidesAffixes_GroupsAreUnique_TiersFollowWave()
        {
            var rng = new System.Random(5);
            var ring = fx.Get<ItemDefinition>("hu_item_ring_iron");
            var magic = HellEconomy.Roll(ring, LootSource.Shop, 1, rng, ItemRarity.Magic);
            Assert.LessOrEqual(magic.affixNames.Count, 2);
            Assert.Greater(magic.affixNames.Count, 0);
            for (int i = 0; i < 40; i++)
            {
                var leg = HellEconomy.Roll(fx.Get<ItemDefinition>("hu_item_body_heavy_t1"), LootSource.Shop, 20, rng, ItemRarity.Legendary);
                Assert.LessOrEqual(leg.affixNames.Count, 6);
                var early = HellEconomy.Roll(ring, LootSource.Shop, 1, rng, ItemRarity.Rare);
                Assert.IsTrue(early.affixNames.All(n => !n.Contains("III")), "na pierwszym piętrze tylko tier I: " + string.Join(", ", early.affixNames));
            }
            Assert.AreNotEqual(ItemRarity.Legendary, Enumerable.Range(0, 300).Select(_ => HellEconomy.RollRarity(LootSource.Boss, 1, rng)).Max(),
                "legendarne od piętra 5");
            var it = HellEconomy.Roll(ring, LootSource.Elite, 10, rng, ItemRarity.Rare);
            float sum = it.Modifiers.Sum(m => m.value);
            Assert.Greater(sum, ring.modifiers.Sum(m => m.value), "affixy dochodzą do premii przedmiotu");
        }

        [Test]
        public void WeaponLevels_TwentyPercentEach_PriceGrows()
        {
            var sword = new ItemInstance(fx.Get<ItemDefinition>("hu_sword"));
            Assert.AreEqual(HellEconomy.MaxWeaponLevel, HellEconomy.MaxLevelOf(sword.definition));
            int p0 = HellEconomy.WeaponLevelPrice(sword);
            sword.level = 2;
            Assert.AreEqual(1.4f, sword.WeaponLevelFactor, 1e-4f, "+20% za poziom");
            Assert.Greater(HellEconomy.WeaponLevelPrice(sword), p0 * 2);
        }

        [Test]
        public void FloorShop_OffersHeldWeaponLevel_PerksAndGuaranteedRarities()
        {
            var run = fx.NewRun("class_knight");
            run.floorsCleared = 5; run.floorIndex = 5;
            var stock = FloorShop.Generate(run, fx.Cfg, new System.Random(3));
            Assert.AreEqual(1, stock.Count(o => o.kind == FloorOffer.Kind.WeaponLevel && !o.item.definition.IsShield), "kolejny poziom trzymanej broni zawsze w ofercie");
            Assert.AreEqual(FloorShop.WeaponCount, stock.Count(o => o.kind == FloorOffer.Kind.WeaponLevel || o.kind == FloorOffer.Kind.Weapon));
            Assert.AreEqual(FloorShop.PerkCount, stock.Count(o => o.kind == FloorOffer.Kind.Perk));
            var items = stock.Where(o => o.kind == FloorOffer.Kind.Item).ToList();
            Assert.AreEqual(6, items.Count);
            Assert.IsTrue(items.Any(o => o.item.rarity == ItemRarity.Magic) && items.Any(o => o.item.rarity == ItemRarity.Rare) && items.Any(o => o.item.rarity == ItemRarity.Legendary),
                "magiczny, rzadki i (od piętra 5) legendarny");
            Assert.IsTrue(items.Take(3).All(o => HellEconomy.ItemRowOf(o.item.definition).Tags.Contains("heavy")), "3 przedmioty dla klasy (wojownik – ciężki pancerz)");

            var lvl = stock.First(o => o.kind == FloorOffer.Kind.WeaponLevel);
            run.souls = lvl.price - 1;
            Assert.IsFalse(FloorShop.Buy(lvl, run), "za mało dusz – nic się nie zmienia");
            run.souls = lvl.price;
            int before = lvl.item.level;
            Assert.IsTrue(FloorShop.Buy(lvl, run));
            Assert.AreEqual(before + 1, lvl.item.level);
            Assert.AreEqual(0, run.souls);
            Assert.IsFalse(FloorShop.Buy(lvl, run), "sprzedane raz");
        }

        [Test]
        public void Roll_UniqueWeapon_StaysUnique_WithoutAffixes()
        {
            var weapon = fx.Cfg.baseItemPool.First(d => d != null && d.IsWeapon && HellEconomy.IsUnique(d));
            var rng = new System.Random(3);
            for (int i = 0; i < 30; i++)
            {
                var it = HellEconomy.Roll(weapon, LootSource.Boss, 20, rng);
                Assert.AreEqual(ItemRarity.Unique, it.rarity, "unikatowa broń z łupu: " + weapon.id);
                Assert.AreEqual(0, it.affixNames.Count, "unikat bez losowych affixów");
            }
        }
    }
}
