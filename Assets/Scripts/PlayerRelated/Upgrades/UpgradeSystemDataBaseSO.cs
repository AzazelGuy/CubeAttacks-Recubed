using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "Entities/Upgrades/Upgrade Database")]
public class UpgradeSystemDataBaseSO : ScriptableObject
{
    [Header("All Registered Upgrades in the Game")]
    [SerializeField] private List<UpgradeSystemDataSO> allUpgrades = new List<UpgradeSystemDataSO>();

    // Internal Dictionary for fast string lookup
    private Dictionary<string, UpgradeSystemDataSO> upgradeLookup;

    private void OnEnable()
    {
        InitializeLookup();
    }

    private void InitializeLookup()
    {
        upgradeLookup = new Dictionary<string, UpgradeSystemDataSO>();

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;

            if (string.IsNullOrEmpty(upgrade.id))
            {
                Debug.LogWarning($"[UpgradeDatabase] Upgrade asset '{upgrade.name}' has no string ID assigned!");
                continue;
            }

            if (!upgradeLookup.ContainsKey(upgrade.id))
            {
                upgradeLookup.Add(upgrade.id, upgrade);
            }
            else
            {
                Debug.LogError($"[UpgradeDatabase] Duplicate upgrade ID found: '{upgrade.id}'!");
            }
        }
    }

    /// <summary>
    /// Converts a string ID saved on disk back into an UpgradeDataSO asset.
    /// </summary>
    public UpgradeSystemDataSO GetUpgradeByID(string id)
    {
        if (upgradeLookup == null || upgradeLookup.Count != allUpgrades.Count)
        {
            InitializeLookup();
        }

        if (upgradeLookup.TryGetValue(id, out UpgradeSystemDataSO foundUpgrade))
        {
            return foundUpgrade;
        }

        Debug.LogWarning($"[UpgradeDatabase] Upgrade ID '{id}' was not found in the database!");
        return null;
    }

    public List<UpgradeSystemDataSO> GetAllUpgrades() => allUpgrades;
}