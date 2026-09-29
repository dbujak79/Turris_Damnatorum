using UnityEngine;

namespace Turris
{
    /// <summary>Czynność, którą animacja ma pokazać. Niezależna od tego, kto ją wykonuje (gracz / AI).</summary>
    public enum AnimAction { None, Attack, Block, Parry, Dodge, Backstep, Cast, Drink, Flinch, GuardBroken, Kneel, Death }

    /// <summary>Styl ruchu ataku. Auto = wyliczany z parametrów ataku i broni.</summary>
    public enum AttackAnim { Auto, SlashRight, SlashLeft, Overhead, Thrust, Slam, Burst, Cast, Claw, Leap }

    public enum WeaponModel { Auto, None, Sword, Axe, GreatAxe, Dagger, Staff, Halberd, GreatSword, Claws }

    public enum ShieldModel { Auto, None, Heater, Buckler, Tower }

    public enum RigStyle { Auto, Knight, Mage, Ghoul, Heretic, Warden, Castellan }

    /// <summary>
    /// Migawka tego, co postać robi, podawana co klatkę sterownikowi animacji.
    /// Czas fazy (phase + phaseProgress) pochodzi wprost z logiki walki, więc animacja jest z nią zsynchronizowana:
    /// ostrze przecina powietrze dokładnie w fazie aktywnej ataku, a tarcza wychodzi w oknie parowania.
    /// </summary>
    public struct CharacterAnimState
    {
        public AnimAction action;
        public AttackAnim attack;
        public ActionPhase phase;
        public float phaseProgress;     // 0..1 w bieżącej fazie
        public float actionTime;        // sekundy od rozpoczęcia akcji
        public float actionDuration;    // łączny czas akcji (dla akcji jednofazowych, np. drgnięcia)
        public bool hasShield;
        public bool weaponGuard;        // blok bronią (brak tarczy)
        public Vector3 dodgeDirection;  // świat
        public float rollDuration;      // czas przewrotu uniku; 0 = wyliczany z actionDuration
        public float spinAngle;         // obrót całego ciała wokół osi pionowej (młynek), stopnie
    }

    public interface ICharacterAnimSource
    {
        CharacterAnimState GetAnimState();
    }

    /// <summary>Wyliczanie stylu animacji i modeli, gdy w danych ustawiono Auto.</summary>
    public static class AnimResolve
    {
        public static AttackAnim ForAttack(AttackDefinition a, int combo, WeaponModel weapon)
        {
            if (a.animation != AttackAnim.Auto) return a.animation;
            if (a.delivery == AttackDelivery.Projectile) return AttackAnim.Cast;
            if (a.delivery == AttackDelivery.AreaAroundSelf) return a.damageType == DamageType.Magic ? AttackAnim.Burst : AttackAnim.Slam;
            if (weapon == WeaponModel.Claws || weapon == WeaponModel.None) return combo % 2 == 0 ? AttackAnim.Claw : AttackAnim.SlashLeft;
            if (a.lunge >= 4f) return a.heavy ? AttackAnim.Leap : AttackAnim.Thrust;
            if (a.heavy) return AttackAnim.Overhead;
            if (weapon == WeaponModel.Dagger) return combo % 2 == 0 ? AttackAnim.Thrust : AttackAnim.SlashRight;
            switch (combo % 3)
            {
                case 0: return AttackAnim.SlashRight;
                case 1: return AttackAnim.SlashLeft;
                default: return weapon == WeaponModel.Sword ? AttackAnim.Thrust : AttackAnim.SlashRight;
            }
        }

        public static WeaponModel ForItem(ItemDefinition item)
        {
            if (item == null) return WeaponModel.None;
            if (item.weaponModel != WeaponModel.Auto) return item.weaponModel;
            string id = item.id ?? "";
            if (id.Contains("fists")) return WeaponModel.None;
            if (id.Contains("greataxe")) return WeaponModel.GreatAxe;
            if (id.Contains("axe")) return WeaponModel.Axe;
            if (id.Contains("dagger")) return WeaponModel.Dagger;
            if (id.Contains("staff")) return WeaponModel.Staff;
            if (id.Contains("halberd")) return WeaponModel.Halberd;
            if (id.Contains("greatsword")) return WeaponModel.GreatSword;
            return item.twoHanded ? WeaponModel.GreatSword : WeaponModel.Sword;
        }

        public static ShieldModel ForShield(ItemDefinition item)
        {
            if (item == null || !item.IsShield) return ShieldModel.None;
            if (item.shieldModel != ShieldModel.Auto) return item.shieldModel;
            string id = item.id ?? "";
            if (id.Contains("buckler")) return ShieldModel.Buckler;
            if (id.Contains("great") || id.Contains("tower")) return ShieldModel.Tower;
            return ShieldModel.Heater;
        }

        public static RigStyle ForEnemy(EnemyDefinition e)
        {
            if (e.rigStyle != RigStyle.Auto) return e.rigStyle;
            string id = e.id ?? "";
            if (e.isBoss) return RigStyle.Castellan;
            if (id.Contains("ghoul")) return RigStyle.Ghoul;
            if (id.Contains("heretic") || e.retreatRange > 0) return RigStyle.Heretic;
            if (id.Contains("warden") || e.guardChance > 0) return RigStyle.Warden;
            return RigStyle.Knight;
        }

        public static WeaponModel ForEnemyWeapon(EnemyDefinition e)
        {
            if (e.weaponModel != WeaponModel.Auto) return e.weaponModel;
            switch (ForEnemy(e))
            {
                case RigStyle.Ghoul: return WeaponModel.Claws;
                case RigStyle.Heretic: return WeaponModel.Staff;
                case RigStyle.Warden: return WeaponModel.Halberd;
                case RigStyle.Castellan: return WeaponModel.GreatSword;
                default: return WeaponModel.Sword;
            }
        }
    }
}
