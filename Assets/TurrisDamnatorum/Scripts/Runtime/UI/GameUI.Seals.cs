using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Pieczęcie w interfejsie: menedżer gniazd i sakwy (zakładka ekwipunku), oferty i dar bossa w sklepie dusz,
    /// znaczki pieczęci pod kartami skilli w HUD i przełączniki pieczęci na poligonie.
    /// </summary>
    public partial class GameUI
    {
        /// <summary>Ekwipunek pokazuje zakładkę pieczęci zamiast przedmiotów.</summary>
        public bool ShowSeals;
        SpellInstance sealTarget;
        readonly ScrollState scrollSealsA = new ScrollState(), scrollSealsB = new ScrollState();
        Vector2 rangeSealScroll;

        static readonly Color SealColor = new Color(0.72f, 0.5f, 1f), DemonColor = new Color(0.95f, 0.25f, 0.2f), SynergyColor = new Color(0.55f, 0.95f, 0.75f), BondColor = new Color(1f, 0.82f, 0.4f);

        static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);

        string SealTitle(HuSeal s, RunState run)
        {
            string kind = s.kind == SealKind.Form ? "forma" : s.kind == SealKind.Synergy ? "zespolenie" : s.kind == SealKind.Bond ? "więź" : s.lootOnly ? "demoniczna" : "wsparcie";
            var c = s.lootOnly ? DemonColor : s.kind == SealKind.Synergy ? SynergyColor : s.kind == SealKind.Bond ? BondColor : SealColor;
            int rank = run != null ? run.SealRank(s) : 1;
            return $"<color=#{Hex(c)}><b>{s.name}</b></color>{(rank > 1 ? " " + Names.Roman(rank) : "")} <size=13>({kind})</size>";
        }

        static string FormFor(HuSeal s)
        {
            if (!s.IsForm) return "";
            var skill = HuCatalog.Skill(s.skillId);
            return skill != null ? $" <size=13>– dla: {skill.name}</size>" : "";
        }

        /// <summary>Krótki opis pieczęci w skillu: działa, śpi (wymagań nie spełnia przerobiony skill).</summary>
        string SealLine(HuSeal s, SealSet set, RunState run)
        {
            bool dormant = set.Shape.Dormant.Contains(s);
            return (dormant ? "<color=#777777>" : "") + SealTitle(s, run) + (dormant ? " – uśpiona</color>" : "");
        }

        // ------------------------------------------------------------------ Zakładka pieczęci w ekwipunku

        void DrawSealManager()
        {
            var run = root.Run;
            var pc = root.Player;
            var skills = run.SealOrder().ToList();
            if (sealTarget == null || !skills.Contains(sealTarget)) sealTarget = skills.FirstOrDefault();
            float colW = (W - 100) / 2f;

            GUILayout.BeginArea(new Rect(40, 80, colW, H - 190), box);
            GUILayout.Label("Skille i ich pieczęcie", header);
            GUILayout.Label("<size=14>Forma przerabia jeden skill i nie zajmuje gniazda. Wsparć jest tyle, ile poziomów skilla (skill klasy: dwa). " +
                            "Pieczęć, której przerobiony skill nie obsługuje, śpi – nie przepada.</size>", small);
            BeginScroll(scrollSealsA, H - 330);
            if (skills.Count == 0) GUILayout.Label("Żaden skill nie przyjmuje pieczęci (przyjmują je skille z Hell Unleashed).", small);
            foreach (var s in skills)
            {
                var sk = s;
                var set = sk.seals;
                int slot = run.SlotOf(sk);
                string where = slot >= 0 ? $"[{SkillButton(slot)}]" : sk == run.signatureSkill ? "[klasa]" : "[kolekcja]";
                Tog(sk == sealTarget, $"{where} {sk.Name}  – gniazda {set.Supports.Count}/{set.Sockets}", button, on => { if (on) sealTarget = sk; }, GUILayout.Height(40));
                if (set.Form != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   " + SealLine(set.Form, set, run), label, GUILayout.Width(colW - 160));
                    var f = set.Form;
                    Btn("Wyjmij", button, () => root.RemoveSeal(sk, f), GUILayout.Width(110));
                    GUILayout.EndHorizontal();
                }
                foreach (var sup in set.Supports.ToList())
                {
                    var x = sup;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   " + SealLine(x, set, run), label, GUILayout.Width(colW - 160));
                    Btn("Wyjmij", button, () => root.RemoveSeal(sk, x), GUILayout.Width(110));
                    GUILayout.EndHorizontal();
                }
                foreach (var syn in set.Shape.Active.Where(a => a.kind == SealKind.Synergy))
                    GUILayout.Label("   " + SealTitle(syn, run) + $" <size=13>{syn.desc}</size>", small);
                foreach (var bond in set.Shape.Bonds)
                    GUILayout.Label("   " + SealTitle(bond, run) + $" <size=13>{bond.desc}</size>", small);
                GUILayout.Space(6);
            }
            if (run.activeBonds.Count > 0)
            {
                GUILayout.Label("Więzi w ręce", header);
                foreach (var b in run.activeBonds) GUILayout.Label(SealTitle(b, run) + $"<size=13> – {b.desc}</size>", small);
            }
            EndScroll();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(60 + colW, 80, colW, H - 190), box);
            GUILayout.Label($"Sakwa ({run.sealPouch.Count})", header);
            GUILayout.Label(sealTarget != null ? $"<size=14>Wkładasz do: <b>{sealTarget.Name}</b></size>" : "<size=14>Wybierz skill po lewej.</size>", small);
            BeginScroll(scrollSealsB, H - 330);
            if (run.sealPouch.Count == 0) GUILayout.Label("Pusto. Pieczęcie wypadają z elit, bossowie dają dar, a sklep dusz sprzedaje je na postoju.", small);
            foreach (var seal in run.sealPouch.ToList())
            {
                var s = seal;
                GUILayout.Label(SealTitle(s, run) + FormFor(s), label);
                GUILayout.Label($"<size=14>{s.desc}</size>", small);
                GUILayout.BeginHorizontal();
                var fit = sealTarget != null ? sealTarget.seals.Fit(s) : SealFit.NotSettable;
                GUI.enabled = fit == SealFit.Fits;
                Btn(fit == SealFit.Fits ? "Włóż" : "Włóż – " + FitText(fit), button, () => root.InsertSeal(sealTarget, s), GUILayout.Width(colW * 0.55f));
                GUI.enabled = true;
                Btn($"Sprzedaj ({RunState.SealSalePrice(s)} dusz)", button, () => root.SellSeal(s));
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
            }
            EndScroll();
            GUILayout.EndArea();
        }

        static string FitText(SealFit f)
        {
            switch (f)
            {
                case SealFit.OtherSkill: return "forma innego skilla";
                case SealFit.Unsupported: return "skill tego nie przyjmie";
                case SealFit.GroupTaken: return "wyklucza się z włożoną";
                case SealFit.Duplicate: return "już w skillu";
                case SealFit.NoSocket: return "brak wolnego gniazda";
                default: return "nie da się";
            }
        }

        // ------------------------------------------------------------------ Sklep dusz: pieczęcie i dar bossa

        void DrawSealShopSection()
        {
            var run = root.Run;
            var b = root.config.balance;
            for (int g = 0; g < run.sealGifts.Count; g++)
            {
                int gift = g;
                GUILayout.Label($"<color=#{Hex(BondColor)}><b>Dar bossa</b></color> – weź jedną, pozostałe przepadną", label);
                for (int i = 0; i < run.sealGifts[g].Count; i++)
                {
                    int choice = i;
                    var s = run.sealGifts[g][i];
                    GUILayout.Label(SealTitle(s, run) + FormFor(s) + $"\n<size=14>{s.desc}</size>", small);
                    Btn("Weź", button, () => root.TakeGift(gift, choice), GUILayout.Height(36));
                }
                GUILayout.Space(8);
            }
            GUILayout.Label("<b>Pieczęcie</b> <size=14>(przerabiają skille z Hell Unleashed)</size>", label);
            if (root.SealOffers.Count == 0) GUILayout.Label("<size=14>Brak ofert – żaden skill nie przyjmie teraz pieczęci.</size>", small);
            foreach (var offer in root.SealOffers)
            {
                var o = offer;
                if (o.sold) { GUILayout.Label($"<color=#777777>{o.seal.name} – kupione</color>", small); continue; }
                bool wakes = HuSealEconomy.Wakes(run, o.seal);
                GUILayout.Label(SealTitle(o.seal, run) + FormFor(o.seal) + (o.rankUp ? $" <size=13>→ poziom {Names.Roman(run.SealRank(o.seal) + 1)}</size>" : "")
                                + (wakes ? $"\n<color=#{Hex(SynergyColor)}><size=13>Obudzi zespolenie albo więź</size></color>" : "") + $"\n<size=14>{o.seal.desc}</size>", small);
                GUI.enabled = run.souls >= o.price;
                Btn($"Kup ({o.price} dusz)", button, () => root.BuySealOffer(o), GUILayout.Height(38));
                GUI.enabled = true;
            }
            if (root.SealOffers.Count > 0)
            {
                GUI.enabled = run.souls >= root.SealRerollCost;
                Btn($"Przelosuj pieczęcie ({root.SealRerollCost} dusz)", button, root.RerollSealOffers, GUILayout.Height(36));
                GUI.enabled = true;
            }
            GUILayout.Space(8);
        }

        // ------------------------------------------------------------------ HUD

        /// <summary>Znaczki pieczęci pod kartą skilla: działające jasne, uśpione przygaszone, więzi złote.</summary>
        void DrawSealStamps(SpellRuntime sk, Rect card)
        {
            var set = sk?.instance.seals;
            if (set == null || (set.Count == 0 && set.Shape.Bonds.Count == 0)) return;
            var parts = new List<string>();
            foreach (var s in set.All) parts.Add((set.Shape.Dormant.Contains(s) ? "<color=#666666>" : $"<color=#{Hex(s.lootOnly ? DemonColor : SealColor)}>") + Short(s.name) + "</color>");
            foreach (var s in set.Shape.Active.Where(a => a.kind == SealKind.Synergy)) parts.Add($"<color=#{Hex(SynergyColor)}>{Short(s.name)}</color>");
            foreach (var s in set.Shape.Bonds) parts.Add($"<color=#{Hex(BondColor)}>{Short(s.name)}</color>");
            GUI.Label(new Rect(card.x + 8, card.yMax - 2, card.width, 22), "<size=13>" + string.Join(" · ", parts) + "</size>", rich);
        }

        static string Short(string name) => name.Length <= 12 ? name : name.Substring(0, 11) + "…";

        // ------------------------------------------------------------------ Poligon

        /// <summary>Poligon: przełączniki wszystkich pieczęci, które przyjmuje wybrany skill – za darmo, bez sakwy.</summary>
        void DrawRangeSeals()
        {
            var range = root.Range;
            var sk = range?.Equipped;
            if (sk == null || sk.instance.seals == null) return;
            var set = sk.instance.seals;
            GUILayout.BeginArea(new Rect(20, 180, 420, 560), box);
            GUILayout.Label("Pieczęcie (poligon)", header);
            GUILayout.Label($"<size=13>{sk.instance.Name}: gniazda {set.Supports.Count}/{set.Sockets}. Ponowne kliknięcie wyjmuje.</size>", small);
            rangeSealScroll = GUILayout.BeginScrollView(rangeSealScroll);
            foreach (var seal in HuCatalog.Seals.Where(s => s.IsItem && (s.IsForm ? s.skillId == set.data.id : true)))
            {
                var s = seal;
                bool inside = set.All.Contains(s);
                if (!inside && set.Fit(s) != SealFit.Fits && !(s.IsForm && set.Fit(s) == SealFit.Fits)) continue;
                string text = (s.lootOnly ? "<color=#ff6655>" : s.IsForm ? "<color=#ffcc66>" : "<color=#c8a8ff>") + s.name + "</color>" + (set.Shape.Dormant.Contains(s) ? " (uśpiona)" : "");
                if (GUILayout.Button(text, inside ? SelectedStyle(small) : small))
                    nav.Enqueue(() =>
                    {
                        if (inside) set.Remove(s); else set.TryInsert(s, out _);
                        root.Run.RefreshBonds();
                        root.Player.RefreshBuild();
                    });
            }
            foreach (var syn in set.Shape.Active.Where(a => a.kind == SealKind.Synergy)) GUILayout.Label($"<color=#{Hex(SynergyColor)}>Zespolenie: {syn.name}</color>", small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
