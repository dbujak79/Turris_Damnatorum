using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wygląd gracza: humanoid budowany z aktualnego sprzętu (zbroja, hełm, broń, tarcza) albo model FBX z klasy,
    /// plus czytelne sygnały stanu – błysk aktywnego okna parowania, poświata w oknie niewrażliwości uniku,
    /// czerwień przy trafieniu, pulsowanie przy przełamanej gardzie, poświata zaklętego ostrza.
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        PlayerCombat combat;
        CharacterVisual visual;
        string lastLookKey;
        ParticleSystem buffAura;

        void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            visual = GetComponent<CharacterVisual>();
            combat.BuildChanged += Refresh;
        }

        /// <summary>Zachowane dla zgodności – kolor klasy barwi teraz tkaniny postaci.</summary>
        public void SetBodyColor(Color c) { }

        public void Refresh()
        {
            if (combat.Build == null || combat.Run == null || visual == null) return;
            var run = combat.Run;
            var look = RigLook.ForPlayer(run, combat.Config);
            var classVisual = run.startingClass != null ? run.startingClass.visual : null;
            string key = $"{(classVisual != null ? classVisual.name : "proc")}|{look.body}|{look.head}|{look.weapon}|{look.shield}|{look.weaponColor}|{look.shieldColor}|{look.cloth}";
            if (key == lastLookKey) return;
            bool onlyGearChanged = lastLookKey != null && visual.Rig != null && classVisual == null &&
                                   key.Split('|')[1] == lastLookKey.Split('|')[1] && key.Split('|')[2] == lastLookKey.Split('|')[2] && key.Split('|')[7] == lastLookKey.Split('|')[7];
            lastLookKey = key;

            if (classVisual != null && classVisual.modelPrefab != null)
                visual.BuildModel(classVisual, 1f, look.weapon, look.weaponColor, look.shield, look.shieldColor);
            else if (onlyGearChanged)
                visual.SetGear(look.weapon, look.weaponColor, look.shield, look.shieldColor);
            else
                visual.BuildProcedural(look, 1f);
            visual.Init(combat);
        }

        void LateUpdate()
        {
            if (combat.Run == null || visual == null) return;
            var a = combat.Actions;
            Color tint = Color.white;
            float amount = 0f;
            switch (a.Current)
            {
                case ActionType.Parry:
                    if (a.Phase == ActionPhase.Active) { tint = Color.white; amount = 0.55f; }
                    else if (a.Phase == ActionPhase.Recovery) { tint = new Color(0.3f, 0.3f, 0.3f); amount = 0.25f; }
                    break;
                case ActionType.Dodge:
                    if (a.Phase == ActionPhase.Active) { tint = new Color(0.6f, 0.9f, 1f); amount = 0.35f; }
                    break;
                case ActionType.Riposte: tint = new Color(1f, 0.85f, 0.3f); amount = 0.35f; break;
                case ActionType.Flinch: tint = Color.red; amount = 0.35f * (1f - a.PhaseProgress); break;
                case ActionType.GuardBroken: tint = new Color(1f, 0.45f, 0.1f); amount = 0.25f + 0.2f * Mathf.Sin(Time.time * 20f); break;
                case ActionType.Dead: tint = Color.black; amount = 0.45f; break;
            }
            visual.SetTint(tint, amount);

            // Zaklęte ostrze: poświata broni + drobinki unoszące się z klingi.
            if (combat.WeaponBuffTime > 0 && buffAura == null && visual.Rig != null)
                buffAura = FxLibrary.Aura(visual.Rig.WeaponSocket, new Vector3(0, 0, 0.55f), combat.WeaponBuffColor, 45f, 0.28f, 0.08f);
            if (combat.WeaponBuffTime <= 0 && buffAura != null)
            {
                buffAura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(buffAura.gameObject, 1f);
                buffAura = null;
            }
            if (combat.WeaponBuffTime > 0) visual.SetWeaponGlow(combat.WeaponBuffColor, 0.6f + 0.2f * Mathf.Sin(Time.time * 6f));
            else if (a.Current == ActionType.Cast && a.Phase != ActionPhase.Recovery) visual.SetWeaponGlow(combat.ActiveSkillColor, 0.4f + 0.6f * a.PhaseProgress);
            else visual.SetWeaponGlow(Color.black, 0f);
        }
    }
}
