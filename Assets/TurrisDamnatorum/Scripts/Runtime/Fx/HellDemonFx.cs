using UnityEngine;

namespace Turris
{
    /// <summary>Doczepienia cząsteczek do kości już przeniesionych demonów (DemonFx.Mounts HU).</summary>
    public static class HellDemonFx
    {
        public static void Attach(Transform model, string visualName)
        {
            if (!FxLibrary.Enabled) return;
            string key = visualName.Replace("visual_demon_", "");
            void Flame(string bone, Vector3 offset, float size)
            {
                var mount = Util.FindDeep(model, bone);
                if (mount != null) HellFx.Flame(mount, offset * 0.35f, size * 0.35f);
            }
            void Trail(string bone, HellFxElement family, float size)
            {
                var mount = Util.FindDeep(model, bone);
                if (mount == null) return;
                var fx = HellFx.Trail(mount, family, size * 0.35f);
                fx.SetAmount(0.3f);
            }
            switch (key)
            {
                case "visual_imp_fire_anime": case "imp":
                    Flame("hand.R", new Vector3(-0.05f,0.07f,0.03f),0.45f);
                    Flame("tail3", new Vector3(0,0.12f,0),0.28f); break;
                case "cultist": Flame("hand.R",new Vector3(-0.05f,0.07f,0.03f),0.3f); break;
                case "visual_imp_water_anime": case "imp_water":
                    var water = HellFx.Ambient(model, Vector3.up * 0.4f, HellFxElement.Water,0.2f);
                    water.SetAmount(0.25f); water.SetScale(0.4f); break;
                case "imp_air": Trail("hand.R",HellFxElement.Air,0.4f); break;
                case "imp_earth": Trail("hand.R",HellFxElement.Earth,0.35f); Trail("hand.L",HellFxElement.Ash,0.2f); break;
                case "gatelord":
                    Flame("hand.L", new Vector3(0,0.06f,0),0.2f);
                    Flame("hand.R", new Vector3(0,0.06f,0),0.2f);
                    Trail("spine",HellFxElement.Fire,0.15f); break;
                case "hound": Flame("head", new Vector3(0,0.04f,0),0.22f); Trail("spine",HellFxElement.Fire,0.25f); break;
                case "emberbrute": Flame("hand.L",Vector3.up * 0.15f,0.7f); Flame("hand.R",Vector3.up * 0.15f,0.7f); Trail("spine",HellFxElement.Fire,0.35f); break;
                case "bomber": Flame("head",Vector3.up * 0.18f,0.3f); break;
                case "frostcaster": case "tidequeen":
                    var drips = HellFx.Ambient(model, Vector3.up * 0.4f,HellFxElement.Water,0.3f); drips.SetAmount(0.25f); break;
                case "stormwitch":
                    var air = HellFx.Ambient(model,Vector3.up * 0.5f,HellFxElement.Air,0.3f); air.SetAmount(0.25f); break;
                case "necromancer": Trail("head",HellFxElement.Soul,0.5f); break;
                case "stalker": Trail("spine",HellFxElement.Soul,0.5f); break;
                case "charger": Trail("head",HellFxElement.Ash,0.25f); break;
                case "mortar": Trail("hand.L",HellFxElement.Earth,0.25f); Trail("hand.R",HellFxElement.Earth,0.25f); break;
                case "titan": Trail("spine",HellFxElement.Fire,0.5f); Trail("hand.L",HellFxElement.Earth,0.3f); Trail("hand.R",HellFxElement.Earth,0.3f); break;
            }
        }
    }
}
