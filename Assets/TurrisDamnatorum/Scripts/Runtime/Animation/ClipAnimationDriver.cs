using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Turris
{
    /// <summary>
    /// Sterownik animacji z klipów (Playables, bez Animator Controllera).
    /// Warstwa 0: mieszanie ruchu (bezczynność / chód / bieg / w bok / w tył).
    /// Warstwa 1: akcja całego ciała, warstwa 2: akcja górnej połowy ciała (maska – tylko dla modeli Humanoid).
    /// Czas klipu akcji jest USTAWIANY z postępu faz walki, a nie odtwarzany, dzięki czemu moment trafienia w klipie
    /// pokrywa się z fazą aktywną ataku niezależnie od długości klipu.
    /// </summary>
    public class ClipAnimationDriver : ICharacterAnimationDriver
    {
        readonly CharacterVisualDefinition def;
        PlayableGraph graph;
        AnimationLayerMixerPlayable layers;
        AnimationMixerPlayable loco;
        AnimationClipPlayable[] locoClips;
        readonly float[] locoSpeeds;
        AnimationClipPlayable actionPlayable;
        ActionClip currentAction;
        int currentLayer = -1;
        float actionWeight;
        readonly bool humanoid;
        readonly Transform character;
        static AnimationClip emptyClip;

        public ActionClip CurrentActionClip => currentAction;
        public float CurrentActionTime => currentAction != null && actionPlayable.IsValid() ? (float)actionPlayable.GetTime() : 0f;
        public PlayableGraph Graph => graph;

        public ClipAnimationDriver(Animator animator, CharacterVisualDefinition def, Transform character)
        {
            this.def = def;
            this.character = character;
            humanoid = animator.isHuman;
            animator.applyRootMotion = false; // ruch prowadzi logika gry
            graph = PlayableGraph.Create("TurrisCharacter_" + animator.name);
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);

            layers = AnimationLayerMixerPlayable.Create(graph, 3);
            output.SetSourcePlayable(layers);

            var clips = new[] { def.idle, def.walk, def.run, def.walkBack, def.strafeLeft, def.strafeRight };
            locoSpeeds = new[] { 0f, def.walkSpeed, def.runSpeed, def.walkSpeed, def.walkSpeed, def.walkSpeed };
            loco = AnimationMixerPlayable.Create(graph, clips.Length);
            locoClips = new AnimationClipPlayable[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i] != null ? clips[i] : (def.idle != null ? def.idle : Empty());
                locoClips[i] = AnimationClipPlayable.Create(graph, clip);
                graph.Connect(locoClips[i], 0, loco, i);
                loco.SetInputWeight(i, i == 0 ? 1f : 0f);
            }
            graph.Connect(loco, 0, layers, 0);
            layers.SetInputWeight(0, 1f);

            // Puste wejścia warstw akcji (podmieniane przy zmianie akcji).
            for (int l = 1; l <= 2; l++)
            {
                var placeholder = AnimationClipPlayable.Create(graph, Empty());
                graph.Connect(placeholder, 0, layers, l);
                layers.SetInputWeight(l, 0f);
            }
            if (humanoid) layers.SetLayerMaskFromAvatarMask(2, UpperBodyMask());
        }

        static AnimationClip Empty()
        {
            if (emptyClip == null) emptyClip = new AnimationClip { name = "Empty" };
            return emptyClip;
        }

        static AvatarMask UpperBodyMask()
        {
            var m = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) m.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            foreach (var p in new[] { AvatarMaskBodyPart.Body, AvatarMaskBodyPart.Head, AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.RightArm,
                                      AvatarMaskBodyPart.LeftFingers, AvatarMaskBodyPart.RightFingers, AvatarMaskBodyPart.LeftHandIK, AvatarMaskBodyPart.RightHandIK })
                m.SetHumanoidBodyPartActive(p, true);
            return m;
        }

        /// <summary>Czas znormalizowany klipu dla bieżącej fazy walki.</summary>
        public static float NormalizedTime(ActionClip c, in CharacterAnimState s, float clipLength)
        {
            switch (s.action)
            {
                case AnimAction.Attack:
                case AnimAction.Parry:
                case AnimAction.Cast:
                    float p = Mathf.Clamp01(s.phaseProgress);
                    switch (s.phase)
                    {
                        case ActionPhase.Startup: return Mathf.Lerp(0f, c.windupEnd, p);
                        case ActionPhase.Active: return Mathf.Lerp(c.windupEnd, c.activeEnd, p);
                        case ActionPhase.Recovery: return Mathf.Lerp(c.activeEnd, 1f, p);
                        default: return 1f;
                    }
                case AnimAction.Block:
                case AnimAction.Kneel:
                case AnimAction.GuardBroken:
                    if (c.loop && clipLength > 0) return (s.actionTime % clipLength) / clipLength;
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.01f, clipLength));
                case AnimAction.Death:
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.01f, clipLength));
                default:
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.05f, s.actionDuration));
            }
        }

        public void Tick(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            if (!graph.IsValid()) return;
            UpdateLocomotion(worldVelocity, dt, s.action);

            var clip = s.action == AnimAction.None ? null : def.Find(s.action, s.attack);
            if (clip != currentAction) SwapAction(clip);

            if (currentAction != null)
            {
                float len = currentAction.clip.length;
                actionPlayable.SetTime(NormalizedTime(currentAction, s, len) * len);
                bool snap = s.action == AnimAction.Attack || s.action == AnimAction.Parry || s.action == AnimAction.Dodge || s.action == AnimAction.Death;
                actionWeight = snap ? 1f : Mathf.MoveTowards(actionWeight, 1f, dt * 10f);
            }
            else actionWeight = Mathf.MoveTowards(actionWeight, 0f, dt * 8f);

            layers.SetInputWeight(1, currentLayer == 1 ? actionWeight : 0f);
            layers.SetInputWeight(2, currentLayer == 2 ? actionWeight : 0f);
            graph.Evaluate(dt);
        }

        void SwapAction(ActionClip clip)
        {
            currentAction = clip;
            if (clip == null) return;
            int layer = clip.upperBodyOnly && humanoid ? 2 : 1;
            var old = layers.GetInput(layer);
            graph.Disconnect(layers, layer);
            if (old.IsValid()) old.Destroy();
            actionPlayable = AnimationClipPlayable.Create(graph, clip.clip);
            actionPlayable.SetSpeed(0); // czas ustawiany ręcznie
            actionPlayable.SetApplyFootIK(false);
            graph.Connect(actionPlayable, 0, layers, layer);
            // Wyłącz drugą warstwę akcji, żeby nie mieszać dwóch klipów.
            if (currentLayer != layer) actionWeight = 0f;
            currentLayer = layer;
        }

        void UpdateLocomotion(Vector3 worldVelocity, float dt, AnimAction action)
        {
            Vector3 local = character != null ? Quaternion.Inverse(character.rotation) * worldVelocity : worldVelocity;
            local.y = 0;
            float speed = local.magnitude;
            // Kierunkowy klip chodu (w tył / w bok), jeśli został przypisany – przy namierzaniu postać porusza się bokiem.
            int walkSlot = 1;
            if (speed > 0.1f)
            {
                Vector3 d = local / speed;
                if (d.z < -0.6f && def.walkBack != null) walkSlot = 3;
                else if (d.x < -0.6f && def.strafeLeft != null) walkSlot = 4;
                else if (d.x > 0.6f && def.strafeRight != null) walkSlot = 5;
            }
            float[] w = new float[locoClips.Length];
            if (speed < 0.1f) w[0] = 1f;
            else if (speed <= def.walkSpeed || walkSlot != 1) { float t = Mathf.Clamp01(speed / Mathf.Max(0.1f, def.walkSpeed)); w[0] = 1f - t; w[walkSlot] = t; }
            else { float t = Mathf.Clamp01((speed - def.walkSpeed) / Mathf.Max(0.1f, def.runSpeed - def.walkSpeed)); w[1] = 1f - t; w[2] = t; }
            for (int i = 0; i < w.Length; i++)
            {
                float cur = loco.GetInputWeight(i);
                loco.SetInputWeight(i, Mathf.MoveTowards(cur, w[i], dt * 6f));
                // Tempo kroków dopasowane do prędkości.
                if (i >= 1) locoClips[i].SetSpeed(speed > 0.1f ? Mathf.Clamp(speed / Mathf.Max(0.1f, locoSpeeds[i]), 0.5f, 1.8f) : 1f);
            }
        }

        public void Dispose()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}
