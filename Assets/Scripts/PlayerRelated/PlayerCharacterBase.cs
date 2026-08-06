using UnityEngine;

/// <summary>
/// Base class for enemy behavior, stats, activation, and combat interactions.
/// </summary>
public class PlayerCharacterBase : MonoBehaviour
{
    [SerializeField] protected Transform damageEffectHealth;
    [SerializeField] protected Transform damageEffectShield;

    [SerializeField] protected Transform popUpDamage;

    // -------------------------------------------------------------------------
    // COMPONENT REFERENCES
    // -------------------------------------------------------------------------
    #region Component References
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spr;
    [SerializeField] protected Animator anim;
    #endregion

    // -------------------------------------------------------------------------
    // PLAYER DATA
    // -------------------------------------------------------------------------
    #region Player Data
    [Header("Informações do jogador")]
    [SerializeField] protected PlayerData playerData;
    protected PlayerVisualData playerVisualData;

    protected string playerName = "Fulano";
    protected int id;
    protected PlayerData.movementTypePlayer movementType;
    protected PlayerData.attackTypePlayer attackType;
    protected bool usesWeapon;

    [ShowIf(nameof(usesWeapon), false)]
    protected float baseMeleedamage;

    [ShowIf(nameof(usesWeapon), false)]
    protected float baseMeleeRange;

    protected bool hasAcceleration;

    #region Acceleration movement variables
    
    protected float moveSpeed = 8f;
    protected float acceleration = 0.12f;
  
    protected float deceleration = 0.18f;
    protected float skidDeceleration = 0.4f;
    #endregion
    protected float movementSpeedBase;
    protected float playerHealthMax = 100.0f;
    protected float playerShieldMax = 100.0f;
    #endregion

    // -------------------------------------------------------------------------
    // PLAYER VISUAL DATA
    // -------------------------------------------------------------------------
    #region Player Visual Data
    [Header("Visual do jogador")]
    protected Sprite playerHudIcon;

    [Range(0, 10)]
    protected ParticleSystem[] particles;
    #endregion

    // -------------------------------------------------------------------------
    // UNITY LIFECYCLE
    // -------------------------------------------------------------------------
    #region Unity Lifecycle
    protected virtual void Start()
    {
        ApplyDataFromScriptableObjects();
    }

    protected virtual void ApplyDataFromScriptableObjects()
    {
        if (playerData == null)
        {
            return;
        }

        playerName = playerData.playerName;
        id = playerData.id;
        movementType = playerData.movementType;
        attackType = playerData.attackType;
        usesWeapon = playerData.usesWeapon;
        baseMeleedamage = playerData.baseMeleedamage;
        baseMeleeRange = playerData.baseMeleeRange;
        hasAcceleration = playerData.hasAcceleration;
        moveSpeed = playerData.moveSpeed;
        acceleration = playerData.acceleration;
        deceleration = playerData.deceleration;
        skidDeceleration = playerData.skidDeceleration;
        movementSpeedBase = playerData.movementSpeedBase;
        playerHealthMax = playerData.playerHealthMax;
        playerShieldMax = playerData.playerShieldMax;

        if (playerVisualData == null)
        {
            return;
        }

        playerHudIcon = playerVisualData.playerHudIcon;
        particles = playerVisualData.particles;
    }
    #endregion

    // -------------------------------------------------------------------------
    // COMBAT LOGIC
    // -------------------------------------------------------------------------
    #region Combat Logic
    public virtual void OnPlayerHit(float recievedDamage)
    {
        if (damageEffectHealth != null && damageEffectShield != null) 
        { 
            if (PlayerInGameDataHolder.Instance.Shield > 0)
            Instantiate(damageEffectShield, transform);
            else Instantiate(damageEffectHealth, transform);
            
        }

        if (PlayerInGameDataHolder.Instance.HasUpgradeEquipped("up_Pyromaniac"))
        {
            UpgradeSystemDataSO Fire = PlayerInGameDataHolder.Instance.GetUpgradeDataBase.GetUpgradeByID("up_Pyromaniac");

            Instantiate(Fire.componentPrefab,transform.position, Quaternion.identity);
        }
        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        var damagepopUp = Instantiate(popUpDamage);

        float critPlayerChance = .2f;
        bool Crit = (Random.Range(0f, 1f) <= critPlayerChance);

        if (Crit)
        {
            recievedDamage *= 2;
            Debug.Log("Critical!");
        }

        float damageFinal = recievedDamage;

        PlayerInGameDataHolder.Instance.TakeDamage(damageFinal);

        damagepopUp.transform.position = transform.position + offset;
        damagepopUp.GetComponent<DamagePopUp>().Setup(damageFinal);



        if (Crit)
        {
            damagepopUp.GetComponent<DamagePopUp>().ONCRIT();
            var criticalText = Instantiate(popUpDamage);
            criticalText.GetComponent<DamagePopUp>().SetupCrit();
            criticalText.transform.position = transform.position;
        }
    }

    #endregion
}
