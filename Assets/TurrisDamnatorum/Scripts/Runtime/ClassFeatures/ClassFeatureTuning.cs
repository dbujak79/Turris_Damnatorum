using System;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Parametry 30 cech klasowych (docs/PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md, rozdz. 8). Wartości startowe są hipotezami
    /// do strojenia – żyją w <see cref="GameConfig.classFeatures"/>, więc można je zmieniać w inspektorze bez kodu.
    /// Opisy kart czytają liczby stąd (bez ręcznie dublowanych wartości).
    /// Pola dodane później dostają wartości domyślne z inicjalizatorów (stare assety działają bez regeneracji).
    /// </summary>
    [Serializable]
    public class ClassFeatureTuning
    {
        [Header("Wojownik A – Walka w gardzie")]
        public float guardSeconds = 6f, guardCooldown = 16f;

        [Header("Wojownik B – Arsenał weterana")]
        [Tooltip("Najkrótszy odstęp między zmianami zestawu (s) – spam G nie usuwa powrotów ataku.")]
        public float arsenalSwapInterval = 0.6f;

        [Header("Paladyn A – Rozgrzeszenie")]
        [Tooltip("Pojemność magazynu Łaski – ułamek normalnego maks. życia.")]
        public float graceCapacity = 0.4f;
        [Tooltip("Obrażenia fali osądu za 1 punkt Łaski.")]
        public float graceToDamage = 1.5f;
        public float graceWaveRadius = 4.5f, graceCooldown = 3f;
        [Tooltip("Współczynniki nadleczenia → Łaska wg źródła.")]
        public float graceFromRegen = 1f, graceFromLeech = 1f, graceFromKill = 1f, graceFromFlask = 0.5f, graceFromOther = 1f;

        [Header("Paladyn B – Sąd przysięgi")]
        public float judgementSeconds = 8f, judgementCooldown = 18f, judgementRange = 12f;
        [Tooltip("Niezłomność: obniżenie odporności fizycznej celu (ułamek, cel nie schodzi poniżej −0,25).")]
        public float judgementDefianceArmour = 0.18f;
        [Tooltip("Czystość: obniżenie odporności żywiołów celu (mnożnik żywiołu +).")]
        public float judgementPurityResist = 0.2f;
        [Tooltip("Zapał: mnożnik tempa ruchu i ataku celu.")]
        public float judgementZealSlow = 0.75f;
        [Tooltip("Męstwo: mnożnik obrażeń zadawanych przez cel.")]
        public float judgementMightWeaken = 0.8f;
        [Tooltip("Odnowa: obrażenia/s na celu za każdy punkt regeneracji życia aury.")]
        public float judgementRenewalPerRegen = 6f;

        [Header("Berserker A – Czerwona linia: Żelazna skóra")]
        public float redlineSeconds = 6f;
        [Tooltip("Odnowienie – startowo jak dawna Żelazna skóra (hu_undying).")]
        public float redlineCooldown = 60f;
        [Tooltip("Ryk wejścia w trans (jak dawna Żelazna skóra): obrażenia fizyczne, promień, odrzut, furia.")]
        public float redlineRoarDamage = 25f, redlineRoarRadius = 3.4f, redlineRoarKnockback = 2.5f, redlineRoarFury = 100f;
        [Tooltip("Dostępne życie w transie: ułamek normalnego maks. życia.")]
        public float redlineCap = 0.35f;
        [Tooltip("R_max krzywej: redukcja = R_max × (1 − PŻ / normalne maks.).")]
        public float redlineMaxReduction = 0.8f;

        [Header("Berserker B – Spalenie furii")]
        public float furySecondsPerToken = 2f, furyTokenLife = 10f;
        public int furyMaxTokens = 3;

        [Header("Rębajło A – Niezgodna para")]
        public float pairSeconds = 5f, pairCooldown = 14f;
        [Tooltip("Część obrażeń własnych każdej broni we wspólnym uderzeniu.")]
        public float pairMainShare = 0.6f, pairOffShare = 0.6f;

        [Header("Rębajło B – Taniec bez przerwy")]
        public float danceSeconds = 5f, danceCooldown = 14f;

        [Header("Mag A – Dług arkanów")]
        public float debtSeconds = 6f, debtCooldown = 18f;
        [Tooltip("Limit długu – ułamek maks. many po rezerwacjach.")]
        public float debtLimit = 0.6f;

        [Header("Mag B – Płaszcz many")]
        public float cloakSeconds = 6f, cloakCooldown = 16f;
        [Tooltip("Udział obrażenia przejmowany przez płaszcz.")]
        public float cloakShare = 0.6f;
        [Tooltip("Mana za 1 PŻ ochrony.")]
        public float cloakManaPerHealth = 2f;

        [Header("Mag ognia A – Żywy piec")]
        public float furnaceSeconds = 6f, furnaceCooldown = 15f;
        [Tooltip("Siła własnego płomienia: ułamek normalnego maks. życia na sekundę (przed odpornością).")]
        public float furnaceBurnPerSecond = 0.035f;
        [Tooltip("Pojemność Żaru – ułamek normalnego maks. życia.")]
        public float furnaceEmberCap = 0.3f;
        public float furnaceEmberToDamage = 2f, furnaceEmberLife = 8f;

        [Header("Mag ognia B – Wielki pożar")]
        public float greatFireSeconds = 6f, greatFireCooldown = 18f;
        public int greatFireFamilies = 3;

        [Header("Mag wody A – Niezmienny szron")]
        public float frostSeconds = 6f, frostCooldown = 14f, frostRange = 14f;
        [Tooltip("Najczęstsze rozliczenie specjalnej reakcji na jednym celu (s).")]
        public float frostReactionInterval = 1f;
        [Tooltip("Błoto ze Szronu bez prawdziwego chłodu: premia do obrażeń ziemi zamiast ogłuszenia.")]
        public float frostMudBonus = 0.35f;

        [Header("Mag wody B – Prąd głębin")]
        public int currentMaxZones = 2;
        public float currentManaPerZone = 10f, currentRange = 16f, currentMoveSeconds = 0.5f, currentCooldown = 3f;

        [Header("Mag powietrza A – Oko ciszy")]
        public float eyeSeconds = 4f, eyeCooldown = 14f, eyeRadius = 2.6f, eyeReleaseSpeed = 22f;
        public int eyeCapacity = 4;
        [Tooltip("Budżet wrogiego pocisku jako gracza: ułamek obrażeń zwykłego ataku gracza (najwyżej oryginał).")]
        public float eyeEnemyBudget = 0.9f;

        [Header("Mag powietrza B – Zderzenie frontów")]
        public float frontsSeconds = 6f, frontsCooldown = 15f;
        [Tooltip("Wyładowanie: ułamek obrażeń zwykłego ataku gracza.")]
        public float frontsDischarge = 0.9f;
        [Tooltip("Naprężenie odpornego celu: ułamek wyładowania.")]
        public float frontsStrain = 0.35f;
        public float frontsRadius = 1.6f;
        public int frontsMaxPerSource = 4;

        [Header("Mag ziemi A – Bastion ziemi")]
        public float wallLength = 4.5f, wallHeight = 2.2f, wallThickness = 0.6f, wallSeconds = 8f, wallCooldown = 14f, wallDistance = 3.5f;
        [Tooltip("Trwałość ściany – ułamek maks. życia gracza.")]
        public float wallDurability = 1.2f;
        [Tooltip("Mnożnik niszczenia ściany przez bossa.")]
        public float wallBossDamage = 4f;
        [Tooltip("Pęknięcie: ułamek obrażeń trafienia, które je pobudziło.")]
        public float wallCrackShare = 0.8f;

        [Header("Mag ziemi B – Krusząca skorupa")]
        public int shellMaxPlates = 3;
        public float shellPlateFactor = 1.1f, shellPlateLife = 15f, shellCooldown = 6f;

        [Header("Przywoływacz A – Wcielenie")]
        public float incarnationSeconds = 8f, incarnationCooldown = 16f;

        [Header("Przywoływacz B – Chimera legionu")]
        public float chimeraRadius = 10f, chimeraPreview = 3f, chimeraCooldown = 12f;
        public int chimeraMaxParts = 3;

        [Header("Mag bitewny A – Arkana oręża")]
        public float arcanaSeconds = 6f, arcanaCooldown = 16f;

        [Header("Mag bitewny B – Dwa takty")]
        public float beatsSeconds = 6f, beatsCooldown = 16f;

        [Header("Łotrzyk A – Fałszywy świadek")]
        public float decoySeconds = 5f, decoyCooldown = 15f, decoyHealth = 0.3f, decoyRange = 12f;
        public int decoyMaxDeceived = 4;

        [Header("Łotrzyk B – Kradzież sztuczki")]
        public float stealObserve = 5f, stealTrickLife = 12f, stealCooldown = 10f, stealRange = 14f;
        [Tooltip("Budżet skradzionej sztuczki – wielokrotność obrażeń zwykłego ataku gracza.")]
        public float stealDamage = 1.6f;

        [Header("Łucznik A – Gwoździe łowcy")]
        public float nailsSeconds = 6f, nailsCooldown = 14f, nailsShare = 0.35f, nailsRip = 1.4f, nailsRange = 16f, nailsExpire = 5f;
        public int nailsPerTarget = 6, nailsPerActivation = 24;

        [Header("Łucznik B – Stanowisko strzeleckie")]
        public float postSeconds = 8f, postCooldown = 14f, postPlaceRange = 9f, postLinkRange = 16f;

        [Header("Zabójca A – Przygwożdżony cień")]
        public float shadowSeconds = 6f, shadowCooldown = 14f, shadowRange = 10f, shadowLink = 14f;

        [Header("Zabójca B – Otwarta tętnica")]
        public float arterySeconds = 4f, arteryCooldown = 12f, arteryRange = 12f;

        [Header("Łucznik arkanów A – Strzała zaklęcia")]
        public float spellArrowPrepare = 4f, spellArrowCharge = 6f;

        [Header("Łucznik arkanów B – Strzała odpowiedzi")]
        public int answerShots = 3;
        public float answerSeconds = 8f, answerCooldown = 12f;

        public static float Pct(float v) => Mathf.Round(v * 100f);
    }
}
