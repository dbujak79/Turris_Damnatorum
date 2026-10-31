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
        /// <summary>Hell Unleashed (etap 6): rzadkość, jakość (×(1 + 0,25·jakość) wszystkich premii) i wylosowane affixy.</summary>
        public ItemRarity rarity;
        public int huQuality;
        public List<StatModifier> affixes = new List<StatModifier>();
        /// <summary>Affixy ze statystyką HU bez odpowiednika w statystykach Turris (uchylenie, dusze, przebicie…).</summary>
        public List<PassiveEffect> affixEffects = new List<PassiveEffect>();
        public List<string> affixNames = new List<string>();

        public ItemInstance(ItemDefinition def, int level = 0)
        {
            definition = def; this.level = level; uid = nextUid++;
        }

        public string Name => level > 0 ? $"{definition.displayName} +{level}" : definition.displayName;

        /// <summary>Mnożnik premii z poziomu jakości (Turris) i jakości HU.</summary>
        public float QualityFactor => (1f + 0.15f * level) * HuQualityFactor;

        public float HuQualityFactor => 1f + 0.25f * huQuality;

        public IEnumerable<StatModifier> Modifiers
        {
            get
            {
                foreach (var m in definition.modifiers) yield return m.Scaled(QualityFactor);
                foreach (var m in affixes) yield return m.Scaled(HuQualityFactor);
            }
        }

        /// <summary>Efekty pasywne: z definicji (moce unikatów, statystyki HU bez odpowiednika) i z affixów, ×jakość HU.</summary>
        public IEnumerable<PassiveEffect> Effects
        {
            get
            {
                float q = HuQualityFactor;
                foreach (var e in definition.effects) yield return q == 1f || !PassiveEffects.ScalesWithQuality(e.type) ? e : new PassiveEffect(e.type, e.value * q);
                foreach (var e in affixEffects) yield return q == 1f || !PassiveEffects.ScalesWithQuality(e.type) ? e : new PassiveEffect(e.type, e.value * q);
            }
        }

        /// <summary>Obrażenia broni z poziomu: +20% za poziom, jak w Hell Unleashed (poziomy 0–4 = HU 1–5).</summary>
        public float WeaponLevelFactor => 1f + 0.2f * level;
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
                foreach (var e in i.Effects)
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
        /// <summary>Umiejętność z założonego przedmiotu – znika, gdy przedmiot zostanie zdjęty.</summary>
        public bool fromItem;
        int chargesSpent;
        float rechargeTimer;

        /// <summary>Pieczęcie skilla z Hell Unleashed (null dla skilli Turris). Kształt po pieczęciach wyznacza odnowienie, ładunki i działanie.</summary>
        public readonly SealSet seals;

        public SpellInstance(SpellDefinition def, int level = 0)
        {
            definition = def; this.level = level;
            var hu = def != null ? def.HuData : null;
            if (hu != null) seals = new SealSet(hu) { Level = () => Math.Min(this.level + 1, hu.MaxLevel), signature = hu.IsSignature };
        }
        public string Name => level > 0 ? $"{definition.displayName} +{level}" : definition.displayName;
        public bool IsMaxLevel => level >= definition.maxLevel;

        /// <summary>Najkrótsze odnowienie jako część odnowienia skilla bez pieczęci – niezależnie od pośpiechu, cech i pieczęci (HU: 0,4).</summary>
        public const float CooldownFloor = 0.4f;

        public int MaxCharges => seals != null ? seals.Shape.MaxCharges : definition.MaxCharges(level);
        public int Charges => Math.Max(0, MaxCharges - chargesSpent);
        public bool Ready => Charges > 0;
        public float Cooldown => seals != null
            ? Math.Max(CooldownFloor * (float)HuSkillShape.Plain(seals.data, seals.Level()).Cooldown, (float)seals.Shape.Cooldown)
            : definition.CooldownAt(level);
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
            if (chargesSpent > MaxCharges) chargesSpent = MaxCharges;
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
