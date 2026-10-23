# Migracja Built-in → URP + Linear (2026-10-23)

Gałąź: `feature/urp-vfx-migration`. Kontrolowana migracja renderowania – bez zmian rozgrywki.
Migawka drzewa roboczego sprzed migracji (515 niezacommitowanych zmian, w tym 382 nowe pliki):
`refs/backup/pre-urp-worktree` (`6cc61c7`). Przywrócenie pojedynczego pliku:
`git checkout refs/backup/pre-urp-worktree -- <ścieżka>`.

## 1. Stan przed migracją (zmierzony w edytorze przez MCP)

| Ustawienie | Wartość |
|---|---|
| Unity | 6000.6.3f1, Windows, D3D12 + D3D11, RTX 4070 |
| Pipeline | Built-in (brak assetu RP we wszystkich 6 poziomach jakości) |
| Color space | Gamma |
| Jakość (bieżąca) | Ultra: pixelLightCount 4 (GameRoot podnosi do 8), cienie All/High, 4 kaskady, dystans 150, MSAA 2×, soft particles on |
| Kamera `Main Camera` | HDR on, MSAA on, ścieżka z PlayerSettings (Forward) |
| Światło sceny | `Sun` kierunkowe 1,1, soft shadows, realtime |
| Mgła / otoczenie | ustawiane w kodzie (`GameRoot`, `ArenaBuilder`): Linear fog, ambient Flat |
| Post-processing | brak (brak PPv2) |
| VFX Graph | 17.6.0 zainstalowany, 0 plików `.vfx` |
| Konsola / kompilacja | 0 błędów, 67 assembly |

Zrzuty „przed”: `TestResults/before-builtin/` (54 PNG; galerie `MainScenePlayTests.CaptureScreenshots`,
`ArenaFxPlayTests.ArenaAndFxGallery`, `HellFxPlayTests.Gallery`, `HeroModelPlayTests.HeroGallery`,
`DemonPlayTests.DemonGallery`; 5/5 testów zaliczonych).

## 2. Zależności od Built-in (inwentarz)

**Shadery własne**
- `Resources/HellBody.shader` – surface shader `Standard` (+ `_HellSurface`: cień, woda, żar, elita).
- `Resources/HellVfx/HellParticle.shader` – CG unlit, `_MainTex/_TintColor/_Intensity/_DstBlend`.
- `Resources/HellVfx/HellGround.shader` – CG unlit, strefy na ziemi.

**Materiały (.mat)**
- 72 × `Standard` (modele bohaterów, demonów i broni w `Models/`).
- 3 × `Legacy Shaders/Particles/Additive` + 1 × `Alpha Blended` (`Resources/TurrisFx*.mat`).

**Shader.Find / nazwy shaderów w kodzie**
- `FxMaterials`: `Legacy Shaders/Particles/Additive`, `.../Alpha Blended`, fallback `Sprites/Default`.
- `HellVfxMaterials.Rock`: `Standard` (`_Glossiness`, `_EmissionColor`).
- `HellBodySurface`: `Turris/HellBody`, warunek `shader.name == "Standard"`.
- `HeroAssetImport` (edytor): `Standard` + `_MainTex/_Color/_MetallicGlossMap/_GlossMapScale/_BumpMap/_EmissionMap`.
- Testy: `HellFxPlayTests`, `HellMigrationGalleryTests`, `FootPosePlayTests` – `Standard`.

**Nazwy właściwości Built-in w kodzie**
- `MaterialLibrary` (`PartBuilder.cs`): `_Glossiness` – w URP Lit istnieje jako *ukryta, nieużywana*; `HasProperty` = true, efekt = brak.
- `TintSet` (`PartBuilder.cs`): `_Color` w MaterialPropertyBlock (błysk trafienia/poświata) – w URP Lit też ukryta i nieużywana.
- `FxDecal`, `HellCocoon`: `_TintColor` + `_Color`.

**Oświetlenie / kamera / inne**
- Światła punktowe efektów `ForcePixel` (do kilkunastu naraz) – Built-in limit 4–8 świateł pikselowych; w URP Forward+.
- Kamery tworzone w kodzie (`GameRoot`, `BestiaryStage`, testy) – w URP post-processing na kamerze domyślnie wyłączony.
- `BestiaryStage`: `Camera.Render()` do RenderTexture – wspierane w URP.
- UI: IMGUI (`OnGUI`) – niezależne od pipeline'u, wrażliwe na Linear.
- Tekstury proceduralne `FxMaterials` (RGBA32, sRGB) – wrażliwe na Linear.
- Brak: projektorów, `OnRenderImage`, `CommandBuffer`, `GrabPass`, lightmap.

