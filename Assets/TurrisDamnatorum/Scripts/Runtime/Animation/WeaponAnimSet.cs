using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Sposób trzymania broni – decyduje o zestawie animacji i o chwycie drugiej ręki.</summary>
    public enum GripStyle { OneHand, TwoHand, Bow, Staff }

    /// <summary>
    /// Zestaw animacji przypisany do rodzaju broni (np. miecz z tarczą, miecz dwuręczny, łuk) – klipy Mixamo
    /// w rigu Humanoid, więc jeden zestaw pasuje do każdego bohatera. Klipy zestawu mają pierwszeństwo przed
    /// klipami modelu (<see cref="CharacterVisualDefinition"/>); czego zestaw nie ma, gra bierze z modelu.
    /// </summary>
    [CreateAssetMenu(menuName = "Turris/Weapon Anim Set", fileName = "WeaponAnimSet")]
    public class WeaponAnimSet : ScriptableObject
    {
        public GripStyle grip = GripStyle.OneHand;

        [Header("Ruch")]
        public AnimationClip idle;
        [Tooltip("Spoczynek z klipów samego modelu (Meshy „Idle_Combat” – postawa bojowa z HU: szeroko, broń uniesiona, tarcza z przodu)\n" +
                 "zamiast klipu idle z zestawu. Tak stoją bohaterowie w Hell Unleashed.")]
        public bool useModelIdle;
        [Tooltip("Postawa bojowa: spoczynek w postawie ciosów (noga wykroczna z przodu) przez chwilę po ciosie i parowaniu –\n" +
                 "kolejny cios zaczyna się bez wykroku, a po ciosie nie ma dostawiania nogi. Puste = zawsze zwykły spoczynek.")]
        public AnimationClip combatIdle;
        public AnimationClip walk, run, walkBack, strafeLeft, strafeRight;
        [Tooltip("Bieg w tył i bokiem (np. przy namierzonym celu). Puste = klip chodu w tym kierunku, przyspieszony.")]
        public AnimationClip runBack, runLeft, runRight;
        [Tooltip("Prędkości nagrania chodu i biegu – gdy klip nie niesie własnej (ruch korzenia), do dopasowania tempa kroków.")]
        public float walkSpeed = 1.4f, runSpeed = 3.9f;

        [Header("Akcje")]
        public List<ActionClip> actions = new List<ActionClip>();

        [Header("Chwyt")]
        [Tooltip("Odległość lewej dłoni od prawej wzdłuż drzewca (broń dwuręczna), m.")]
        public float gripSpan = 0.2f;
        [Tooltip("Tarcza przypięta do przedramienia (orientacja liczona co klatkę z kierunku przedramienia – CharacterVisual.CarryShield). " +
                 "Położenie i obrót poniżej to tylko poza startowa (z pozy bloku), zanim ruszy animacja. Bez tego tarcza obraca się jak ostrze w dłoni.")]
        public bool hasShieldGrip;
        public Vector3 shieldGripPosition, shieldGripEuler;

        public ActionClip Find(AnimAction action, AttackAnim attack) => CharacterVisualDefinition.FindIn(actions, action, attack);
    }
}
