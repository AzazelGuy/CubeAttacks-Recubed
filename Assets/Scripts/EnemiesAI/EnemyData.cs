using UnityEngine;

/// <summary>
/// ScriptableObject containing the configuration data for an enemy.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Entities/Enemies/Enemy")]
public class EnemyData : ScriptableObject
{
    // -------------------------------------------------------------------------
    // BASIC IDENTITY
    // -------------------------------------------------------------------------
    #region Basic Identity
    [Tooltip("Name displayed for the enemy in-game and in editor tools.")]
    public string enemyName;
    #endregion

    // -------------------------------------------------------------------------
    // CORE STATS
    // -------------------------------------------------------------------------
    #region Core Stats
    [Tooltip("If enabled, the enemy becomes immortal and ignores HP and defense values.")]
    public bool isImortal = false;

    [ShowIf(nameof(isImortal), false)]
    [Range(10f, 9999f)]
    [Tooltip("Maximum health points of the enemy.")]
    public float maxHp;

    [ShowIf(nameof(isImortal), false)]
    [Tooltip("Amount of damage reduction applied to incoming hits.")]
    public float defense;

    [Tooltip("Movement speed of the enemy.")]
    public float speed;

    [Tooltip("Distance at which the enemy can detect the player.")]
    public float detectionDistance;
    #endregion

    // -------------------------------------------------------------------------
    // ATTACK SETTINGS
    // -------------------------------------------------------------------------
    #region Attack Settings
    [Tooltip("Enables attack behavior for the enemy.")]
    public bool hasAttack = false;

    [ShowIf(nameof(hasAttack))]
    [Tooltip("Maximum attack range for this enemy.")]
    public float attackDistance;

    [ShowIf(nameof(hasAttack))]
    [Range(0.01f, 30f)]
    [Tooltip("Time between attacks in seconds.")]
    public float attackSpeed;
    #endregion

    // -------------------------------------------------------------------------
    // ENEMY CLASSIFICATION
    // -------------------------------------------------------------------------
    #region Enemy Classification
    public enum Tier { Alfa, Beta, Gamma, Delta }
    public enum TypeOfEnemy { LongRange, Melee, Followers, Especial }

    [Tooltip("Combat tier used for balancing and wave design.")]
    public Tier tier;

    [Tooltip("Enemy category used to define behavior patterns.")]
    public TypeOfEnemy type;
    #endregion

    // -------------------------------------------------------------------------
    // DROPS
    // -------------------------------------------------------------------------
    #region Drops
    [Tooltip("Enables item drops when the enemy is defeated.")]
    public bool hasDrops = false;

    [ShowIf(nameof(hasDrops), true)]
    [Tooltip("List of possible drops for this enemy.")]
    public DropEntry[] drops;
    #endregion

    // -------------------------------------------------------------------------
    // WEAPON SETTINGS
    // -------------------------------------------------------------------------
    #region Weapon Settings
    [Tooltip("Use a weapon ScriptableObject instead of a simple numeric damage value.")]
    public bool usesWeapon = false;

    [ShowIf(nameof(usesWeapon))]
    [Tooltip("Weapon asset used by this enemy when usesWeapon is enabled.")]
    public WeaponData weapon;

    [ShowIf(nameof(usesWeapon), false)]
    [Tooltip("Fallback damage value used when the enemy does not use a weapon asset.")]
    public float damageAttack;
    #endregion

    // -------------------------------------------------------------------------
    // COMBAT BALANCE
    // -------------------------------------------------------------------------
    #region Combat Balance
    [Range(0f, 100f)]
    [Tooltip("Chance for attacks to critically hit, in percent.")]
    public float critChance;

    [Range(0.1f, 1f)]
    [Tooltip("Variation percentage applied to damage calculations.")]
    public float variationPercentage = 0.2f;
    #endregion

    // -------------------------------------------------------------------------
    // WAVE SPAWNING
    // -------------------------------------------------------------------------
    #region Wave Spawning
    [Tooltip("Prefab used when spawning this enemy in waves.")]
    public GameObject enemyPrefab;
    #endregion
}

/// <summary>
/// Data container for a possible drop item from an enemy.
/// </summary>
[System.Serializable]
public class DropEntry
{
    [Tooltip("Prefab of the item that can be dropped.")]
    public GameObject itemPrfab;

    [Range(0f, 100f)]
    [Tooltip("Chance for this drop to occur, in percent.")]
    public float chance;
}