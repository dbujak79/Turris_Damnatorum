using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>
    /// Pieczęcie z Hell Unleashed wczytane z katalogu (port <c>ShippedSealTests</c> z HU): każda operacja się parsuje,
    /// każdy dodany komponent istnieje, każda pieczęć pasuje do jakiegoś skilla, a kombinacje, na których stoi projekt
    /// (kometa, przekaźnik, przemiany), wychodzą tak, jak zamierzono.
    /// </summary>
    public class HuSealTests
    {
        const string Orb = "skill.mage.infernal_orb";

        static SealSet Shipped(string skillId, params string[] seals)
        {
            var set = new SealSet(HuCatalog.Skill(skillId)) { Level = () => 3 };
            foreach (var id in seals)
            {
                var seal = HuCatalog.Seal(id);
                Assert.IsNotNull(seal, id);
                Assert.IsTrue(set.TryInsert(seal, out _), $"{id} nie wchodzi do {skillId}: {set.Fit(seal)}");
            }
            return set;
        }

        static SealSet Fresh(HuSkillData skill) => new SealSet(skill) { Level = () => 3 };

        [Test]
        public void Catalog_HasAllSkillsAndSeals_AndEveryOpParses()
        {
            // 61 skilli i 186 pieczęci z HU + skille Turris na komponentach HU (Ściana ognia z dwiema formami, pieczęć Grzmot).
            Assert.AreEqual(61 + 1, HuCatalog.Skills.Count);
            Assert.AreEqual(186 + 3, HuCatalog.Seals.Count);
            CollectionAssert.IsEmpty(HuCatalog.Errors, string.Join("\n", HuCatalog.Errors));
            Assert.IsTrue(HuCatalog.Seals.All(s => !string.IsNullOrEmpty(s.name) && s.name != s.id), "każda pieczęć ma polską nazwę");
        }

        [Test]
        public void EverySeal_FitsSomeSkill_AndAddsOnlyKnownComponents()
        {
            foreach (var seal in HuCatalog.Seals)
            {
                foreach (var op in seal.ops.Where(o => o.Kind == SealOpKind.AddComponent || o.Kind == SealOpKind.ReplaceComponent))
                {
                    string added = op.Kind == SealOpKind.AddComponent ? op.Key : op.Other;
                    Assert.IsTrue(HuSkillCaster.IsKnown(added), $"{seal.id} dodaje nieznany komponent {added}");
                }
                if (seal.kind == SealKind.Synergy)
                {
                    Assert.IsFalse(seal.shop, $"{seal.id}: zespolenie nie jest w sprzedaży");
                    foreach (var id in seal.requiresSeals) Assert.IsNotNull(HuCatalog.Seal(id), $"{seal.id} → {id}");
                    continue;
                }
                if (seal.kind == SealKind.Bond)
                {
                    Assert.IsFalse(seal.shop || seal.lootOnly, $"{seal.id}: więź nie jest w sprzedaży ani w łupach");
                    continue;
                }
                if (seal.lootOnly) Assert.IsFalse(seal.shop, $"{seal.id}: demoniczna nie jest w sprzedaży");
                Assert.IsTrue(HuCatalog.Skills.Values.Any(s => Fresh(s).Fit(seal) == SealFit.Fits), $"{seal.id} nie pasuje do żadnego skilla");
            }
            foreach (var skill in HuCatalog.Skills.Values)
                foreach (var c in skill.components)
                    Assert.IsTrue(HuSkillCaster.IsKnown(c), $"{skill.id}: nieznany komponent {c}");
        }

        [Test]
        public void OrbForms_ChangeHowItIsDelivered()
        {
            var plain = Shipped(Orb).Shape;
            Assert.IsTrue(plain.HasComponent("Projectile"));
            Assert.AreEqual(HuTargeting.Direction, plain.Targeting);

            var comet = Shipped(Orb, "seal.form.orb.comet").Shape;
            Assert.IsTrue(comet.HasComponent("Fall"));
            Assert.IsFalse(comet.HasComponent("Projectile"));
            Assert.AreEqual(HuTargeting.Point, comet.Targeting);

            Assert.AreEqual(3, Shipped(Orb, "seal.form.orb.mine").Shape.MaxCharges);
            Assert.AreEqual(5.0, Shipped(Orb, "seal.form.orb.swarm").Shape.Parameters.Get("count"), 1e-6);
            Assert.IsTrue(Shipped(Orb, "seal.form.orb.ember").Shape.Parameters.On("burst_at_end"));
        }

        [Test]
        public void ProjectileSeal_SleepsInComet_WakesInSwarm()
        {
            var orb = Shipped(Orb, "seal.split");
            Assert.AreEqual(3.0, orb.Shape.Parameters.Get("count"), 1e-6);
            orb.TryInsert(HuCatalog.Seal("seal.form.orb.comet"), out _);
            CollectionAssert.Contains(orb.Shape.Dormant, HuCatalog.Seal("seal.split"));
            orb.TryInsert(HuCatalog.Seal("seal.form.orb.swarm"), out _);
            CollectionAssert.IsEmpty(orb.Shape.Dormant);
            Assert.AreEqual(7.0, orb.Shape.Parameters.Get("count"), 1e-6);
        }

        [Test]
        public void GrimoireNovas_TakeTheRelay_AimedSkillDoesNot()
        {
            foreach (var nova in new[] { "skill.nova.water", "skill.nova.earth", "skill.nova.air" })
                Assert.IsTrue(Shipped(nova, "seal.relay").Shape.Parameters.On("relay"), nova);
            Assert.AreEqual(SealFit.Unsupported, Shipped(Orb).Fit(HuCatalog.Seal("seal.relay")));
        }

        [Test]
        public void Synergies_WakeOnShippedPairs()
        {
            Assert.IsTrue(Shipped(Orb, "seal.form.orb.comet", "seal.gravity").Shape.IsActive("seal.syn.black_star"));
            var mines = Shipped(Orb, "seal.form.orb.mine", "seal.charges").Shape;
            Assert.IsTrue(mines.IsActive("seal.syn.minefield"));
            Assert.AreEqual(5, mines.MaxCharges);
            var marsh = Shipped(Orb, "seal.scorched", "seal.element.water").Shape;
            Assert.IsTrue(marsh.IsActive("seal.syn.marsh"));
            Assert.AreEqual(HuElement.Water, marsh.Element);
            Assert.IsTrue(marsh.Parameters.Effects.Any(e => e.Key == "Slow"));
        }

        [Test]
        public void EveryForm_WorksInItsSkill_AndChangesIt()
        {
            foreach (var form in HuCatalog.Seals.Where(s => s.IsForm))
            {
                var skill = HuCatalog.Skill(form.skillId);
                Assert.IsNotNull(skill, $"{form.id} wskazuje brakujący skill");
                var reshaped = Shipped(form.skillId, form.id).Shape;
                CollectionAssert.Contains(reshaped.Active, form);
                var plain = HuSkillShape.Plain(skill, 3);
                bool changed = !plain.Components.SequenceEqual(reshaped.Components) || plain.Targeting != reshaped.Targeting || plain.Element != reshaped.Element
                    || !plain.Parameters.Values.OrderBy(p => p.Key).SequenceEqual(reshaped.Parameters.Values.OrderBy(p => p.Key));
                Assert.IsTrue(changed, $"{form.id} nic nie zmienia w {form.skillId}");
            }
        }

        [Test]
        public void EverySynergy_CanWakeInSomeSkill()
        {
            foreach (var syn in HuCatalog.Seals.Where(s => s.kind == SealKind.Synergy))
            {
                var parts = syn.requiresSeals.Select(HuCatalog.Seal).OrderBy(s => s.IsForm ? 0 : 1).ToArray();
                bool woke = HuCatalog.Skills.Values.Any(skill =>
                {
                    var set = Fresh(skill);
                    return parts.All(p => set.TryInsert(p, out _)) && set.Shape.IsActive(syn.id);
                });
                Assert.IsTrue(woke, $"{syn.id} nigdy się nie budzi");
            }
        }

        static IEnumerable<SealSet> Meeting(string condition)
        {
            foreach (var skill in HuCatalog.Skills.Values)
            {
                var forms = new HuSeal[] { null }.Concat(HuCatalog.Seals.Where(s => s.IsForm && s.skillId == skill.id));
                foreach (var form in forms)
                {
                    var set = Fresh(skill);
                    if (form != null && !set.TryInsert(form, out _)) continue;
                    bool meets = condition[0] == '#' || condition[0] == '@' ? HuSeal.BondConditionHolds(condition, set.Unbonded) : set.TryInsert(HuCatalog.Seal(condition), out _);
                    if (meets) { yield return set; break; }
                }
            }
        }

        [Test]
        public void EveryBond_CanWakeBetweenTwoSkills()
        {
            foreach (var bond in HuCatalog.Seals.Where(s => s.kind == SealKind.Bond))
            {
                Assert.AreEqual(2, bond.requiresSeals.Length, bond.id);
                var firsts = Meeting(bond.requiresSeals[0]).ToList();
                var seconds = Meeting(bond.requiresSeals[1]).ToList();
                Assert.IsTrue(firsts.Any(f => seconds.Any(s => s.data.id != f.data.id)), $"{bond.id}: żadne dwa różne skille nie spełniają warunków");
                Assert.IsTrue(bond.ops.Count > 0 && bond.partnerOps.Count > 0, $"{bond.id} musi zmieniać oba skille");
            }
        }

        [Test]
        public void DemonicSeals_ExistAndEachFitsSomeSkill()
        {
            var demonic = HuCatalog.Seals.Where(s => s.lootOnly).ToList();
            Assert.GreaterOrEqual(demonic.Count, 8);
            foreach (var s in demonic) Assert.IsTrue(HuCatalog.Skills.Values.Any(k => Fresh(k).Fit(s) == SealFit.Fits), s.id);
        }

        [Test]
        public void Undying_TakesNoTrigger_AndNothingThatBringsItBackSooner()
        {
            var undying = new SealSet(HuCatalog.Skill("skill.sub.undying")) { Level = () => 1 };
            foreach (var id in new[] { "seal.trigger.dodge", "seal.trigger.hurt", "seal.trigger.kill", "seal.trigger.defend", "seal.demon.chernobog_eye", "seal.charges", "seal.haste", "seal.demon.infinity" })
                Assert.AreNotEqual(SealFit.Fits, undying.Fit(HuCatalog.Seal(id)), id);
        }

        [Test]
        public void EverySupport_ReshapesEverySkillItFits_WithoutBreakingIt()
        {
            foreach (var skill in HuCatalog.Skills.Values)
                foreach (var seal in HuCatalog.Seals.Where(s => s.kind == SealKind.Support))
                {
                    var set = Fresh(skill);
                    if (!set.TryInsert(seal, out _)) continue;
                    Assert.Greater(set.Shape.Cooldown, 0.0, $"{seal.id} zostawia {skill.id} bez odnowienia");
                    Assert.GreaterOrEqual(set.Shape.MaxCharges, 1);
                    Assert.IsNotEmpty(set.Shape.Components);
                }
        }

        [Test]
        public void SupportDamageFactors_AddUp_AndAreCapped()
        {
            // Pakt z diabłem (×1,8) i Ofiara krwi (×1,4) razem: +120%, nie ×2,52 – suma, nie iloczyn.
            var skill = HuCatalog.Skill(Orb);
            var plain = HuSkillShape.Plain(skill, 3).Parameters.Damage(HuElement.Fire);
            var set = Shipped(Orb, "seal.demon.devil_pact", "seal.blood");
            Assert.AreEqual(plain * 2.2, set.Shape.Parameters.Damage(HuElement.Fire), 1e-6);
            Assert.AreEqual(0.17, set.Shape.Parameters.Get(HuParams.LifeCost), 1e-6, "koszty w zdrowiu się sumują");
        }

        [Test]
        public void Bonds_HoldBetweenSkillsInHand()
        {
            var fire = Fresh(HuCatalog.Skill(Orb));
            var water = Fresh(HuCatalog.Skill("skill.mage.frost_shard"));
            var matches = HuBondRules.Holding(HuCatalog.Bonds, new[] { fire.Unbonded, water.Unbonded });
            Assert.IsTrue(matches.Any(m => m.bond.id == "seal.bond.steam"), "Para: ogień + woda");
        }

        [Test]
        public void Thunder_AddsKnockdown_AndAirSignatureKnocksDown()
        {
            var orb = Shipped(Orb, "seal.turris.thunder");
            Assert.AreEqual(0.7, orb.Shape.Parameters.Get("down_seconds"), 1e-6);
            Assert.AreEqual(Fresh(HuCatalog.Skill(Orb)).Shape.Parameters.Damage(HuElement.Fire) * 0.85, orb.Shape.Parameters.Damage(HuElement.Fire), 1e-6);
            Assert.AreNotEqual(SealFit.Fits, Fresh(HuCatalog.Skill("skill.aura.purity")).Fit(HuCatalog.Seal("seal.turris.thunder")), "aura nie powala");
            Assert.AreEqual(0.5, Fresh(HuCatalog.Skill("skill.sub.tempest")).Shape.Parameters.Get("down_seconds"), 1e-6);
            Assert.AreEqual(0.6, Fresh(HuCatalog.Skill("skill.nova.air")).Shape.Parameters.Get("down_seconds"), 1e-6);
        }
    }
}
