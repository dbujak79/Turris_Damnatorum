using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Bohaterowie przeniesieni z Hell Unleashed (docs/PORT_HELL_UNLEASHED.md): trzy typy bohatera (Wojownik, Mag, Łotrzyk –
    /// w miejscu dawnego Rycerza, Maga i Łotrzyka, z tymi samymi id) i dwanaście specjalizacji, ich bronie startowe oraz skille.
    /// Obrażenia z Hell Unleashed × 2,5 (skala Turris), zasięgi, promienie, czasy i odnowienia – bez zmian.
    /// Modele 3D broni i wygląd bohaterów przypisuje edytor (HeroAssetImport) – tu tylko dane.
    /// </summary>
    public static partial class DefaultContent
    {
        const float HuDamage = 2.5f;

        static readonly Color Steel = new Color(0.82f, 0.83f, 0.86f);
        static readonly Color FireC = new Color(1f, 0.5f, 0.15f), FrostC = new Color(0.55f, 0.85f, 1f), StormC = new Color(0.78f, 0.82f, 1f),
            EarthC = new Color(0.78f, 0.6f, 0.35f), BloodC = new Color(0.85f, 0.2f, 0.2f), HolyC = new Color(1f, 0.82f, 0.45f), SpiritC = new Color(0.45f, 1f, 0.65f);

        static void AddHeroes(GameConfig cfg, ClassDefinition warrior, ClassDefinition mage, ClassDefinition rogue,
            ItemDefinition swordTemplate, ItemDefinition greatAxeTemplate, ItemDefinition daggerTemplate, ItemDefinition bowTemplate,
            ItemDefinition heaterTemplate, ItemDefinition chain, ItemDefinition robe, ItemDefinition leather, ItemDefinition rogueHood)
        {
            // ============================================================ BRONIE
            ItemDefinition Melee(ItemDefinition src, string id, string name, string desc, WeaponModel model, float light, float heavy, BuildTag tags, params AttributeRequirement[] reqs)
            {
                var w = Make<ItemDefinition>(id, name, desc, tags);
                w.kind = ItemKind.Weapon; w.weight = src.weight; w.color = Steel; w.twoHanded = src.twoHanded; w.weaponModel = model;
                w.requirements.AddRange(reqs);
                var d = src.weapon;
                w.weapon = new WeaponData
                {
                    light = d.light.Clone(), heavy = d.heavy.Clone(), lightComboLength = d.lightComboLength,
                    strengthScaling = d.strengthScaling, dexterityScaling = d.dexterityScaling, intelligenceScaling = d.intelligenceScaling,
                    riposteMultiplier = d.riposteMultiplier, canBlock = d.canBlock, guard = d.guard, canParry = d.canParry, parry = d.parry,
                };
                w.weapon.light.baseDamage = light; w.weapon.heavy.baseDamage = heavy;
                w.weapon.light.statuses = new List<StatusApplication>();
                w.weapon.heavy.statuses = new List<StatusApplication>();
                return w;
            }
            void OnHit(ItemDefinition w, StatusKind k, int stacks = 1, float lightChance = 0.5f)
            {
                w.weapon.light.statuses.Add(St(k, stacks, lightChance));
                w.weapon.heavy.statuses.Add(St(k, stacks));
            }
            void SetElement(ItemDefinition w, Element e, float share)
            {
                foreach (var a in new[] { w.weapon.light, w.weapon.heavy }) { a.element = e; a.elementShare = share; }
            }

            var sword = Melee(swordTemplate, "hu_sword", "Miecz jednoręczny", "Kierunkowe cięcie łukiem. Obrażenia fizyczne, krwawienie.", WeaponModel.Sword, 40, 68,
                BuildTag.Melee | BuildTag.Guard | BuildTag.Parry, Req(AttributeType.Strength, 10));
            OnHit(sword, StatusKind.Bleed);
            var broad = Melee(swordTemplate, "hu_sword_broad", "Miecz szeroki", "Szeroka głownia, solidny jelec. Wymaga Siły 11.", WeaponModel.Sword, 45, 76,
                BuildTag.Melee | BuildTag.Guard | BuildTag.Parry, Req(AttributeType.Strength, 11));
            OnHit(broad, StatusKind.Bleed);
            ItemDefinition ElementSword(string id, string name, Element e, StatusKind k, string desc, Color c)
            {
                var w = Melee(swordTemplate, id, name, desc, WeaponModel.Sword, 40, 68, BuildTag.Melee | BuildTag.Guard | BuildTag.Parry | BuildTag.Magic, Req(AttributeType.Strength, 10));
                w.color = c;
                SetElement(w, e, 0.625f);
                OnHit(w, StatusKind.Bleed);
                OnHit(w, k, 1, 0.35f);
                return w;
            }
            var swordFire = ElementSword("hu_sword_fire", "Płonący miecz", global::Turris.Element.Fire, StatusKind.Burn, "Cięcie ogniem i stalą: podpala i rani do krwi.", FireC);
            var swordWater = ElementSword("hu_sword_water", "Ostrze przypływu", global::Turris.Element.Frost, StatusKind.Chill, "Cięcie wodą i stalą: wychładza, spowalnia i rani do krwi.", FrostC);
            var swordAir = ElementSword("hu_sword_air", "Ostrze wichru", global::Turris.Element.Lightning, StatusKind.Shock, "Cięcie wiatrem i stalą: poraża i rani do krwi.", StormC);
            var swordEarth = ElementSword("hu_sword_earth", "Kamienny miecz", global::Turris.Element.Earth, StatusKind.Stagger, "Cięcie kamieniem i stalą: zachwiewa i rani do krwi.", EarthC);

            var greatsword = Melee(greatAxeTemplate, "hu_greatsword", "Miecz dwuręczny", "Wolniejszy zamach, szerszy łuk, wyższe obrażenia i lekki odrzut. Wyklucza tarczę.", WeaponModel.GreatSword, 100, 170,
                BuildTag.Melee | BuildTag.Heavy | BuildTag.Guard, Req(AttributeType.Strength, 14));
            OnHit(greatsword, StatusKind.Bleed);
            var greataxe = Melee(greatAxeTemplate, "hu_greataxe", "Topór dwuręczny", "Szerokie ostrze w kształcie półksiężyca: głębokie rany, dwa krwawienia. Wymaga Siły 16.", WeaponModel.GreatAxe, 110, 187,
                BuildTag.Melee | BuildTag.Heavy | BuildTag.Guard, Req(AttributeType.Strength, 16));
            OnHit(greataxe, StatusKind.Bleed, 2, 0.6f);
            var warhammer = Melee(greatAxeTemplate, "hu_warhammer", "Młot bojowy", "Wolny jak lawina i tak samo nie do zatrzymania. Kruszy pancerz. Wymaga Siły 16.", WeaponModel.Hammer, 112, 200,
                BuildTag.Melee | BuildTag.Heavy | BuildTag.Guard, Req(AttributeType.Strength, 16));
            OnHit(warhammer, StatusKind.Stagger, 2, 0.6f);
            OnHit(warhammer, StatusKind.Sunder, 1, 0.6f);
            foreach (var a in new[] { warhammer.weapon.light, warhammer.weapon.heavy }) { a.poiseDamage *= 1.4f; a.bonusVsFrozen = 0.5f; }
            var flail = Melee(swordTemplate, "hu_flail", "Korbacz", "Kula na łańcuchu: szeroki zamach, zachwiewa. Wymaga Siły 12.", WeaponModel.Mace, 50, 90,
                BuildTag.Melee | BuildTag.Heavy, Req(AttributeType.Strength, 12));
            OnHit(flail, StatusKind.Stagger, 1, 0.4f);
            flail.weapon.canParry = false;
            foreach (var a in new[] { flail.weapon.light, flail.weapon.heavy }) { a.radius *= 1.2f; a.poiseDamage *= 1.3f; }

            var shield = Make<ItemDefinition>("hu_shield", "Tarcza", "Pełna ochrona przed zwykłymi ciosami; garda pod osobnym przyciskiem.", BuildTag.Shield | BuildTag.Guard | BuildTag.Parry);
            shield.kind = ItemKind.Shield; shield.weight = 5; shield.color = new Color(0.45f, 0.4f, 0.32f); shield.shield = heaterTemplate.shield;

            // Różdżki: szybki pocisk żywiołu (lekki atak) i mocniejszy, wolniejszy (ciężki). Rysowane jako kostur żywiołu.
            ItemDefinition Wand(string id, string name, Element e, float dmg, float speed, string desc, Color c, StatusKind? k, float knock = 0f)
            {
                var w = Make<ItemDefinition>(id, name, desc, BuildTag.Magic);
                w.kind = ItemKind.Weapon; w.weight = 1; w.color = c; w.weaponModel = WeaponModel.Staff;
                w.requirements.Add(Req(AttributeType.Intelligence, 10));
                w.weapon = new WeaponData
                {
                    light = Proj(name, dmg, 0.2f, speed, poise: 8, guardLoad: 10, recovery: 0.22f),
                    heavy = Proj(name + " – pełna moc", dmg * 2.2f, 0.6f, speed * 1.15f, poise: 25, guardLoad: 25, heavy: true, recovery: 0.4f),
                    strengthScaling = 0f, dexterityScaling = 0.2f, intelligenceScaling = 1.4f, riposteMultiplier = 2f, lightComboLength = 3,
                };
                foreach (var a in new[] { w.weapon.light, w.weapon.heavy })
                {
                    a.element = e; a.staminaCost = 4; a.animation = AttackAnim.Cast;
                    if (k.HasValue) a.statuses.Add(St(k.Value, 1, a == w.weapon.light ? 0.5f : 1f));
                }
                w.modifiers.Add(Pct(StatType.SpellPower, 5));
                return w;
            }
            var wandFire = Wand("hu_wand_fire", "Różdżka ognia", global::Turris.Element.Fire, 28, 16f, "Szybki pojedynczy pocisk ognia, który podpala.", FireC, StatusKind.Burn);
            var wandWater = Wand("hu_wand_water", "Różdżka wody", global::Turris.Element.Frost, 25, 14f, "Pocisk wody, który wychładza i spowalnia.", FrostC, StatusKind.Chill);
            var wandAir = Wand("hu_wand_air", "Różdżka powietrza", global::Turris.Element.Lightning, 23, 19f, "Podmuch z iskrą: poraża i lekko odpycha.", StormC, StatusKind.Shock);
            var wandEarth = Wand("hu_wand_earth", "Różdżka ziemi", global::Turris.Element.Earth, 42, 12f, "Wolny, ciężki kamień. Zachwiewa; pełne zachwianie ogłusza.", EarthC, StatusKind.Stagger);

            // ============================================================ SKILLE
            SpellDefinition Hu(string id, string name, SpellKind kind, bool spell, float cost, float cooldown, float huDamage, string desc, Color color,
                Element el = global::Turris.Element.None, float elementShare = 0f, float radius = 0f, float reach = 0f)
            {
                var s = Make<SpellDefinition>(id, name, desc, spell ? BuildTag.Magic : BuildTag.Melee);
                s.kind = kind; s.category = spell ? SkillCategory.Spell : SkillCategory.Technique; s.flatDamage = true;
                s.manaCost = spell ? cost : 0f; s.staminaCost = spell ? 0f : cost; s.intelligenceScaling = spell ? 1f : 0f;
                s.cooldown = cooldown; s.castTime = 0.25f; s.recovery = 0.35f; s.cancelAfter = 0.2f;
                float dmg = huDamage * HuDamage;
                s.attack = Atk(name, dmg, 0.25f, 0.1f, 0.35f, reach, radius, 0, 0, 25, 20, type: spell ? DamageType.Magic : DamageType.Physical, delivery: AttackDelivery.AreaAroundSelf);
                s.attack.parryable = false;
                s.attack.element = el; s.attack.elementShare = elementShare;
                s.amount = dmg; s.color = color; s.powerPerLevel = 0.12f; s.maxLevel = 2;
                return s;
            }

            // ---- Skille klas (sygnatury: osobny przycisk, jeden poziom)
            var warcry = Hu("hu_warcry", "Okrzyk bojowy", SpellKind.Warcry, false, 24, 14, 8, "Ryk, który odrzuca i zachwiewa demony wokół. Przez 5 s +20% redukcji obrażeń i +35 furii.", new Color(1f, 0.45f, 0.3f), radius: 3.6f);
            warcry.telegraph = 0.05f; warcry.knockback = 2.5f; warcry.damageReductionBuff = 0.2f; warcry.duration = 5f; warcry.furyGain = 35f;
            warcry.attack.statuses.Add(St(StatusKind.Stagger));
            var undying = Hu("hu_undying", "Żelazna skóra", SpellKind.Undying, false, 0, 60, 10, "Ryk, który odpycha demony. Przez 3 s berserker przyjmuje o 80% mniej obrażeń (leczenie z kradzieży życia i zabójstw o połowę słabsze); wpada w szał.", new Color(1f, 0.15f, 0.05f), radius: 3.4f);
            undying.telegraph = 0.05f; undying.knockback = 2.5f; undying.duration = 3f; undying.furyGain = 100f;
            undying.attack.statuses.Add(St(StatusKind.Stagger));
            var bladeDance = Hu("hu_blade_dance", "Taniec ostrzy", SpellKind.Whirlwind, false, 15, 12, 12, "Wir dwóch ostrzy przez 2,4 s – w biegu – zakończony szerokim cięciem.", new Color(0.85f, 0.88f, 1f), radius: 2.5f);
            bladeDance.duration = 2.4f; bladeDance.tickInterval = 0.2f; bladeDance.moveMultiplier = 0.95f; bladeDance.finalSlashShare = 2f; bladeDance.finalRadius = 3f;
            bladeDance.attack.statuses.Add(St(StatusKind.Bleed));
            var judgement = Hu("hu_judgement", "Boski osąd", SpellKind.Nova, false, 15, 16, 48, "Święty ogień wokół paladyna: obrażenia, odrzut, a paladyn odzyskuje 15% zdrowia.", HolyC,
                global::Turris.Element.Fire, 0.5f, radius: 4f);
            judgement.telegraph = 0.1f; judgement.knockback = 2f; judgement.healPercent = 0.15f;
            judgement.attack.statuses.Add(St(StatusKind.Stagger));
            var overload = Hu("hu_overload", "Przeciążenie", SpellKind.Overload, true, 20, 12, 0, "Następny skill ze slotu zadaje +60% obrażeń i ma większy zasięg. Potem sloty przegrzewają się na 2,5 s.", new Color(0.75f, 0.45f, 1f));
            var meteorHu = Hu("hu_meteor", "Meteor", SpellKind.Meteor, true, 20, 12, 70, "Kamień z ognia spada we wskazane miejsce: wielki wybuch, podpalenie i płonąca ziemia.", FireC,
                global::Turris.Element.Fire, radius: 3.2f, reach: 11f);
            meteorHu.duration = 4f; meteorHu.tickInterval = 0.5f; meteorHu.knockback = 1.5f;
            meteorHu.attack.statuses.Add(St(StatusKind.Burn));
            var maelstrom = Hu("hu_maelstrom", "Wir głębin", SpellKind.Nova, true, 20, 11, 30, "Trzy fale wody wokół maga: wciągają demony, wychładzają je i spowalniają.", FrostC,
                global::Turris.Element.Frost, radius: 4.5f);
            maelstrom.telegraph = 0.2f; maelstrom.followCaster = true; maelstrom.pulses = 3; maelstrom.pulseInterval = 0.45f; maelstrom.knockback = -0.8f;
            maelstrom.attack.statuses.Add(St(StatusKind.Chill));
            var tempest = Hu("hu_tempest", "Nawałnica", SpellKind.Zone, true, 22, 13, 4.8f, "Burza nad wskazanym miejscem: pioruny biją przez 4 sekundy.", StormC,
                global::Turris.Element.Lightning, radius: 3.6f, reach: 11f);
            tempest.duration = 4f; tempest.tickInterval = 0.3f;
            tempest.attack.statuses.Add(St(StatusKind.Shock, 1, 0.15f));
            var quakeHu = Hu("hu_quake", "Trzęsienie ziemi", SpellKind.Nova, true, 22, 13, 48, "Ziemia pęka wokół maga: obrażenia od ziemi i dwie warstwy zachwiania.", EarthC,
                global::Turris.Element.Earth, radius: 5f);
            quakeHu.telegraph = 0.35f; quakeHu.knockback = 2f;
            quakeHu.attack.statuses.Add(St(StatusKind.Stagger, 2));
            var legion = Hu("hu_legion", "Legion", SpellKind.Summon, true, 25, 28, 16, "Przyzywa dwa widmowe ogary na 16 s; gryzą do krwi.", SpiritC);
            legion.servant = ServantKind.Hound; legion.servantCount = 2; legion.duration = 16f; legion.servantAttackInterval = 0.8f;
            legion.attack.damageType = DamageType.Physical;
            legion.attack.statuses.Add(St(StatusKind.Bleed));
            var stormCleave = Hu("hu_storm_cleave", "Cięcie burzy", SpellKind.Cone, true, 15, 9, 48, "Szerokie cięcie naładowane piorunem przed siebie: fizyczne i błyskawica, odrzuca.", StormC,
                global::Turris.Element.Lightning, 0.5f, reach: 3.4f);
            stormCleave.attack.damageType = DamageType.Physical; stormCleave.arcAngle = 160f; stormCleave.knockback = 1.5f;
            stormCleave.attack.statuses.Add(St(StatusKind.Stagger));
            var bladeFanHu = Hu("hu_blade_fan", "Pierścień ostrzy", SpellKind.Projectile, false, 15, 12, 18, "Pierścień ostrzy wokół łotrzyka; trafieni krwawią.", new Color(0.85f, 0.85f, 0.9f));
            bladeFanHu.attack.delivery = AttackDelivery.Projectile; bladeFanHu.attack.projectileCount = 14; bladeFanHu.attack.spreadAngle = 360f; bladeFanHu.attack.projectileSpeed = 16f;
            bladeFanHu.projectileLifetime = 0.55f;
            bladeFanHu.attack.statuses.Add(St(StatusKind.Bleed));
            var arrowStorm = Hu("hu_arrow_storm", "Burza strzał", SpellKind.Projectile, false, 15, 12, 15, "Piętnaście strzał w szerokim wachlarzu; każda przebija pierwszego wroga.", new Color(0.9f, 0.85f, 0.7f));
            arrowStorm.attack.delivery = AttackDelivery.Projectile; arrowStorm.attack.projectileCount = 15; arrowStorm.attack.spreadAngle = 35f; arrowStorm.attack.projectileSpeed = 22f;
            arrowStorm.projectileLifetime = 0.9f; arrowStorm.pierce = 1; arrowStorm.attack.animation = AttackAnim.BowDraw;
            var deathMark = Hu("hu_death_mark", "Znak śmierci", SpellKind.Dash, false, 12, 10, 55, "Doskok i cios, który rozcina wszystko wokół: trzy warstwy krwawienia, +40% przeciw krwawiącym.", BloodC, reach: 6f);
            deathMark.dashTime = 0.16f; deathMark.attack.explosionRadius = 1.7f; deathMark.attack.bonusVsBleeding = 0.4f;
            deathMark.attack.statuses.Add(St(StatusKind.Bleed, 3));
            var elementalArrow = Hu("hu_elemental_arrow", "Strzała żywiołów", SpellKind.Projectile, true, 16, 8, 44, "Strzała z ognia i wiatru: wybucha przy trafieniu i zostawia płomień.", FireC, global::Turris.Element.Fire);
            elementalArrow.attack.delivery = AttackDelivery.Projectile; elementalArrow.attack.projectileSpeed = 20f; elementalArrow.projectileLifetime = 1.2f;
            elementalArrow.attack.explosionRadius = 2.6f; elementalArrow.fullExplosion = true;
            elementalArrow.zoneRadius = 2.2f; elementalArrow.zoneDuration = 2.5f; elementalArrow.zoneInterval = 0.5f; elementalArrow.zoneShare = 0.1f;
            elementalArrow.attack.statuses.Add(St(StatusKind.Burn)); elementalArrow.attack.animation = AttackAnim.BowDraw;

            // ---- Skille startowe (do wyboru) i z broni
            var cleaveHu = Hu("hu_cleave", "Rozpłatanie", SpellKind.Cone, false, 15, 5, 34, "Ciężkie cięcie w stożku przed wojownikiem: nakłada krwawienie i powala trafione demony na 1 s (bossy są odporne).", new Color(1f, 0.78f, 0.45f), reach: 3.2f);
            cleaveHu.arcAngle = 130f; cleaveHu.attack.statuses.Add(St(StatusKind.Bleed, 2));
            cleaveHu.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.ExtraStatusStacks, 1));
            var windSlash = Hu("hu_wind_slash", "Cięcie wichru", SpellKind.Wave, false, 15, 5, 32, "Fala ostrza leci na dystans i przenika przez wrogów.", StormC, global::Turris.Element.Lightning, 1f, reach: 10f);
            windSlash.waveWidth = 1.6f; windSlash.waveSpeed = 16f; windSlash.knockback = 0.8f;
            windSlash.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.RadiusBonus, 0.75f));
            var holyFire = Hu("hu_aura_holy_fire", "Aura świętego ognia", SpellKind.Aura, false, 0, 1, 12, "Aura: co sekundę parzy demony wokół paladyna.", HolyC, global::Turris.Element.Fire, 1f, radius: 3.5f);
            holyFire.tickInterval = 1f; holyFire.attack.damageType = DamageType.Magic; holyFire.powerPerLevel = 0.4f; holyFire.castTime = 0.05f;
            var defiance = Hu("hu_aura_defiance", "Aura niezłomności", SpellKind.Aura, false, 0, 1, 0, "Aura: pancerz i redukcja obrażeń, dopóki jej nie zgasisz.", EarthC, radius: 3.5f);
            defiance.auraModifiers.Add(Flat(StatType.PhysicalDefense, 18)); defiance.auraModifiers.Add(Flat(StatType.DamageReduction, 3)); defiance.castTime = 0.05f;
            var piercing = Hu("hu_piercing_shot", "Strzał przeszywający", SpellKind.Projectile, false, 14, 4, 46, "Ciężka strzała przebija kolejnych wrogów w linii.", new Color(0.9f, 0.85f, 0.75f));
            piercing.attack.delivery = AttackDelivery.Projectile; piercing.attack.projectileSpeed = 24f; piercing.projectileLifetime = 0.9f; piercing.pierce = 4; piercing.attack.animation = AttackAnim.BowDraw;
            piercing.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.ExtraTargets, 3));
            var splinter = Hu("hu_splinter_arrow", "Strzała rozszczepu", SpellKind.Projectile, false, 12, 5, 34, "Nacięty grot pęka na pierwszym trafieniu i rozlatuje się na dwie drzazgi, które lecą dalej i ranią do krwi.", new Color(0.85f, 0.7f, 0.55f));
            splinter.attack.delivery = AttackDelivery.Projectile; splinter.attack.projectileSpeed = 22f; splinter.projectileLifetime = 0.9f; splinter.split = true; splinter.attack.animation = AttackAnim.BowDraw;
            splinter.attack.statuses.Add(St(StatusKind.Bleed));
            splinter.attack.spreadAngle = 8f; splinter.levelFeatures.Add(new LevelFeature(2, LevelFeatureKind.ExtraTargets, 1));
            var crossCut = Hu("hu_cross_cut", "Cięcie krzyżowe", SpellKind.Cone, false, 14, 5, 34, "Dwa szybkie cięcia przed siebie; nakłada dwie warstwy krwawienia.", BloodC, reach: 2.6f);
            crossCut.arcAngle = 130f; crossCut.attack.statuses.Add(St(StatusKind.Bleed, 2));
            crossCut.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.RadiusBonus, 0.15f));
            var snare = Hu("hu_snare", "Sidła", SpellKind.Zone, false, 12, 10, 3, "Rozrzucone kolce i linki: wrogowie w obszarze zwalniają i krwawią.", new Color(0.75f, 0.65f, 0.5f), radius: 2.2f, reach: 8f);
            snare.duration = 4f; snare.tickInterval = 0.5f;
            snare.attack.statuses.Add(St(StatusKind.Chill)); snare.attack.statuses.Add(St(StatusKind.Bleed, 1, 0.3f));
            snare.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.RadiusBonus, 0.27f)); snare.levelFeatures.Add(new LevelFeature(2, LevelFeatureKind.DurationBonus, 2f));

            SpellDefinition Nova(string id, string name, Element e, float dmg, float cd, float mana, string desc, Color c)
            {
                var n = Hu(id, name, SpellKind.Nova, true, mana, cd, dmg, desc, c, e, radius: 3.5f);
                n.telegraph = 0.45f; n.followCaster = true;
                return n;
            }
            var novaFire = Nova("hu_nova_fire", "Nova ognia", global::Turris.Element.Fire, 26, 3.5f, 15, "Pierścień ognia rozchodzi się od bohatera, idzie za nim i wybucha na pełnym promieniu. Podpala.", FireC);
            novaFire.attack.statuses.Add(St(StatusKind.Burn));
            var novaWater = Nova("hu_nova_water", "Nova wody", global::Turris.Element.Frost, 18, 4f, 16, "Fala wody rozchodzi się od bohatera i na pełnym promieniu wychładza i spowalnia.", FrostC);
            novaWater.attack.statuses.Add(St(StatusKind.Chill));
            var novaAir = Nova("hu_nova_air", "Nova powietrza", global::Turris.Element.Lightning, 24, 3.5f, 15, "Podmuch rozchodzi się od bohatera i na pełnym promieniu odrzuca wrogów.", StormC);
            novaAir.knockback = 2f;
            var novaEarth = Nova("hu_nova_earth", "Nova ziemi", global::Turris.Element.Earth, 14, 4f, 16, "Dwie fale ziemi jedna po drugiej, każda zachwiewa.", EarthC);
            novaEarth.pulses = 2; novaEarth.pulseInterval = 0.3f; novaEarth.attack.statuses.Add(St(StatusKind.Stagger));
            var imps = Hu("hu_summon_imps", "Przyzwanie chochlików", SpellKind.Summon, true, 14, 10, 12, "Chochliki ognia na 12 s: trzymają dystans i plują ogniem.", FireC, global::Turris.Element.Fire);
            imps.servant = ServantKind.Imp; imps.servantCount = 2; imps.duration = 12f; imps.servantAttackInterval = 1.1f;
            var knifeThrow = Hu("hu_knife_throw", "Rzut nożami", SpellKind.Projectile, false, 10, 4, 12, "Trzy noże w wachlarzu; każdy zadaje krwawienie.", new Color(0.85f, 0.85f, 0.9f));
            knifeThrow.attack.delivery = AttackDelivery.Projectile; knifeThrow.attack.projectileCount = 3; knifeThrow.attack.spreadAngle = 10f; knifeThrow.attack.projectileSpeed = 20f;
            knifeThrow.projectileLifetime = 0.7f; knifeThrow.attack.statuses.Add(St(StatusKind.Bleed));
            knifeThrow.levelFeatures.Add(new LevelFeature(1, LevelFeatureKind.ExtraTargets, 1)); knifeThrow.levelFeatures.Add(new LevelFeature(2, LevelFeatureKind.ExtraTargets, 1));

            // ---- Druga ręka: grymuary i sztylet do rzucania dają skill, dopóki są założone
            ItemDefinition OffHand(string id, string name, string desc, SpellDefinition grants, Color c, BuildTag tags, params AttributeRequirement[] reqs)
            {
                var o = Make<ItemDefinition>(id, name, desc, tags);
                o.kind = ItemKind.OffHand; o.weight = 1; o.color = c; o.grantedSkill = grants; o.weaponModel = WeaponModel.None;
                o.requirements.AddRange(reqs);
                return o;
            }
            var grimoireFire = OffHand("hu_grimoire_fire", "Grymuar ognia", "Odblokowuje Novę ognia. Wymaga wolnego slotu skilla.", novaFire, FireC, BuildTag.Magic, Req(AttributeType.Intelligence, 12));
            var grimoireWater = OffHand("hu_grimoire_water", "Grymuar wody", "Odblokowuje Novę wody. Wymaga wolnego slotu skilla.", novaWater, FrostC, BuildTag.Magic, Req(AttributeType.Intelligence, 12));
            var grimoireAir = OffHand("hu_grimoire_air", "Grymuar powietrza", "Odblokowuje Novę powietrza. Wymaga wolnego slotu skilla.", novaAir, StormC, BuildTag.Magic, Req(AttributeType.Intelligence, 12));
            var grimoireEarth = OffHand("hu_grimoire_earth", "Grymuar ziemi", "Odblokowuje Novę ziemi. Wymaga wolnego slotu skilla.", novaEarth, EarthC, BuildTag.Magic, Req(AttributeType.Intelligence, 12));
            var grimoireSummon = OffHand("hu_grimoire_summoning", "Grymuar przywołań", "Druga ręka: odblokowuje Przyzwanie chochlików. Wymaga Inteligencji 13.", imps, SpiritC, BuildTag.Magic, Req(AttributeType.Intelligence, 13));
            foreach (var g in new[] { grimoireFire, grimoireWater, grimoireAir, grimoireEarth, grimoireSummon }) g.modifiers.Add(Flat(StatType.MaxMana, 10));
            var offDagger = OffHand("hu_offdagger", "Sztylet do rzucania", "Druga ręka: odblokowuje Rzut nożami.", knifeThrow, Steel, BuildTag.Agile, Req(AttributeType.Dexterity, 10));

            var bowHu = Melee(bowTemplate, "hu_bow", "Łuk myśliwski", "Dwuręczny, strzały lecą daleko i szybko. Wymaga Zręczności 12.", WeaponModel.Bow, 38, 90, BuildTag.Agile, Req(AttributeType.Dexterity, 12));
            bowHu.color = new Color(0.5f, 0.35f, 0.22f);
            var daggerHu = Melee(daggerTemplate, "hu_dagger", "Sztylet", "Krótki i bardzo szybki, każde cięcie krwawi. Wymaga Zręczności 10.", WeaponModel.Dagger, 20, 36, BuildTag.Melee | BuildTag.Agile | BuildTag.Parry, Req(AttributeType.Dexterity, 10));
            OnHit(daggerHu, StatusKind.Bleed, 1, 0.6f);
            var poniard = Melee(daggerTemplate, "hu_poniard", "Puginał", "Krótki, prosty, zawsze pod ręką. Wymaga Zręczności 11.", WeaponModel.Dagger, 22, 40, BuildTag.Melee | BuildTag.Agile | BuildTag.Parry, Req(AttributeType.Dexterity, 11));
            OnHit(poniard, StatusKind.Bleed, 1, 0.6f);

            // ============================================================ KLASY
            StartingLoadout L(string label, Element e, ItemDefinition w, ItemDefinition off = null) => new StartingLoadout { label = label, element = e, weapon = w, offHand = off };
            const global::Turris.Element Phys = global::Turris.Element.None;

            // Cechy Hell Unleashed → Turris: Siła, Zręczność, Inteligencja bez zmian; Wytrzymałość = średnia Wytrzymałości i Kondycji.
            void Setup(ClassDefinition c, string name, string tagline, string desc, int str, int dex, int intel, int end, int con, ClassTrait trait, DodgeStyle dodge,
                SpellDefinition signature, SpellDefinition[] choices, StartingLoadout[] loadouts, int hpFlasks, int mpFlasks, Color color, params StatModifier[] mods)
            {
                c.displayName = name; c.tagline = tagline; c.description = desc;
                c.strength = str; c.dexterity = dex; c.intelligence = intel; c.toughness = Mathf.RoundToInt((end + con) * 0.5f);
                c.trait = trait; c.dodgeStyle = dodge; c.signatureSkill = signature;
                c.startingSkillChoices.Clear(); c.startingSkillChoices.AddRange(choices);
                c.loadouts.Clear(); c.loadouts.AddRange(loadouts);
                c.classModifiers.Clear(); c.classModifiers.AddRange(mods);
                c.startingSpells.Clear(); c.alternativeWeapons.Clear();
                c.healthFlasks = hpFlasks; c.manaFlasks = mpFlasks; c.color = color;
            }
            ClassDefinition Spec(ClassDefinition parent, string id)
            {
                var c = Make<ClassDefinition>(id, "", "");
                c.parent = parent;
                c.startingItems.AddRange(parent.startingItems);
                cfg.classes.Add(c);
                return c;
            }
            // Statystyki klasy z Hell Unleashed: zdrowie bazowe (różnica od 110 × 2), pancerz (× 6 obrony), redukcja, premia do obrażeń,
            // odporności (woda → mróz, powietrze → błyskawice), szybkość ruchu (piksele / 112).
            StatModifier[] Stats(float hp, float armour, float dr, float dmg, float phys, float fire, float water, float air, float earth, float move, params StatModifier[] extra)
            {
                var l = new List<StatModifier>();
                if (hp != 110) l.Add(Flat(StatType.MaxHealth, (hp - 110) * 2f));
                if (armour != 0) l.Add(Flat(StatType.PhysicalDefense, armour * 6f + phys * 100f));
                else if (phys != 0) l.Add(Flat(StatType.PhysicalDefense, phys * 100f));
                if (dr != 0) l.Add(Flat(StatType.DamageReduction, dr * 100f));
                if (dmg != 0) l.Add(Flat(StatType.GlobalDamage, dmg * 100f));
                if (fire != 0) l.Add(Flat(StatType.FireResist, fire * 100f));
                if (water != 0) l.Add(Flat(StatType.FrostResist, water * 100f));
                if (air != 0) l.Add(Flat(StatType.LightningResist, air * 100f));
                if (earth != 0) l.Add(Flat(StatType.EarthResist, earth * 100f));
                if (Mathf.Abs(move - 112f) > 0.5f) l.Add(Flat(StatType.MoveSpeed, (move / 112f - 1f) * 100f));
                l.AddRange(extra);
                return l.ToArray();
            }

            // ---- Wojownik (dawny Rycerz)
            warrior.startingItems.Clear(); warrior.startingItems.Add(chain);
            Setup(warrior, "Wojownik", "Miecz i pancerz", "Miecz z tarczą albo miecz dwuręczny. Więcej zdrowia, pancerz, walka w zwarciu. Furia: ciosy i trafienia ładują szał.",
                14, 10, 7, 13, 12, ClassTrait.Fury, DodgeStyle.ShoulderCharge, warcry, new[] { cleaveHu, windSlash },
                new[] { L("Miecz i tarcza", Phys, sword, shield), L("Dwuręczny", Phys, greatsword) }, 4, 1, new Color(0.7f, 0.72f, 0.8f),
                Stats(125, 3, 0.10f, 0.10f, 0.15f, -0.1f, -0.1f, -0.1f, -0.1f, 112));
            // 30.10: specjalizacje wojownika +10% szybkości ataku i +10% szybkości odnowień (serie botów: odstawały od wojownika bazowego).
            var paladin = Spec(warrior, "class_hu_paladin");
            Setup(paladin, "Paladyn", "Tarcza i aury", "Rycerz z korbaczem i tarczą. Nowy rodzaj skilli: aury – działają stale, dopóki ich nie zgasisz. Pobożność: dwie aury naraz – pierwsza o 20% silniejsza, druga na 75% siły. Skill klasy: Boski osąd.",
                13, 9, 10, 14, 12, ClassTrait.Devotion, DodgeStyle.ShoulderCharge, judgement, new[] { holyFire, defiance },
                new[] { L("Korbacz i tarcza", Phys, flail, shield), L("Miecz i tarcza", Phys, sword, shield) }, 4, 2, new Color(0.9f, 0.8f, 0.5f),
                Stats(135, 4, 0.12f, 0.05f, 0.15f, 0f, -0.05f, -0.05f, -0.05f, 108, Flat(StatType.HealthRegen, 1f), Flat(StatType.FireResist, 10), Flat(StatType.AttackSpeed, 10), Flat(StatType.CooldownReduction, 10)));
            var berserker = Spec(warrior, "class_hu_berserker");
            Setup(berserker, "Berserker", "Broń dwuręczna i szał", "Broń dwuręczna i szał. Żądza krwi: im mniej zdrowia, tym mocniejsze ciosy (do +60%); furia chroni i leczy – przy pełnej do 20% redukcji obrażeń i 10 życia na sekundę. Skill klasy: Żelazna skóra – przez 3 s 80% mniej obrażeń, długie odnowienie.",
                16, 11, 6, 13, 12, ClassTrait.Bloodlust, DodgeStyle.ShoulderCharge, undying, new[] { cleaveHu, windSlash },
                new[] { L("Topór dwuręczny", Phys, greataxe), L("Młot bojowy", Phys, warhammer) }, 4, 1, new Color(0.7f, 0.25f, 0.15f),
                Stats(140, 1, 0.05f, 0.15f, 0.05f, -0.1f, -0.1f, -0.1f, -0.1f, 116, Flat(StatType.AttackSpeed, 15), Flat(StatType.LifeLeech, 1), Flat(StatType.CooldownReduction, 10)));
            var dual = Spec(warrior, "class_hu_dual");
            Setup(dual, "Rębajło", "Broń w każdej ręce", "Broń w każdej ręce: ciosy idą na zmianę z prawej i lewej. Dwie klingi: +20% szybkości ataku z dwiema broniami. Skill klasy: Taniec ostrzy.",
                12, 14, 7, 11, 12, ClassTrait.TwinBlades, DodgeStyle.Roll, bladeDance, new[] { cleaveHu, windSlash },
                new[] { L("Dwa miecze", Phys, sword, broad), L("Korbacz i miecz", Phys, flail, sword) }, 4, 1, new Color(0.65f, 0.15f, 0.2f),
                Stats(120, 2, 0.05f, 0.05f, 0.10f, -0.05f, -0.05f, -0.05f, -0.05f, 120, Flat(StatType.BleedDamage, 20), Flat(StatType.AttackSpeed, 10), Flat(StatType.CooldownReduction, 10)));

            // ---- Mag
            mage.startingItems.Clear(); mage.startingItems.Add(robe);
            Setup(mage, "Mag", "Różdżka i grymuar", "Różdżka i grymuar z novą tego samego żywiołu. Mniej zdrowia, walka na dystans. Arkanowa osłona pochłania ciosy przed życiem.",
                7, 11, 15, 9, 10, ClassTrait.ArcaneWard, DodgeStyle.Blink, overload, new SpellDefinition[0],
                new[] { L("Ogień", global::Turris.Element.Fire, wandFire, grimoireFire), L("Woda", global::Turris.Element.Frost, wandWater, grimoireWater),
                        L("Powietrze", global::Turris.Element.Lightning, wandAir, grimoireAir), L("Ziemia", global::Turris.Element.Earth, wandEarth, grimoireEarth) },
                3, 3, new Color(0.35f, 0.35f, 0.75f),
                Stats(105, 0, 0, 0, -0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 118));
            var mageFire = Spec(mage, "class_hu_mage_fire");
            Setup(mageFire, "Mag ognia", "Ogień i meteory", "Specjalizacja w ogniu: +25% obrażeń od ognia. Skill klasy: Meteor – spada we wskazane miejsce i zostawia płonącą ziemię.",
                7, 10, 16, 9, 10, ClassTrait.ArcaneWard, DodgeStyle.Blink, meteorHu, new SpellDefinition[0],
                new[] { L("Ogień", global::Turris.Element.Fire, wandFire, grimoireFire) }, 3, 3, FireC,
                Stats(100, 0, 0, 0, -0.15f, 0.35f, -0.05f, 0.15f, 0.15f, 118, Flat(StatType.FireDamage, 35)));
            var mageWater = Spec(mage, "class_hu_mage_water");
            Setup(mageWater, "Mag wody", "Woda i spowolnienie", "Specjalizacja w wodzie (mrozie): +20% obrażeń od mrozu, więcej many. Skill klasy: Wir głębin – pulsujący krąg wody, który wciąga i spowalnia.",
                7, 11, 15, 10, 10, ClassTrait.ArcaneWard, DodgeStyle.Blink, maelstrom, new SpellDefinition[0],
                new[] { L("Woda", global::Turris.Element.Frost, wandWater, grimoireWater) }, 3, 3, FrostC,
                Stats(105, 0, 0, 0, -0.15f, -0.05f, 0.35f, 0.15f, 0.15f, 118, Flat(StatType.FrostDamage, 30), Flat(StatType.MaxMana, 20)));
            var mageAir = Spec(mage, "class_hu_mage_air");
            Setup(mageAir, "Mag powietrza", "Wiatr i błyskawice", "Specjalizacja w powietrzu (błyskawicach): +20% obrażeń od błyskawic, szybszy krok. Skill klasy: Nawałnica – burza nad wskazanym miejscem.",
                7, 12, 15, 8, 10, ClassTrait.ArcaneWard, DodgeStyle.Blink, tempest, new SpellDefinition[0],
                new[] { L("Powietrze", global::Turris.Element.Lightning, wandAir, grimoireAir) }, 3, 3, StormC,
                Stats(110, 0, 0, 0, -0.15f, 0.15f, 0.15f, 0.35f, -0.05f, 124, Flat(StatType.LightningDamage, 30), Flat(StatType.MoveSpeed, 5)));
            var mageEarth = Spec(mage, "class_hu_mage_earth");
            Setup(mageEarth, "Mag ziemi", "Skały i pancerz", "Specjalizacja w ziemi: +20% obrażeń od ziemi, kamienna skóra (pancerz). Skill klasy: Trzęsienie ziemi – zachwiewa wszystko wokół.",
                9, 9, 15, 11, 10, ClassTrait.ArcaneWard, DodgeStyle.Blink, quakeHu, new SpellDefinition[0],
                new[] { L("Ziemia", global::Turris.Element.Earth, wandEarth, grimoireEarth) }, 3, 3, EarthC,
                Stats(115, 2, 0, 0, -0.05f, 0.15f, 0.15f, -0.05f, 0.35f, 112, Flat(StatType.EarthDamage, 30)));
            var summoner = Spec(mage, "class_hu_summoner");
            Setup(summoner, "Przywoływacz", "Przywołane demony", "Walczą za niego przywołane demony: chochliki i ogary. Więź dusz: każdy żywy sługa daje 3% redukcji obrażeń (do 15%). Skill klasy: Legion.",
                6, 10, 16, 10, 10, ClassTrait.SoulBond, DodgeStyle.Blink, legion, new SpellDefinition[0],
                new[] { L("Przywołania", global::Turris.Element.Fire, wandFire, grimoireSummon) }, 3, 3, SpiritC,
                Stats(125, 0, 0, 0, -0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 116, Flat(StatType.CooldownReduction, 10), Flat(StatType.MaxMana, 15)));
            var battleMage = Spec(mage, "class_hu_battlemage");
            Setup(battleMage, "Mag bitewny", "Miecz i czary", "Hybryda wojownika i maga: miecz żywiołu i grymuar. Klinga zaklęć: moc czarów działa na cios mieczem, a każdy trafiony wróg oddaje manę. Skill klasy: Cięcie burzy.",
                11, 10, 13, 11, 10, ClassTrait.Spellblade, DodgeStyle.Blink, stormCleave, new SpellDefinition[0],
                new[] { L("Ogień", global::Turris.Element.Fire, swordFire, grimoireFire), L("Woda", global::Turris.Element.Frost, swordWater, grimoireWater),
                        L("Powietrze", global::Turris.Element.Lightning, swordAir, grimoireAir), L("Ziemia", global::Turris.Element.Earth, swordEarth, grimoireEarth) },
                3, 2, new Color(0.35f, 0.45f, 0.8f),
                Stats(118, 2, 0, 0, 0f, 0.05f, 0.05f, 0.05f, 0.05f, 115, Flat(StatType.SpellPower, 10), Flat(StatType.MaxMana, 15)));
            battleMage.startingItems.Clear(); battleMage.startingItems.Add(chain);

            // ---- Łotrzyk
            rogue.startingItems.Clear(); rogue.startingItems.Add(leather); rogue.startingItems.Add(rogueHood);
            Setup(rogue, "Łotrzyk", "Łuk albo sztylety", "Szybki i lekki, uderza słabiej, ale z zaskoczenia. Łuk albo sztylet z nożami do rzucania. Unik to przewrót w cień; cios w plecy zadaje +60%, a pierwszy cios do 1,5 s po uniku +35%.",
                9, 15, 8, 9, 13, ClassTrait.Ambush, DodgeStyle.Vanish, bladeFanHu, new[] { piercing, splinter },
                new[] { L("Łuk", Phys, bowHu), L("Sztylety", Phys, daggerHu, offDagger) }, 3, 1, new Color(0.25f, 0.42f, 0.3f),
                Stats(110, 1, 0, -0.10f, 0f, 0f, 0f, 0.10f, -0.10f, 128));
            var archer = Spec(rogue, "class_hu_archer");
            Setup(archer, "Łucznik", "Mistrz łuku", "Mistrz łuku. Sokole oko: im dalej cel, tym mocniejsza strzała (do +45%). Skill klasy: Burza strzał.",
                9, 16, 8, 9, 13, ClassTrait.Hawkeye, DodgeStyle.Vanish, arrowStorm, new[] { piercing, splinter },
                new[] { L("Łuk", Phys, bowHu) }, 3, 1, new Color(0.35f, 0.55f, 0.3f),
                Stats(105, 1, 0, -0.05f, 0f, 0f, 0f, 0.10f, -0.10f, 128));
            var assassin = Spec(rogue, "class_hu_assassin");
            Setup(assassin, "Zabójca", "Sztylety i krwawienie", "Nożownik: sztylet i nóż do rzucania albo dwa sztylety. Zasadzka (+60% w plecy, +35% po uniku), krwawienie i egzekucje mocniejsze. Skill klasy: Znak śmierci.",
                8, 16, 8, 9, 14, ClassTrait.Ambush, DodgeStyle.Vanish, deathMark, new[] { crossCut, snare },
                new[] { L("Sztylet i noże", Phys, daggerHu, offDagger), L("Dwa sztylety", Phys, daggerHu, poniard) }, 3, 1, BloodC,
                Stats(105, 1, 0, -0.05f, 0f, 0f, 0f, 0.10f, -0.10f, 132, Flat(StatType.BleedDamage, 30), Flat(StatType.ExecuteDamage, 30)));
            var arcane = Spec(rogue, "class_hu_arcane_archer");
            Setup(arcane, "Łucznik arkanów", "Łuk i czary", "Hybryda łotrzyka i maga: łuk, w który wplata czary. Klinga zaklęć: moc czarów działa na strzały. Skill klasy: Strzała żywiołów.",
                8, 13, 12, 9, 12, ClassTrait.Spellblade, DodgeStyle.Blink, elementalArrow, new[] { piercing, splinter },
                new[] { L("Łuk", Phys, bowHu) }, 3, 2, new Color(0.35f, 0.4f, 0.75f),
                Stats(105, 1, 0, -0.05f, 0f, 0.10f, 0.10f, 0.10f, 0f, 124, Flat(StatType.SpellPower, 10), Flat(StatType.LightningDamage, 10)));

            // Skille startowe i z broni – także w nagrodach w trakcie podejścia (jak sklep Hell Unleashed).
            foreach (var s in new[] { cleaveHu, windSlash, holyFire, defiance, piercing, splinter, crossCut, snare, novaFire, novaWater, novaAir, novaEarth, imps, knifeThrow })
                cfg.baseSpellPool.Add(s);
            foreach (var w in new[] { sword, broad, greatsword, greataxe, warhammer, flail, shield, wandFire, wandWater, wandAir, wandEarth, swordFire, swordWater, swordAir, swordEarth,
                                      grimoireFire, grimoireWater, grimoireAir, grimoireEarth, bowHu, daggerHu, poniard, offDagger })
                cfg.baseItemPool.Add(w);
        }
    }
}
