using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Bot Wojownika (class_knight): miecz z tarczą albo miecz dwuręczny, Furia, szarża barkiem,
    /// skill klasy Okrzyk bojowy (odrzut, zachwianie, redukcja obrażeń, furia), skill startowy Rozpłatanie albo Cięcie wichru.
    /// </summary>
    public class WarriorBot : HeroBot
    {
        public const string ClassId = "class_knight";
        public const int SignatureSlot = 3;

        // Tester: 4 kombinacje (2 zestawy × 2 skille) po kolei; gracz: najpierw próbuje każdej, potem trzyma się najlepszej.
        public override void PreparePlan(LoadoutPlan plan, int runIndex, IReadOnlyList<BotRunRecord> history)
        {
            var cls = plan.classDef;
            int loadouts = Mathf.Max(1, cls.loadouts.Count), skills = Mathf.Max(1, cls.startingSkillChoices.Count);
            int combos = loadouts * skills;
            int pick;
            if (P.persona == BotPersona.Tester || runIndex < combos) pick = runIndex % combos;
            else
            {
                // Epsilon-zachłannie: średnie piętro (zwycięstwo liczone jako +2) danej kombinacji.
                var score = new float[combos];
                var n = new int[combos];
                foreach (var r in history)
                {
                    int c = ComboOf(cls, r);
                    if (c < 0) continue;
                    score[c] += r.floorReached + (r.Victory ? 2f : 0f);
                    n[c]++;
                }
                pick = Enumerable.Range(0, combos).OrderByDescending(c => n[c] > 0 ? score[c] / n[c] : 99f).First();
                if (rng.NextDouble() < 0.15) pick = rng.Next(combos);
            }
            Root.Meta.SelectLoadout(plan, pick / skills);
            if (cls.startingSkillChoices.Count > 0) Root.Meta.SelectStartingSkill(plan, cls.startingSkillChoices[pick % skills]);
        }

        static int ComboOf(ClassDefinition cls, BotRunRecord r)
        {
            int l = cls.loadouts.FindIndex(x => LoadoutName(x) == r.loadout);
            int s = cls.startingSkillChoices.FindIndex(x => x.displayName == r.startSkill);
            if (l < 0) return -1;
            return l * Mathf.Max(1, cls.startingSkillChoices.Count) + Mathf.Max(0, s);
        }

        // Souls: jakość (Siła/Wytrzymałość); tester rotuje wszystkie cztery, by sprawdzić miękkie wymagania i Zręczność.
        protected override AttributeType NextAttribute(int i)
        {
            if (P.persona == BotPersona.Tester)
            {
                var all = new[] { AttributeType.Strength, AttributeType.Toughness, AttributeType.Dexterity, AttributeType.Intelligence };
                return all[i % all.Length];
            }
            return i % 3 == 2 ? AttributeType.Strength : (i % 3 == 0 ? AttributeType.Toughness : AttributeType.Strength);
        }

        protected override bool TryUseSkills()
        {
            if (rng.NextDouble() > P.skillEagerness && P.persona != BotPersona.Tester) return false;
            float d = target != null ? FlatDist(target) : 99f;
            int close = EnemiesWithin(3.6f);
            float hp = Pc.Health.Fraction;

            // Okrzyk bojowy: otoczony, w tarapatach albo (tester) przy każdej okazji w zwarciu.
            var sig = Pc.Skill(SignatureSlot);
            if (sig != null)
            {
                bool want = close >= 3 || (close >= 1 && hp < 0.5f) || (P.persona == BotPersona.Tester && close >= 1)
                            || (close >= 2 && !Pc.Raging && Pc.Fury >= 50f);
                if (want && UseSkill(SignatureSlot)) return true;
            }

            for (int slot = 0; slot < RunState.SkillSlotCount; slot++)
            {
                var sk = Pc.Skill(slot);
                if (sk == null || !Pc.SkillUsable(slot)) continue;
                if (WantsSkill(sk.Def, d, close, hp) && UseSkill(slot)) return true;
            }
            return false;
        }

        protected override ActionType ChooseAttack(EnemyBrain e)
        {
            // Dwuręczny: mocny cios łamie postawę – gracz souls chętnie otwiera nim serię na odsłoniętym celu.
            if (P.persona == BotPersona.SoulsSlasher && Pc.Build.mainHand != null && Pc.Build.mainHand.definition.twoHanded &&
                e.CurrentState == EnemyBrain.State.Recoil && Pc.Stamina.Fraction > 0.6f)
                return ActionType.HeavyAttack;
            return base.ChooseAttack(e);
        }
    }
}
