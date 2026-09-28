using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Klip akcji z zaznaczonymi granicami faz walki (w czasie znormalizowanym klipu 0..1).</summary>
    [Serializable]
    public class ActionClip
    {
        public AnimAction action;
        [Tooltip("Dla ataków: styl, którego dotyczy klip. Auto = dowolny atak bez własnego klipu.")]
        public AttackAnim attack = AttackAnim.Auto;
        public AnimationClip clip;
        [Tooltip("Koniec zamachu (początek fazy aktywnej) w czasie znormalizowanym klipu.")]
        [Range(0, 1)] public float windupEnd = 0.35f;
        [Tooltip("Koniec fazy aktywnej (moment po trafieniu).")]
        [Range(0, 1)] public float activeEnd = 0.55f;
        [Tooltip("Tylko górna połowa ciała (np. blok, picie, czar) – nogi animuje chód.")]
        public bool upperBodyOnly;
        [Tooltip("Klip zapętlony (np. trzymanie bloku, klęczenie).")]
        public bool loop;
    }

    /// <summary>
    /// Wygląd postaci z modelu FBX (np. Mixamo) z animacjami. Przypisz w ClassDefinition.visual lub EnemyDefinition.visual.
    /// Gdy pole jest puste, gra używa proceduralnego humanoida.
    /// </summary>
    [CreateAssetMenu(menuName = "Turris/Character Visual (FBX)", fileName = "CharacterVisual")]
    public class CharacterVisualDefinition : ScriptableObject
    {
        [Header("Model")]
        [Tooltip("Prefab/FBX z komponentem Animator (Rig: Humanoid zalecany).")]
        public GameObject modelPrefab;
        public float scale = 1f;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        [Header("Broń i tarcza (modele z prymitywów doczepiane do kości)")]
        public bool attachGear = true;
        public HumanBodyBones weaponBone = HumanBodyBones.RightHand;
        public Vector3 weaponPosition = new Vector3(0, 0.08f, 0.02f);
        public Vector3 weaponRotation = new Vector3(0, 0, 0);
        public HumanBodyBones shieldBone = HumanBodyBones.LeftLowerArm;
        public Vector3 shieldPosition = new Vector3(0, 0.12f, -0.08f);
        public Vector3 shieldRotation = new Vector3(0, -90, 0);
        [Tooltip("Dla modeli Generic (nie Humanoid): ścieżki do kości w hierarchii.")]
        public string weaponBonePath, shieldBonePath;

        [Header("Ruch")]
        public AnimationClip idle;
        public AnimationClip walk, run, walkBack, strafeLeft, strafeRight;
        [Tooltip("Prędkości (m/s), przy których nagrano klipy chodu i biegu – do dopasowania tempa kroków.")]
        public float walkSpeed = 2f, runSpeed = 4.6f;

        [Header("Akcje")]
        public List<ActionClip> actions = new List<ActionClip>();

        public ActionClip Find(AnimAction action, AttackAnim attack)
        {
            ActionClip generic = null;
            foreach (var a in actions)
            {
                if (a == null || a.clip == null || a.action != action) continue;
                if (a.attack == attack) return a;
                if (a.attack == AttackAnim.Auto && generic == null) generic = a;
            }
            if (generic != null) return generic;
            if (action == AnimAction.Attack)
                foreach (var a in actions) if (a != null && a.clip != null && a.action == AnimAction.Attack) return a;
            return null;
        }
    }
}
