using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Item", fileName = "Item")]
    public class ItemDefinition : ContentDefinition
    {
        public ItemKind kind;
        public string huSourceId, huSourceHash;
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
        [Tooltip("Model 3D przedmiotu w dłoni postaci FBX (początek w środku rękojeści, ostrze wzdłuż +Y). Puste = model proceduralny.")]
        public GameObject model;
        public float modelScale = 1f;
        [Tooltip("Przesunięcie chwytu wzdłuż drzewca (+Y modelu), m – np. młot trzymany wyżej.")]
        public float modelGripShift;
        [Tooltip("Najwcześniej po tylu ukończonych piętrach przedmiot pojawia się w nagrodach.")]
        public int minFloor;
        [Tooltip("Względna waga w losowaniu nagród (unikaty rzadziej).")]
        public float offerWeight = 1f;
        [Tooltip("Umiejętność, którą daje przedmiot, dopóki jest założony (np. grymuar – nova, sztylet do rzucania – rzut nożami).")]
        public SpellDefinition grantedSkill;
        [Tooltip("Natywny unikat Turris: id karty z UniqueCatalog (moc legendarna). Puste = zwykły przedmiot.")]
        public string uniquePowerId = "";
        /// <summary>Natywny unikat: przedmiot bazowy (rodzaj broni, model, chwyt) – ustawia rejestr w pamięci.</summary>
        [NonSerialized] public ItemDefinition familyBase;

        /// <summary>Natywny unikat Turris (rejestr w kodzie, nie katalog HU).</summary>
        public bool IsNativeUnique => !string.IsNullOrEmpty(uniquePowerId);

        public bool IsWeapon => kind == ItemKind.Weapon;
        public bool IsShield => kind == ItemKind.Shield;
        /// <summary>Broń jednoręczna do walki wręcz – może iść do drugiej ręki (walka dwiema brońmi).</summary>
        public bool IsOneHandedMelee => kind == ItemKind.Weapon && !twoHanded && weapon != null && weapon.light != null && weapon.light.delivery == AttackDelivery.Melee;

        public bool FitsSlot(EquipSlot slot)
        {
            switch (kind)
            {
                case ItemKind.Weapon: return slot == EquipSlot.MainHand || (slot == EquipSlot.OffHand && IsOneHandedMelee);
                case ItemKind.Shield: return slot == EquipSlot.OffHand;
                case ItemKind.OffHand: return slot == EquipSlot.OffHand;
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
                    case ItemKind.OffHand: return EquipSlot.OffHand;
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
