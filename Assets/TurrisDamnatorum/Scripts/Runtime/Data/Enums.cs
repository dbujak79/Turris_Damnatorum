namespace Turris
{
    public enum DamageType { Physical, Magic }

    /// <summary>
    /// Żywioł trafienia. Sam żywioł niczego nie nakłada – efekty pochodzą z jawnej listy w ataku.
    /// Decyduje o słabościach/odpornościach wroga i o reakcjach (szok termiczny, przewodzenie).
    /// Obrażenia żywiołów liczą się jako magiczne (obrona magiczna je redukuje).
    /// </summary>
    public enum Element { None, Fire, Frost, Lightning }

    /// <summary>Efekty trwające na postaci. Zamrożenie powstaje z trzeciej warstwy chłodu.</summary>
    public enum StatusKind { Bleed, Burn, Chill, Shock, Frozen }

    public enum ArenaStyle { Auto, Courtyard, Crypt, Summit }

    public enum Faction { Player, Enemy }

    /// <summary>
    /// Wszystkie modyfikowalne wartości postaci. Atrybuty (Vigor..Intelligence) są zwykłymi statystykami,
    /// więc przedmioty i wzmocnienia mogą je podnosić tak samo jak statystyki pochodne.
    /// </summary>
    public enum StatType
    {
        Vigor, Endurance, Mind, Strength, Dexterity, Intelligence,
        MaxHealth, MaxStamina, MaxMana,
        HealthRegen, StaminaRegen, ManaRegen,
        PhysicalDefense, MagicDefense,
        PhysicalDamage,   // % premii do obrażeń broni
        SpellPower,       // % premii do mocy czarów
        FlaskPotency,     // % premii do siły flaszek
        RiposteDamage,    // % premii do riposty
        ParryWindow,      // dodatkowe sekundy aktywnego okna parowania
        EquipLoad,        // udźwig (limit ciężaru wyposażenia)
        MoveSpeed,        // % premii do szybkości ruchu
        FireResist,       // % redukcji obrażeń od ognia (limit 75%)
        FrostResist,
        LightningResist,
        FireDamage,       // % premii do obrażeń od ognia
        FrostDamage,
        LightningDamage,
        BleedDamage,      // % premii do obrażeń krwawienia
    }

    public enum AttributeType { Vigor, Endurance, Mind, Strength, Dexterity, Intelligence }

    public enum ModifierMode { Flat, Percent }

    public enum EquipSlot { MainHand, OffHand, Head, Body, Hands, Belt, Feet, Ring1, Ring2, Amulet }

    public enum ItemKind { Weapon, Shield, Head, Body, Hands, Belt, Feet, Ring, Amulet }

    public enum AttackDelivery { Melee, Projectile, AreaAroundSelf }

    /// <summary>
    /// Zachowanie umiejętności. Czary: Projectile, Nova, Heal, WeaponBuff, Barrier.
    /// Techniki bronią: Cleave (łuk przed sobą), Charge (szarża), Whirlwind (młynek), Quake (uderzenie w ziemię), ShieldBash (uderzenie tarczą).
    /// </summary>
    /// Cone (stożek czaru), Chain (łańcuch skaczący między wrogami), Zone (strefa na ziemi), Flurry (seria cięć przed sobą).
    /// Warcry (okrzyk: premia do obrażeń), Meteor (zapowiedziany krąg), Storm (pioruny w losowych wrogów), BloodPact (życie za darmowe czary),
    /// FrostArmor (osłona + odwet chłodem), Pull (przyciąganie), Counter (postawa kontry), Rupture (rozdarcie ran).
    public enum SpellKind { Projectile, Nova, Heal, WeaponBuff, Barrier, Cleave, Charge, Whirlwind, Quake, ShieldBash, Cone, Chain, Zone, Flurry,
        Warcry, Meteor, Storm, BloodPact, FrostArmor, Pull, Counter, Rupture }

    /// <summary>Czar kosztuje manę i skaluje z Inteligencją; technika kosztuje wytrzymałość i skaluje z obrażeniami broni.</summary>
    public enum SkillCategory { Spell, Technique }

    /// <summary>Efekty pasywne wpływające na styl gry. Wartości tego samego typu sumują się.</summary>
    public enum PassiveEffectType
    {
        None,
        ParryRestoreStamina,  // przywraca X wytrzymałości po udanym parowaniu
        ParryRestoreMana,     // przywraca X many po udanym parowaniu
        ParryHeal,            // leczy X po udanym parowaniu
        ManaOnMeleeHit,       // X many za trafienie bronią
        HealOnKill,           // X życia za zabicie przeciwnika
        RiposteHeal,          // X życia za ripostę
        BlockManaGain,        // X many za zablokowany cios
        SpellStaminaRefund,   // X wytrzymałości po rzuceniu czaru
        BurnDurationBonus,    // +X s podpalenia zadawanego przez postać
        BleedMaxStacksBonus,  // +X maks. warstw krwawienia na celach
        BleedMovingBonus,     // +X do mnożnika krwawienia w ruchu
        FreezeStacksReduction,// o X mniej warstw chłodu do zamrożenia (min. 1)
        ConductionBonus,      // +X% obrażeń przewodzenia
        ConductionJump,       // >0: przewodzenie przeskakuje na najbliższego wroga (połowa)
        DodgeShockCharge,     // >0: po uniku następny cios bronią poraża
        BurnImmunity,         // >0: postać nie płonie
        BurningDamageBonus,   // +X% obrażeń, gdy postać płonie
    }

    public enum HitOutcome { Ignored, Hit, Dodged, Parried, Blocked, GuardBroken }

    public enum ActionType { None, LightAttack, HeavyAttack, Riposte, Block, Parry, Dodge, Cast, Flask, Flinch, GuardBroken, Dead }

    public enum ActionPhase { Startup, Active, Recovery, Finished }

    public enum RewardKind { Item, Boon, LearnSpell, UpgradeSpell }

    public enum UnlockKind { StartingItem, Spell, Boon, Difficulty }

    [System.Flags]
    public enum BuildTag
    {
        None = 0,
        Melee = 1 << 0,
        Magic = 1 << 1,
        Shield = 1 << 2,
        Parry = 1 << 3,
        Heavy = 1 << 4,
        Agile = 1 << 5,
        Flask = 1 << 6,
        Guard = 1 << 7,
    }
}
