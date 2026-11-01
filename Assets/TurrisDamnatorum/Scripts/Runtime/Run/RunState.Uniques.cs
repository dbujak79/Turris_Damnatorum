using System.Collections.Generic;

namespace Turris
{
    /// <summary>Natywne unikaty w stanie podejścia: lokalne wyjątki slotów (różdżka w lewej ręce) i ich wycofanie.</summary>
    public partial class RunState
    {
        public const string DoubleSecretId = "item.unique.turris.dual.double_secret";

        public RunState()
        {
            equipment.ExtraFit = UniqueSlotAllowed;
            equipment.Changed += ValidateUniqueSlots;
        }

        /// <summary>Różdżka jednoręczna w drugiej ręce: tylko z założonymi Rękawicami podwójnego sekretu i przy jednoręcznej głównej.</summary>
        public bool WandOffhandAllowed
        {
            get
            {
                var hands = equipment.Get(EquipSlot.Hands);
                var main = equipment.Get(EquipSlot.MainHand);
                return hands != null && hands.definition.id == DoubleSecretId && (main == null || !main.definition.twoHanded);
            }
        }

        static bool IsWand(ItemDefinition d) => d != null && d.IsWeapon && !d.twoHanded && d.weapon?.light != null && d.weapon.light.delivery == AttackDelivery.Projectile;

        bool UniqueSlotAllowed(ItemInstance item, EquipSlot slot) => slot == EquipSlot.OffHand && IsWand(item.definition) && WandOffhandAllowed;

        bool validating;

        /// <summary>Po zdjęciu rękawic różdżka z drugiej ręki wraca do plecaka (wyjątek jest odwracalny, bez ukrytej trzeciej ręki).</summary>
        void ValidateUniqueSlots()
        {
            if (validating) return;
            var off = equipment.Get(EquipSlot.OffHand);
            if (off == null || !IsWand(off.definition) || WandOffhandAllowed) return;
            validating = true;
            equipment.Unequip(EquipSlot.OffHand);
            inventory.Add(off);
            validating = false;
        }
    }
}
