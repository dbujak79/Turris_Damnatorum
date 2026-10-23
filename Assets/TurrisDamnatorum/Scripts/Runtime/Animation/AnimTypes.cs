using UnityEngine;

namespace Turris
{
    /// <summary>Czynność, którą animacja ma pokazać. Niezależna od tego, kto ją wykonuje (gracz / AI).</summary>
    public enum AnimAction { None, Attack, Block, Parry, Dodge, Backstep, Cast, Drink, Flinch, GuardBroken, Kneel, Death,
        /// <summary>Uczepienie (czerw z Hell Unleashed wgryziony w bohatera) – klip w pętli.</summary>
        Latched }

    /// <summary>Styl ruchu ataku. Auto = wyliczany z parametrów ataku i broni.</summary>
    /// Special i Latch – własne klipy demonów z Hell Unleashed (wielki ruch ataku specjalnego, skok czerwia).
    public enum AttackAnim { Auto, SlashRight, SlashLeft, Overhead, Thrust, Slam, Burst, Cast, Claw, Leap, BowDraw, Special, Latch }

    /// <summary>Model broni. Wartości zapisane w assetach – nowe tylko na końcu.</summary>
    public enum WeaponModel { Auto, None, Sword, Axe, GreatAxe, Dagger, Staff, Halberd, GreatSword, Claws, Mace, Hammer, Spear, Scythe, Knives, Bow }

    public enum ShieldModel { Auto, None, Heater, Buckler, Tower }

    public enum RigStyle { Auto, Knight, Mage, Ghoul, Heretic, Warden, Castellan, Archer }

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
        public float windupSeconds;     // atak: długość zapowiedzi (s) – demony prowadzą klip do szczytu i ciosu jak w Hell Unleashed
        public bool hasShield;
        public bool offhandAttack;
        public bool weaponGuard;        // blok bronią (brak tarczy)
        public Vector3 dodgeDirection;  // świat
        public float rollDuration;      // czas przewrotu uniku; 0 = wyliczany z actionDuration
        public float spinAngle;         // obrót całego ciała wokół osi pionowej (młynek), stopnie
        public Vector3 hitDirection;    // świat: kierunek ostatniego ciosu (od napastnika do postaci) – w tę stronę postać pada
        public float hitWeight;         // ciężar ostatniego ciosu 0..1+ (odrzut ciała przy upadku)
    }

    public interface ICharacterAnimSource
    {
        CharacterAnimState GetAnimState();
    }

    /// <summary>Sterownik animacji reagujący ciałem na trafienie (odchylenie tułowia i głowy w kierunku ciosu).</summary>
    public interface IImpactReactive
    {
        /// <summary>localDir – kierunek pchnięcia w przestrzeni postaci (+Z = do przodu); strength ≈ 0..1,5.</summary>
        void AddImpact(Vector3 localDir, float strength);
    }

    /// <summary>Postać, którą trafienie może na chwilę zatrzymać (hitstop lokalny – bez zatrzymywania całej gry).</summary>
    public interface IHitStop
    {
        void HitStop(float seconds);
    }

    /// <summary>Wyliczanie stylu animacji i modeli, gdy w danych ustawiono Auto.</summary>
    public static class AnimResolve
    {
        public static AttackAnim ForAttack(AttackDefinition a, int combo, WeaponModel weapon)
        {
            if (a.animation != AttackAnim.Auto) return a.animation;
            if (weapon == WeaponModel.Bow && a.delivery == AttackDelivery.Projectile) return AttackAnim.BowDraw;
            if (a.delivery == AttackDelivery.Projectile) return AttackAnim.Cast;
            if (a.delivery == AttackDelivery.AreaAroundSelf) return a.damageType == DamageType.Magic ? AttackAnim.Burst : AttackAnim.Slam;
            if (weapon == WeaponModel.Claws || weapon == WeaponModel.None) return combo % 2 == 0 ? AttackAnim.Claw : AttackAnim.SlashLeft;
            if (a.lunge >= 4f) return a.heavy ? AttackAnim.Leap : AttackAnim.Thrust;
            if (a.heavy) return AttackAnim.Overhead;
            if (weapon == WeaponModel.Dagger) return combo % 2 == 0 ? AttackAnim.Thrust : AttackAnim.SlashRight;
            if (weapon == WeaponModel.Spear) return AttackAnim.Thrust; // włócznia zawsze pchnięciami
            if (weapon == WeaponModel.Bow) return AttackAnim.BowDraw;  // łuk: naciągnięcie cięciwy i strzał
            if (weapon == WeaponModel.Knives) return combo % 2 == 0 ? AttackAnim.SlashRight : AttackAnim.SlashLeft; // na przemian
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
            if (id.Contains("knives")) return WeaponModel.Knives;
            if (id.Contains("bow")) return WeaponModel.Bow;
            if (id.Contains("mace")) return WeaponModel.Mace;
            if (id.Contains("hammer")) return WeaponModel.Hammer;
            if (id.Contains("spear")) return WeaponModel.Spear;
            if (id.Contains("scythe")) return WeaponModel.Scythe;
            if (id.Contains("greataxe")) return WeaponModel.GreatAxe;
            if (id.Contains("axe")) return WeaponModel.Axe;
            if (id.Contains("dagger")) return WeaponModel.Dagger;
            if (id.Contains("staff")) return WeaponModel.Staff;
            if (id.Contains("halberd")) return WeaponModel.Halberd;
            if (id.Contains("greatsword")) return WeaponModel.GreatSword;
            return item.twoHanded ? WeaponModel.GreatSword : WeaponModel.Sword;
        }

        /// <summary>Chwyt broni dla zestawu animacji modelu FBX.</summary>
        public static GripStyle GripFor(WeaponModel weapon)
        {
            switch (weapon)
            {
                case WeaponModel.Bow: return GripStyle.Bow;
                case WeaponModel.Staff: return GripStyle.Staff;
                case WeaponModel.GreatAxe: case WeaponModel.GreatSword: case WeaponModel.Halberd:
                case WeaponModel.Hammer: case WeaponModel.Spear: case WeaponModel.Scythe:
                    return GripStyle.TwoHand;
                default: return GripStyle.OneHand;
            }
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
            if (id.Contains("archer")) return RigStyle.Archer;
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
                case RigStyle.Archer: return WeaponModel.Bow;
                default: return WeaponModel.Sword;
            }
        }
    }
}
