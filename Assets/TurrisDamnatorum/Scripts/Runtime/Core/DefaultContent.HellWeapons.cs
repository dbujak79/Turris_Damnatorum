using UnityEngine;

namespace Turris
{
    public static partial class DefaultContent
    {
        /// <summary>Używane warianty P1. Bez zastępowania wcześniej dostrojonych broni i ich nagrań.</summary>
        static void AddHellWeapons(GameConfig cfg)
        {
            void Variant(string source, string id, string title, WeaponModel model, bool twoHanded, float damage, float weight)
            {
                var template = bundle.Get<ItemDefinition>(source);
                var item = Make<ItemDefinition>(id, title, "Broń z Hell Unleashed. " + (twoHanded ? "Wymaga obu dłoni." : ""));
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(template), item);
                item.id = item.name = id; item.displayName = title; item.weaponModel = model;
                item.twoHanded = twoHanded; item.weight = weight;
                item.weapon.light.baseDamage = damage; item.weapon.heavy.baseDamage = damage * 1.7f;
                HellWeaponCatalog.Apply(item);
                cfg.baseItemPool.Add(item);
            }
            Variant("hu_greatsword", "hu_flail2h", "Korbacz dwuręczny", WeaponModel.GreatAxe, true, 105, 8);
            Variant("hu_greatsword", "hu_flail2h_iron", "Żelazny korbacz dwuręczny", WeaponModel.GreatAxe, true, 115, 9);
            Variant("hu_warhammer", "hu_morningstar2h", "Morgenstern dwuręczny", WeaponModel.Hammer, true, 110, 8);
            Variant("hu_warhammer", "hu_morningstar2h_runic", "Runiczny morgenstern", WeaponModel.Hammer, true, 120, 9);
            Variant("weapon_staff", "hu_staff", "Kostur", WeaponModel.Staff, true, 45, 3);
            Variant("weapon_staff", "hu_staff_summoner", "Kostur przywoływacza", WeaponModel.Staff, true, 42, 3);
            Variant("weapon_staff", "hu_staff_witch", "Kostur wiedźmy", WeaponModel.Staff, true, 48, 3);
            Variant("hu_greataxe", "hu_greataxe_bearded", "Topór brodaty", WeaponModel.GreatAxe, true, 118, 9);
            Variant("hu_greataxe", "hu_greataxe_bardiche", "Berdysz", WeaponModel.GreatAxe, true, 125, 10);
            Variant("hu_warhammer", "hu_warhammer_runic", "Młot runiczny", WeaponModel.Hammer, true, 120, 9);
            Variant("hu_warhammer", "hu_warhammer_siege", "Młot oblężniczy", WeaponModel.Hammer, true, 132, 11);
        }
    }
}
