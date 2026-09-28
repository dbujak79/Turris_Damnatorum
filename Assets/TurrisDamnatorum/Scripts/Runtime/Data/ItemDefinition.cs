using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Item", fileName = "Item")]
    public class ItemDefinition : ContentDefinition
    {
        public ItemKind kind;
        [Tooltip("Broń dwuręczna zajmuje główną rękę i blokuje drugą.")]
        public bool twoHanded;
        public float weight = 2f;
        public List<AttributeRequirement> requirements = new List<AttributeRequirement>();
        public List<StatModifier> modifiers = new List<StatModifier>();
        public List<PassiveEffect> effects = new List<PassiveEffect>();
        [Tooltip("Używane tylko gdy kind == Weapon.")]
        public WeaponData weapon = new WeaponData();
        [Tooltip("Używane tylko gdy kind == Shield.")]
        public ShieldData shield = new ShieldData();
        public Color color = Color.gray;
        [Tooltip("Model broni/tarczy na postaci. Auto = na podstawie id.")]
        public WeaponModel weaponModel = WeaponModel.Auto;
        public ShieldModel shieldModel = ShieldModel.Auto;

        public bool IsWeapon => kind == ItemKind.Weapon;
        public bool IsShield => kind == ItemKind.Shield;

        public bool FitsSlot(EquipSlot slot)
        {
            switch (kind)
            {
                case ItemKind.Weapon: return slot == EquipSlot.MainHand;
                case ItemKind.Shield: return slot == EquipSlot.OffHand;
                case ItemKind.Head: return slot == EquipSlot.Head;
                case ItemKind.Body: return slot == EquipSlot.Body;
                case ItemKind.Hands: return slot == EquipSlot.Hands;
                case ItemKind.Belt: return slot == EquipSlot.Belt;
                case ItemKind.Feet: return slot == EquipSlot.Feet;
                case ItemKind.Ring: return slot == EquipSlot.Ring1 || slot == EquipSlot.Ring2;
                case ItemKind.Amulet: return slot == EquipSlot.Amulet;
            }
            return false;
        }

        public EquipSlot DefaultSlot
        {
            get
            {
                switch (kind)
                {
                    case ItemKind.Weapon: return EquipSlot.MainHand;
                    case ItemKind.Shield: return EquipSlot.OffHand;
                    case ItemKind.Head: return EquipSlot.Head;
                    case ItemKind.Body: return EquipSlot.Body;
                    case ItemKind.Hands: return EquipSlot.Hands;
                    case ItemKind.Belt: return EquipSlot.Belt;
                    case ItemKind.Feet: return EquipSlot.Feet;
                    case ItemKind.Ring: return EquipSlot.Ring1;
                    default: return EquipSlot.Amulet;
                }
            }
        }
    }
}
