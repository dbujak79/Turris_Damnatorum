using System;

namespace Turris.Tests
{
    /// <summary>
    /// Skill wymaga rodzaju broni (<see cref="WeaponFamilies"/>): testy rzucające dowolny skill zakładają najpierw broń,
    /// która na niego pozwala (wzorcową dla rodzaju, więc siła broni = ×1).
    /// </summary>
    public static class TestWeapons
    {
        public static void EnsureFor(RunState run, SpellDefinition def, Func<string, ItemDefinition> item)
        {
            var main = run.equipment.Get(EquipSlot.MainHand)?.definition;
            var off = run.equipment.Get(EquipSlot.OffHand)?.definition;
            if (WeaponFamilies.Usable(def, main, off)) return;
            var need = def.requiredTags;
            // Szarża: broń dwuręczna albo tarcza – wystarczy młot dwuręczny.
            if ((need & BuildTag.TwoHanded) != 0) { Put(run, item("hu_warhammer"), EquipSlot.MainHand); return; }
            if ((need & BuildTag.Shield) != 0) Put(run, item("hu_shield"), EquipSlot.OffHand);
            var families = need & WeaponFamilies.All;
            if (families == BuildTag.None || WeaponFamilies.Usable(def, run.equipment.Get(EquipSlot.MainHand)?.definition, run.equipment.Get(EquipSlot.OffHand)?.definition)) return;
            string id = (families & BuildTag.Blade) != 0 ? "hu_sword"
                : (families & BuildTag.HeavyWeapon) != 0 ? "hu_warhammer" // broń ciężka to tylko dwuręczna
                : (families & BuildTag.Dagger) != 0 ? "hu_dagger"
                : (families & BuildTag.Ranged) != 0 ? "hu_bow"
                : "hu_wand_fire";
            Put(run, item(id), EquipSlot.MainHand);
        }

        static void Put(RunState run, ItemDefinition def, EquipSlot slot)
        {
            if (def == null) return;
            var it = new ItemInstance(def);
            run.inventory.Add(it);
            run.EquipFromInventory(it, slot);
        }
    }
}
