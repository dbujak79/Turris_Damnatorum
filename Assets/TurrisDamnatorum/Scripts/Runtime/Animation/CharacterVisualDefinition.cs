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
        [Tooltip("Ciosy: wysunięcie ciała do przodu w nagraniu (czas znormalizowany → RootT.z w jednostkach awatara). " +
                 "Bohater idzie o tyle naprzód, ile ciało w klipie – stopa podporowa stoi, zamiast jechać do tyłu. Wypełnia HeroAssetImport.")]
        public AnimationCurve rootForward;
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
        [Tooltip("Demon z Hell Unleashed (rig Generic, własne klipy): sterownik DemonAnimationDriver, bez doczepianej broni.")]
        public bool demon;
        [Tooltip("Sztywny stalowy nosek: palce zachowują pozę modelu względem stopy, także podczas biegu i ataków.")]
        public bool rigidBootToes;
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

        [Header("Zestawy animacji broni (Mixamo, Humanoid)")]
        [Tooltip("Gniazda broni w modelu (kości dodane przy konwersji). Puste = kości dłoni z rigu Humanoid.")]
        public string rightSlot = "HandSlot.R", leftSlot = "HandSlot.L";
        public WeaponAnimSet oneHandSet, twoHandSet, bowSet;
        [Tooltip("Zestaw dla kostura i różdżki. Puste = klipy samego modelu (np. rzucanie czarów).")]
        public WeaponAnimSet staffSet;

        public WeaponAnimSet SetFor(WeaponModel weapon)
        {
            switch (AnimResolve.GripFor(weapon))
            {
                case GripStyle.Bow: return bowSet;
                case GripStyle.TwoHand: return twoHandSet;
                case GripStyle.Staff: return staffSet;
                default: return weapon == WeaponModel.None ? null : oneHandSet;
            }
        }

        public ActionClip Find(AnimAction action, AttackAnim attack) => FindIn(actions, action, attack);

        public static ActionClip FindIn(List<ActionClip> actions, AnimAction action, AttackAnim attack)
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
