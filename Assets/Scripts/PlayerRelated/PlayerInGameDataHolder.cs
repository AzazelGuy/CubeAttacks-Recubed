using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInGameDataHolder : MonoBehaviour
{
    #region Fields & References

    public static PlayerInGameDataHolder Instance;

    [Header("Player Data Config")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform originPlayer;

    [Header("Health & Shield State")]
    private float playerHealthCurr = 100.0f;
    private float playerShieldCurr = 100.0f;
    private string nextSpawnID = null;
    private float playerHealthMax;
    private float playerShieldMax;
    private bool setUp = false;

    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image shieldFillImage;
    [SerializeField] private TextMeshProUGUI healthAmntText;
    [SerializeField] private TextMeshProUGUI shieldAmntText;

    [Header("UI Game Feel")]
    [Tooltip("How fast the UI bar smoothly catches up to actual health.")]
    [SerializeField] private float fillLerpSpeed = 5.0f;
    private float displayedHealthRatio = 1.0f;
    private float displayedShieldRatio = 1.0f;

    //PlayerInstance
    private GameObject PlayerIndex = null;

    [Header("Health Regen")]
    [SerializeField] private float timeUntilRegen = 15f;
    [SerializeField] private float regenMult = 0.01f;
    private float timerUntilRegen;

    [Header("Shield Regen")]
    [SerializeField] private float shieldRegenDelay = 5f;
    [SerializeField] private float shieldRegenMult = 0.01f;
    [SerializeField] private float shieldRegenDelayMultiplierWhenEmpty = 2f;
    private float shieldRegenTimer;

    [Header("Weapon Ammo Tracking")]
    public List<WeaponData> Arsenal;
    private List<int> currentAmmoList = new List<int>();

    [Header("Upgrades System")]
    [SerializeField] private UpgradeSystemDataBaseSO upgradeDatabase;
    [SerializeField] private List<UpgradeSystemDataSO> activeStageUpgrades = new List<UpgradeSystemDataSO>();

    [Header("Debug / Testing (Bypasses Save System)")]
    [SerializeField] private bool enableDebugUpgrades = false;
    [SerializeField] private List<UpgradeSystemDataSO> debugTestUpgrades = new List<UpgradeSystemDataSO>();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);

            playerHealthMax = playerData.playerHealthMax;
            playerShieldMax = playerData.playerShieldMax;
            // --- DEBUG OVERRIDE CHECK ---
            if (enableDebugUpgrades)
            {
                activeStageUpgrades.Clear();
                activeStageUpgrades.AddRange(debugTestUpgrades);
                Debug.Log("[DEBUG] Loaded test upgrades directly from Inspector!");
            }
            else if (GameManager.Instance != null && GameManager.Instance.currentSaveData != null)
            {
                LoadStageLoadout(GameManager.Instance.currentSaveData);
            }

            // Spawn player
            var player = Instantiate(playerData.Prefab, originPlayer.position, Quaternion.identity);
            // Apply loaded upgrades
            ApplyEquippedUpgrades(player);
            PlayerIndex = player;
            OnPlayerSpawned?.Invoke(player.transform);
            string id = nextSpawnID;
            if (string.IsNullOrEmpty(id)) return;

            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawns)
            {
                if (sp.spawnID == id)
                {
                    player.transform.position = sp.transform.position;
                    break;
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Fetch UI elements dynamically if available
        if (UIControllerInGame.Instance != null)
        {
            healthFillImage = UIControllerInGame.Instance.Healthfill;
            shieldFillImage = UIControllerInGame.Instance.Shieldfill;
            healthAmntText = UIControllerInGame.Instance.TextHealth;
            shieldAmntText = UIControllerInGame.Instance.TextShield;
        }

        if (playerData != null)
        {

            SetUpVariables(playerHealthMax, playerShieldMax);
            displayedHealthRatio = playerHealthCurr / playerHealthMax;
            displayedShieldRatio = playerShieldCurr / playerShieldMax;
        }

        UpdateHealthUI();
    }

    private void Update()
    {
        if (PauseMenuActions.instance != null && PauseMenuActions.instance.Paused) return;

        timerUntilRegen -= Time.deltaTime;
        shieldRegenTimer -= Time.deltaTime;

        playerHealthCurr = Mathf.Clamp(playerHealthCurr, 0f, playerHealthMax);
        playerShieldCurr = Mathf.Clamp(playerShieldCurr, 0f, playerShieldMax);

        // Health regeneration
        if (timerUntilRegen <= 0f && playerHealthCurr < playerHealthMax)
        {
            playerHealthCurr = Mathf.Lerp(playerHealthCurr, playerHealthMax, regenMult);
            if (playerHealthCurr >= playerHealthMax - 0.5f)
            {
                playerHealthCurr = playerHealthMax;
            }
        }

        // Shield regeneration
        if (shieldRegenTimer <= 0f && playerShieldCurr < playerShieldMax)
        {
            playerShieldCurr = Mathf.Lerp(playerShieldCurr, playerShieldMax, shieldRegenMult);
            if (playerShieldCurr >= playerShieldMax - 0.5f)
            {
                playerShieldCurr = playerShieldMax;
            }
        }

        SmoothUpdateUI();

        if (UIControllerInGame.Instance != null && playerData != null)
        {
            UIControllerInGame.Instance.SetHealthEffect(playerHealthCurr, playerHealthMax);
        }
    }
    #endregion

    #region PlayerTPing
    public void Teleport()
    {
        string id = nextSpawnID;
            if (string.IsNullOrEmpty(id)) return;

            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawns)
            {
                if (sp.spawnID == id)
                {
                    PlayerIndex.transform.position = sp.transform.position;
                    Debug.Log("Teleported!");
                    break;
                    
                }
            }
        return;
    }
    #endregion

    #region Upgrade Processing

    public void LoadStageLoadout(PlayerSaveData saveData)
    {
        if (upgradeDatabase == null || saveData == null) return;

        activeStageUpgrades.Clear();

        // Convert saved string IDs into actual UpgradeSystemDataSO assets
        foreach (string id in saveData.equippedUpgradeIDs)
        {
            UpgradeSystemDataSO upgrade = upgradeDatabase.GetUpgradeByID(id);
            if (upgrade != null)
            {
                activeStageUpgrades.Add(upgrade);
            }
        }
    }

    private void ApplyEquippedUpgrades(GameObject playerObj)
    {
        if (playerObj == null) return;

        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        PlayerCharacterBase character = playerObj.GetComponent<PlayerCharacterBase>();
        int charID = playerData != null ? playerData.id : -1;

        foreach (var upgrade in activeStageUpgrades)
        {
            if (upgrade == null || !upgrade.CanBeEquippedBy(charID)) continue;

            switch (upgrade.type)
            {
                case UpgradeSystemDataSO.UpgradeType.StatModifier:
                    if (movement != null) movement.ApplySpeedUpgrade(upgrade.bonusSpeed, upgrade.speedMultiplier, upgrade.desacelerationMultipliyer);
                    playerHealthMax = playerData.playerHealthMax + upgrade.bonusHealth;
                    playerShieldMax = playerData.playerShieldMax + upgrade.bonusShield;
                    break;

                case UpgradeSystemDataSO.UpgradeType.MovementMechanic:
                    if (movement != null)
                    {
                        if (upgrade.extraJumps > 0) movement.AddExtraJumps(upgrade.extraJumps);
                        if (upgrade.enableWallStick) movement.EnableWallStick();
                        if (upgrade.enableWallClimb) movement.EnableWallClimb(upgrade.replaceWallJumpWithClimb);
                    }
                    break;

                case UpgradeSystemDataSO.UpgradeType.ComponentSpawner:
                    if (upgrade.componentPrefab != null)
                    {
                        Instantiate(upgrade.componentPrefab, playerObj.transform);
                    }
                    break;

                case UpgradeSystemDataSO.UpgradeType.DamageReaction:
                    // Checked dynamically inside PlayerCharacterBase.OnPlayerHit()
                    break;
            }
        }
    }

    public bool HasUpgradeEquipped(string upgradeID)
    {
        return activeStageUpgrades.Exists(u => u != null && u.id == upgradeID);
    }

    #endregion

    #region Health and Shield Systems

    public void ResetTimer()
    {
        timerUntilRegen = timeUntilRegen;
        shieldRegenTimer = shieldRegenDelay;
    }

    public void SetUpVariables(float maxHealth, float maxShield)
    {
        if (setUp) return;

        playerHealthCurr = maxHealth;
        playerShieldCurr = maxShield;
        setUp = true;

        if (maxHealth > 0) displayedHealthRatio = playerHealthCurr / maxHealth;
        if (maxShield > 0) displayedShieldRatio = playerShieldCurr / maxShield;

        UpdateHealthUI();
    }

    public void DestroyMe()
    {
        Instance = null;
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        ResetTimer();

        if (playerShieldCurr > 0f)
        {
            float shieldAbsorbed = Mathf.Min(playerShieldCurr, damage);
            playerShieldCurr -= shieldAbsorbed;
            damage -= shieldAbsorbed;
        }

        if (damage > 0f)
        {
            playerHealthCurr -= damage;
        }

        if (playerShieldCurr <= 0f)
        {
            shieldRegenTimer = shieldRegenDelay * shieldRegenDelayMultiplierWhenEmpty;
        }
    }

    #endregion

    #region UI Management

    private void SmoothUpdateUI()
    {
        if (playerData == null) return;

        float targetHealthRatio = playerHealthCurr / playerHealthMax;
        float targetShieldRatio = playerShieldCurr / playerShieldMax;

        displayedHealthRatio = Mathf.Lerp(displayedHealthRatio, targetHealthRatio, Time.deltaTime * fillLerpSpeed);
        displayedShieldRatio = Mathf.Lerp(displayedShieldRatio, targetShieldRatio, Time.deltaTime * fillLerpSpeed);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (PauseMenuActions.instance?.Paused ?? false) return;

        if (healthFillImage != null)
        {
            healthFillImage.materialForRendering.SetFloat("_FillAmount", displayedHealthRatio);
        }

        if (shieldFillImage != null)
        {
            shieldFillImage.materialForRendering.SetFloat("_FillAmount", displayedShieldRatio);
        }

        if (healthAmntText != null) healthAmntText.text = $"{MathF.Ceiling(playerHealthCurr)}";
        if (shieldAmntText != null) shieldAmntText.text = $"{MathF.Ceiling(playerShieldCurr)}";
    }

    #endregion

    #region Ammo Management

    public void InitializeAmmo(List<WeaponData> arsenal)
    {
        currentAmmoList.Clear();
        for (int i = 0; i < arsenal.Count; i++)
        {
            currentAmmoList.Add(arsenal[i].BulletsMax);
        }
    }

    public int GetAmmo(int index)
    {
        if (index >= 0 && index < currentAmmoList.Count)
        {
            return currentAmmoList[index];
        }
        return 0;
    }

    public void ConsumeAmmo(int index, int amount = 1)
    {
        if (index >= 0 && index < currentAmmoList.Count)
        {
            currentAmmoList[index] = Mathf.Max(0, currentAmmoList[index] - amount);
        }
    }

    public void RefillAllAmmo(List<WeaponData> arsenal)
    {
        if (arsenal == null) return;

        for (int i = 0; i < currentAmmoList.Count; i++)
        {
            if (i < arsenal.Count && arsenal[i] != null)
            {
                currentAmmoList[i] = arsenal[i].BulletsMax;
            }
        }

        Debug.Log("All weapons refilled!");
    }

    public void AddAmmoToSlot(int slotIndex, int amount, WeaponData weaponData)
    {
        if (slotIndex >= 0 && slotIndex < currentAmmoList.Count && weaponData != null)
        {
            currentAmmoList[slotIndex] = Mathf.Clamp(currentAmmoList[slotIndex] + amount, 0, weaponData.BulletsMax);
        }
    }

    #endregion

    #region Properties & Events

    public void setSpawnID(string id) { nextSpawnID = id; }
    public string SpawnId => nextSpawnID;
    public float Health => playerHealthCurr;
    public float MaxHealth => playerData != null ? playerHealthMax : 100f;
    public float Shield => playerShieldCurr;
    public float MaxShield => playerData != null ? playerShieldMax : 100f;
    public GameObject currentPlayer => PlayerIndex;
    public PlayerData playerCurrData => playerData;
    public UpgradeSystemDataBaseSO GetUpgradeDataBase => upgradeDatabase;

    public static event Action<Transform> OnPlayerSpawned;

    #endregion
}