## 3. Checklista

- [x] Gałąź + migawka, baseline zrzutów i ustawień
- [x] URP 17.6.0, asset pipeline + renderer (Forward+), przypisanie w Graphics (poziomy jakości dziedziczą domyślny)
- [x] Konwerter materiałów Built-in → URP (`Converters.RunInBatchMode`, Material + ReadonlyMaterial): 72 × Standard → Lit, GUID bez zmian
- [x] `Turris/HellBody` → URP (kopia Lit 17.6 + `_HellSurface`, przebiegi ForwardLit/ShadowCaster/DepthOnly/DepthNormals/MotionVectors)
- [x] `Turris/HellParticle`, `Turris/HellGround` → URP HLSL (ta sama nazwa, ścieżka i GUID)
- [x] Legacy Particles → `Turris/FxAdditive`, `Turris/FxAlphaBlended` (ten sam wzór 2×kolor×tint×tekstura, soft particles, mgła)
- [x] Kod: `Standard` → URP Lit, `_Glossiness` → `_Smoothness`, `_Color` → `_BaseColor` (MaterialPropertyBlock błysku trafień)
- [x] Post-processing na kamerach gry (`RenderingSetup`)
- [x] Kompilacja, konsola, testy, zrzuty URP (Gamma)
- [x] Linear, HDR, Volume (Bloom, ACES, Color Adjustments, winieta), kalibracja świateł i ekspozycji
- [x] Testy EditMode + PlayMode, zrzuty „po”, porównanie, pomiar wydajności

## 4. Co zmieniono

**Pakiety i ustawienia:** `com.unity.render-pipelines.universal` 17.6.0; Color Space Linear; `GraphicsSettings`
→ `Settings/URP/TurrisURP.asset`; URP ustawił `m_LightsUseLinearIntensity` i temperaturę barwową świateł;
VFX Graph uzupełnił swoje zasoby obliczeniowe w `VFXManager.asset` (przyda się w etapie efektów).

**`Settings/URP/`:**
- `TurrisURP.asset`: HDR, MSAA 2×, Depth Texture (soft particles), bez Opaque Texture, cienie główne 4096 / 4 kaskady / 80 m,
  miękkie cienie, światła dodatkowe per pixel (8 na obiekt), color grading HDR.
- `TurrisURP_Renderer.asset`: Universal Renderer, **Forward+** (efekty tworzą wiele krótkich świateł punktowych).
- `UniversalRenderPipelineGlobalSettings.asset`, `DefaultVolumeProfile.asset` – profil domyślny działa w każdej scenie
  i na każdej kamerze (także w scenach testów): Bloom (próg 1,0, intensywność 0,35, scatter 0,6, HQ), Tonemapping ACES,
  Color Adjustments (ekspozycja +1,0, kontrast 8, nasycenie −6), Vignette 0,22 / 0,45.

**Shadery (`Shaders/`, `Resources/`):** `TurrisCommon.hlsl` (`TurrisColor` – stałe kolorów zapisane w gammie przeliczane
w Linear, `TurrisGammaOf`), `TurrisFxParticle.hlsl` + `TurrisFxAdditive.shader` / `TurrisFxAlphaBlended.shader`,
`HellBodyForward.hlsl` + `HellBody.shader`, `HellParticle.shader`, `HellGround.shader`. Wszystkie zgodne z SRP Batcherem.

**Materiały:** 72 materiały modeli (bohaterowie, demony, broń) – Lit z zachowanymi teksturami albedo/normal/metal;
4 × `Resources/TurrisFx*.mat` przepięte na nowe shadery (te same GUID, tint i _InvFade).

**Kod (tylko renderowanie, bez logiki gry):**
- `RenderingSetup` (nowy): włącza post-processing na kamerach gry; `PointIntensity` – kompensacja spadku światła
  punktowego (URP 1/d² zamiast łagodnej krzywej Built-in: × max(1; 0,03·zasięg²)); stałe kalibracji
  `SunIntensity = 2`, `AmbientScale = 1,6`.
