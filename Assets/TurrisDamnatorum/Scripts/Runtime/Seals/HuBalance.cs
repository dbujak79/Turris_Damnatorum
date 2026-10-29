using System.Collections.Generic;

namespace Turris
{
    /// <summary>
    /// Jawne korekty Turris nałożone na dane skilli z Hell Unleashed po wczytaniu katalogu. Dane HU zostają nietknięte
    /// (generuje je importer); każda korekta ma powód z audytu balansu i jest sprawdzana testem. Wpis: id skilla HU,
    /// poziom HU (1–3, 0 = wszystkie) i wartości <c>klucz=liczba</c>.
    /// </summary>
    public static class HuBalance
    {
        public struct Patch
        {
            public string skill; public int level; public string[] values; public string reason;
        }

        public static readonly Patch[] Patches =
        {
            // Audyt P1-b: chochliki żyły dłużej, niż trwa odnowienie (12–15 s przy 10 s), więc z jednego slotu były stałe
            // dla każdej klasy. Czas życia ≤ odnowienie – tak jak Legion w HU.
            new Patch { skill = "skill.summon.imps", level = 0, values = new[] { "duration_seconds=9" }, reason = "audyt P1-b: stałe sługi z jednego slotu" },
            new Patch { skill = "skill.summon.imps", level = 3, values = new[] { "duration_seconds=10" }, reason = "audyt P1-b: stałe sługi z jednego slotu" },
            // Ta sama zasada na poziomie III: ogary 16 s przy 12 s, golem 22 s przy 18 s (sługi na zakładkę).
            new Patch { skill = "skill.summon.hounds", level = 3, values = new[] { "duration_seconds=12" }, reason = "audyt P1-b: sługi dłużej niż odnowienie" },
            new Patch { skill = "skill.summon.golem", level = 3, values = new[] { "duration_seconds=18" }, reason = "audyt P1-b: sługi dłużej niż odnowienie" },
            // Audyt skilli P1-b (pomiar z oknem = odnowienie): na pojedynczy cel chochliki 170/s, golem 155/s, ogary 106/s
            // przy medianie 28/s. Obrażenia ×0,4 / ×0,45 / ×0,6 (liczebność i czas życia bez zmian) – ok. 2–2,5× mediany,
            // bo w grze sługi giną i biorą na siebie ciosy (manekiny ich nie atakują).
            new Patch { skill = "skill.summon.imps", level = 1, values = new[] { "damage.fire=5" }, reason = "audyt skilli P1-b: sługi 6× mediany" },
            new Patch { skill = "skill.summon.imps", level = 2, values = new[] { "damage.fire=6" }, reason = "audyt skilli P1-b: sługi 6× mediany" },
            new Patch { skill = "skill.summon.imps", level = 3, values = new[] { "damage.fire=8" }, reason = "audyt skilli P1-b: sługi 6× mediany" },
            new Patch { skill = "skill.summon.golem", level = 1, values = new[] { "damage.physical=10", "damage.earth=5" }, reason = "audyt skilli P1-b: sługi 5,5× mediany" },
            new Patch { skill = "skill.summon.golem", level = 2, values = new[] { "damage.physical=13", "damage.earth=6" }, reason = "audyt skilli P1-b: sługi 5,5× mediany" },
            new Patch { skill = "skill.summon.golem", level = 3, values = new[] { "damage.physical=13", "damage.earth=6" }, reason = "audyt skilli P1-b: sługi 5,5× mediany" },
            new Patch { skill = "skill.summon.hounds", level = 1, values = new[] { "damage.physical=4" }, reason = "audyt skilli P1-b: sługi 3,8× mediany" },
            new Patch { skill = "skill.summon.hounds", level = 2, values = new[] { "damage.physical=5" }, reason = "audyt skilli P1-b: sługi 3,8× mediany" },
            new Patch { skill = "skill.summon.hounds", level = 3, values = new[] { "damage.physical=6" }, reason = "audyt skilli P1-b: sługi 3,8× mediany" },
            // Audyt skilli P1-a (docs/AUDYT_SKILLI_PIECZECI.md): aura nie zajmuje czasu akcji, a dawała 60/s na cel
            // (2,2× mediany pojedynczego celu) – 12/17/24 → 8/11/14, ok. 35/s na cel na poziomie III.
            new Patch { skill = "skill.aura.holy_fire", level = 1, values = new[] { "damage.fire=8" }, reason = "audyt skilli P1-a: stałe obrażenia bez kosztu akcji" },
            new Patch { skill = "skill.aura.holy_fire", level = 2, values = new[] { "damage.fire=11" }, reason = "audyt skilli P1-a: stałe obrażenia bez kosztu akcji" },
            new Patch { skill = "skill.aura.holy_fire", level = 3, values = new[] { "damage.fire=14" }, reason = "audyt skilli P1-a: stałe obrażenia bez kosztu akcji" },
            // Raport botów Wojownika (docs/raporty_botow/RAPORT_WOJOWNIK.md): skill startowy Rozpłatanie wygrywał 1/14 podejść
            // wobec 19/26 Cięcia wichru. Obrażenia +15% (34/38/38 → 39/44/44); po drugiej serii (nadal 1/13) odnowienie −30% (5 → 3,5 s);
            // potem powalenie trafionych na 1 s (bossy odporne) w zamian za odnowienie 4 s.
            // Seria 5 (skille przypisane do broni, cel: wiele skilli w użyciu): Rozpłatanie miało 41,8 obr./s odnowienia, Cięcie wichru 30,4,
            // a pozostałe skille wojownika 7–19 – gracz nie wymieniał startowych. Cel dla wszystkich: ok. 24–28 obr./s odnowienia.
            // Rozpłatanie: 32/36/36, odnowienie 5 s, powalenie zostaje (~27).
            new Patch { skill = "skill.warrior.cleave", level = 1, values = new[] { "damage.physical=32", "cooldown=5", "down_seconds=1" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            new Patch { skill = "skill.warrior.cleave", level = 2, values = new[] { "damage.physical=36", "cooldown=5", "down_seconds=1" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            new Patch { skill = "skill.warrior.cleave", level = 3, values = new[] { "damage.physical=36", "cooldown=5", "down_seconds=1" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            // Cięcie wichru: 32 → 28/30/30 (~27, poziomy coś dają).
            new Patch { skill = "skill.warrior.wind_slash", level = 1, values = new[] { "damage.air=28" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            new Patch { skill = "skill.warrior.wind_slash", level = 2, values = new[] { "damage.air=30" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            new Patch { skill = "skill.warrior.wind_slash", level = 3, values = new[] { "damage.air=30" }, reason = "raport botów, seria 5: skill startowy za mocny" },
            // Fala przełamania: 34,9 → ~27 (obrażenia −12%, odnowienie 7 → 8 s).
            new Patch { skill = "skill.warrior.breaker_wave", level = 1, values = new[] { "damage.water=30", "cooldown=8" }, reason = "raport botów, seria 5: ponad celem" },
            new Patch { skill = "skill.warrior.breaker_wave", level = 2, values = new[] { "damage.water=35", "cooldown=8" }, reason = "raport botów, seria 5: ponad celem" },
            new Patch { skill = "skill.warrior.breaker_wave", level = 3, values = new[] { "damage.water=40", "cooldown=8" }, reason = "raport botów, seria 5: ponad celem" },
            // Upadek kata: 18,6 → ~26 (obrażenia +10%, odnowienie 9 → 7 s).
            new Patch { skill = "skill.warrior.executioners_fall", level = 1, values = new[] { "damage.physical=46", "cooldown=7" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.executioners_fall", level = 2, values = new[] { "damage.physical=54", "cooldown=7" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.executioners_fall", level = 3, values = new[] { "damage.physical=54", "cooldown=7" }, reason = "raport botów, seria 5: poniżej celu" },
            // Piekielna szarża: 19,1 → ~26 (obrażenia 38 → 42, odnowienie 8 → 6,5 s).
            new Patch { skill = "skill.warrior.infernal_charge", level = 1, values = new[] { "damage.fire=42", "cooldown=6.5" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.infernal_charge", level = 2, values = new[] { "damage.fire=42", "cooldown=6.5" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.infernal_charge", level = 3, values = new[] { "damage.fire=42", "cooldown=6.5" }, reason = "raport botów, seria 5: poniżej celu" },
            // Trzęsienie: 16,6 → ~26 (obrażenia 34 → 40, odnowienie 8 → 6 s).
            new Patch { skill = "skill.warrior.quake", level = 1, values = new[] { "damage.earth=40", "cooldown=6" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.quake", level = 2, values = new[] { "damage.earth=40", "cooldown=6" }, reason = "raport botów, seria 5: poniżej celu" },
            new Patch { skill = "skill.warrior.quake", level = 3, values = new[] { "damage.earth=40", "cooldown=6" }, reason = "raport botów, seria 5: poniżej celu" },
            // Młynek: 7,1 → ~22 (obrażenia obrotu 11 → 23, odnowienie 9 → 6 s).
            new Patch { skill = "skill.warrior.whirlwind", level = 1, values = new[] { "damage.physical=23", "cooldown=6" }, reason = "raport botów, seria 5: daleko poniżej celu" },
            new Patch { skill = "skill.warrior.whirlwind", level = 2, values = new[] { "damage.physical=23", "cooldown=6" }, reason = "raport botów, seria 5: daleko poniżej celu" },
            new Patch { skill = "skill.warrior.whirlwind", level = 3, values = new[] { "damage.physical=23", "cooldown=6" }, reason = "raport botów, seria 5: daleko poniżej celu" },
            // Seria mag-zywioly-v1: Mag powietrza najsłabszy (6/10 i 3/10 przy 8/8/8 innych żywiołów). Ginie od pocisków,
            // a jego skill klasy Nawałnica jako jedyny skill klasy żywiołu nie kontroluje (Trzęsienie zachwiewa, Wir ciągnie,
            // Meteor odrzuca). Powietrze = odrzut i powalenie: burza powala na 0,5 s (odporność 2 s → ok. co 2,5 s),
            // Nova powietrza po odrzucie powala na 0,6 s – mag ma chwilę oddechu, strzelcy przerywają rzut.
            // Powalenie kosztuje obrażenia (jak pieczęć Grzmot): oba skille −10%.
            new Patch { skill = "skill.sub.tempest", level = 0, values = new[] { "down_seconds=0.5", "damage.air=14.4" }, reason = "seria mag-zywioly-v1: mag powietrza bez kontroli" },
            new Patch { skill = "skill.nova.air", level = 1, values = new[] { "down_seconds=0.6", "damage.air=21.6" }, reason = "seria mag-zywioly-v1: mag powietrza bez kontroli" },
            new Patch { skill = "skill.nova.air", level = 2, values = new[] { "down_seconds=0.6", "damage.air=26.4" }, reason = "seria mag-zywioly-v1: mag powietrza bez kontroli" },
            new Patch { skill = "skill.nova.air", level = 3, values = new[] { "down_seconds=0.6", "damage.air=31.1" }, reason = "seria mag-zywioly-v1: mag powietrza bez kontroli" },
        };

        /// <summary>Korekta pieczęci: nowa lista operacji i opis (zastępują dane HU w całości); nazwa – tylko gdy podana.</summary>
        public struct SealPatch
        {
            public string seal; public string[] ops; public string desc; public string name; public string reason;
        }

        public static readonly SealPatch[] SealPatches =
        {
            // Audyt skilli P1-a: Sztandar był najlepszym wynikiem w obu kontekstach (615/s gromada, 90/s kolos).
            new SealPatch
            {
                seal = "seal.form.holy_fire.banner", ops = new[] { "aura_banner=1", "banner_power=1.25", "radius*=1.25" },
                desc = "Ogień zostaje tam, gdzie go wzniesiono, i pali o jedną czwartą mocniej – demony trzeba w niego wciągnąć. Zasięg +25%.",
                reason = "audyt skilli P1-a: najlepsza forma w obu kontekstach",
            },
            // Nazwa „Ściana ognia” należy teraz do skilla Turris (dawny Ognisty krąg) – forma Kręgu żaru dostaje inną.
            new SealPatch
            {
                seal = "seal.form.ring.wall", name = "Szereg kręgów", ops = new[] { "cascade=2", "cascade_spacing=2", "cascade_share=1", "zone_radius*=0.75" },
                desc = "Trzy kręgi w linii wzdłuż celowania – szereg, przez który pociski przelatują i zapalają się. Każdy krąg mniejszy.",
                reason = "nazwa zajęta przez skill Ściana ognia",
            },
            // Audyt skilli P2-b: formy ≥ 1,6× bazy w gromadzie i na kolosie naraz – darmowy mnożnik zamiast nowego sposobu
            // gry. Cena w odnowieniu (+1,5 s, jak Słońce i Sopel w HU), obrażenia bez zmian: ok. 1,3–1,5× bazy.
            new SealPatch
            {
                seal = "seal.form.chain.ball",
                ops = new[] { "Chain>Projectile", "+#projectile", "speed=4.5", "lifetime=3", "pierce=99", "projectile_radius=0.5", "burst_at_end=1", "pulse_radius=2.4", "pulse_interval=0.3", "pulse_share=0.3", "cooldown+=1.5" },
                desc = "Zamiast łańcucha powolna kula piorunów: płynie przez stado 3 s i co 0,3 s razi wszystko w promieniu 2,4 m (30%). +1,5 s odnowienia.",
                reason = "audyt skilli P2-b: 1,7×/2,2× bazy",
            },
            new SealPatch
            {
                seal = "seal.form.slash.boomerang", ops = new[] { "return_share=1", "range*=0.8", "cooldown+=1.5" },
                desc = "Fala wraca do ciebie z pełną siłą, ale leci o 20% krócej. +1,5 s odnowienia.",
                reason = "audyt skilli P2-b: 2,0×/2,0× bazy",
            },
            new SealPatch
            {
                seal = "seal.form.splinter.boomerang", ops = new[] { "return_flight=1", "pierce=99", "splits=0", "cooldown+=1.5" },
                desc = "Strzała przelatuje przez wszystkich i wraca do ciebie, trafiając jeszcze raz; nie pęka. +1,5 s odnowienia.",
                reason = "audyt skilli P2-b: 2,0×/2,0× bazy",
            },
            new SealPatch
            {
                seal = "seal.form.bomb.napalm",
                ops = new[] { "+Zone", "zone_seconds=5", "zone_radius=2.6", "zone_share=0.35", "zone_interval=0.5", "damage*=0.7", "cooldown+=1.5" },
                desc = "Wybuch słabszy (70%), ale rozlewa płonącą siarkę na 5 s (35% na sekundę). +1,5 s odnowienia.",
                reason = "audyt skilli P2-b: 1,85×/1,9× bazy",
            },
            new SealPatch
            {
                seal = "seal.form.bomb.sticky",
                ops = new[] { "Fall>Mine", "+#trap", "trigger_radius=1.5", "arm_seconds=0.4", "mine_seconds=12", "charges=3", "cooldown=5", "damage*=1.1" },
                desc = "Bomba przykleja się do ziemi i czeka na demona. Trzy ładunki, +10% obrażeń, odnowienie 5 s.",
                reason = "audyt skilli P2-b: 1,65×/1,65× bazy przy krótszym odnowieniu",
            },
            // Audyt skilli P2-c: formy ≤ 0,6× bazy w obu kontekstach, bez wyraźnej korzyści użytkowej.
            new SealPatch
            {
                seal = "seal.form.sulfur.mine",
                ops = new[] { "Projectile>Mine", "@targeting=Point", "-#projectile", "+#trap", "range=8", "charges=3", "trigger_radius=1.5", "arm_seconds=0.4", "mine_seconds=12", "damage*=1.4" },
                desc = "Strzała wbija się w ziemię we wskazanym miejscu i wybucha, gdy podejdzie demon: +40% obrażeń. Trzy ładunki.",
                reason = "audyt skilli P2-c: 0,54×/0,47× bazy",
            },
            new SealPatch
            {
                seal = "seal.form.meteor.shower",
                ops = new[] { "-Explosion", "+Barrage", "strikes=5", "spread=3.5", "strike_radius=1.8", "strike_delay=0.7", "strike_interval=0.18", "strike_size=1.8", "damage*=0.8" },
                desc = "Pięć mniejszych meteorów rozsypanych po 3,5 m zamiast jednego, każdy 80%.",
                reason = "audyt skilli P2-c: 0,6×/0,6× bazy",
            },
        };

        public static void Apply(IDictionary<string, HuSkillData> skills)
        {
            foreach (var p in Patches)
            {
                if (!skills.TryGetValue(p.skill, out var data)) continue;
                for (int i = 0; i < data.levels.Length; i++)
                {
                    if (p.level != 0 && p.level != i + 1) continue;
                    var values = new Dictionary<string, double>();
                    foreach (var kv in data.levels[i].Values) values[kv.Key] = kv.Value;
                    foreach (var kv in HuParams.Parse(p.values).Values) values[kv.Key] = kv.Value;
                    data.levels[i] = new HuParams(values);
                }
            }
        }

        public static void Apply(IDictionary<string, HuSeal> seals, List<string> errors)
        {
            foreach (var p in SealPatches)
            {
                if (!seals.TryGetValue(p.seal, out var seal)) { errors.Add("HuBalance: brak pieczęci " + p.seal); continue; }
                seal.ops = SealOp.ParseAll(p.ops, errors);
                seal.desc = p.desc;
                if (!string.IsNullOrEmpty(p.name)) seal.name = p.name;
            }
        }
    }
}
