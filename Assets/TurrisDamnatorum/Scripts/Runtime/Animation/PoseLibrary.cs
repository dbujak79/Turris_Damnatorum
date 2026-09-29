using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Poza górnej części ciała w przestrzeni postaci (stopy w 0, przód +Z, prawo +X, wysokość ≈ 1,8 m).
    /// Dłonie są celami IK, broń i tarcza mają kierunki w przestrzeni postaci – dzięki temu keyframe'y
    /// ataków opisuje się jak tor ruchu ostrza, bez ręcznego liczenia kątów stawów.
    /// </summary>
    public struct Pose
    {
        public Vector3 handR, handL, hintR, hintL;
        public Vector3 weaponDir, weaponUp;
        public Vector3 shieldNormal, shieldUp;
        public float chestYaw, chestPitch, spinePitch, headPitch;
        public float crouch;   // obniżenie bioder (m)
        public float stance;   // 0 = normalnie, 1 = wypad (lewa noga z przodu)
        public float armsOverride; // 0 = ręce mogą bujać się w rytm chodu, 1 = poza całkowicie kontroluje ręce

        public static Pose Lerp(in Pose a, in Pose b, float t)
        {
            return new Pose
            {
                handR = Vector3.Lerp(a.handR, b.handR, t),
                handL = Vector3.Lerp(a.handL, b.handL, t),
                hintR = Vector3.Lerp(a.hintR, b.hintR, t),
                hintL = Vector3.Lerp(a.hintL, b.hintL, t),
                weaponDir = Vector3.Slerp(a.weaponDir, b.weaponDir, t),
                weaponUp = Vector3.Slerp(a.weaponUp, b.weaponUp, t),
                shieldNormal = Vector3.Slerp(a.shieldNormal, b.shieldNormal, t),
                shieldUp = Vector3.Slerp(a.shieldUp, b.shieldUp, t),
                chestYaw = Mathf.Lerp(a.chestYaw, b.chestYaw, t),
                chestPitch = Mathf.Lerp(a.chestPitch, b.chestPitch, t),
                spinePitch = Mathf.Lerp(a.spinePitch, b.spinePitch, t),
                headPitch = Mathf.Lerp(a.headPitch, b.headPitch, t),
                crouch = Mathf.Lerp(a.crouch, b.crouch, t),
                stance = Mathf.Lerp(a.stance, b.stance, t),
                armsOverride = Mathf.Lerp(a.armsOverride, b.armsOverride, t),
            };
        }
    }

    /// <summary>Trzy kluczowe pozy ataku: szczyt zamachu, moment trafienia, wybrzmienie.</summary>
    public struct AttackKeys
    {
        public Pose wind, hit, follow;
    }

    public static class PoseLibrary
    {
        static readonly Vector3 HintR = new Vector3(0.6f, -1f, -0.4f);
        static readonly Vector3 HintL = new Vector3(-0.6f, -1f, -0.4f);

        /// <summary>Postawa gotowości zależna od broni i tarczy (idle w walce).</summary>
        public static Pose Ready(RigLook look)
        {
            var p = new Pose
            {
                hintR = HintR, hintL = HintL,
                weaponUp = Vector3.up, shieldUp = Vector3.up,
                shieldNormal = new Vector3(-0.8f, 0f, 0.6f),
                armsOverride = 0.6f,
            };
            switch (look.weapon)
            {
                case WeaponModel.Staff:
                    p.handR = new Vector3(0.28f, 1.0f, 0.18f);
                    p.weaponDir = new Vector3(0.02f, 1f, 0.12f);
                    p.weaponUp = Vector3.forward;
                    break;
                case WeaponModel.GreatAxe:
                case WeaponModel.GreatSword:
                case WeaponModel.Halberd:
                case WeaponModel.Hammer:
                case WeaponModel.Spear:
                case WeaponModel.Scythe:
                    p.handR = new Vector3(0.2f, 0.95f, 0.22f);
                    p.weaponDir = new Vector3(-0.25f, 0.75f, 0.6f);
                    p.armsOverride = 1f;
                    break;
                case WeaponModel.None:
                case WeaponModel.Claws:
                    p.handR = new Vector3(0.3f, 0.95f, 0.3f);
                    p.weaponDir = Vector3.forward;
                    break;
                case WeaponModel.Bow:
                    // Łuk trzymany pionowo przed sobą.
                    p.handR = new Vector3(0.22f, 1.12f, 0.32f);
                    p.weaponDir = new Vector3(0.1f, 0f, 1f);
                    p.weaponUp = Vector3.up;
                    break;
                case WeaponModel.Knives:
                    // Noże: obie dłonie nisko z przodu, ostrza do przodu.
                    p.handR = new Vector3(0.3f, 1.0f, 0.3f);
                    p.handL = new Vector3(-0.3f, 1.0f, 0.3f);
                    p.weaponDir = new Vector3(0.1f, 0.35f, 1f);
                    p.armsOverride = 1f;
                    break;
                default:
                    p.handR = new Vector3(0.3f, 0.98f, 0.28f);
                    p.weaponDir = new Vector3(0.05f, 0.45f, 1f);
                    break;
            }

            if (look.shield != ShieldModel.None) p.handL = new Vector3(-0.3f, 1.08f, 0.24f);
            else if (look.weapon == WeaponModel.Claws || look.weapon == WeaponModel.None) p.handL = new Vector3(-0.3f, 0.95f, 0.3f);
            else p.handL = new Vector3(-0.27f, 0.9f, 0.05f);

            if (look.style == RigStyle.Ghoul) { p.crouch = 0.1f; p.handR = new Vector3(0.32f, 0.8f, 0.4f); p.handL = new Vector3(-0.32f, 0.8f, 0.4f); }
            if (look.style == RigStyle.Heretic || look.style == RigStyle.Mage) p.handL = new Vector3(-0.1f, 1.05f, 0.25f);
            return p;
        }

        public static Pose Block(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.crouch = 0.06f;
            p.chestPitch = 6f;
            if (look.shield != ShieldModel.None)
            {
                p.handL = new Vector3(-0.08f, 1.22f, 0.36f);
                p.hintL = new Vector3(-1f, -0.4f, 0.2f);
                p.shieldNormal = new Vector3(-0.05f, 0.05f, 1f);
                p.handR = new Vector3(0.32f, 1.0f, 0.2f);
            }
            else
            {
                // Garda bronią: ostrze w poprzek przed tułowiem.
                p.handR = new Vector3(0.22f, 1.3f, 0.32f);
                p.hintR = new Vector3(1f, -0.5f, 0f);
                p.weaponDir = new Vector3(-0.9f, 0.4f, 0.15f);
                p.weaponUp = Vector3.forward;
                p.handL = new Vector3(-0.2f, 1.3f, 0.35f);
            }
            return p;
        }

        public static Pose ParryActive(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.chestYaw = -15f;
            if (look.shield != ShieldModel.None)
            {
                p.handL = new Vector3(-0.3f, 1.3f, 0.5f);
                p.hintL = new Vector3(-1f, -0.2f, 0f);
                p.shieldNormal = new Vector3(-0.55f, 0.25f, 0.8f);
            }
            else
            {
                p.handR = new Vector3(-0.05f, 1.4f, 0.45f);
                p.hintR = new Vector3(1f, -0.3f, 0.3f);
                p.weaponDir = new Vector3(-0.6f, 0.75f, 0.2f);
            }
            return p;
        }

        public static Pose ParryWind(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.chestYaw = 20f;
            if (look.shield != ShieldModel.None) p.handL = new Vector3(-0.1f, 1.15f, 0.2f);
            else p.handR = new Vector3(0.35f, 1.2f, 0.1f);
            return p;
        }

        public static Pose Cast(RigLook look, float t)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.handR = Vector3.Lerp(new Vector3(0.18f, 1.35f, 0.15f), new Vector3(0.15f, 1.45f, 0.7f), t);
            p.hintR = new Vector3(1f, -0.6f, 0f);
            p.weaponDir = Vector3.Slerp(new Vector3(0, 1f, 0.2f), new Vector3(0, 0.5f, 1f), t);
            p.weaponUp = Vector3.forward;
            if (look.shield == ShieldModel.None) p.handL = Vector3.Lerp(new Vector3(-0.02f, 1.35f, 0.25f), new Vector3(-0.25f, 1.3f, 0.45f), t);
            p.chestPitch = Mathf.Lerp(5f, -5f, t);
            return p;
        }

        public static Pose Drink(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.handL = new Vector3(-0.06f, 1.6f, 0.2f);
            p.hintL = new Vector3(-1f, -0.6f, 0f);
            p.headPitch = -25f;
            p.chestPitch = -5f;
            return p;
        }

        public static Pose Flinch(RigLook look, float t)
        {
            var p = Ready(look);
            float k = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) ;
            p.chestPitch = -18f * k;
            p.headPitch = -20f * k;
            p.crouch += 0.06f * k;
            p.handR += new Vector3(0.1f, 0.05f, -0.15f) * k;
            p.handL += new Vector3(-0.1f, 0.05f, -0.15f) * k;
            p.armsOverride = 1f;
            return p;
        }

        public static Pose GuardBroken(RigLook look, float time)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.chestPitch = -25f + Mathf.Sin(time * 9f) * 5f;
            p.headPitch = -15f;
            p.crouch = 0.15f;
            p.handR = new Vector3(0.6f, 1.15f, -0.1f);
            p.handL = new Vector3(-0.6f, 1.15f, -0.1f);
            p.weaponDir = new Vector3(0.6f, 0.6f, -0.4f);
            p.shieldNormal = new Vector3(-1f, 0.4f, -0.2f);
            return p;
        }

        public static Pose Kneel(RigLook look, float time)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.crouch = 0.42f;
            p.chestPitch = 38f + Mathf.Sin(time * 3f) * 3f;
            p.headPitch = 25f;
            p.handR = new Vector3(0.28f, 0.35f, 0.3f);
            p.handL = new Vector3(-0.28f, 0.35f, 0.3f);
            p.weaponDir = new Vector3(0.2f, -0.6f, 0.8f);
            p.shieldNormal = new Vector3(-1f, -0.3f, 0.2f);
            return p;
        }

        public static Pose Tucked(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.crouch = 0.4f;
            p.chestPitch = 30f;
            p.handR = new Vector3(0.18f, 1.05f, 0.35f);
            p.handL = new Vector3(-0.18f, 1.05f, 0.35f);
            return p;
        }

        public static Pose Dead(RigLook look)
        {
            var p = Ready(look);
            p.armsOverride = 1f;
            p.handR = new Vector3(0.65f, 1.3f, 0.05f);
            p.handL = new Vector3(-0.65f, 1.3f, 0.05f);
            p.weaponDir = new Vector3(1f, 0.2f, 0f);
            p.headPitch = -20f;
            return p;
        }

        // ------------------------------------------------------------------ Ataki

        static Pose Key(Pose ready, Vector3 handR, Vector3 hintR, Vector3 weaponDir, float chestYaw = 0, float chestPitch = 0, float crouch = 0, float stance = 0)
        {
            var p = ready;
            p.handR = handR; p.hintR = hintR; p.weaponDir = weaponDir; p.weaponUp = Vector3.up;
            p.chestYaw = chestYaw; p.chestPitch = chestPitch; p.crouch = crouch; p.stance = stance;
            p.armsOverride = 1f;
            return p;
        }

        public static AttackKeys Attack(AttackAnim anim, RigLook look)
        {
            var r = Ready(look);
            var k = new AttackKeys();
            switch (anim)
            {
                case AttackAnim.SlashLeft:
                    k.wind = Key(r, new Vector3(-0.3f, 1.42f, 0.18f), new Vector3(0.5f, -0.4f, 1f), new Vector3(-0.6f, 0.55f, -0.55f), -38f, 0, 0.05f, 0.3f);
                    k.hit = Key(r, new Vector3(0.12f, 1.28f, 0.66f), new Vector3(1f, -0.5f, 0f), new Vector3(0.35f, 0.15f, 1f), 0f, 5f, 0.08f, 1f);
                    k.follow = Key(r, new Vector3(0.55f, 1.02f, 0.25f), new Vector3(1f, -0.3f, -0.5f), new Vector3(0.9f, -0.25f, -0.2f), 38f, 8f, 0.07f, 0.8f);
                    break;
                case AttackAnim.Overhead:
                    k.wind = Key(r, new Vector3(0.18f, 1.95f, -0.08f), new Vector3(1f, 0.3f, 0f), new Vector3(0.05f, 0.25f, -1f), 10f, -14f, 0.02f, 0.4f);
                    k.hit = Key(r, new Vector3(0.1f, 1.55f, 0.58f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0.55f, 0.85f), 0f, 8f, 0.08f, 1f);
                    k.follow = Key(r, new Vector3(0.1f, 0.82f, 0.55f), new Vector3(1f, -0.5f, -0.3f), new Vector3(0f, -0.75f, 0.65f), -5f, 32f, 0.2f, 1f);
                    break;
                case AttackAnim.Thrust:
                    k.wind = Key(r, new Vector3(0.36f, 1.15f, -0.25f), new Vector3(0.8f, -0.6f, -0.4f), new Vector3(0f, 0.05f, 1f), 28f, 0f, 0.1f, 0.5f);
                    k.hit = Key(r, new Vector3(0.1f, 1.25f, 0.85f), new Vector3(1f, -0.6f, 0f), new Vector3(0f, 0f, 1f), -12f, 12f, 0.14f, 1f);
                    k.follow = Key(r, new Vector3(0.12f, 1.22f, 0.7f), new Vector3(1f, -0.6f, 0f), new Vector3(0f, -0.05f, 1f), -8f, 10f, 0.12f, 1f);
                    break;
                case AttackAnim.Slam:
                    k.wind = Key(r, new Vector3(0.12f, 1.95f, -0.05f), new Vector3(1f, 0.4f, 0f), new Vector3(0f, 0.3f, -1f), 0f, -15f, 0.05f, 0.3f);
                    k.hit = Key(r, new Vector3(0.08f, 0.85f, 0.6f), new Vector3(1f, -0.3f, -0.2f), new Vector3(0f, -0.85f, 0.5f), 0f, 35f, 0.3f, 1f);
                    k.follow = Key(r, new Vector3(0.08f, 0.8f, 0.58f), new Vector3(1f, -0.3f, -0.2f), new Vector3(0f, -0.9f, 0.4f), 0f, 35f, 0.32f, 1f);
                    k.wind.handL = new Vector3(-0.1f, 1.9f, -0.02f); k.hit.handL = new Vector3(-0.08f, 0.85f, 0.55f); k.follow.handL = k.hit.handL;
                    break;
                case AttackAnim.Burst:
                    k.wind = Key(r, new Vector3(0.08f, 1.3f, 0.25f), new Vector3(1f, -0.6f, 0f), new Vector3(0f, 1f, 0.2f), 0f, 12f, 0.12f, 0f);
                    k.wind.handL = new Vector3(-0.08f, 1.3f, 0.25f);
                    k.hit = Key(r, new Vector3(0.72f, 1.5f, 0.15f), new Vector3(0.3f, -1f, -0.5f), new Vector3(0.3f, 1f, 0f), 0f, -15f, 0.02f, 0f);
                    k.hit.handL = new Vector3(-0.72f, 1.5f, 0.15f);
                    k.follow = k.hit;
                    break;
                case AttackAnim.Cast:
                    k.wind = Cast(look, 0f); k.wind.crouch = 0.05f;
                    k.hit = Cast(look, 1f);
                    k.follow = Cast(look, 0.85f);
                    break;
                case AttackAnim.Claw:
                    k.wind = Key(r, new Vector3(0.45f, 1.55f, -0.12f), new Vector3(1f, 0f, -0.5f), Vector3.forward, 32f, -5f, 0.12f, 0.3f);
                    k.hit = Key(r, new Vector3(-0.05f, 1.1f, 0.65f), new Vector3(1f, -0.5f, 0f), Vector3.forward, -15f, 15f, 0.15f, 1f);
                    k.follow = Key(r, new Vector3(-0.4f, 0.8f, 0.3f), new Vector3(0.8f, -1f, 0f), Vector3.forward, -35f, 20f, 0.18f, 1f);
                    break;
                case AttackAnim.BowDraw:
                    // Ręka z łukiem wyciągnięta przed siebie na wysokości barku, druga naciąga cięciwę pod brodę;
                    // przy strzale dłoń odskakuje do tyłu (puszczenie), potem obie ręce wracają.
                    k.wind = Key(r, new Vector3(0.08f, 1.42f, 0.62f), new Vector3(1f, -0.3f, 0.2f), new Vector3(0.05f, 0f, 1f), -25f, 0f, 0.05f, 0.4f);
                    k.wind.weaponUp = Vector3.up;
                    k.wind.handL = new Vector3(-0.02f, 1.45f, 0.08f); k.wind.hintL = new Vector3(-1f, 0.2f, -0.6f);
                    k.hit = k.wind;
                    k.hit.handL = new Vector3(-0.14f, 1.47f, -0.06f); k.hit.hintL = new Vector3(-1f, 0.4f, -0.8f);
                    k.follow = k.hit;
                    k.follow.handR = new Vector3(0.14f, 1.3f, 0.55f);
                    k.follow.handL = new Vector3(-0.22f, 1.2f, 0.1f);
                    k.follow.chestYaw = -12f;
                    break;
                case AttackAnim.Leap:
                    k.wind = Key(r, new Vector3(0.35f, 0.95f, -0.35f), new Vector3(0.6f, -0.6f, -1f), new Vector3(0f, 0.3f, -1f), 0f, 25f, 0.38f, 0f);
                    k.wind.handL = new Vector3(-0.35f, 0.95f, -0.35f);
                    k.hit = Key(r, new Vector3(0.1f, 1.35f, 0.72f), new Vector3(1f, -0.3f, 0f), new Vector3(0f, -0.4f, 1f), 0f, 10f, 0.05f, 1f);
                    k.follow = Key(r, new Vector3(0.1f, 0.8f, 0.55f), new Vector3(1f, -0.5f, -0.3f), new Vector3(0f, -0.8f, 0.5f), 0f, 30f, 0.3f, 1f);
                    break;
                default: // SlashRight
                    k.wind = Key(r, new Vector3(0.5f, 1.5f, -0.05f), new Vector3(1f, -0.2f, -0.6f), new Vector3(0.35f, 0.6f, -0.7f), 40f, 0f, 0.05f, 0.3f);
                    k.hit = Key(r, new Vector3(0.15f, 1.25f, 0.62f), new Vector3(1f, -0.5f, -0.2f), new Vector3(-0.3f, 0.1f, 1f), 0f, 5f, 0.08f, 1f);
                    k.follow = Key(r, new Vector3(-0.35f, 1.0f, 0.35f), new Vector3(0.6f, -1f, 0f), new Vector3(-0.9f, -0.3f, -0.1f), -40f, 8f, 0.07f, 0.8f);
                    break;
            }
            // Pazury: lewa ręka zamachuje się lustrzanie w kontrze (wygląda jak szarpnięcie obiema łapami).
            if (anim == AttackAnim.Claw)
            {
                k.wind.handL = new Vector3(-0.3f, 1.1f, 0.35f);
                k.hit.handL = new Vector3(-0.35f, 1.3f, 0.2f);
                k.follow.handL = new Vector3(-0.2f, 1.2f, 0.45f);
            }
            return k;
        }

        /// <summary>Poza ataku w danej fazie. Zamach kończy się krótkim zawieszeniem (czytelny telegraf), trafienie jest szybkie.</summary>
        public static Pose SampleAttack(in AttackKeys k, in Pose ready, ActionPhase phase, float p)
        {
            switch (phase)
            {
                case ActionPhase.Startup:
                {
                    float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(p / 0.75f), 3f); // dojście do szczytu w 75% zamachu, potem zawieszenie
                    var wind = k.wind;
                    // Lekkie „dociąganie” w końcówce zamachu – wzmacnia sygnał, że cios zaraz padnie.
                    float pull = Mathf.Clamp01((p - 0.75f) / 0.25f);
                    wind.chestYaw *= 1f + 0.12f * pull;
                    return Pose.Lerp(ready, wind, e);
                }
                case ActionPhase.Active:
                {
                    float q = Mathf.Clamp01(p);
                    if (q < 0.5f) return Pose.Lerp(k.wind, k.hit, EaseIn(q * 2f));
                    return Pose.Lerp(k.hit, k.follow, EaseOut((q - 0.5f) * 2f));
                }
                case ActionPhase.Recovery:
                {
                    float q = Mathf.Clamp01(p);
                    // Pozostań chwilę w wybrzmieniu (okno kary), potem powrót do gardy.
                    return Pose.Lerp(k.follow, ready, SmoothStep(Mathf.Clamp01((q - 0.2f) / 0.8f)));
                }
            }
            return ready;
        }

        static float EaseIn(float t) => t * t;
        static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        static float SmoothStep(float t) => t * t * (3f - 2f * t);
    }
}