- `GameRoot`: natężenie słońca i mnożnik światła otoczenia z `RenderingSetup`, `Prepare(cam)`.
- `FxLibrary.PointLight`, `HellFxPool.FlashLight`, `DemonPortal`, `BestiaryStage`: `RenderingSetup.PointIntensity`.
- `MaterialLibrary`: `LitShader`, `_Smoothness`; `TintSet`: `_BaseColor`; `HellBodySurface`: kopiuje materiały Lit;
  `HellVfxMaterials.Rock`, `FxMaterials` (nazwy shaderów), `HeroAssetImport` (właściwości Lit).
- `TurrisDamnatorum.Runtime.asmdef`: referencje `Unity.RenderPipelines.Core/Universal.Runtime`.
- Testy: `Standard` → `MaterialLibrary.LitShader` (3 pliki); galeria aren używa tej samej kalibracji światła co gra.

## 5. Wyniki weryfikacji (2026-10-23, edytor przez MCP)

- Kompilacja: 0 błędów; konsola: 0 błędów; `validate_shaderErrors` 0; brakujące referencje 0; brakujące skrypty 0.
- Shadery Turris skompilowane dla D3D12 we wszystkich przebiegach, w Gamma i Linear (błąd `SRGBToLinear` w gałęzi
  Linear wykryty przez galerię i naprawiony dołączeniem `Color.hlsl`).
- EditMode: **156 zaliczonych, 0 błędów, 1 pominięty** (opcjonalny eksport WAV).
- PlayMode (z galeriami): **115 / 117**. Niezaliczone: `Demon_Walks_AndRecoilsFromBlow` (waga chodu 0,43 < 0,8)
  i `Impact_HitstopBothSides_KnockbackAndBodyReaction` (hitstop). Oba padają identycznie po tymczasowym przełączeniu
  na Built-in (bez zapisu), więc nie wynikają z pipeline'u. Edytor bez fokusu był dławiony do ~15 kl./s
  (`dt` = 0,0667 s); oba testy zależą od długości klatki (`CharacterVisual.LateUpdate` → `Tick(Time.deltaTime)`,
  hitstop 0,05 s < 1 klatka). **Do potwierdzenia:** powtórzyć oba przy aktywnym oknie Unity.
- Zrzuty: `TestResults/before-builtin` (54), `urp-gamma`, `urp-linear-v1..v3`, `after-urp-linear` (94, pełny przebieg);
  0 klatek magenta. Arkusze przed/po: `TestResults/porownanie-urp/*.png`.
- Jasność względem Built-in (średnia luminancja): scena gry ~0,75, efekty na arenie ~0,79, bohaterowie ~0,70,
  demony ~0,71, areny ~0,56. Celowo ciemniej (dark fantasy); kalibracja na scenie piętra I.
- Wydajność (URP, edytor, scena piętra I, 1920×1080, 40 renderów kamery z synchronizacją GPU): **4,8 ms/klatkę**
  (7 świateł, 46 ParticleSystem, 110 rendererów, SRP Batcher on). Pomiaru Built-in przed migracją nie wykonano –
  brak liczbowego porównania; wynik jest daleko w budżecie 16,7 ms.

## 6. Znane problemy i obserwacje

- **Ciemne albedo pod Linear.** Jasny wygląd Built-in w dużej mierze dawały odbicia skyboxa przy gammowym F0 0,22
  (posadzka: albedo 0,11, ~70/255 z 95/255 jasności pochodziło z odbić). URP liczy fizycznie (F0 0,04). Kalibracja
  słońce/otoczenie/ekspozycja przywraca ~75% jasności; docelowo warto podnieść albedo palet aren (treść, nie kod).
- Areny w galerii testowej są najciemniejsze (~56%), szczególnie Krypta i Szczyt – do oceny w grze.
- Księżyc na Szczycie zmienił barwę z brzoskwiniowej na białą (ACES odbarwia bardzo jasne kolory) – do korekty emisji.
- Galerie w pełnym przebiegu PlayMode dostają tło z areny poprzedniego testu (wyciek obiektów między testami,
  niezależny od migracji); do porównań używać przebiegu 5 galerii (`urp-linear-v3`).
- Pierwszy zrzut galerii bohaterów w długiej serii bywa ciemny (raz w 3 przebiegach, nie odtworzono osobno).
- KarnelLabs MCP: `unity_editor_runTests` nie działa z Test Framework 1.8 (`ExecutionSettings`), a `debug_screenshot`
  bez fokusu zwraca nieodświeżoną klatkę. Testy i zrzuty uruchamiano przez UnityMCP (`run_tests`) i galerie.
