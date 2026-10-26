using System;
using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Pieczęcie jednego skilla HU w podejściu (port części <c>SkillInstance</c> z HU): forma, wsparcia w gniazdach,
    /// zespolenia i więzi oraz przerobiony kształt. Definicja skilla nigdy się nie zmienia.
    ///
    /// Gniazd jest tyle, ile poziomów skilla (I–III); skill klasy (jeden poziom) ma dwa.
    /// </summary>
    public sealed class SealSet
    {
        public const int MaxSockets = 3;

        public readonly HuSkillData data;
        readonly List<HuSeal> supports = new List<HuSeal>(MaxSockets);
        HuSeal form;
        HuSkillShape shape, unbonded;
        int shapeLevel = -1;
        IReadOnlyList<KeyValuePair<HuSeal, bool>> bonds = Array.Empty<KeyValuePair<HuSeal, bool>>();

        /// <summary>Poziom HU (1–3) – podawany przez właściciela (SpellInstance: poziom Turris + 1).</summary>
        public Func<int> Level = () => 1;
        /// <summary>Poziom pieczęci w podejściu (I, gdy nie podano).</summary>
        public Func<HuSeal, int> Rank;
        /// <summary>Skill klasy: dwa gniazda, nie rośnie poziomem.</summary>
        public bool signature;

        public SealSet(HuSkillData data) { this.data = data; }

        public HuSeal Form => form;
        public IReadOnlyList<HuSeal> Supports => supports;
        public IEnumerable<HuSeal> All { get { if (form != null) yield return form; foreach (var s in supports) yield return s; } }
        public int Count => supports.Count + (form != null ? 1 : 0);
        public IReadOnlyList<KeyValuePair<HuSeal, bool>> BondList => bonds;

        public int Sockets => Math.Min(MaxSockets, signature || data.MaxLevel == 1 ? 2 : Level());

        /// <summary>Skill po wszystkich pieczęciach i więziach, na bieżącym poziomie.</summary>
        public HuSkillShape Shape { get { Sync(); return shape ?? (shape = HuSkillShape.Compose(data, Level(), form, supports, HuCatalog.Synergies.ToList(), bonds, Rank)); } }
        /// <summary>Skill tylko z własnymi pieczęciami – na nim ocenia się więzi.</summary>
        public HuSkillShape Unbonded { get { Sync(); return unbonded ?? (unbonded = HuSkillShape.Compose(data, Level(), form, supports, HuCatalog.Synergies.ToList(), null, Rank)); } }

        void Sync()
        {
            int lv = Level();
            if (lv != shapeLevel) { shapeLevel = lv; shape = null; unbonded = null; }
        }

        /// <summary>Zmiana poziomu pieczęci albo katalogu – kształt budowany od nowa.</summary>
        public void Invalidate() { shape = null; unbonded = null; }

        public void SetBonds(IReadOnlyList<KeyValuePair<HuSeal, bool>> list)
        {
            list = list ?? Array.Empty<KeyValuePair<HuSeal, bool>>();
            if (list.Count == bonds.Count && list.Zip(bonds, (a, b) => a.Key == b.Key && a.Value == b.Value).All(x => x)) return;
            bonds = list;
            shape = null;
        }

        /// <summary>Czy pieczęć dałoby się teraz włożyć, a jeśli nie – dlaczego.</summary>
        public SealFit Fit(HuSeal seal)
        {
            if (seal == null || !seal.IsItem) return SealFit.NotSettable;
            if (seal.IsForm)
            {
                if (seal.skillId != data.id) return SealFit.OtherSkill;
                return form == seal ? SealFit.Duplicate : HuSkillShape.Plain(data, Level()).Supports(seal) ? SealFit.Fits : SealFit.Unsupported;
            }
            if (supports.Contains(seal)) return SealFit.Duplicate;
            if (seal.group.Length > 0 && supports.Exists(o => o.group == seal.group)) return SealFit.GroupTaken;
            if (supports.Count >= Sockets) return SealFit.NoSocket;
            return Shape.Supports(seal) ? SealFit.Fits : SealFit.Unsupported;
        }

        /// <summary>Wkłada pieczęć. Forma zajmuje miejsce formy i oddaje wypartą; wsparcie bierze następne gniazdo.</summary>
        public bool TryInsert(HuSeal seal, out HuSeal displaced)
        {
            displaced = null;
            if (Fit(seal) != SealFit.Fits) return false;
            if (seal.IsForm) { displaced = form; form = seal; }
            else supports.Add(seal);
            Invalidate();
            return true;
        }

        public bool Remove(HuSeal seal)
        {
            bool removed = form == seal ? (form = null) == null : supports.Remove(seal);
            if (removed) Invalidate();
            return removed;
        }

        public List<HuSeal> RemoveAll()
        {
            var all = All.ToList();
            form = null; supports.Clear(); Invalidate();
            return all;
        }
    }

    /// <summary>
    /// Więzi między skillami w ręce (port <c>BondRules</c> z HU), oceniane na kształtach bez więzi.
    /// Więzi nie napędzają się nawzajem.
    /// </summary>
    public static class HuBondRules
    {
        public struct Match { public HuSeal bond; public int first, second; }

        /// <summary>Każda więź, która zachodzi między kształtami, raz, z pierwszym dopasowaniem w kolejności listy.</summary>
        public static List<Match> Holding(IEnumerable<HuSeal> all, IReadOnlyList<HuSkillShape> shapes)
        {
            var matches = new List<Match>();
            foreach (var bond in all)
            {
                if (bond.kind != SealKind.Bond || bond.requiresSeals.Length != 2) continue;
                for (int first = 0; first < shapes.Count; first++)
                {
                    if (!HuSeal.BondConditionHolds(bond.requiresSeals[0], shapes[first])) continue;
                    int second = -1;
                    for (int other = 0; other < shapes.Count; other++)
                        if (other != first && HuSeal.BondConditionHolds(bond.requiresSeals[1], shapes[other])) { second = other; break; }
                    if (second >= 0) { matches.Add(new Match { bond = bond, first = first, second = second }); break; }
                }
            }
            return matches;
        }
    }
}
