using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>
    /// Testy PlayMode grają bez dźwięku: wyciszenie edytora (jak przycisk „Mute Audio”) na czas całego przebiegu,
    /// potem przywrócenie poprzedniego stanu. Nie rusza <c>AudioListener.volume</c>, które sprawdzają testy dźwięku.
    /// Dźwięk włącza zmienna środowiskowa TURRIS_TEST_SOUND=1.
    /// </summary>
    [SetUpFixture]
    public class MuteDuringTests
    {
#if UNITY_EDITOR
        bool wasMuted;

        [OneTimeSetUp]
        public void Mute()
        {
            wasMuted = UnityEditor.EditorUtility.audioMasterMute;
            if (System.Environment.GetEnvironmentVariable("TURRIS_TEST_SOUND") != "1") UnityEditor.EditorUtility.audioMasterMute = true;
        }

        [OneTimeTearDown]
        public void Restore() => UnityEditor.EditorUtility.audioMasterMute = wasMuted;
#endif
    }
}
