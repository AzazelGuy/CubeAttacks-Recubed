using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Entities/Upgrades/Upgrade Data")]
public class UpgradeSystemDataSO : ScriptableObject
{
    public string id;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    public enum UpgradeType
    {
        StatModifier,       // Speed, Acceleration, Max HP/Shield
        MovementMechanic,   // Extra Jumps, Wall Stick, Wall Climb
        ComponentSpawner,   // Spawns scripts/objects (Pyromaniac, Drones, Auras)
        DamageReaction      // Triggered when taking damage (Masochist)
    }

    [Header("Upgrade Category")]
    public UpgradeType type;

    [Header("Character Restrictions")]
    public bool isCharacterExclusive = false;
    public List<int> allowedCharacterIDs = new List<int>();
    public List<int> blacklistedCharacterIDs = new List<int>();

    [Header("Stat Modifiers")]
    public float bonusSpeed = 0f;
    public float speedMultiplier = 1f;
    public float bonusHealth = 0f;
    public float bonusShield = 0f;
    public float desacelerationMultipliyer = 1f;

    [Header("Movement Mechanics")]
    public int extraJumps = 0;
    public bool enableWallStick = false;
    public bool enableWallClimb = false;
    public bool replaceWallJumpWithClimb = false;

    [Header("Component / Spawner Settings (Pyromaniac, Dashes, Drones)")]
    public GameObject componentPrefab;

    /// Check if character is compatible
    public bool CanBeEquippedBy(int characterID)
    {
        if (blacklistedCharacterIDs.Contains(characterID)) return false;
        if (isCharacterExclusive) return allowedCharacterIDs.Contains(characterID);
        return true;
    }
}