using UnityEngine;

namespace Turris
{
    public enum HeadGear { Bare, GreatHelm, Hood, HornedHelm, Skull, Coif }
    public enum BodyGear { Tunic, Chain, Plate, Robe, Rags }

    /// <summary>Opis wyglądu proceduralnego humanoida (sylwetka, pancerz, kolory, broń).</summary>
    public class RigLook
    {
        public RigStyle style = RigStyle.Knight;
        public HeadGear head = HeadGear.Bare;
        public BodyGear body = BodyGear.Tunic;
        public bool cape;
        public bool pauldrons;
        public float bulk = 1f;         // szerokość barków i tułowia
        public float limbThickness = 1f;
        public float armLength = 1f;
        public float hunch;             // pochylenie kręgosłupa (stopnie)

        public Color skin = new Color(0.78f, 0.62f, 0.5f);
        public Color armor = new Color(0.6f, 0.62f, 0.66f);
        public Color cloth = new Color(0.45f, 0.12f, 0.12f);
        public Color trim = new Color(0.78f, 0.62f, 0.25f);
        public Color leather = new Color(0.3f, 0.2f, 0.13f);
        public Color eyes = new Color(0.1f, 0.1f, 0.1f);
        public bool glowingEyes;

        public WeaponModel weapon = WeaponModel.Sword;
        public Color weaponColor = new Color(0.82f, 0.83f, 0.86f);
        public ShieldModel shield = ShieldModel.None;
        public Color shieldColor = new Color(0.55f, 0.35f, 0.2f);

        public bool TwoHanded => weapon == WeaponModel.GreatAxe || weapon == WeaponModel.GreatSword || weapon == WeaponModel.Halberd;

        /// <summary>Wygląd gracza wynika z założonego sprzętu (a nie z klasy) – kolor klasy tylko barwi tkaniny.</summary>
        public static RigLook ForPlayer(RunState run, GameConfig cfg)
        {
            var look = new RigLook { style = RigStyle.Knight };
            var cls = run.startingClass;
            // Kolor klasy barwi tkaniny; zbyt szary kolor zastępujemy karmazynem (heraldyka).
            if (cls != null)
            {
                Color.RGBToHSV(cls.color, out _, out float sat, out _);
                look.cloth = sat < 0.25f ? new Color(0.45f, 0.08f, 0.08f) : cls.color * 0.75f;
            }
            look.cloth.a = 1f;

            var body = run.equipment.Get(EquipSlot.Body)?.definition;
            string bid = body != null ? body.id : "";
            if (bid.Contains("plate")) { look.body = BodyGear.Plate; look.pauldrons = true; look.cape = true; look.bulk = 1.08f; }
            else if (bid.Contains("chain")) { look.body = BodyGear.Chain; look.cape = true; look.armor = new Color(0.52f, 0.53f, 0.56f); }
            else if (bid.Contains("robe")) { look.body = BodyGear.Robe; look.style = RigStyle.Mage; }
            else look.body = BodyGear.Tunic;

            var head = run.equipment.Get(EquipSlot.Head)?.definition;
            string hid = head != null ? head.id : "";
            if (hid.Contains("helm")) look.head = HeadGear.GreatHelm;
            else if (hid.Contains("hood")) look.head = HeadGear.Hood;
            else if (look.body == BodyGear.Robe) look.head = HeadGear.Hood;
            else if (look.body == BodyGear.Chain || look.body == BodyGear.Plate) look.head = HeadGear.Coif; // kaptur kolczy pod zbroją
            else look.head = HeadGear.Bare;

            var main = run.equipment.Get(EquipSlot.MainHand)?.definition;
            look.weapon = main != null ? AnimResolve.ForItem(main) : WeaponModel.None;
            if (main != null) look.weaponColor = main.color;
            var off = run.equipment.Get(EquipSlot.OffHand)?.definition;
            look.shield = AnimResolve.ForShield(off);
            if (off != null) look.shieldColor = off.color;
            return look;
        }

        public static RigLook ForEnemy(EnemyDefinition e)
        {
            var look = new RigLook { style = AnimResolve.ForEnemy(e), weapon = AnimResolve.ForEnemyWeapon(e) };
            switch (look.style)
            {
                case RigStyle.Ghoul:
                    look.head = HeadGear.Skull; look.body = BodyGear.Rags; look.hunch = 28f; look.armLength = 1.25f; look.limbThickness = 0.7f;
                    look.skin = new Color(0.45f, 0.5f, 0.35f); look.cloth = new Color(0.25f, 0.22f, 0.18f);
                    look.eyes = new Color(1f, 0.8f, 0.2f); look.glowingEyes = true; look.weaponColor = new Color(0.85f, 0.8f, 0.7f);
                    break;
                case RigStyle.Heretic:
                    look.head = HeadGear.Hood; look.body = BodyGear.Robe; look.cloth = new Color(0.32f, 0.14f, 0.42f); look.trim = new Color(0.7f, 0.6f, 0.2f);
                    look.eyes = new Color(0.8f, 0.4f, 1f); look.glowingEyes = true; look.weaponColor = new Color(0.35f, 0.22f, 0.15f);
                    break;
                case RigStyle.Warden:
                    look.head = HeadGear.GreatHelm; look.body = BodyGear.Plate; look.pauldrons = true; look.bulk = 1.2f; look.limbThickness = 1.2f;
                    look.armor = new Color(0.45f, 0.47f, 0.52f); look.cloth = new Color(0.2f, 0.25f, 0.35f);
                    look.shield = ShieldModel.Tower; look.shieldColor = new Color(0.35f, 0.37f, 0.42f); look.weaponColor = new Color(0.6f, 0.6f, 0.62f);
                    break;
                case RigStyle.Castellan:
                    look.head = HeadGear.HornedHelm; look.body = BodyGear.Plate; look.pauldrons = true; look.cape = true; look.bulk = 1.15f;
                    look.armor = new Color(0.25f, 0.12f, 0.12f); look.cloth = new Color(0.35f, 0.05f, 0.05f); look.trim = new Color(0.75f, 0.55f, 0.2f);
                    look.eyes = new Color(1f, 0.3f, 0.1f); look.glowingEyes = true; look.weaponColor = new Color(0.55f, 0.5f, 0.5f);
                    break;
                default:
                    look.head = HeadGear.GreatHelm; look.body = BodyGear.Chain; look.cape = true;
                    break;
            }
            // Kolor z definicji przeciwnika barwi tkaniny, żeby łatwo odróżnić warianty.
            if (look.style != RigStyle.Ghoul) look.cloth = Color.Lerp(look.cloth, e.color, 0.35f);
            return look;
        }
    }
}
