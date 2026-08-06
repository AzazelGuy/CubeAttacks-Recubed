using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    // Economy
    public int currency = 0;

    // Upgrades System
    public List<string> unlockedUpgradeIDs = new List<string>();
    public List<string> equippedUpgradeIDs = new List<string>(); // Loadout for next stage run

    // Permanent Progression (Meta-Upgrades from Hub)
    public float permanentExtraHealth = 0f;
    public float permanentExtraShield = 0f;

    // Character State
    public int lastSelectedCharacterID = 0;

    // Default Constructor
    public PlayerSaveData()
    {
        currency = 0;
        unlockedUpgradeIDs = new List<string>();
        equippedUpgradeIDs = new List<string>();
        permanentExtraHealth = 0f;
        permanentExtraShield = 0f;
        lastSelectedCharacterID = 0;
    }
}