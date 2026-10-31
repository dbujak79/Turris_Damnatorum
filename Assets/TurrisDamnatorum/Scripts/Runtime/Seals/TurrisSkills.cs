using System.Collections.Generic;

namespace Turris
{
    /// <summary>
    /// Skille Turris zbudowane z komponentów HU (te same pieczęcie, poziomy i wykonawca co skille z Hell Unleashed),
    /// dopisywane do katalogu po wczytaniu danych HU. Katalog HU generuje importer, więc własne skille żyją tutaj, w kodzie.
    /// </summary>
    public static class TurrisSkills
    {
        public const string FireWall = "skill.turris.fire_wall";
        public const string Charge = "skill.turris.charge", ChargeTarget = "hu_charge";

        public static void AddTo(IDictionary<string, HuSkillData> skills, IDictionary<string, HuSkillData> byTarget, List<HuSeal> seals,
            IDictionary<string, HuSeal> sealsById, List<string> errors)
        {
            // Ściana ognia (dawny Ognisty krąg – dublet Novy ognia): ściana ~7 m w poprzek celowania płonie kilka sekund
            // i parzy wszystko, co przez nią przechodzi. Odcina drogę zamiast wybuchać wokół bohatera.
            var wall = new HuSkillData
            {
                id = FireWall, targetId = "spell_firering", name = "Ściana ognia",
                desc = "Ściana ognia w poprzek drogi demonów: płonie kilka sekund i parzy oraz podpala wszystko, co przez nią przejdzie.",
                l2 = "Poziom II: mocniej parzy.", l3 = "Poziom III: dłuższa i płonie 5 s.", classId = "",
                element = HuElement.Fire, targeting = HuTargeting.Point, minWave = 1, shop = true,
                levels = new[]
                {
                    HuParams.Parse(new[] { "cooldown=9", "mana=22", "range=8", "damage.fire=10", "wall_length=7", "wall_width=1.2", "wall_seconds=4", "wall_interval=0.5", "wall_share=1", "effect.Ignite=1" }),
                    HuParams.Parse(new[] { "cooldown=9", "mana=22", "range=8", "damage.fire=13", "wall_length=7", "wall_width=1.2", "wall_seconds=4", "wall_interval=0.5", "wall_share=1", "effect.Ignite=1" }),
                    HuParams.Parse(new[] { "cooldown=9", "mana=22", "range=8", "damage.fire=15", "wall_length=8", "wall_width=1.2", "wall_seconds=5", "wall_interval=0.5", "wall_share=1", "effect.Ignite=1" }),
                },
            };
            foreach (var t in new[] { "area", "fire", "spell", "field" }) wall.tags.Add(t);
            wall.components.Add("Wall");
            skills[wall.id] = wall;
            byTarget[wall.targetId] = wall;

            // Szarża: gwałtowny bieg na wroga (8 m w 0,4 s, zatrzymuje się na pierwszym), w trakcie 50% mniej obrażeń (osobny
            // mnożnik, poza limitem). Uderzenie na końcu z szansą powalenia (ogłuszenia). Wymaga broni dwuręcznej albo tarczy –
            // z tarczą kończy się uderzeniem tarczą, które powala zawsze i na dłużej (WeaponFamilies, PlayerCombat.HuHit).
            string[] Lvl(int dmg, float chance, float distance, int cd) => new[]
            {
                $"cooldown={cd}", $"distance={distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}", "dash_seconds=0.4", "stop_on_enemy=1",
                $"damage.physical={dmg}", "explosion_radius=1.8", "explosion_share=1", "knockback=1",
                "down_seconds=1", $"down_chance={chance.ToString(System.Globalization.CultureInfo.InvariantCulture)}", "shield_bash_down=1.5", "dash_reduction=0.5",
            };
            var charge = new HuSkillData
            {
                id = Charge, targetId = ChargeTarget, name = "Szarża",
                desc = "Gwałtowny bieg na wroga zakończony uderzeniem: 40% szansy na powalenie (ogłuszenie). W trakcie szarży 50% mniej obrażeń. Wymaga broni dwuręcznej albo tarczy – z tarczą szarżę kończy uderzenie tarczą, które powala zawsze.",
                l2 = "Poziom II: mocniejsze uderzenie, 50% szansy na powalenie.", l3 = "Poziom III: dłuższa szarża (10 m), 60% szansy na powalenie, krótsze odnowienie.",
                classId = "warrior", element = HuElement.Physical, targeting = HuTargeting.Direction, minWave = 4, shop = true,
                levels = new[]
                {
                    HuParams.Parse(Lvl(32, 0.4f, 8f, 7)),
                    HuParams.Parse(Lvl(38, 0.5f, 8f, 7)),
                    HuParams.Parse(Lvl(42, 0.6f, 10f, 6)),
                },
            };
            foreach (var t in new[] { "melee", "area", "movement" }) charge.tags.Add(t);
            charge.components.Add("Dash");
            charge.components.Add("Explosion");
            skills[charge.id] = charge;
            byTarget[charge.targetId] = charge;

            // Nieśmiertelność HU (przez 3 s żaden cios nie zabije) była za mocna – w Turris to Żelazna skóra: 80% mniej obrażeń.
            if (skills.TryGetValue("skill.sub.undying", out var undying))
            {
                undying.name = "Żelazna skóra";
                undying.desc = "Ryk, który odpycha demony. Przez 3 s berserker przyjmuje o 80% mniej obrażeń (leczenie z kradzieży życia i zabójstw o połowę słabsze); wpada w szał.";
            }

            Form(seals, sealsById, errors, "seal.form.wall.wave", "Fala ognia",
                "Ściana rusza przed siebie jak fala (3,5 m/s) i pali wszystko po drodze, ale płonie o jedną czwartą krócej.",
                "wall_speed=3.5", "wall_seconds*=0.75");
            Form(seals, sealsById, errors, "seal.form.wall.fortress", "Forteca ognia",
                "Ogień zamyka się w pierścień wokół ciebie (promień 4 m) – demony muszą przejść przez płomienie. +3 s odnowienia.",
                "@targeting=Self", "wall_ring=1", "wall_ring_radius=4", "cooldown+=3");

            // Grzmot: powalenie jako pieczęć wsparcia (dotąd miały je tylko Rozpłatanie i Odtrącenie), więc kontrolę tłumu
            // można dołożyć do dowolnego skilla zadającego obrażenia. Powalony demon przerywa atak i rzut pocisku;
            // po powaleniu ma 2 s odporności na ogłuszenie, więc strefy nie trzymają go na ziemi bez końca.
            var thunder = new HuSeal
            {
                id = "seal.turris.thunder", name = "Grzmot", kind = SealKind.Support, basePrice = 45f,
                desc = "Trafienie powala demony na 0,7 s (bossy odporne; potem 2 s odporności na ogłuszenie). 85% obrażeń.",
                forbidsComponents = new[] { "Aura", "Heal", "Undying", "Overload", "Barrier" },
            };
            thunder.ops = SealOp.ParseAll(new[] { "down_seconds+=0.7", "damage*=0.85" }, errors);
            seals.Add(thunder);
            sealsById[thunder.id] = thunder;
        }

        static void Form(List<HuSeal> seals, IDictionary<string, HuSeal> byId, List<string> errors, string id, string name, string desc, params string[] ops)
        {
            var s = new HuSeal { id = id, name = name, desc = desc, kind = SealKind.Form, skillId = FireWall, basePrice = 60f };
            s.ops = SealOp.ParseAll(ops, errors);
            seals.Add(s);
            byId[id] = s;
        }
    }
}
