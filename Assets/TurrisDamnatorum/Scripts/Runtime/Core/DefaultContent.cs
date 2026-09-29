using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Domyślna treść prototypu tworzona w pamięci. Skrypt edytora (Turris → Setup) zapisuje ją jako assety,
    /// po czym wszystkie wartości można balansować w inspektorze bez zmian kodu.
    /// Ta sama fabryka jest używana w testach oraz jako awaryjny fallback, gdy scena nie ma przypisanej konfiguracji.
    /// </summary>
    public static class DefaultContent
    {
        public class Bundle
        {
            public GameConfig config;
            public readonly List<ScriptableObject> all = new List<ScriptableObject>();
            public readonly Dictionary<string, ContentDefinition> byId = new Dictionary<string, ContentDefinition>();
            public T Get<T>(string id) where T : ContentDefinition => (T)byId[id];
        }

        static Bundle bundle;

        static T Make<T>(string id, string name, string desc, BuildTag tags = BuildTag.None) where T : ContentDefinition
        {
            var so = ScriptableObject.CreateInstance<T>();
            so.id = id; so.displayName = name; so.description = desc; so.tags = tags;
            so.name = id;
            bundle.all.Add(so);
            bundle.byId[id] = so;
            return so;
        }

        static StatModifier Flat(StatType s, float v) => new StatModifier(s, v);
        static StatModifier Pct(StatType s, float v) => new StatModifier(s, v, ModifierMode.Percent);
        static PassiveEffect Fx(PassiveEffectType t, float v) => new PassiveEffect(t, v);
        static AttributeRequirement Req(AttributeType a, int v) => new AttributeRequirement(a, v);

        static AttackDefinition Atk(string name, float dmg, float windup, float active, float recovery, float reach, float radius,
            float stamina = 0, float lunge = 0.4f, float poise = 15, float guardLoad = 20, bool heavy = false,
            bool blockable = true, bool parryable = true, bool dodgeable = true, float cancel = 0.2f,
            DamageType type = DamageType.Physical, AttackDelivery delivery = AttackDelivery.Melee, float tracking = 360f)
        {
            return new AttackDefinition
            {
                name = name, baseDamage = dmg, windup = windup, active = active, recovery = recovery, reach = reach, radius = radius,
                staminaCost = stamina, lunge = lunge, poiseDamage = poise, guardLoad = guardLoad, heavy = heavy,
                blockable = blockable, parryable = parryable, dodgeable = dodgeable, cancelAfter = cancel,
                damageType = type, delivery = delivery, tracking = tracking,
            };
        }

        static AttackDefinition Proj(string name, float dmg, float windup, float speed, int count = 1, float spread = 0,
            float poise = 10, float guardLoad = 16, bool heavy = false, DamageType type = DamageType.Magic, float recovery = 0.5f)
        {
            var a = Atk(name, dmg, windup, 0.1f, recovery, 0, 0.3f, 0, 0, poise, guardLoad, heavy, true, false, true, 0.2f, type, AttackDelivery.Projectile, 540f);
            a.projectileSpeed = speed; a.projectileCount = count; a.spreadAngle = spread;
            return a;
        }

        static EnemyAttackEntry Entry(AttackDefinition a, float minR, float maxR, float weight = 1, float cooldown = 0,
            int followUp = -1, float followChance = 0.5f, bool hyperArmor = false, int minPhase = 0, bool eliteOnly = false,
            bool bossExtraOnly = false, float retreatAfter = 0)
        {
            return new EnemyAttackEntry
            {
                attack = a, minRange = minR, maxRange = maxR, weight = weight, cooldown = cooldown, followUp = followUp,
                followUpChance = followChance, hyperArmor = hyperArmor, minPhase = minPhase, eliteOnly = eliteOnly,
                bossExtraOnly = bossExtraOnly, retreatAfter = retreatAfter,
            };
        }

        public static Bundle Create()
        {
            bundle = new Bundle();
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            cfg.name = "GameConfig";
            bundle.config = cfg;
            bundle.all.Add(cfg);

            // ============================================================ BRONIE
            var fists = Make<ItemDefinition>("weapon_fists", "Pięści", "Zawsze dostępny, słaby atak bez many.", BuildTag.Melee);
            fists.kind = ItemKind.Weapon; fists.weight = 0;
            fists.weapon = new WeaponData
            {
                light = Atk("Cios pięścią", 16, 0.25f, 0.12f, 0.3f, 1.4f, 0.7f, 10, 0.3f, 8, 12, cancel: 0.12f),
                heavy = Atk("Kopnięcie", 26, 0.5f, 0.15f, 0.5f, 1.6f, 0.8f, 18, 0.5f, 25, 30, true, cancel: 0.25f),
                strengthScaling = 0.6f, dexterityScaling = 0.6f, riposteMultiplier = 2.5f,
            };
            cfg.unarmed = fists;

            var sword = Make<ItemDefinition>("weapon_longsword", "Miecz długi", "Wszechstronny miecz. Pozwala blokować i parować bez tarczy.", BuildTag.Melee | BuildTag.Guard | BuildTag.Parry);
            sword.kind = ItemKind.Weapon; sword.weight = 4; sword.color = new Color(0.8f, 0.8f, 0.85f);
            sword.requirements.Add(Req(AttributeType.Strength, 10)); sword.requirements.Add(Req(AttributeType.Dexterity, 9));
            sword.weapon = new WeaponData
            {
                light = Atk("Cięcie", 40, 0.32f, 0.14f, 0.42f, 2.2f, 0.8f, 16, 0.6f, 18, 20, cancel: 0.18f),
                heavy = Atk("Potężne cięcie", 68, 0.7f, 0.18f, 0.6f, 2.4f, 0.9f, 28, 0.9f, 40, 45, true, cancel: 0.3f),
                strengthScaling = 1.2f, dexterityScaling = 1.0f, riposteMultiplier = 3f,
                canBlock = true,
                guard = new GuardData { physicalReduction = 0.6f, magicReduction = 0.2f, stabilityMultiplier = 1.2f, blockAngle = 100f, staminaRegenMultiplier = 0.3f },
                canParry = true,
                parry = new ParryData { startup = 0.1f, activeWindow = 0.16f, recovery = 0.55f, staminaCost = 16f, poiseDamage = 65f, angle = 140f },
            };

            var axe = Make<ItemDefinition>("weapon_axe", "Topór bojowy", "Ciężkie ciosy łamiące postawę. Słabo blokuje, nie paruje.", BuildTag.Melee | BuildTag.Heavy);
            axe.kind = ItemKind.Weapon; axe.weight = 6; axe.color = new Color(0.6f, 0.5f, 0.4f);
            axe.requirements.Add(Req(AttributeType.Strength, 10));
            axe.weapon = new WeaponData
            {
                light = Atk("Rąbnięcie", 48, 0.4f, 0.15f, 0.5f, 2.1f, 0.85f, 20, 0.6f, 28, 30, cancel: 0.22f),
                heavy = Atk("Rozłupanie", 85, 0.85f, 0.18f, 0.7f, 2.3f, 0.95f, 32, 0.9f, 55, 60, true, cancel: 0.35f),
                strengthScaling = 1.5f, dexterityScaling = 0.5f, riposteMultiplier = 3.2f,
                canBlock = true,
                guard = new GuardData { physicalReduction = 0.5f, magicReduction = 0.1f, stabilityMultiplier = 1.4f, blockAngle = 90f, staminaRegenMultiplier = 0.25f },
                canParry = false,
            };

            var greatAxe = Make<ItemDefinition>("weapon_greataxe", "Wielki topór", "Broń dwuręczna. Blokuje całym trzonem.", BuildTag.Melee | BuildTag.Heavy | BuildTag.Guard);
            greatAxe.kind = ItemKind.Weapon; greatAxe.twoHanded = true; greatAxe.weight = 10; greatAxe.color = new Color(0.45f, 0.4f, 0.35f);
            greatAxe.requirements.Add(Req(AttributeType.Strength, 16));
            greatAxe.weapon = new WeaponData
            {
                light = Atk("Zamach", 72, 0.58f, 0.2f, 0.65f, 2.7f, 1.1f, 26, 0.7f, 45, 50, cancel: 0.3f),
                heavy = Atk("Egzekucja", 125, 1.1f, 0.2f, 0.9f, 2.8f, 1.1f, 38, 1.0f, 80, 85, true, cancel: 0.4f),
                strengthScaling = 1.8f, dexterityScaling = 0.3f, riposteMultiplier = 3f, lightComboLength = 2,
                canBlock = true,
                guard = new GuardData { physicalReduction = 0.75f, magicReduction = 0.2f, stabilityMultiplier = 0.95f, blockAngle = 110f, staminaRegenMultiplier = 0.3f },
            };

            var dagger = Make<ItemDefinition>("weapon_dagger", "Sztylet parujący", "Szybkie pchnięcia i najdłuższe okno parowania. Nie blokuje.", BuildTag.Melee | BuildTag.Parry | BuildTag.Agile);
            dagger.kind = ItemKind.Weapon; dagger.weight = 1.5f; dagger.color = new Color(0.9f, 0.85f, 0.6f);
            dagger.requirements.Add(Req(AttributeType.Dexterity, 11));
            dagger.weapon = new WeaponData
            {
                light = Atk("Pchnięcie", 26, 0.2f, 0.1f, 0.3f, 1.7f, 0.6f, 10, 0.5f, 10, 14, cancel: 0.1f),
                heavy = Atk("Podwójne pchnięcie", 44, 0.45f, 0.15f, 0.45f, 1.9f, 0.6f, 20, 0.8f, 22, 25, true, cancel: 0.22f),
                strengthScaling = 0.4f, dexterityScaling = 1.7f, riposteMultiplier = 4f, lightComboLength = 4,
                canParry = true,
                parry = new ParryData { startup = 0.06f, activeWindow = 0.24f, recovery = 0.45f, staminaCost = 12f, poiseDamage = 80f, angle = 150f },
            };

            var staff = Make<ItemDefinition>("weapon_staff", "Kostur uczonego", "Katalizator: wzmacnia czary i regeneruje manę. W zwarciu – słaby cios bez many.", BuildTag.Magic);
            staff.kind = ItemKind.Weapon; staff.weight = 3; staff.color = new Color(0.45f, 0.3f, 0.2f);
            staff.modifiers.Add(Pct(StatType.SpellPower, 15)); staff.modifiers.Add(Flat(StatType.ManaRegen, 0.8f));
            staff.weapon = new WeaponData
            {
                light = Atk("Uderzenie kosturem", 22, 0.35f, 0.14f, 0.45f, 2.3f, 0.8f, 14, 0.4f, 14, 18, cancel: 0.2f),
                heavy = Atk("Zamach kosturem", 34, 0.62f, 0.16f, 0.6f, 2.4f, 0.9f, 22, 0.6f, 30, 32, true, cancel: 0.3f),
                strengthScaling = 0.6f, dexterityScaling = 0.4f, intelligenceScaling = 0.5f, riposteMultiplier = 2.5f,
            };

            // ============================================================ TARCZE
            var heater = Make<ItemDefinition>("shield_heater", "Tarcza herbowa", "Pełna ochrona przed zwykłymi ciosami fizycznymi.", BuildTag.Shield | BuildTag.Guard | BuildTag.Parry);
            heater.kind = ItemKind.Shield; heater.weight = 4; heater.color = new Color(0.55f, 0.35f, 0.2f);
            heater.shield = new ShieldData
            {
                guard = new GuardData { physicalReduction = 1f, magicReduction = 0.4f, stabilityMultiplier = 0.65f, blockAngle = 140f, staminaRegenMultiplier = 0.3f },
                canParry = true,
                parry = new ParryData { startup = 0.08f, activeWindow = 0.18f, recovery = 0.5f, staminaCost = 14f, poiseDamage = 70f, angle = 150f },
            };

            var buckler = Make<ItemDefinition>("shield_buckler", "Puklerz", "Lekki. Słaby blok, ale szerokie okno parowania.", BuildTag.Shield | BuildTag.Parry | BuildTag.Agile);
            buckler.kind = ItemKind.Shield; buckler.weight = 1.5f; buckler.color = new Color(0.6f, 0.6f, 0.55f);
            buckler.shield = new ShieldData
            {
                guard = new GuardData { physicalReduction = 0.7f, magicReduction = 0.2f, stabilityMultiplier = 0.9f, blockAngle = 110f, staminaRegenMultiplier = 0.5f },
                canParry = true,
                parry = new ParryData { startup = 0.06f, activeWindow = 0.24f, recovery = 0.42f, staminaCost = 12f, poiseDamage = 85f, angle = 150f },
            };

            var greatshield = Make<ItemDefinition>("shield_great", "Pawęż", "Bastion: ogromna stabilność i ochrona magiczna. Nie paruje.", BuildTag.Shield | BuildTag.Guard | BuildTag.Heavy);
            greatshield.kind = ItemKind.Shield; greatshield.weight = 9; greatshield.color = new Color(0.4f, 0.42f, 0.45f);
            greatshield.requirements.Add(Req(AttributeType.Strength, 14));
            greatshield.shield = new ShieldData
            {
                guard = new GuardData { physicalReduction = 1f, magicReduction = 0.65f, stabilityMultiplier = 0.4f, blockAngle = 160f, staminaRegenMultiplier = 0.2f },
                canParry = false,
            };

            // ============================================================ PANCERZ I AKCESORIA
            ItemDefinition Armor(string id, string name, ItemKind kind, float weight, string desc, BuildTag tags, params StatModifier[] mods)
            {
                var it = Make<ItemDefinition>(id, name, desc, tags);
                it.kind = kind; it.weight = weight;
                it.modifiers.AddRange(mods);
                return it;
            }

            var chain = Armor("body_chainmail", "Kolczuga", ItemKind.Body, 7, "", BuildTag.Melee | BuildTag.Guard, Flat(StatType.PhysicalDefense, 35), Flat(StatType.MagicDefense, 10));
            var robe = Armor("body_robe", "Szata uczonego", ItemKind.Body, 2, "", BuildTag.Magic | BuildTag.Agile, Flat(StatType.MagicDefense, 25), Flat(StatType.ManaRegen, 0.5f), Flat(StatType.MaxMana, 10));
            var plate = Armor("body_plate", "Zbroja płytowa", ItemKind.Body, 13, "Ciężka – przy dużym obciążeniu unik jest krótszy.", BuildTag.Heavy | BuildTag.Guard, Flat(StatType.PhysicalDefense, 70), Flat(StatType.MagicDefense, 20), Flat(StatType.MaxHealth, 30));
            var helm = Armor("head_helm", "Hełm rycerski", ItemKind.Head, 3, "", BuildTag.Melee | BuildTag.Heavy, Flat(StatType.PhysicalDefense, 15), Flat(StatType.MagicDefense, 3));
            var hood = Armor("head_hood", "Kaptur mistyka", ItemKind.Head, 1, "", BuildTag.Magic, Pct(StatType.SpellPower, 8), Flat(StatType.MagicDefense, 8));
            var gauntlets = Armor("hands_smith", "Rękawice kowala", ItemKind.Hands, 2, "", BuildTag.Melee, Pct(StatType.PhysicalDamage, 8), Flat(StatType.PhysicalDefense, 5));
            var spellGloves = Armor("hands_channel", "Rękawice zaklinacza", ItemKind.Hands, 1, "Hybryda: ciosy bronią ładują manę.", BuildTag.Magic | BuildTag.Melee, Pct(StatType.SpellPower, 5));
            spellGloves.effects.Add(Fx(PassiveEffectType.ManaOnMeleeHit, 3));
            var pilgrimBelt = Armor("belt_pilgrim", "Pas pielgrzyma", ItemKind.Belt, 1, "", BuildTag.Flask, Pct(StatType.FlaskPotency, 20));
            var athleteBelt = Armor("belt_athlete", "Pas atlety", ItemKind.Belt, 1.5f, "", BuildTag.Melee | BuildTag.Agile, Flat(StatType.MaxStamina, 15), Flat(StatType.StaminaRegen, 5));
            var scoutBoots = Armor("feet_scout", "Buty zwiadowcy", ItemKind.Feet, 1, "", BuildTag.Agile, Pct(StatType.MoveSpeed, 8), Flat(StatType.EquipLoad, 5));
            var ironBoots = Armor("feet_iron", "Okute buty", ItemKind.Feet, 3, "", BuildTag.Heavy | BuildTag.Guard, Flat(StatType.PhysicalDefense, 12), Flat(StatType.EquipLoad, 3));
            var bloodRing = Armor("ring_blood", "Pierścień krwi", ItemKind.Ring, 0, "Powolna regeneracja życia.", BuildTag.None, Flat(StatType.HealthRegen, 1.2f), Flat(StatType.Vigor, 2));
            var manaRing = Armor("ring_mana", "Pierścień źródła", ItemKind.Ring, 0, "", BuildTag.Magic, Flat(StatType.ManaRegen, 1.0f));
            var strRing = Armor("ring_strength", "Pierścień siły", ItemKind.Ring, 0, "", BuildTag.Melee | BuildTag.Heavy, Flat(StatType.Strength, 4));
            var intRing = Armor("ring_intellect", "Pierścień intelektu", ItemKind.Ring, 0, "", BuildTag.Magic, Flat(StatType.Intelligence, 4));
            var riposteRing = Armor("ring_riposte", "Pierścień riposty", ItemKind.Ring, 0, "", BuildTag.Parry, Pct(StatType.RiposteDamage, 40));
            riposteRing.effects.Add(Fx(PassiveEffectType.ParryRestoreStamina, 25));
            var parryAmulet = Armor("amulet_parry", "Amulet szermierza", ItemKind.Amulet, 0, "", BuildTag.Parry, Flat(StatType.ParryWindow, 0.05f));
            parryAmulet.effects.Add(Fx(PassiveEffectType.ParryHeal, 30));
            var casterAmulet = Armor("amulet_caster", "Amulet zaklinacza", ItemKind.Amulet, 0, "", BuildTag.Magic, Pct(StatType.SpellPower, 12));
            casterAmulet.effects.Add(Fx(PassiveEffectType.SpellStaminaRefund, 10));
            var bastionAmulet = Armor("amulet_bastion", "Amulet bastionu", ItemKind.Amulet, 0, "Zablokowane ciosy zasilają manę.", BuildTag.Shield | BuildTag.Guard | BuildTag.Magic, Flat(StatType.MaxStamina, 10));
            bastionAmulet.effects.Add(Fx(PassiveEffectType.BlockManaGain, 4));

            // ============================================================ UMIEJĘTNOŚCI: CZARY
            SpellDefinition Spell(string id, string name, SpellKind kind, float mana, float cast, float recovery, int reqInt, string desc, BuildTag tags, float cooldown = 1f)
            {
                var s = Make<SpellDefinition>(id, name, desc, tags | BuildTag.Magic);
                s.kind = kind; s.category = SkillCategory.Spell; s.manaCost = mana; s.castTime = cast; s.recovery = recovery; s.cooldown = cooldown;
                if (reqInt > 0) s.requirements.Add(Req(AttributeType.Intelligence, reqInt));
                return s;
            }

            var bolt = Spell("spell_bolt", "Pocisk arkanów", SpellKind.Projectile, 12, 0.4f, 0.35f, 8, "Szybki pocisk namierzający cel.", BuildTag.None, 0.6f);
            bolt.attack = Proj("Pocisk arkanów", 38, 0.4f, 22f, poise: 10, guardLoad: 15);
            bolt.color = new Color(0.45f, 0.65f, 1f);
            var nova = Spell("spell_nova", "Fala mocy", SpellKind.Nova, 22, 0.55f, 0.5f, 12, "Odpycha wrogów wokół. Silnie narusza postawę.", BuildTag.None, 6f);
            nova.attack = Atk("Fala mocy", 45, 0.55f, 0.1f, 0.5f, 0, 3.5f, 0, 0, 45, 40, type: DamageType.Magic, delivery: AttackDelivery.AreaAroundSelf);
            nova.attack.parryable = false;
            nova.color = new Color(0.6f, 0.5f, 1f);
            var spear = Spell("spell_spear", "Włócznia potępionych", SpellKind.Projectile, 30, 0.8f, 0.5f, 14, "Wolny, bardzo silny pocisk.", BuildTag.None, 3f);
            spear.attack = Proj("Włócznia", 85, 0.8f, 13f, poise: 45, guardLoad: 45, heavy: true);
            spear.color = new Color(0.8f, 0.3f, 1f);
            var scatter = Spell("spell_scatter", "Rozprysk arkanów", SpellKind.Projectile, 20, 0.5f, 0.45f, 10, "Trzy pociski w wachlarzu – dobre na grupy.", BuildTag.None, 2f);
            scatter.attack = Proj("Rozprysk", 24, 0.5f, 18f, 3, 15f, poise: 8, guardLoad: 12);
            scatter.color = new Color(0.4f, 0.9f, 1f);
            var heal = Spell("spell_heal", "Kojące światło", SpellKind.Heal, 30, 0.9f, 0.4f, 8, "Leczenie rozłożone w czasie – ryzykowne w walce.", BuildTag.Flask, 14f);
            heal.amount = 90; heal.duration = 3f; heal.intelligenceScaling = 2f; heal.color = new Color(1f, 0.9f, 0.5f);
            var enchant = Spell("spell_enchant", "Zaklęte ostrze", SpellKind.WeaponBuff, 25, 0.6f, 0.35f, 8, "Hybryda: broń zadaje dodatkowe obrażenia magiczne.", BuildTag.Melee, 20f);
            enchant.amount = 18; enchant.duration = 20f; enchant.intelligenceScaling = 2f; enchant.color = new Color(0.5f, 0.7f, 1f);
            var barrier = Spell("spell_barrier", "Kamienna osłona", SpellKind.Barrier, 25, 0.5f, 0.35f, 10, "Osłona pochłania obrażenia; w pełni pochłonięty cios nie przerywa akcji.", BuildTag.Guard, 16f);
            barrier.amount = 70; barrier.duration = 8f; barrier.intelligenceScaling = 2f; barrier.color = new Color(0.8f, 0.66f, 0.42f);

            // ============================================================ UMIEJĘTNOŚCI: TECHNIKI BRONIĄ
            // Obrażenia = lekki atak aktualnej broni × mnożnik, więc techniki rosną razem z bronią i jej ulepszeniami.
            SpellDefinition Tech(string id, string name, SpellKind kind, float stamina, float cast, float active, float recovery, float cooldown,
                float mult, float reach, float radius, float poise, string desc, BuildTag tags)
            {
                var s = Make<SpellDefinition>(id, name, desc, tags | BuildTag.Melee);
                s.kind = kind; s.category = SkillCategory.Technique;
                s.manaCost = 0; s.staminaCost = stamina; s.castTime = cast; s.recovery = recovery; s.cancelAfter = 0.2f;
                s.cooldown = cooldown; s.weaponMultiplier = mult; s.intelligenceScaling = 0f; s.duration = active;
                s.attack = Atk(name, 0, cast, active, recovery, reach, radius, 0, 0, poise, poise * 0.8f, poise >= 50);
                return s;
            }

            var cleave = Tech("skill_cleave", "Rozpłatanie", SpellKind.Cleave, 22, 0.32f, 0.12f, 0.4f, 5f, 1.6f, 3.0f, 1f, 30,
                "Szeroki cios w łuku przed sobą – odpowiedź na grupę.", BuildTag.None);
            cleave.arcAngle = 160f; cleave.color = new Color(1f, 0.78f, 0.45f);
            var bash = Tech("skill_shieldbash", "Uderzenie tarczą", SpellKind.ShieldBash, 16, 0.18f, 0.1f, 0.35f, 6f, 0.8f, 2.2f, 1f, 60,
                "Szybkie pchnięcie tarczą – mało obrażeń, ale mocno łamie postawę (otwiera ripostę).", BuildTag.Shield);
            bash.arcAngle = 100f; bash.requiredTags = BuildTag.Shield; bash.color = new Color(0.92f, 0.9f, 0.72f);
            var charge = Tech("skill_charge", "Szarża", SpellKind.Charge, 20, 0.25f, 0.35f, 0.35f, 7f, 1.3f, 6f, 0.9f, 35,
                "Zryw naprzód, trafia każdego na drodze. Bez niewrażliwości – to atak, nie unik.", BuildTag.Agile);
            charge.extraChargeAtLevel = 3; charge.color = new Color(1f, 0.55f, 0.25f);
            var whirl = Tech("skill_whirlwind", "Młynek", SpellKind.Whirlwind, 30, 0.25f, 1.5f, 0.45f, 10f, 0.7f, 0f, 2.6f, 12,
                "Wirujące cięcia wokół; można powoli się poruszać. Unik lub trafienie przerywają młynek.", BuildTag.Melee);
            whirl.tickInterval = 0.35f; whirl.moveMultiplier = 0.5f; whirl.requirements.Add(Req(AttributeType.Strength, 12)); whirl.color = new Color(0.85f, 0.88f, 1f);
            var quakeSkill = Tech("skill_quake", "Trzęsienie", SpellKind.Quake, 32, 0.55f, 0.12f, 0.6f, 9f, 2.0f, 1.6f, 3.2f, 70,
                "Uderzenie w ziemię przed sobą: duże obrażenia i łamanie postawy w kręgu.", BuildTag.Heavy);
            quakeSkill.requirements.Add(Req(AttributeType.Strength, 14)); quakeSkill.color = new Color(0.82f, 0.6f, 0.35f);

            // ============================================================ WZMOCNIENIA / TALENTY
            BoonDefinition Boon(string id, string name, string desc, BuildTag tags, int maxStacks, params StatModifier[] mods)
            {
                var b = Make<BoonDefinition>(id, name, desc, tags);
                b.maxStacks = maxStacks;
                b.modifiers.AddRange(mods);
                return b;
            }

            var bIron = Boon("boon_ironskin", "Żelazna skóra", "", BuildTag.Melee | BuildTag.Guard | BuildTag.Heavy, 3, Flat(StatType.PhysicalDefense, 20));
            var bVigor = Boon("boon_vigor", "Hart ducha", "", BuildTag.None, 3, Flat(StatType.MaxHealth, 40));
            var bBreath = Boon("boon_breath", "Oddech wojownika", "", BuildTag.Melee | BuildTag.Agile | BuildTag.Guard, 3, Flat(StatType.MaxStamina, 10), Flat(StatType.StaminaRegen, 6));
            var bWrath = Boon("boon_wrath", "Ostrze gniewu", "", BuildTag.Melee, 3, Pct(StatType.PhysicalDamage, 10));
            var bWell = Boon("boon_manawell", "Źródło many", "", BuildTag.Magic, 3, Flat(StatType.ManaRegen, 0.8f), Flat(StatType.MaxMana, 10));
            var bArcane = Boon("boon_arcane", "Moc arkanów", "", BuildTag.Magic, 3, Pct(StatType.SpellPower, 12));
            var bBlood = Boon("boon_bloodline", "Krew przodków", "Słaba regeneracja – nie zastąpi flaszek.", BuildTag.None, 2, Flat(StatType.HealthRegen, 1f));
            var bFlask = Boon("boon_flask", "Kolekcjoner flaszek", "", BuildTag.Flask, 2);
            bFlask.extraHealthFlasks = 1;
            var bManaFlask = Boon("boon_manaflask", "Jasność umysłu", "", BuildTag.Magic | BuildTag.Flask, 2);
            bManaFlask.extraManaFlasks = 1;
            var bParry = Boon("boon_parrymaster", "Mistrz parowania", "", BuildTag.Parry, 2, Flat(StatType.ParryWindow, 0.03f));
            bParry.effects.Add(Fx(PassiveEffectType.ParryRestoreStamina, 20));
            var bTitan = Boon("boon_titan", "Siła tytana", "", BuildTag.Melee | BuildTag.Heavy, 3, Flat(StatType.Strength, 3));
            var bMind = Boon("boon_mind", "Bystry umysł", "", BuildTag.Magic, 3, Flat(StatType.Intelligence, 3), Flat(StatType.Mind, 1));
            var bAgile = Boon("boon_agile", "Zwinność", "", BuildTag.Agile | BuildTag.Parry, 3, Flat(StatType.Dexterity, 3), Flat(StatType.EquipLoad, 4));
            var bVamp = Boon("boon_vampire", "Pijawka", "", BuildTag.Melee, 2);
            bVamp.effects.Add(Fx(PassiveEffectType.HealOnKill, 40));
            var bAlch = Boon("boon_alchemist", "Alchemik", "", BuildTag.Flask, 2, Pct(StatType.FlaskPotency, 20));
            var bBulwark = Boon("boon_bulwark", "Obrońca", "Garda regeneruje manę.", BuildTag.Shield | BuildTag.Guard, 2, Flat(StatType.MaxStamina, 10));
            bBulwark.effects.Add(Fx(PassiveEffectType.BlockManaGain, 3));

            // Talenty odblokowywane (dostępne dla każdej postaci, także jako talent startowy).
            var tBloodRiposte = Boon("talent_bloodriposte", "Krwawa riposta", "Talent: riposty leczą.", BuildTag.Parry, 1, Pct(StatType.RiposteDamage, 30));
            tBloodRiposte.effects.Add(Fx(PassiveEffectType.RiposteHeal, 60));
            var tSpellguard = Boon("talent_spellguard", "Strażnik zaklęć", "Talent hybrydowy: ciosy i blok ładują manę.", BuildTag.Magic | BuildTag.Guard | BuildTag.Melee, 1);
            tSpellguard.effects.Add(Fx(PassiveEffectType.BlockManaGain, 5)); tSpellguard.effects.Add(Fx(PassiveEffectType.ManaOnMeleeHit, 2));
            var tSurvivor = Boon("talent_survivor", "Przetrwanie", "Talent: więcej leczenia z flaszek.", BuildTag.Flask, 1, Pct(StatType.FlaskPotency, 15));
            tSurvivor.extraHealthFlasks = 1;
            var tVigilance = Boon("talent_vigilance", "Czujność", "Talent startowy dostępny dla każdego.", BuildTag.Parry | BuildTag.Agile, 1, Flat(StatType.ParryWindow, 0.03f), Flat(StatType.MaxStamina, 10));

            // ============================================================ KLASY
            var knight = Make<ClassDefinition>("class_knight", "Rycerz", "Miecz, tarcza i solidny pancerz. Techniki: Rozpłatanie i Uderzenie tarczą. Może później uczyć się czarów.");
            knight.vigor = 12; knight.endurance = 12; knight.mind = 6; knight.strength = 14; knight.dexterity = 12; knight.intelligence = 8;
            knight.startingItems.AddRange(new[] { sword, heater, chain });
            knight.startingSpells.AddRange(new[] { cleave, bash });
            knight.healthFlasks = 4; knight.manaFlasks = 1; knight.color = new Color(0.7f, 0.72f, 0.8f);

            var mage = Make<ClassDefinition>("class_mage", "Mag", "Kostur, dwa czary i wysoka Inteligencja. Może później sięgnąć po topór i ciężką zbroję.");
            mage.vigor = 9; mage.endurance = 9; mage.mind = 14; mage.strength = 10; mage.dexterity = 10; mage.intelligence = 16;
            mage.startingItems.AddRange(new[] { staff, robe });
            mage.startingSpells.AddRange(new[] { bolt, nova });
            mage.healthFlasks = 3; mage.manaFlasks = 3; mage.color = new Color(0.35f, 0.35f, 0.75f);

            cfg.classes.Add(knight); cfg.classes.Add(mage);

            // ============================================================ PRZECIWNICY
            var ghoul = Make<EnemyDefinition>("enemy_ghoul", "Ghul Wieży", "Szybki, agresywny, łatwo traci postawę.");
            ghoul.archetype = "Szybki (wręcz)";
            ghoul.maxHealth = 260; ghoul.maxPoise = 30; ghoul.moveSpeed = 4.2f; ghoul.turnSpeed = 540; ghoul.preferredRange = 1.8f;
            ghoul.attackInterval = new Vector2(0.45f, 1.1f); ghoul.strafeChance = 0.5f; ghoul.evadeChance = 0.3f;
            ghoul.color = new Color(0.5f, 0.58f, 0.32f); ghoul.scale = 0.95f; ghoul.soulReward = 60;
            ghoul.attacks.Add(Entry(Atk("Pazury", 28, 0.38f, 0.12f, 0.45f, 1.9f, 0.8f, 0, 1.2f, 12, 18, tracking: 420f), 0, 2.4f, 3, followUp: 1, followChance: 0.6f));
            ghoul.attacks.Add(Entry(Atk("Druga seria", 28, 0.3f, 0.12f, 0.6f, 1.9f, 0.8f, 0, 0.9f, 12, 18, tracking: 300f), 0, 2.6f, 0));
            ghoul.attacks.Add(Entry(Atk("Skok", 42, 0.6f, 0.22f, 0.7f, 1.9f, 0.9f, 0, 5f, 25, 30, true, tracking: 240f), 3.5f, 7.5f, 1.5f, cooldown: 3));
            ghoul.attacks.Add(Entry(Atk("Wściekły szał", 34, 0.5f, 0.35f, 0.7f, 2.3f, 1.2f, 0, 1.5f, 20, 28, parryable: false, tracking: 300f), 0, 2.5f, 1.5f, cooldown: 6, eliteOnly: true));

            var heretic = Make<EnemyDefinition>("enemy_heretic", "Heretyk", "Trzyma dystans i ciska pociskami. Odpycha, gdy podejdziesz.");
            heretic.archetype = "Dystansowy";
            heretic.maxHealth = 220; heretic.maxPoise = 25; heretic.moveSpeed = 3.4f; heretic.preferredRange = 9; heretic.retreatRange = 5;
            heretic.attackInterval = new Vector2(1.0f, 1.8f); heretic.strafeChance = 0.7f; heretic.magicResist = 0.3f;
            heretic.color = new Color(0.45f, 0.25f, 0.6f); heretic.soulReward = 70;
            heretic.attacks.Add(Entry(Proj("Pocisk herezji", 30, 0.7f, 15f), 4, 18, 3));
            heretic.attacks.Add(Entry(Proj("Wachlarz", 22, 1.0f, 14f, 3, 14f), 4, 14, 1.5f, cooldown: 4));
            var push = Atk("Odepchnięcie", 25, 0.8f, 0.1f, 0.5f, 0, 3f, 0, 0, 30, 30, parryable: false, type: DamageType.Magic, delivery: AttackDelivery.AreaAroundSelf);
            heretic.attacks.Add(Entry(push, 0, 3.2f, 4, cooldown: 5, retreatAfter: 5));
            heretic.attacks.Add(Entry(Proj("Opóźniony pocisk", 50, 1.2f, 8f, poise: 30, guardLoad: 35, heavy: true), 5, 18, 1.5f, cooldown: 6, eliteOnly: true));

            var warden = Make<EnemyDefinition>("enemy_warden", "Strażnik Bramy", "Wolny i opancerzony. Osłania się tarczą, a ciężkich ciosów nie przerwiesz.");
            warden.archetype = "Opancerzony";
            warden.maxHealth = 520; warden.maxPoise = 80; warden.maxStamina = 120; warden.physicalResist = 0.35f; warden.magicResist = 0f;
            warden.moveSpeed = 2.6f; warden.turnSpeed = 200; warden.preferredRange = 2.4f; warden.attackInterval = new Vector2(1.0f, 2.0f);
            warden.guardChance = 0.35f; warden.guard = new GuardData { physicalReduction = 0.8f, magicReduction = 0.2f, stabilityMultiplier = 0.8f, blockAngle = 140f };
            warden.color = new Color(0.5f, 0.52f, 0.58f); warden.scale = 1.25f; warden.soulReward = 110;
            warden.attacks.Add(Entry(Atk("Cięcie halabardą", 50, 0.8f, 0.18f, 0.8f, 3.0f, 1.0f, 0, 0.8f, 25, 35, tracking: 200f), 0, 3.2f, 3));
            warden.attacks.Add(Entry(Atk("Miażdżący cios", 90, 1.3f, 0.15f, 1.1f, 3.0f, 1.1f, 0, 0.6f, 45, 70, true, parryable: false, tracking: 160f), 0, 3.2f, 2, cooldown: 3, hyperArmor: true));
            warden.attacks.Add(Entry(Atk("Szarża tarczą", 60, 1.0f, 0.4f, 0.9f, 1.8f, 1.1f, 0, 7f, 40, 50, blockable: false, parryable: false, tracking: 220f), 4, 10, 2, cooldown: 5, hyperArmor: true));
            var quake = Atk("Wstrząs", 45, 1.1f, 0.1f, 0.9f, 0, 4f, 0, 0, 35, 40, parryable: false, dodgeable: false, delivery: AttackDelivery.AreaAroundSelf);
            warden.attacks.Add(Entry(quake, 0, 3.5f, 1.5f, cooldown: 7, eliteOnly: true, hyperArmor: true));

            var boss = Make<EnemyDefinition>("enemy_castellan", "Kasztelan Potępionych", "Pan wieży. Po utracie połowy życia zmienia styl walki.");
            boss.archetype = "Boss";
            boss.maxHealth = 1250; boss.maxPoise = 120; boss.maxStamina = 200; boss.physicalResist = 0.2f; boss.magicResist = 0.2f;
            boss.moveSpeed = 3.4f; boss.turnSpeed = 260; boss.preferredRange = 2.8f; boss.attackInterval = new Vector2(0.8f, 1.6f);
            boss.strafeChance = 0.5f; boss.riposteWindow = 2.0f; boss.isBoss = true;
            boss.color = new Color(0.55f, 0.12f, 0.12f); boss.scale = 1.6f; boss.soulReward = 400;
            boss.phases.Add(new EnemyPhase { healthThreshold = 0.5f, speedMultiplier = 1.2f, aggressionMultiplier = 1.4f, announcement = "Kasztelan wpada w szał!" });
            boss.attacks.Add(Entry(Atk("Szeroki zamach", 70, 0.85f, 0.2f, 0.75f, 3.4f, 1.4f, 0, 1f, 30, 40, tracking: 260f), 0, 3.8f, 3, followUp: 1, followChance: 0.4f));
            boss.attacks.Add(Entry(Atk("Miażdżące uderzenie", 110, 1.25f, 0.15f, 1.0f, 3.3f, 1.3f, 0, 0.8f, 50, 75, true, parryable: false, tracking: 180f), 0, 3.8f, 2, cooldown: 2.5f, hyperArmor: true));
            boss.attacks.Add(Entry(Atk("Pchnięcie szarży", 85, 1.0f, 0.35f, 1.0f, 2.2f, 1.1f, 0, 9f, 40, 50, blockable: false, parryable: true, tracking: 200f), 4, 12, 2, cooldown: 4, hyperArmor: true));
            var doom = Atk("Fala potępienia", 65, 1.5f, 0.1f, 1.2f, 0, 6f, 0, 0, 40, 50, parryable: false, dodgeable: false, type: DamageType.Magic, delivery: AttackDelivery.AreaAroundSelf);
            boss.attacks.Add(Entry(doom, 0, 5f, 2, cooldown: 8, minPhase: 1, hyperArmor: true));
            boss.attacks.Add(Entry(Proj("Salwa dusz", 40, 0.9f, 16f, 3, 12f, poise: 20, guardLoad: 25), 5, 20, 2, cooldown: 5, minPhase: 1));
            boss.attacks.Add(Entry(Atk("Opóźnione cięcie", 80, 1.6f, 0.2f, 0.8f, 3.5f, 1.4f, 0, 1.2f, 35, 45, tracking: 300f), 0, 3.8f, 2, cooldown: 5, bossExtraOnly: true));

            // ============================================================ ARENY I WIEŻA
            var courtyard = Make<ArenaDefinition>("arena_courtyard", "Dziedziniec", "");
            courtyard.circular = true; courtyard.size = 13;
            courtyard.pillars.AddRange(new[] { new Vector3(6, 0, 6), new Vector3(-6, 0, 6), new Vector3(6, 0, -6), new Vector3(-6, 0, -6) });
            var crypt = Make<ArenaDefinition>("arena_crypt", "Krypta", "");
            crypt.circular = false; crypt.size = 22;
            crypt.pillars.AddRange(new[] { new Vector3(5, 0, 0), new Vector3(-5, 0, 0), new Vector3(5, 0, 6), new Vector3(-5, 0, 6), new Vector3(5, 0, -6), new Vector3(-5, 0, -6) });
            crypt.floorColor = new Color(0.2f, 0.2f, 0.24f); crypt.wallColor = new Color(0.28f, 0.28f, 0.33f); crypt.lightColor = new Color(0.75f, 0.8f, 1f);
            var summit = Make<ArenaDefinition>("arena_summit", "Szczyt", "");
            summit.circular = true; summit.size = 17;
            summit.floorColor = new Color(0.28f, 0.2f, 0.2f); summit.wallColor = new Color(0.3f, 0.18f, 0.18f); summit.lightColor = new Color(1f, 0.6f, 0.5f); summit.fogColor = new Color(0.15f, 0.05f, 0.05f);

            var tower = Make<TowerDefinition>("tower_main", "Turris Damnatorum", "");
            tower.victoryAshBonus = 40;
            tower.floors.Add(new FloorDefinition { name = "I. Przedsionek", arena = courtyard, enemies = { ghoul }, statScale = 1.0f, isDuel = true, ashReward = 3 });
            tower.floors.Add(new FloorDefinition { name = "II. Skryptorium herezji", arena = crypt, enemies = { heretic }, statScale = 1.05f, isDuel = true, ashReward = 6, restAfter = true });
            tower.floors.Add(new FloorDefinition { name = "III. Zbrojownia", arena = crypt, enemies = { ghoul, ghoul, heretic }, statScale = 0.9f, isDuel = false, ashReward = 10 });
            tower.floors.Add(new FloorDefinition { name = "IV. Brama Strażnika", arena = courtyard, enemies = { warden }, statScale = 1.12f, isDuel = true, ashReward = 15, restAfter = true });
            tower.floors.Add(new FloorDefinition { name = "V. Szczyt Wieży", arena = summit, enemies = { boss }, statScale = 1.0f, isDuel = true, isBoss = true, ashReward = 30 });
            cfg.tower = tower;

            // ============================================================ TRUDNOŚĆ
            DifficultyDefinition Diff(int tier, string id, string name, string desc)
            {
                var d = Make<DifficultyDefinition>(id, name, desc);
                d.tier = tier;
                cfg.difficulties.Add(d);
                return d;
            }
            var d0 = Diff(0, "diff_pilgrim", "Pielgrzym", "Podstawowe wyzwanie.");
            d0.ashMultiplier = 1f;
            var d1 = Diff(1, "diff_damned", "Potępiony", "Elity pojawiają się w pojedynkach.");
            d1.enemyHealthMultiplier = 1.2f; d1.enemyDamageMultiplier = 1.15f; d1.enemyAggressionMultiplier = 1.1f; d1.elites = true;
            d1.ashMultiplier = 1.6f; d1.soulMultiplier = 1.2f; d1.itemQualityBonus = 1;
            var d2 = Diff(2, "diff_cursed", "Przeklęty", "Mniej leczenia, groźniejszy boss.");
            d2.enemyHealthMultiplier = 1.35f; d2.enemyDamageMultiplier = 1.3f; d2.enemyAggressionMultiplier = 1.2f; d2.elites = true;
            d2.flaskChargeDelta = -1; d2.bossExtraBehavior = true; d2.ashMultiplier = 2.3f; d2.soulMultiplier = 1.4f; d2.itemQualityBonus = 1;
            var d3 = Diff(3, "diff_lost", "Zatracony", "Leczenie tylko w kapliczkach.");
            d3.enemyHealthMultiplier = 1.5f; d3.enemyDamageMultiplier = 1.45f; d3.enemyAggressionMultiplier = 1.3f; d3.elites = true;
            d3.flaskChargeDelta = -1; d3.restoreOnlyAtRest = true; d3.bossExtraBehavior = true; d3.ashMultiplier = 3.2f; d3.soulMultiplier = 1.6f; d3.itemQualityBonus = 2;

            // ============================================================ PULE NAGRÓD
            cfg.baseItemPool.AddRange(new[] { sword, axe, staff, heater, buckler, chain, robe, plate, helm, hood, gauntlets, spellGloves,
                pilgrimBelt, athleteBelt, scoutBoots, ironBoots, bloodRing, manaRing, strRing, intRing, parryAmulet, casterAmulet, bastionAmulet });
            cfg.baseBoonPool.AddRange(new[] { bIron, bVigor, bBreath, bWrath, bWell, bArcane, bBlood, bFlask, bManaFlask, bParry, bTitan, bMind, bAgile, bVamp, bAlch, bBulwark });
            // Umiejętności do zdobycia tymczasowo (na jedno podejście). Te z listy odblokowań dochodzą, gdy gracz dotrze na ich piętro.
            cfg.baseSpellPool.AddRange(new[] { bolt, nova, enchant, cleave, bash, charge });

            // ============================================================ ODBLOKOWANIA
            UnlockDefinition Unlock(string id, UnlockKind kind, ContentDefinition target, int ash, int loadout, bool byDefault = false, int floor = 0, int victory = -1)
            {
                var u = Make<UnlockDefinition>(id, target.displayName, target.description);
                u.kind = kind; u.target = target; u.ashCost = ash; u.loadoutCost = loadout; u.unlockedByDefault = byDefault;
                u.requiredBestFloor = floor; u.requiredVictoryTier = victory;
                cfg.unlocks.Add(u);
                return u;
            }
            Unlock("unlock_axe", UnlockKind.StartingItem, axe, 0, 2, true);
            Unlock("unlock_bolt", UnlockKind.Spell, bolt, 0, 2, true);
            Unlock("unlock_vigilance", UnlockKind.Boon, tVigilance, 0, 1, true);
            Unlock("unlock_buckler", UnlockKind.StartingItem, buckler, 10, 1);
            Unlock("unlock_dagger", UnlockKind.StartingItem, dagger, 15, 2);
            Unlock("unlock_greatshield", UnlockKind.StartingItem, greatshield, 25, 2, floor: 2);
            Unlock("unlock_greataxe", UnlockKind.StartingItem, greatAxe, 30, 3, floor: 3);
            Unlock("unlock_ring_riposte", UnlockKind.StartingItem, riposteRing, 20, 2);
            Unlock("unlock_heal", UnlockKind.Spell, heal, 20, 2);
            Unlock("unlock_scatter", UnlockKind.Spell, scatter, 25, 2, floor: 3);
            Unlock("unlock_spear", UnlockKind.Spell, spear, 35, 3, floor: 4);
            // Umiejętności odblokowane na stałe są w kolekcji każdego podejścia (bez kosztu w budżecie).
            Unlock("unlock_cleave", UnlockKind.Spell, cleave, 10, 0);
            Unlock("unlock_charge", UnlockKind.Spell, charge, 15, 0);
            Unlock("unlock_shieldbash", UnlockKind.Spell, bash, 15, 0);
            Unlock("unlock_barrier", UnlockKind.Spell, barrier, 20, 0);
            Unlock("unlock_whirlwind", UnlockKind.Spell, whirl, 25, 0, floor: 2);
            Unlock("unlock_quake", UnlockKind.Spell, quakeSkill, 30, 0, floor: 3);
            Unlock("unlock_bloodriposte", UnlockKind.Boon, tBloodRiposte, 20, 2);
            Unlock("unlock_spellguard", UnlockKind.Boon, tSpellguard, 20, 2);
            Unlock("unlock_survivor", UnlockKind.Boon, tSurvivor, 15, 2);
            Unlock("unlock_diff1", UnlockKind.Difficulty, d1, 20, 0, floor: 3);
            Unlock("unlock_diff2", UnlockKind.Difficulty, d2, 45, 0, floor: 5);
            Unlock("unlock_diff3", UnlockKind.Difficulty, d3, 80, 0, victory: 1);

            var result = bundle;
            bundle = null;
            return result;
        }
    }
}
