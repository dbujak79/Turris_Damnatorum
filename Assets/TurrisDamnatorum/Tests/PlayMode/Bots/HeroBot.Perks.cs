using System.Linq;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Perki specjalizacyjne (natywne, docs/RAPORT_PERKI_SPECJALIZACYJNE.md) w decyzjach bota:
    /// sklep – ocena tylko wtedy, gdy build perk wykorzysta (Sfora dopiero przy dwóch rodzajach sług, Manifestacja przy strefie czaru);
    /// ekwipunek – działający perk ma wartość w ocenie buildu, więc tarcza nie wypiera Pojedynkowicza dla drobnej obrony;
    /// walka – cel z otwartym wyłomem ma pierwszeństwo i dostaje mocny cios; Sfora – najpierw przywołanie brakującego rodzaju.
    /// </summary>
    public abstract partial class HeroBot
    {
        /// <summary>Wartość działającego natywnego perka w ocenie buildu (ten sam rząd co moc unikatu).</summary>
        public const float NativePerkValue = 30f;

        /// <summary>Ocena natywnego perka w sklepie; −1 – to nie natywny perk (ocenia zwykła ścieżka).</summary>
        protected float NativePerkScore(BoonDefinition b)
        {
            if (!NativePerkCatalog.IsNative(b)) return -1f;
            var run = Root.Run;
            if (run.BoonStacks(b) > 0 || !NativePerkCatalog.Usable(b.id, run)) return 0f; // jeden poziom; uśpiony nic nie da
            switch (b.id)
            {
                case NativePerkCatalog.Duelist: return 3f;        // stałe tempo ciosów – główne źródło obrażeń bota wręcz
                case NativePerkCatalog.Breach: return IsMageBot ? 2f : 2.8f; // bot wręcz łamie postawę i ripostuje
                case NativePerkCatalog.Pack: return SummonKindsInSlots() >= 2 ? 3f : 1f; // jeden rodzaj – premia 0%
                case NativePerkCatalog.Manifestation: return 2.8f; // te same obrażenia stref w połowie czasu
                case NativePerkCatalog.OpenWound: return 2.8f;    // premia do ciosów przy źródle krwawienia w buildzie
                case NativePerkCatalog.FarShot: return 2.8f;      // bot strzela z dystansu
                case NativePerkCatalog.LongBreath: return 2.5f;   // bot po uniku wraca do ciosu; słabszy cios w plecy
                case NativePerkCatalog.LightStep: return 2.5f;    // oszczędność wytrzymałości
            }
            return 1f;
        }

        bool IsMageBot => this is MageBot;

        /// <summary>Różne rodzaje sług ze skilli w slotach (przywołania HU z pieczęci liczą się jako jeden dodatkowy rodzaj).</summary>
        int SummonKindsInSlots()
        {
            int mask = 0, hu = 0;
            foreach (var s in Root.Run.skillSlots)
            {
                if (s?.definition == null) continue;
                if (s.definition.kind == SpellKind.Summon) mask |= 1 << (int)s.definition.servant;
                else if (s.seals != null && s.seals.Shape.HasComponent("Summon")) hu = 1;
            }
            int k = hu;
            for (int i = 0; i < 3; i++) if ((mask & (1 << i)) != 0) k++;
            return k;
        }

        /// <summary>Wartość posiadanych, działających natywnych perków przy obecnym wyposażeniu (składnik GearScore).</summary>
        protected static float NativePerkGearScore(RunState run)
        {
            float v = 0f;
            foreach (var b in run.boons)
                if (b.definition != null && b.stacks > 0 && NativePerkCatalog.IsNative(b.definition) && NativePerkCatalog.Usable(b.definition.id, run))
                    v += NativePerkValue;
            return v;
        }

        /// <summary>Sfora: przywołanie, którego rodzaju jeszcze nie ma, gdy perk działa (null – bez preferencji).</summary>
        protected bool PackWantsOtherKind(SpellDefinition def)
        {
            if (def == null || def.kind != SpellKind.Summon || !Pc.PerkActive(NativePerkCatalog.Pack)) return false;
            bool have = Servant.Active.Any(s => s != null && s.Master == Pc && s.Kind == def.servant);
            if (!have) return false;
            // Ten rodzaj już stoi: czekaj, jeśli inny gotowy slot przywołuje brakujący rodzaj.
            for (int slot = 0; slot < RunState.SkillSlotCount; slot++)
            {
                var sk = Pc.Skill(slot);
                if (sk == null || sk.Def.kind != SpellKind.Summon || sk.Def.servant == def.servant || !Pc.SkillUsable(slot)) continue;
                if (!Servant.Active.Any(s => s != null && s.Master == Pc && s.Kind == sk.Def.servant)) return true;
            }
            return false;
        }

        /// <summary>Wyłom: najbliższy wróg z otwartym oknem w zasięgu dojścia zastępuje bieżący cel.</summary>
        void PreferBreachTarget()
        {
            if (Pc.BreachWindows == 0) return;
            EnemyBrain best = null; float bd = 6f;
            foreach (var e in alive) if (Pc.HasBreach(e) && FlatDist(e) < bd) { bd = FlatDist(e); best = e; }
            if (best != null) target = best;
        }
    }
}
