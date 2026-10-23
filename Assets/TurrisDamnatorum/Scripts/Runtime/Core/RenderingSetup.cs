using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Turris
{
    /// <summary>
    /// Ustawienia URP wspólne dla wszystkich kamer gry. Kamery powstają w kodzie (GameRoot, BestiaryStage, testy),
    /// a URP domyślnie wyłącza na nich post-processing – bez tego Volume (bloom, ACES) nie działa.
    /// </summary>
    public static class RenderingSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        static void OnBeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera.cameraType != CameraType.Game) return;
            Prepare(camera);
        }

        /// <summary>
        /// Natężenie światła punktowego w URP odpowiadające wartości strojonej w Built-in. URP gaśnie fizycznie (1/d²),
        /// Built-in łagodnie względem zasięgu – bez kompensacji pochodnie i błyski o zasięgu od ~6 m wyraźnie słabną.
        /// </summary>
        public static float PointIntensity(float builtInIntensity, float range)
        {
            return builtInIntensity * Mathf.Max(1f, PointRangeFactor * range * range);
        }

        /// <summary>Dobrane porównaniem zrzutów Built-in i URP (galerie aren i efektów).</summary>
        public const float PointRangeFactor = 0.03f;

        // Kalibracja URP + Linear + ACES (ekspozycja +1 EV w DefaultVolumeProfile). W Built-in/Gamma ciemne albedo
        // rozjaśniały odbicia skyboxa (gammowe F0 0,22) i liczenie światła w gammie; tu wraca to światło sceny.
        // Pomiar w grze (piętro I): średnia jasność 0,204 wobec 0,293 w Built-in – celowo ciemniej, bez przepaleń.
        /// <summary>Natężenie słońca areny (w Built-in 1,1).</summary>
        public const float SunIntensity = 2f;
        /// <summary>Mnożnik płaskiego światła otoczenia ustawianego przez GameRoot.</summary>
        public const float AmbientScale = 1.6f;

        /// <summary>Włącza post-processing na kamerze; wywoływane też od razu przy tworzeniu kamer gry.</summary>
        public static void Prepare(Camera camera)
        {
            if (camera == null) return;
            var data = camera.GetUniversalAdditionalCameraData();
            if (!data.renderPostProcessing) data.renderPostProcessing = true;
        }
    }
}
