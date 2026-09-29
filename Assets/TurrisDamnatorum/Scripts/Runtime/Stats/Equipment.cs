using System;
using System.Collections.Generic;

namespace Turris
{
    /// <summary>Konkretny egzemplarz przedmiotu (definicja + poziom jakości). Definicja pozostaje niezmienna.</summary>
    [Serializable]
    public class ItemInstance
    {
        static int nextUid = 1;

        public ItemDefinition definition;
        public int level;
        public int uid;

        public ItemInstance(ItemDefinition def, int level = 0)
        {
            definition = def; this.level = level; uid = nextUid++;
        }

        public string Name => level > 0 ? $"{definition.displayName} +{level}" : definition.displayName;

        /// <summary>Mnożnik premii z poziomu jakości.</summary>
        public float QualityFactor => 1f + 0.15f * level;

        public IEnumerable<StatModifier> Modifiers
        {
            get { foreach (var m in definition.modifiers) yield return m.Scaled(QualityFactor); }
        }

        public float WeaponLevelFactor => 1f + 0.1f * level;
    }

    /// <summary>Sloty wyposażenia z kontrolą zgodności i obsługą broni dwuręcznej.</summary>
    public class EquipmentSet
    {
        readonly Dictionary<EquipSlot, ItemInstance> slots = new Dictionary<EquipSlot, ItemInstance>();

        public event Action Changed;

        public ItemInstance Get(EquipSlot slot) => slots.TryGetValue(slot, out var i) ? i : null;

        public IEnumerable<KeyValuePair<EquipSlot, ItemInstance>> Equipped => slots;

        public IEnumerable<ItemInstance> Items => slots.Values;

        public bool IsEquipped(ItemInstance item)
        {
            foreach (var kv in slots) if (kv.Value == item) return true;
            return false;
        }

        public bool CanEquip(ItemInstance item, EquipSlot slot, out string reason)
        {
            reason = null;
            if (item == null || item.definition == null) { reason = "Brak przedmiotu"; return false; }
            if (!item.definition.FitsSlot(slot)) { reason = $"{item.Name} nie pasuje do slotu {Names.Slot(slot)}"; return false; }
            return true;
        }

        /// <summary>
        /// Zakłada przedmiot. Przedmioty wyparte ze slotów (w tym druga ręka przy broni dwuręcznej)
        /// trafiają do listy <paramref name="displaced"/>.
        /// </summary>
        public bool Equip(ItemInstance item, EquipSlot slot, List<ItemInstance> displaced)
        {
            if (!CanEquip(item, slot, out _)) return false;

            // Ten sam egzemplarz w innym slocie (np. przełożenie pierścienia) – najpierw zdejmij.
            EquipSlot? previous = null;
            foreach (var kv in slots) if (kv.Value == item) previous = kv.Key;
            if (previous.HasValue) slots.Remove(previous.Value);

            if (slots.TryGetValue(slot, out var old) && old != null) displaced?.Add(old);
            slots[slot] = item;

            if (slot == EquipSlot.MainHand && item.definition.twoHanded)
            {
                if (slots.TryGetValue(EquipSlot.OffHand, out var off) && off != null) displaced?.Add(off);
                slots.Remove(EquipSlot.OffHand);
            }
            else if (slot == EquipSlot.OffHand)
            {
                var main = Get(EquipSlot.MainHand);
                if (main != null && main.definition.twoHanded)
                {
                    displaced?.Add(main);
                    slots.Remove(EquipSlot.MainHand);
                }
            }

            Changed?.Invoke();
            return true;
        }

        public ItemInstance Unequip(EquipSlot slot)
        {
            if (!slots.TryGetValue(slot, out var item)) return null;
            slots.Remove(slot);
            Changed?.Invoke();
            return item;
        }

        public float TotalWeight
        {
            get
            {
                float w = 0;
                foreach (var i in slots.Values) w += i.definition.weight;
                return w;
            }
        }

        public IEnumerable<StatModifier> AllModifiers()
        {
            foreach (var i in slots.Values)
                foreach (var m in i.Modifiers)
                    yield return m;
        }

        public IEnumerable<PassiveEffect> AllEffects()
        {
            foreach (var i in slots.Values)
                foreach (var e in i.definition.effects)
                    yield return e;
        }
    }

    [Serializable]
    /// <summary>
    /// Umiejętność w kolekcji podejścia: poziom i stan odnowienia. Odnowienie należy do umiejętności,
    /// nie do slotu – przełożenie jej w inny slot niczego nie zeruje.
    /// </summary>
    public class SpellInstance
    {
        public SpellDefinition definition;
        public int level;
        /// <summary>Odblokowana na stałe (wraca w każdym podejściu); w przeciwnym razie zdobyta tylko na to podejście.</summary>
        public bool permanent;
        int chargesSpent;
        float rechargeTimer;

        public SpellInstance(SpellDefinition def, int level = 0) { definition = def; this.level = level; }
        public string Name => level > 0 ? $"{definition.displayName} +{level}" : definition.displayName;
        public bool IsMaxLevel => level >= definition.maxLevel;

        public int MaxCharges => definition.MaxCharges(level);
        public int Charges => Math.Max(0, MaxCharges - chargesSpent);
        public bool Ready => Charges > 0;
        public float Cooldown => definition.CooldownAt(level);
        /// <summary>Sekundy do odnowienia najbliższego ładunku (0 = wszystkie gotowe).</summary>
        public float RechargeRemaining => chargesSpent > 0 ? rechargeTimer : 0f;
        /// <summary>0..1 postęp odnawiania najbliższego ładunku (1 = gotowe).</summary>
        public float RechargeProgress => chargesSpent > 0 && Cooldown > 0 ? 1f - rechargeTimer / Cooldown : 1f;

        public bool TryUse()
        {
            if (!Ready) return false;
            if (chargesSpent == 0) rechargeTimer = Cooldown;
            chargesSpent++;
            return true;
        }

        /// <summary>Zwrot ładunku (użycie przerwane, zanim cokolwiek się stało).</summary>
        public void Refund()
        {
            if (chargesSpent <= 0) return;
            chargesSpent--;
            if (chargesSpent == 0) rechargeTimer = 0;
        }

        public void Tick(float dt)
        {
            if (chargesSpent <= 0) return;
            rechargeTimer -= dt;
            if (rechargeTimer > 0) return;
            chargesSpent--;
            rechargeTimer = chargesSpent > 0 ? Cooldown : 0f;
        }

        public void ResetCooldown() { chargesSpent = 0; rechargeTimer = 0; }
    }

    public class BoonStack
    {
        public BoonDefinition definition;
        public int stacks;
    }
}
