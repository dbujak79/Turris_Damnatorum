using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Stan jednego podejścia. Wszystko tutaj przepada po śmierci
    /// (poza popiołem, który jest od razu dopisywany do profilu przy ukończeniu piętra).
    /// </summary>
    public partial class RunState
    {
        public ClassDefinition startingClass;
        public ClassDefinition appearance;
        public DifficultyDefinition difficulty;
        public AttributeBlock attributes;
        public int seed;

        public int floorIndex;          // 0-based, aktualne piętro
        public int floorsCleared;
        public int souls;               // waluta podejścia
        public int ashEarned;           // popiół zdobyty w tym podejściu (już zapisany w profilu)
        public int kills;
        /// <summary>Nierozdane punkty cech (1 za każde ukończone piętro, za darmo).</summary>
        public int attributePoints;

        /// <summary>Wydaje punkt na rozwój cechy.</summary>
        public bool SpendAttributePoint(AttributeType a)
        {
            if (attributePoints <= 0) return false;
            attributePoints--;
            attributes.Add(a);
            return true;
        }

        public readonly EquipmentSet equipment = new EquipmentSet();
        public readonly List<ItemInstance> inventory = new List<ItemInstance>();
        public const int SkillSlotCount = 3;
        /// <summary>Kolekcja umiejętności podejścia (odblokowane na stałe + zdobyte w tym podejściu).</summary>
        public readonly List<SpellInstance> knownSpells = new List<SpellInstance>();
        /// <summary>Trzy sloty umiejętności – indeks = przycisk (1/2/3). null = pusty slot.</summary>
        public readonly SpellInstance[] skillSlots = new SpellInstance[SkillSlotCount];
        public readonly List<BoonStack> boons = new List<BoonStack>();
        /// <summary>Skill klasy pod osobnym przyciskiem (nie zajmuje slotu, nie rośnie poziomem).</summary>
        public SpellInstance signatureSkill;
        /// <summary>Premie chwilowe (aury, okrzyk, szał) – ustawia PlayerCombat; liczone jak premie z wyposażenia.</summary>
        public readonly List<StatModifier> tempModifiers = new List<StatModifier>();

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

        /// <summary>
        /// Umiejętności z założonych przedmiotów (grymuar → nova, sztylet do rzucania → rzut nożami): dochodzą do kolekcji,
        /// gdy przedmiot jest założony, i znikają razem z nim. Zwraca true, jeśli coś się zmieniło.
        /// </summary>
        public bool SyncGrantedSkills()
        {
            bool changed = false;
            var granted = new List<SpellDefinition>();
            foreach (var slot in new[] { EquipSlot.MainHand, EquipSlot.OffHand })
            {
                var it = equipment.Get(slot);
                if (it != null && it.definition.grantedSkill != null) granted.Add(it.definition.grantedSkill);
            }
            for (int i = knownSpells.Count - 1; i >= 0; i--)
            {
                var s = knownSpells[i];
                if (!s.fromItem || granted.Contains(s.definition)) continue;
                int slot = SlotOf(s);
                if (slot >= 0) skillSlots[slot] = null;
                ReturnSealsOf(s);
                knownSpells.RemoveAt(i);
                changed = true;
            }
            foreach (var g in granted)
            {
                if (FindSpell(g) != null) continue;
                var inst = LearnSpell(g);
                inst.fromItem = true;
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Dodaje umiejętność do kolekcji; trafia do pierwszego wolnego slotu, a przy zajętych czeka w kolekcji.
        /// <paramref name="level"/> – poziom startowy nowej (np. <see cref="SkillTiers.StartLevel"/>); znana podnosi się najwyżej do niego.
        /// </summary>
        public SpellInstance LearnSpell(SpellDefinition def, bool permanent = false, int level = 0)
        {
            var known = FindSpell(def);
            if (known != null) { known.permanent |= permanent; known.level = System.Math.Max(known.level, System.Math.Min(level, def.maxLevel)); return known; }
            var inst = new SpellInstance(def, System.Math.Max(0, System.Math.Min(level, def.maxLevel))) { permanent = permanent };
            AttachSeals(inst);
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

        /// <summary>
        /// Zakłada przedmiot, nawet jeśli slot jest zajęty (dodatek wybrany świadomie na starcie podejścia).
        /// Wyparte przedmioty trafiają do plecaka. Pierścień idzie do wolnego slotu pierścienia, jeśli jest.
        /// </summary>
        public void AddAndEquip(ItemInstance item)
        {
            var slot = item.definition.DefaultSlot;
            if (item.definition.kind == ItemKind.Ring && equipment.Get(EquipSlot.Ring1) != null && equipment.Get(EquipSlot.Ring2) == null) slot = EquipSlot.Ring2;
            var displaced = new List<ItemInstance>();
            if (equipment.Equip(item, slot, displaced)) inventory.AddRange(displaced);
            else inventory.Add(item);
        }

        public IEnumerable<StatModifier> AllModifiers()
        {
            if (startingClass != null)
                foreach (var m in startingClass.classModifiers) yield return m;
            foreach (var m in tempModifiers) yield return m;
            foreach (var m in equipment.AllModifiers()) yield return m;
            foreach (var b in boons)
            {
                if (!PerkMechanics.Active(b.definition, this)) continue; // uśpiony perk HU (brak pasującego wyposażenia)
                foreach (var m in b.definition.modifiers)
                    yield return m.Scaled(b.stacks);
            }
        }

        public IEnumerable<PassiveEffect> AllEffects()
        {
            foreach (var e in equipment.AllEffects()) yield return e;
            foreach (var b in boons)
            {
                if (!PerkMechanics.Active(b.definition, this)) continue;
                foreach (var e in b.definition.effects)
                    yield return new PassiveEffect(e.type, e.value * b.stacks);
            }
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
            if (snap.weapon != null && snap.weapon.light != null && snap.weapon.light.delivery == AttackDelivery.Projectile) t |= BuildTag.Ranged;
            // Rodzaje broni z obu rąk (ostrze, ciężka broń, sztylet, łuk, fokus) – o skillach decyduje broń, nie klasa.
            t |= WeaponFamilies.Of(equipment.Get(EquipSlot.MainHand)?.definition) | WeaponFamilies.Of(equipment.Get(EquipSlot.OffHand)?.definition);
            return t;
        }
    }
}
