namespace Turris
{
    /// <summary>Tworzy kontroler dla definicji cechy (jeden wariant na postać).</summary>
    public static class ClassFeatureFactory
    {
        public static ClassFeatureController Create(ClassFeatureDefinition def)
        {
            if (def == null) return null;
            switch (def.id)
            {
                case "class_feature.warrior.guard": return new GuardFeature();
                case "class_feature.warrior.arsenal": return new ArsenalFeature();
                case "class_feature.paladin.absolution": return new AbsolutionFeature();
                case "class_feature.paladin.judgement": return new JudgementFeature();
                case "class_feature.berserker.redline": return new RedLineFeature();
                case "class_feature.berserker.fury_spend": return new FurySpendFeature();
                case "class_feature.dual.mismatched_pair": return new MismatchedPairFeature();
                case "class_feature.dual.unbroken_dance": return new UnbrokenDanceFeature();
                case "class_feature.mage.arcane_debt": return new ArcaneDebtFeature();
                case "class_feature.mage.mana_cloak": return new ManaCloakFeature();
                case "class_feature.fire.living_furnace": return new LivingFurnaceFeature();
                case "class_feature.fire.great_fire": return new GreatFireFeature();
                case "class_feature.water.enduring_frost": return new EnduringFrostFeature();
                case "class_feature.water.deep_current": return new DeepCurrentFeature();
                case "class_feature.air.silent_eye": return new SilentEyeFeature();
                case "class_feature.air.colliding_fronts": return new CollidingFrontsFeature();
                case "class_feature.earth.bastion": return new BastionFeature();
                case "class_feature.earth.shattering_shell": return new ShatteringShellFeature();
                case "class_feature.summoner.incarnation": return new IncarnationFeature();
                case "class_feature.summoner.chimera": return new ChimeraFeature();
                case "class_feature.battlemage.weapon_arcana": return new WeaponArcanaFeature();
                case "class_feature.battlemage.two_beats": return new TwoBeatsFeature();
                case "class_feature.rogue.false_witness": return new FalseWitnessFeature();
                case "class_feature.rogue.stolen_trick": return new StolenTrickFeature();
                case "class_feature.archer.hunters_nails": return new HuntersNailsFeature();
                case "class_feature.archer.firing_position": return new FiringPositionFeature();
                case "class_feature.assassin.pinned_shadow": return new PinnedShadowFeature();
                case "class_feature.assassin.open_artery": return new OpenArteryFeature();
                case "class_feature.arcane_archer.spell_arrow": return new SpellArrowFeature();
                case "class_feature.arcane_archer.answering_arrow": return new AnsweringArrowFeature();
            }
            UnityEngine.Debug.LogWarning("Cecha klasowa bez kontrolera: " + def.id);
            return null;
        }
    }
}
