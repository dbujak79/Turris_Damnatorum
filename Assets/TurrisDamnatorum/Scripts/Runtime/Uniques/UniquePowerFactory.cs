namespace Turris
{
    /// <summary>Tworzy kontroler mocy dla karty (jeden na właściciela i definicję).</summary>
    public static class UniquePowerFactory
    {
        public static UniquePower Create(UniqueCard card)
        {
            if (card == null) return null;
            switch (card.code)
            {
                case "W01": return new LastPavisePower();
                case "W02": return new TwoOathsPower();
                case "W03": return new BellringerPower();
                case "W04": return new LeftWatchPower();
                case "W05": return new EmptyFrontPower();
                case "P01": return new HungryReliquaryPower();
                case "P02": return new DividedLitanyPower();
                case "P03": return new EmptyMonasteryPower();
                case "P04": return new PenitentBellsPower();
                case "P05": return new LongPenancePower();
                case "B01": return new HungryScarsPower();
                case "B02": return new AngrySpiritsPower();
                case "B03": return new WalkingSlaughterPower();
                case "B04": return new ChainedButcherPower();
                case "B05": return new HeldRagePower();
                case "R01": return new DoubleSecretPower();
                case "R02": return new UnfinishedStepPower();
                case "R03": return new BladeJugglerPower();
                case "R04": return new ReverseWoundsPower();
                case "R05": return new WanderingBladePower();
                case "M01": return new MoneylendersNotePower();
                case "M02": return new SealedSourcePower();
                case "M03": return new SleepingCirclesPower();
                case "M04": return new BoundSpellsPower();
                case "M05": return new LatePresencePower();
                case "F01": return new PyreDevourerPower();
                case "F02": return new AshPuppetsPower();
                case "F03": return new BorrowedSunPower();
                case "F04": return new DeferredWildfirePower();
                case "F05": return new SkyFurnacePower();
                case "H01": return new DrownedKingPower();
                case "H02": return new WanderingRiverPower();
                case "H03": return new SilentGlacierPower();
                case "H04": return new EternalEbbPower();
                case "H05": return new SaltMirrorPower();
                case "A01": return new LastBreathPower();
                case "A02": return new StormSmithPower();
                case "A03": return new WindlessNightPower();
                case "A04": return new EmptyNodesPower();
                case "A05": return new LastGustPower();
                case "E01": return new MovingBastionPower();
                case "E02": return new GraveMortarPower();
                case "E03": return new UndugGravePower();
                case "E04": return new LivingQuarryPower();
                case "E05": return new WoundUnderfootPower();
                case "S01": return new ThirdHungerPower();
                case "S02": return new SixthRibPower();
                case "S03": return new BoneConductorPower();
                case "S04": return new OpenMawsPower();
                case "S05": return new InheritedBeastPower();
                case "T01": return new RunicWrestlerPower();
                case "T02": return new UnfinishedRunePower();
                case "T03": return new RuneEaterPower();
                case "T04": return new ParryingRunePower();
                case "T05": return new BrokenWandPower();
                case "L01": return new FalseKingPower();
                case "L02": return new ProfessionalCheatPower();
                case "L03": return new DudsPower();
                case "L04": return new UnfinishedEscapePower();
                case "L05": return new AbyssSignalPower();
                case "K01": return new BowstringWeaverPower();
                case "K02": return new ExiledShooterPower();
                case "K03": return new GravediggersBowPower();
                case "K04": return new UnfinishedVolleyPower();
                case "K05": return new ReversedSightPower();
                case "Z01": return new OwnFuneralPower();
                case "Z02": return new DeferredVerdictPower();
                case "Z03": return new GraveNeedlePower();
                case "Z04": return new SilentWitnessPower();
                case "Z05": return new CrimsonKnotPower();
                case "Q01": return new SeventhSealPower();
                case "Q02": return new StolenElementsPower();
                case "Q03": return new HereticChoirPower();
                case "Q04": return new UnfinishedIncantationPower();
                case "Q05": return new BetweenWorldsPower();
            }
            UnityEngine.Debug.LogWarning("Unikat bez kontrolera mocy: " + card.id);
            return null;
        }
    }
}
