using Unity.Mathematics;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInGameDataHolder : MonoBehaviour
{
    public static PlayerInGameDataHolder Instance;

    private float playerHealthCurr = 100.0f;
    private float playerShieldCurr = 100.0f;

    private bool setUp = false;

    [SerializeField] private PlayerData playerData;

    // --- UI FILL REFERENCES & SMOOTHING ---
    [Header("UI Fill References")]
    [SerializeField] private Image healthFillImage; 
    [SerializeField] private Image shieldFillImage; 

    [Header("UI Game Feel")]
    [Tooltip("How fast the UI bar smoothly catches up to actual health.")]
    [SerializeField] private float fillLerpSpeed = 5.0f; 

    // Internal trackers for smooth interpolation
    private float displayedHealthRatio = 1.0f;
    private float displayedShieldRatio = 1.0f;

    [Header("Health Regen")]
    [SerializeField] float timeUntilRegen = 15f;
    [SerializeField] float regenMult = 0.01f;
    float timerUntilRegen;

    [Header("Shield Regen")]
    [SerializeField] float shieldRegenDelay = 5f;
    [SerializeField] float shieldRegenMult = 0.01f;
    [SerializeField] float shieldRegenDelayMultiplierWhenEmpty = 2f;
    float shieldRegenTimer;

    [SerializeField] Transform originPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            var player = Instantiate(playerData.Prefab, originPlayer.position, quaternion.identity);

            OnPlayerSpawned?.Invoke(player.transform);
        }
        else
        {
            Destroy(gameObject);
        }

        SetUpVariables(playerData.playerHealthMax, playerData.playerShieldMax);
    }

    void Start()
    {
        healthFillImage = UIControllerInGame.Instance.Healthfill;
        shieldFillImage = UIControllerInGame.Instance.Shieldfill;

        // Instantly snap displayed values on start so health doesn't drain from 0
        if (playerData != null)
        {
            displayedHealthRatio = playerHealthCurr / playerData.playerHealthMax;
            displayedShieldRatio = playerShieldCurr / playerData.playerShieldMax;
        }

        UpdateHealthUI();
    }

    void Update()
    {
        timerUntilRegen -= Time.deltaTime;
        shieldRegenTimer -= Time.deltaTime;

        playerHealthCurr = Mathf.Clamp(playerHealthCurr, 0f, playerData.playerHealthMax);
        playerShieldCurr = Mathf.Clamp(playerShieldCurr, 0f, playerData.playerShieldMax);

        if (timerUntilRegen <= 0f && playerHealthCurr < playerData.playerHealthMax)
        {
            playerHealthCurr = Mathf.Lerp(playerHealthCurr, playerData.playerHealthMax, regenMult);
            if (playerHealthCurr >= (playerData.playerHealthMax - 0.5f)) playerHealthCurr = playerData.playerHealthMax;
        }

        if (shieldRegenTimer <= 0f && playerShieldCurr < playerData.playerShieldMax)
        {
            playerShieldCurr = Mathf.Lerp(playerShieldCurr, playerData.playerShieldMax, shieldRegenMult);
            if (playerShieldCurr >= (playerData.playerShieldMax - 0.5f)) playerShieldCurr = playerData.playerShieldMax;
        }

        // SMOOTHLY INTERPOLATE UI DISPLAY EVERY FRAME
        SmoothUpdateUI();
    }

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

        // Note: No need to manually call UpdateHealthUI here anymore 
        // because Update() now smoothly handles transitions continuously!
    }

    // --- SMOOTH LERP LOGIC ---
    private void SmoothUpdateUI()
    {
        if (playerData == null) return;

        float targetHealthRatio = playerHealthCurr / playerData.playerHealthMax;
        float targetShieldRatio = playerShieldCurr / playerData.playerShieldMax;

        // Smoothly interpolate current display values towards target values
        displayedHealthRatio = Mathf.Lerp(displayedHealthRatio, targetHealthRatio, Time.deltaTime * fillLerpSpeed);
        displayedShieldRatio = Mathf.Lerp(displayedShieldRatio, targetShieldRatio, Time.deltaTime * fillLerpSpeed);

        // Apply updated values to materials
        UpdateHealthUI();
    }

    // --- MATERIAL PROPERTY SETTER ---
    private void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.materialForRendering.SetFloat("_FillAmount", displayedHealthRatio);
        }

        if (shieldFillImage != null)
        {
            shieldFillImage.materialForRendering.SetFloat("_FillAmount", displayedShieldRatio);
        }
    }

    public float Health => playerHealthCurr;
    public float MaxHealth => playerData.playerHealthMax;
    public float Shield => playerShieldCurr;
    public float MaxShield => playerData.playerShieldMax;

    public static event Action<Transform> OnPlayerSpawned;
}