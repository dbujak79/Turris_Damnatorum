using UnityEngine;
namespace Turris
{
    /// <summary>Brzeg od początku pokazuje zasięg skilla; wypełnienie odmierza zapowiedź.</summary>
    public sealed class HellTelegraph : MonoBehaviour
    {
        public float seconds;
        float age;
        FxDecal decal;
        void Awake() => decal = GetComponent<FxDecal>();
        void Update() { age += Time.deltaTime; if (decal != null) decal.Progress = Mathf.Clamp01(age / Mathf.Max(0.01f, seconds)); }
    }
}
