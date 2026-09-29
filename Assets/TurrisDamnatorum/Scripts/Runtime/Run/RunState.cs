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
        public const int SkillSlotCount = 3;
        /// <summary>Kolekcja umiejętności podejścia (odblokowane na stałe + zdobyte w tym podejściu).</summary>
        public readonly List<SpellInstance> knownSpells = new List<SpellInstance>();
        /// <summary>Trzy sloty umiejętności – indeks = przycisk (1/2/3). null = pusty slot.</summary>
        public readonly SpellInstance[] skillSlots = new SpellInstance[SkillSlotCount];
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

        /// <summary>Dodaje umiejętność do kolekcji; trafia do pierwszego wolnego slotu, a przy zajętych czeka w kolekcji.</summary>
        public SpellInstance LearnSpell(SpellDefinition def, bool permanent = false)
        {
            var known = FindSpell(def);
            if (known != null) { known.permanent |= permanent; return known; }
            var inst = new SpellInstance(def) { permanent = permanent };
            knownSpells.Add(inst);
            int free = System.Array.IndexOf(skillSlots, null);
            if (free >= 0) skillSlots[free] = inst;
            return inst;
        }

        public int SlotOf(SpellInstance spell) => spell == null ? -1 : System.Array.IndexOf(skillSlots, spell);

        /// <summary>Wkłada umiejętność do slotu. Jeśli była w innym slocie – zamienia je miejscami.</summary>
        public void AssignSlot(SpellInstance spell, int slot)
        {
            if (slot < 0 || slot >= SkillSlotCount || spell == null || !knownSpells.Contains(spell)) return;
            int from = SlotOf(spell);
            if (from == slot) return;
            if (from >= 0) skillSlots[from] = skillSlots[slot];
            skillSlots[slot] = spell;
        }

        public void ClearSlot(int slot)
        {
            if (slot >= 0 && slot < SkillSlotCount) skillSlots[slot] = null;
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
            if (knownSpells.Any(s => s.definition.IsSpell)) t |= BuildTag.Magic;
            if (snap.guardIsShield) t |= BuildTag.Shield;
            if (snap.guard != null) t |= BuildTag.Guard;
            if (snap.parry != null) t |= BuildTag.Parry;
            if (snap.loadRatio > 0.6f) t |= BuildTag.Heavy; else t |= BuildTag.Agile;
            return t;
        }
    }
}
