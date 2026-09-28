using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Stan jednego podejścia. Wszystko tutaj przepada po śmierci
    /// (poza popiołem, który jest od razu dopisywany do profilu przy ukończeniu piętra).
    /// </summary>
    public class RunState
    {
        public ClassDefinition startingClass;
        public DifficultyDefinition difficulty;
        public AttributeBlock attributes;
        public int seed;

        public int floorIndex;          // 0-based, aktualne piętro
        public int floorsCleared;
        public int souls;               // waluta podejścia
        public int ashEarned;           // popiół zdobyty w tym podejściu (już zapisany w profilu)
        public int kills;

        public readonly EquipmentSet equipment = new EquipmentSet();
        public readonly List<ItemInstance> inventory = new List<ItemInstance>();
        public readonly List<SpellInstance> knownSpells = new List<SpellInstance>();
        public readonly List<SpellInstance> attunedSpells = new List<SpellInstance>();
        public readonly List<BoonStack> boons = new List<BoonStack>();

        public int healthFlasks, manaFlasks;
        public int bonusHealthFlasks;   // zakupione w kapliczce

        /// <summary>Życie/mana przenoszone między piętrami (ułamek), gdy trudność nie odnawia ich po piętrze.</summary>
        public float healthFraction = 1f, manaFraction = 1f;

        public bool IsFinished;
        public bool Victory;

        public int BoonStacks(BoonDefinition def) => boons.FirstOrDefault(b => b.definition == def)?.stacks ?? 0;

        public void AddBoon(BoonDefinition def)
        {
            var s = boons.FirstOrDefault(b => b.definition == def);
            if (s == null) boons.Add(new BoonStack { definition = def, stacks = 1 });
            else if (s.stacks < def.maxStacks) s.stacks++;
        }

        public SpellInstance FindSpell(SpellDefinition def) => knownSpells.FirstOrDefault(s => s.definition == def);

        public void LearnSpell(SpellDefinition def, int maxAttuned)
        {
            if (FindSpell(def) != null) return;
            var inst = new SpellInstance(def);
            knownSpells.Add(inst);
            if (attunedSpells.Count < maxAttuned) attunedSpells.Add(inst);
        }

        public bool ToggleAttune(SpellInstance spell, int maxAttuned)
        {
            if (attunedSpells.Contains(spell)) { attunedSpells.Remove(spell); return true; }
            if (attunedSpells.Count >= maxAttuned) return false;
            attunedSpells.Add(spell);
            return true;
        }

        /// <summary>Zakłada przedmiot z plecaka. Wyparte przedmioty wracają do plecaka.</summary>
        public bool EquipFromInventory(ItemInstance item, EquipSlot slot)
        {
            if (!equipment.CanEquip(item, slot, out _)) return false;
            var displaced = new List<ItemInstance>();
            bool wasInInventory = inventory.Remove(item);
            if (!equipment.Equip(item, slot, displaced))
            {
                if (wasInInventory) inventory.Add(item);
                return false;
            }
            inventory.AddRange(displaced);
            return true;
        }

        public bool UnequipToInventory(EquipSlot slot)
        {
            var item = equipment.Unequip(slot);
            if (item == null) return false;
            inventory.Add(item);
            return true;
        }

        /// <summary>Dodaje przedmiot; zakłada go automatycznie, jeśli odpowiedni slot jest pusty.</summary>
        public void AddItem(ItemInstance item, bool autoEquip)
        {
            if (autoEquip)
            {
                var slot = item.definition.DefaultSlot;
                if (item.definition.kind == ItemKind.Ring && equipment.Get(EquipSlot.Ring1) != null) slot = EquipSlot.Ring2;
                bool offHandBlocked = slot == EquipSlot.OffHand && equipment.Get(EquipSlot.MainHand)?.definition.twoHanded == true;
                if (equipment.Get(slot) == null && !offHandBlocked)
                {
                    var displaced = new List<ItemInstance>();
                    equipment.Equip(item, slot, displaced);
                    inventory.AddRange(displaced);
                    return;
                }
            }
            inventory.Add(item);
        }

        public IEnumerable<StatModifier> AllModifiers()
        {
            foreach (var m in equipment.AllModifiers()) yield return m;
            foreach (var b in boons)
                foreach (var m in b.definition.modifiers)
                    yield return m.Scaled(b.stacks);
        }

        public IEnumerable<PassiveEffect> AllEffects()
        {
            foreach (var e in equipment.AllEffects()) yield return e;
            foreach (var b in boons)
                foreach (var e in b.definition.effects)
                    yield return new PassiveEffect(e.type, e.value * b.stacks);
        }

        public int MaxHealthFlasks
        {
            get
            {
                int n = startingClass.healthFlasks + bonusHealthFlasks + (difficulty != null ? difficulty.flaskChargeDelta : 0);
                foreach (var b in boons) n += b.definition.extraHealthFlasks * b.stacks;
                return System.Math.Max(1, n);
            }
        }

        public int MaxManaFlasks
        {
            get
            {
                int n = startingClass.manaFlasks + (difficulty != null ? difficulty.flaskChargeDelta : 0);
                foreach (var b in boons) n += b.definition.extraManaFlasks * b.stacks;
                return System.Math.Max(0, n);
            }
        }

        public void RefillFlasks()
        {
            healthFlasks = MaxHealthFlasks;
            manaFlasks = MaxManaFlasks;
        }

        /// <summary>Tagi aktualnego buildu – wyliczane z wyposażenia i czarów, nigdy z klasy startowej.</summary>
        public BuildTag CurrentTags(BuildSnapshot snap)
        {
            BuildTag t = BuildTag.Melee;
            if (knownSpells.Count > 0) t |= BuildTag.Magic;
            if (snap.guardIsShield) t |= BuildTag.Shield;
            if (snap.guard != null) t |= BuildTag.Guard;
            if (snap.parry != null) t |= BuildTag.Parry;
            if (snap.loadRatio > 0.6f) t |= BuildTag.Heavy; else t |= BuildTag.Agile;
            return t;
        }
    }
}
