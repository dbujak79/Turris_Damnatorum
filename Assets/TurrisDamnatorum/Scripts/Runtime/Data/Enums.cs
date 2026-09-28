namespace Turris
{
    public enum DamageType { Physical, Magic }

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
    }

    public enum AttributeType { Vigor, Endurance, Mind, Strength, Dexterity, Intelligence }

    public enum ModifierMode { Flat, Percent }

    public enum EquipSlot { MainHand, OffHand, Head, Body, Hands, Belt, Feet, Ring1, Ring2, Amulet }

    public enum ItemKind { Weapon, Shield, Head, Body, Hands, Belt, Feet, Ring, Amulet }

    public enum AttackDelivery { Melee, Projectile, AreaAroundSelf }

    public enum SpellKind { Projectile, Nova, Heal, WeaponBuff }

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
