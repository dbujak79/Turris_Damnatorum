using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>
    /// Treść z Hell Unleashed w Turris (etap 4 i pieczęcie w podejściu): zasada DPS broni, korekty balansu skilli,
    /// komplet definicji, ekonomia pieczęci, sakwa i więzi, ładunki i odnowienia z przerobionego kształtu.
    /// </summary>
    public class HuContentTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void EveryHuWeapon_KeepsHuDpsRatioToTheSword()
        {
            var sword = fx.Get<ItemDefinition>("hu_sword");
            float swordDps = HellWeaponCatalog.TurrisDps(sword.weapon);
            float swordHu = HellWeaponCatalog.Find("weapon.sword").HuDps;
            int checkedCount = 0;
            foreach (var item in fx.bundle.all.OfType<ItemDefinition>().Where(i => i.kind == ItemKind.Weapon && !string.IsNullOrEmpty(i.huSourceId)))
            {
                var row = HellWeaponCatalog.Find(item.huSourceId);
                if (row == null || row.HuDps <= 0) continue;
                float expected = row.HuDps / swordHu, actual = HellWeaponCatalog.TurrisDps(item.weapon) / swordDps;
                Assert.AreEqual(expected, actual, expected * 0.02f, $"{item.id}: DPS względem miecza {actual:0.00} zamiast {expected:0.00}");
                checkedCount++;
            }
            Assert.GreaterOrEqual(checkedCount, 40, "wszystkie bronie HU są przeliczone");
            Assert.AreEqual(40f, sword.weapon.light.baseDamage, 0.01f, "miecz wzorcowy bez zmian");
        }

        [Test]
        public void EarthWand_IsSlowerThanFireWand_WithSimilarDps()
        {
            var fire = fx.Get<ItemDefinition>("hu_wand_fire").weapon;
            var earth = fx.Get<ItemDefinition>("hu_wand_earth").weapon;
            Assert.Greater(earth.light.windup, fire.light.windup * 1.5f, "ciężki kamień leci rzadziej");
            Assert.Less(HellWeaponCatalog.TurrisDps(earth), HellWeaponCatalog.TurrisDps(fire), "audyt P1-a: różdżka ziemi nie bije mocniej niż ognia");
        }

        [Test]
        public void TwoHanders_HitAboutAQuarterHarderThanTheSword_NotHalf()
        {
            float sword = HellWeaponCatalog.TurrisDps(fx.Get<ItemDefinition>("hu_sword").weapon);
            float great = HellWeaponCatalog.TurrisDps(fx.Get<ItemDefinition>("hu_greatsword").weapon);
            Assert.AreEqual(1.25f, great / sword, 0.05f);
        }

        [Test]
        public void Summons_DoNotOutliveTheirCooldown()
        {
            foreach (var skill in HuCatalog.Skills.Values.Where(s => s.HasComponent("Summon")))
                for (int lv = 1; lv <= skill.MaxLevel; lv++)
                {
                    var p = skill.At(lv);
                    Assert.LessOrEqual(p.Get("duration_seconds"), p.Get("cooldown"), $"{skill.id} poziom {lv}: sługi stałe z jednego slotu (audyt P1-b)");
                }
        }

        /// <summary>
        /// Audyt skilli P1-b: obrażenia sług na sekundę (liczebność × obrażenia / odstęp ataku, jednostki HU) ≤ 25 – przy
        /// ×2,5 Turris ok. 60/s, 2–2,5× mediany pojedynczego celu. Legion (sygnatura, odnowienie 28 s) poza limitem.
        /// </summary>
        [Test]
        public void Summons_DamagePerSecond_StaysNearTwiceTheMedian()
        {
            foreach (var skill in HuCatalog.Skills.Values.Where(s => s.HasComponent("Summon") && s.id != "skill.sub.legion"))
                for (int lv = 1; lv <= skill.MaxLevel; lv++)
                {
                    var p = skill.At(lv);
                    double dps = p.Get("count") * p.TotalDamage / p.Get("attack_interval");
                    Assert.LessOrEqual(dps, 25.0, $"{skill.id} poziom {lv}: {dps:0.0}/s");
                }
        }

        /// <summary>Ściana ognia (dawny Ognisty krąg): skill Turris na komponencie Wall, dwie formy – fala i forteca.</summary>
        [Test]
        public void FireWall_ReplacesFireRing_AndTakesItsForms()
        {
            var def = fx.Get<SpellDefinition>("spell_firering");
            Assert.AreEqual("Ściana ognia", def.displayName);
            Assert.IsNotNull(def.HuData, "wykonuje go silnik HU – przyjmuje pieczęcie");
            Assert.IsTrue(def.HuData.HasComponent("Wall"));
            Assert.IsTrue(HuSkillCaster.IsKnown("Wall"));
            var plain = HuSkillShape.Compose(def.HuData, 1, null, new HuSeal[0]);
            Assert.AreEqual(7f, plain.Parameters.GetF("wall_length"), 1e-3f, "ściana ok. 7 m");
            Assert.AreEqual(HuTargeting.Point, plain.Targeting);

            var wave = HuSkillShape.Compose(def.HuData, 1, HuCatalog.Seal("seal.form.wall.wave"), new HuSeal[0]);
            Assert.Greater(wave.Parameters.GetF("wall_speed"), 0f, "Fala ognia rusza przed siebie");
            var fortress = HuSkillShape.Compose(def.HuData, 1, HuCatalog.Seal("seal.form.wall.fortress"), new HuSeal[0]);
            Assert.AreEqual(HuTargeting.Self, fortress.Targeting, "Forteca wokół bohatera");
            Assert.IsTrue(fortress.Parameters.On("wall_ring"));
            Assert.AreEqual(plain.Parameters.GetF("cooldown") + 3f, fortress.Parameters.GetF("cooldown"), 1e-3f);
            Assert.AreEqual("Szereg kręgów", HuCatalog.Seal("seal.form.ring.wall").name, "nazwa Ściana ognia nie jest zdublowana");
        }

        /// <summary>Audyt skilli P2-b: formy mocne w obu kontekstach płacą odnowieniem (+1,5 s), opis to mówi.</summary>
        [Test]
        public void DominantForms_PayWithCooldown()
        {
            foreach (var (skill, form) in new[] { ("skill.mage.damned_chain", "seal.form.chain.ball"), ("skill.warrior.wind_slash", "seal.form.slash.boomerang"),
                ("skill.rogue.splinter_arrow", "seal.form.splinter.boomerang"), ("skill.rogue.sulfur_bomb", "seal.form.bomb.napalm") })
            {
                var data = HuCatalog.Skill(skill);
                Assert.IsNotNull(data, skill);
                var seal = HuCatalog.Seal(form);
                float plain = HuSkillShape.Compose(data, 3, null, new HuSeal[0]).Parameters.GetF("cooldown");
                float sealed_ = HuSkillShape.Compose(data, 3, seal, new HuSeal[0]).Parameters.GetF("cooldown");
                Assert.AreEqual(plain + 1.5f, sealed_, 1e-3f, form);
                StringAssert.Contains("+1,5 s odnowienia", seal.desc, form);
            }
        }

        /// <summary>Audyt skilli P1-a: Aura świętego ognia 8/11/14 (było 12/17/24), Sztandar ×1,25 (było ×1,5).</summary>
        [Test]
        public void HolyFireAura_AndBanner_AreWeakened()
        {
            var aura = HuCatalog.Skill("skill.aura.holy_fire");
            Assert.AreEqual(new[] { 8.0, 11.0, 14.0 }, Enumerable.Range(1, 3).Select(lv => aura.At(lv).Damage(HuElement.Fire)).ToArray());
            var banner = HuCatalog.Seal("seal.form.holy_fire.banner");
            var shape = HuSkillShape.Compose(aura, 3, banner, new HuSeal[0]);
            Assert.AreEqual(1.25f, shape.Parameters.GetF("banner_power"), 1e-4f);
            StringAssert.Contains("jedną czwartą", banner.desc);
        }

        [Test]
        public void EveryHuSkill_HasADefinition_AndNewOnesAreOffered()
        {
            foreach (var hu in HuCatalog.Skills.Values)
            {
                Assert.IsTrue(fx.bundle.byId.TryGetValue(hu.targetId, out var def), $"{hu.id} → {hu.targetId}");
                var s = (SpellDefinition)def;
                Assert.AreSame(hu, s.HuData, hu.id);
                if (!hu.IsSignature && hu.shop) Assert.Contains(s, fx.Cfg.baseSpellPool, $"{s.id} w puli nagród");
            }
            Assert.AreEqual(3, fx.Get<SpellDefinition>("hu_whirlwind").minFloor, "MinWave 16 → po trzecim piętrze");
            Assert.AreEqual(0, fx.Get<SpellDefinition>("hu_volley").minFloor);
        }

        [Test]
        public void SealedShape_DrivesChargesAndCooldown()
        {
            var inst = new SpellInstance(fx.Get<SpellDefinition>("hu_infernal_orb"), 2);
            Assert.AreEqual(1, inst.MaxCharges);
            Assert.IsTrue(inst.seals.TryInsert(HuCatalog.Seal("seal.form.orb.mine"), out _));
            Assert.AreEqual(3, inst.MaxCharges, "Ognista mina: trzy ładunki");
            var haste = new SpellInstance(fx.Get<SpellDefinition>("hu_infernal_orb"), 2);
            float before = haste.Cooldown;
            haste.seals.TryInsert(HuCatalog.Seal("seal.haste"), out _);
            Assert.Less(haste.Cooldown, before);
            Assert.GreaterOrEqual(haste.Cooldown, before * SpellInstance.CooldownFloor - 1e-4f);
        }

        [Test]
        public void SealPouch_InsertRemoveAndSell()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            var orb = run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            var split = HuCatalog.Seal("seal.split");
            Assert.AreEqual(SealGain.Pouch, run.AddSeal(split, toPouch: true));
            Assert.IsTrue(run.InsertSeal(orb, split));
            Assert.IsFalse(run.sealPouch.Contains(split));
            Assert.AreEqual(3.0, orb.seals.Shape.Parameters.Get("count"), 1e-6);
            Assert.IsTrue(run.RemoveSeal(orb, split));
            int souls = run.souls;
            Assert.IsTrue(run.SellSeal(split));
            Assert.AreEqual(souls + RunState.SealSalePrice(split), run.souls);
        }

        [Test]
        public void SecondCopy_RaisesRank_AndRankBonusIsCapped()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            var orb = run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            orb.level = 2;
            var blood = HuCatalog.Seal("seal.blood");
            Assert.AreEqual(SealGain.Pouch, run.AddSeal(blood, toPouch: true));
            Assert.IsTrue(run.InsertSeal(orb, blood));
            double one = orb.seals.Shape.Parameters.Damage(HuElement.Fire);
            Assert.AreEqual(SealGain.RankUp, run.AddSeal(blood));
            Assert.AreEqual(2, run.SealRank(blood));
            Assert.AreEqual(one * (1 + HuSkillShape.RankBonus), orb.seals.Shape.Parameters.Damage(HuElement.Fire), 1e-6);
            run.AddSeal(blood);
            Assert.AreEqual(SealGain.None, run.AddSeal(blood), "poziom III to maksimum");
        }

        [Test]
        public void Bonds_WakeBetweenSkillsInHand_AndLeaveWithTheSlot()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            var orb = run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            var shard = run.LearnSpell(fx.Get<SpellDefinition>("hu_frost_shard"));
            run.AssignSlot(orb, 0); run.AssignSlot(shard, 1);
            run.RefreshBonds();
            Assert.IsTrue(run.activeBonds.Any(b => b.id == "seal.bond.steam"), "Para: ogień + woda w ręce");
            Assert.IsTrue(orb.seals.Shape.Bonds.Any(b => b.id == "seal.bond.steam"));
            run.ClearSlot(1);
            run.RefreshBonds();
            Assert.IsFalse(run.activeBonds.Any(b => b.id == "seal.bond.steam"));
            Assert.IsEmpty(orb.seals.Shape.Bonds);
        }

        [Test]
        public void SealPurchase_IsAtomic()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            var offer = new SealOffer { seal = HuCatalog.Seal("seal.split"), price = 100 };
            run.souls = 99;
            Assert.AreEqual(SealGain.None, HuSealEconomy.Buy(run, offer));
            Assert.AreEqual(99, run.souls);
            run.souls = 150;
            Assert.AreNotEqual(SealGain.None, HuSealEconomy.Buy(run, offer));
            Assert.AreEqual(50, run.souls);
            Assert.AreEqual(SealGain.None, HuSealEconomy.Buy(run, offer), "sprzedana oferta nie sprzeda się drugi raz");
        }

        [Test]
        public void Offers_IncludeAFormForAnOwnedSkill_AndNoDemonicSeals()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            for (int seed = 0; seed < 20; seed++)
            {
                var offers = HuSealEconomy.Offers(run, new System.Random(seed), fx.Cfg.balance);
                Assert.IsTrue(offers.Any(o => o.seal.IsForm), $"ziarno {seed}");
                Assert.IsFalse(offers.Any(o => o.seal.lootOnly));
                Assert.AreEqual(offers.Count, offers.Select(o => o.seal).Distinct().Count());
            }
        }

        [Test]
        public void BossGift_OffersDifferentSealsTheHeroCanUse()
        {
            var run = fx.NewRun("class_hu_mage_fire");
            run.LearnSpell(fx.Get<SpellDefinition>("hu_infernal_orb"));
            for (int seed = 0; seed < 20; seed++)
            {
                var gift = HuSealEconomy.RollGift(run, new System.Random(seed));
                Assert.AreEqual(3, gift.Count);
                Assert.AreEqual(3, gift.Distinct().Count());
                Assert.IsTrue(gift.All(s => HuSealEconomy.CanTake(run, s)));
            }
        }

        [Test]
        public void UniqueWeapons_CarryTheirHuPower()
        {
            Assert.Greater(fx.Get<ItemDefinition>("hu_unique_szczerbiec").effects.Sum(e => e.type == PassiveEffectType.CheatDeath ? e.value : 0f), 0f);
            Assert.Greater(fx.Get<ItemDefinition>("hu_unique_last_word").effects.Sum(e => e.type == PassiveEffectType.CorpseExplosion ? e.value : 0f), 0f);
            Assert.Greater(fx.Get<ItemDefinition>("hu_bow_composite").effects.Sum(e => e.type == PassiveEffectType.ProjectilePierce ? e.value : 0f), 0f);
            Assert.Less(fx.Get<ItemDefinition>("hu_unique_penance").offerWeight, 1f, "unikaty rzadziej w nagrodach");
        }
    }
}
