using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Cecha klasowa w stanie podejścia: trwały wybór (jeden wariant na całe podejście) oraz zobowiązania, które
    /// muszą przetrwać piętra i zapis (dług many, drugi zestaw Arsenału). Liczniki chwilowe (Łaska, Żar, groty,
    /// przechwycone pociski…) żyją w kontrolerze postaci i znikają zgodnie z kartą cechy.
    /// </summary>
    public partial class RunState
    {
        /// <summary>Id wybranej cechy klasowej (np. <c>class_feature.berserker.redline</c>); null – brak (klasa spoza katalogu, stare testy).</summary>
        public string classFeatureId;
        /// <summary>Definicja wybranej cechy – tylko gdy należy do klasy postaci (obca nie daje mocy).</summary>
        public ClassFeatureDefinition ClassFeature => ClassFeatureCatalog.Owns(startingClass, classFeatureId) ? ClassFeatureCatalog.Get(classFeatureId) : null;
        public bool HasFeature(string id) => classFeatureId == id && ClassFeature != null;

        /// <summary>Dług arkanów: ujemne saldo many (≥ 0). Przechodzi przez piętra i zapis – zobowiązania nie da się skasować wyjściem.</summary>
        public float manaDebt;

        /// <summary>Arsenał weterana: drugi zestaw (główna i druga ręka) czekający poza rękami. Obciąża postać, nie daje premii.</summary>
        public ItemInstance arsenalMain, arsenalOff;
        /// <summary>Skille z przedmiotów schowanego zestawu – z odnowieniem i pieczęciami (zmiana zestawu nie odnawia ładunków).</summary>
        public readonly List<SpellInstance> parkedItemSkills = new List<SpellInstance>();

        /// <summary>Arkana oręża (Mag bitewny A): okno, w którym broń wręcz jest fokusem; liczy BuildCalculator.</summary>
        public bool weaponArcanaActive;

        public IEnumerable<ItemInstance> ArsenalItems
        {
            get
            {
                if (arsenalMain != null) yield return arsenalMain;
                if (arsenalOff != null) yield return arsenalOff;
            }
        }

        /// <summary>Waga schowanego zestawu (Arsenał: oba zestawy liczą się do obciążenia).</summary>
        public float ArsenalWeight => ArsenalItems.Sum(i => i.definition != null ? i.definition.weight : 0f);

        /// <summary>
        /// Zamienia trzymany zestaw (główna + druga ręka) z drugim zestawem Arsenału – atomowo: egzemplarze tylko się przenoszą,
        /// broń 2H nie zachowuje tarczy ani grymuaru. Skille z przedmiotów schowanego zestawu są parkowane (z odnowieniem
        /// i pieczęciami), a wracające – odtwarzane z parkingu. Zwraca false, gdy drugi zestaw jest pusty.
        /// </summary>
        public bool SwapArsenal()
        {
            if (arsenalMain == null && arsenalOff == null) return false;
            var curMain = equipment.Get(EquipSlot.MainHand);
            var curOff = equipment.Get(EquipSlot.OffHand);
            // Skille z przedmiotów trzymanego zestawu idą na parking razem ze slotem.
            ParkItemSkills(curMain, curOff);
            equipment.Unequip(EquipSlot.MainHand);
            equipment.Unequip(EquipSlot.OffHand);
            var displaced = new List<ItemInstance>();
            var nextMain = arsenalMain; var nextOff = arsenalOff;
            arsenalMain = curMain; arsenalOff = curOff;
            if (nextMain != null && !equipment.Equip(nextMain, EquipSlot.MainHand, displaced)) inventory.Add(nextMain);
            if (nextOff != null && !(nextMain != null && nextMain.definition.twoHanded) && !equipment.Equip(nextOff, EquipSlot.OffHand, displaced)) inventory.Add(nextOff);
            else if (nextOff != null && nextMain != null && nextMain.definition.twoHanded) inventory.Add(nextOff);
            inventory.AddRange(displaced.Where(d => d != arsenalMain && d != arsenalOff));
            UnparkItemSkills();
            SyncGrantedSkills();
            return true;
        }

        void ParkItemSkills(params ItemInstance[] items)
        {
            foreach (var it in items)
            {
                var g = it?.definition?.grantedSkill;
                if (g == null) continue;
                var inst = FindSpell(g);
                if (inst == null || !inst.fromItem) continue;
                inst.parkedSlot = SlotOf(inst);
                if (inst.parkedSlot >= 0) skillSlots[inst.parkedSlot] = null;
                knownSpells.Remove(inst);
                parkedItemSkills.Add(inst);
            }
        }

        void UnparkItemSkills()
        {
            for (int i = parkedItemSkills.Count - 1; i >= 0; i--)
            {
                var inst = parkedItemSkills[i];
                bool held = new[] { EquipSlot.MainHand, EquipSlot.OffHand }.Any(s => equipment.Get(s)?.definition.grantedSkill == inst.definition);
                if (!held || FindSpell(inst.definition) != null) continue;
                parkedItemSkills.RemoveAt(i);
                knownSpells.Add(inst);
                int slot = inst.parkedSlot >= 0 && skillSlots[inst.parkedSlot] == null ? inst.parkedSlot : System.Array.IndexOf(skillSlots, null);
                if (slot >= 0) skillSlots[slot] = inst;
                inst.parkedSlot = -1;
            }
        }

        /// <summary>Przenosi przedmiot z plecaka do drugiego zestawu (broń → główna ręka zestawu II, tarcza/druga ręka → lewa).</summary>
        public bool MoveToArsenal(ItemInstance item)
        {
            if (item == null || !inventory.Contains(item)) return false;
            var d = item.definition;
            if (d.IsWeapon)
            {
                inventory.Remove(item);
                if (arsenalMain != null) inventory.Add(arsenalMain);
                arsenalMain = item;
                if (d.twoHanded && arsenalOff != null) { inventory.Add(arsenalOff); arsenalOff = null; }
                return true;
            }
            if (d.IsShield || d.kind == ItemKind.OffHand)
            {
                if (arsenalMain != null && arsenalMain.definition.twoHanded) return false;
                inventory.Remove(item);
                if (arsenalOff != null) inventory.Add(arsenalOff);
                arsenalOff = item;
                return true;
            }
            return false;
        }

        /// <summary>Zdejmuje przedmiot z drugiego zestawu do plecaka.</summary>
        public bool TakeFromArsenal(ItemInstance item)
        {
            if (item == null) return false;
            if (item == arsenalMain) arsenalMain = null;
            else if (item == arsenalOff) arsenalOff = null;
            else return false;
            inventory.Add(item);
            return true;
        }
    }
}
