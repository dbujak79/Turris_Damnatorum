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
