using UnityEngine;

/// <summary>
/// Base class for enemy behavior, stats, activation, and combat interactions.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // COMPONENT REFERENCES
    // -------------------------------------------------------------------------
    #region Component References
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spr;
    [SerializeField] protected Vector3 dropPositionOffset;
    #endregion

    // -------------------------------------------------------------------------
    // ENEMY DATA
    // -------------------------------------------------------------------------
    #region Enemy Data
    [Header("Informações do Inimigo")]
    [SerializeField] protected EnemyData enemyData;

    protected bool hasAttack;
    protected bool hasDrops;
    protected bool usesWeapon;

    protected DropEntry[] drops;
    protected string enemyName;
    protected float maxHp;
    protected float currHp;
    protected float defense;
    protected float speed;
    protected float detectionDistance;
    protected float attackDistance;
    protected float attackSpeed;
    protected bool isImortal;
    protected EnemyData.Tier tier;
    protected EnemyData.TypeOfEnemy type;
    protected WeaponData weapon;
    protected float damage;
    protected float critChance;
    protected float variationPercentage;
    public GameObject popUpDamage;
    public GameObject damageEffect;
    #endregion

    // -------------------------------------------------------------------------
    // STATE AND BEHAVIOR SETTINGS
    // -------------------------------------------------------------------------
    #region State and Behavior Settings
    [Header("Outros")]
    [SerializeField] protected float waitStartTime = 0.5f;
    protected float waitStartTimer;
    private float checkTimer = 0.2f;

    protected bool isActive;
    [SerializeField] private float activationDistance = 45f;
    [SerializeField] private float deactivationDistance = 50f;

    private Transform cam;
    #endregion

    // -------------------------------------------------------------------------
    // UNITY LIFECYCLE
    // -------------------------------------------------------------------------
    #region Unity Lifecycle
    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"[ERROR!] The object: {gameObject.name} has no data atached!");
            Destroy(gameObject);
            return;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (spr == null) spr = GetComponentInChildren<SpriteRenderer>();

        // Setup enemy stats from data asset.
        enemyName = enemyData.enemyName;
        isImortal = enemyData.isImortal;

        if (!isImortal)
        {
            maxHp = enemyData.maxHp;
            currHp = maxHp;
            defense = enemyData.defense;
        }

        speed = enemyData.speed;
        detectionDistance = enemyData.detectionDistance;

        hasAttack = enemyData.hasAttack;

        if (hasAttack)
        {
            attackDistance = enemyData.attackDistance;
            attackSpeed = enemyData.attackSpeed;
        }

        tier = enemyData.tier;
        type = enemyData.type;

        hasDrops = enemyData.hasDrops;

        if (hasDrops)
        {
            drops = enemyData.drops;
        }

        usesWeapon = enemyData.usesWeapon;

        if (usesWeapon)
        {
            weapon = enemyData.weapon;
        }
        else
        {
            damage = enemyData.damageAttack;
        }

        critChance = enemyData.critChance;
        variationPercentage = enemyData.variationPercentage;

        cam = Camera.main.transform;
        waitStartTimer = waitStartTime;
    }
    #endregion

    // -------------------------------------------------------------------------
    // ACTIVATION LOGIC
    // -------------------------------------------------------------------------
    #region Activation Logic
    protected virtual void CheckActivation()
    {
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0)
        {
            checkTimer = 0.2f;
            ActivateInView();
        }

        if (!isActive)
            return;
    }

    protected void ActivateInView()
    {
        float distance = Vector2.Distance(transform.position, cam.position);

        if (!isActive && distance <= activationDistance)
        {
            SetEnemyActive(true);
        }
        else if (isActive && distance >= deactivationDistance)
        {
            SetEnemyActive(false);
        }
    }

    protected virtual void SetEnemyActive(bool active)
    {
        isActive = active;

        rb.simulated = active;

        if (col != null)
            col.enabled = active;

        if (spr != null)
            spr.enabled = active;

        // Animator
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.enabled = active;

        Debug.Log(active ? "Ativado" : "Desativado");
    }
    #endregion

    // -------------------------------------------------------------------------
    // COMBAT LOGIC
    // -------------------------------------------------------------------------
    #region Combat Logic
    public virtual void OnHit(float recievedDamage)
    {
        if (damageEffect != null) {Instantiate(damageEffect);}
        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        var damagepopUp = Instantiate(popUpDamage);

        if (isImortal)
        {
            damagepopUp.transform.position = transform.position + offset;
            damagepopUp.GetComponent<DamagePopUp>().Imortal();
            return;
        }

        float critPlayerChance = .2f;
        bool playerCrit = (Random.Range(0f, 1f) <= critPlayerChance);

        if (playerCrit)
        {
            recievedDamage *= 2;
            Debug.Log("critico!");
        }

        float damageFinal = recievedDamage;

        if (!isImortal)
        {
            damageFinal *= (255f / (255 + defense));
            currHp -= damageFinal;
        }

        Debug.Log(currHp);

        damagepopUp.transform.position = transform.position + offset;
        damagepopUp.GetComponent<DamagePopUp>().Setup(damageFinal);

        if (currHp <= 0f && !isImortal)
        {
            TryDropLoot();
            Destroy(gameObject);
            return;
        }

        if (playerCrit)
        {
            damagepopUp.GetComponent<DamagePopUp>().ONCRIT();
            var criticalText = Instantiate(popUpDamage);
            criticalText.GetComponent<DamagePopUp>().SetupCrit();
            criticalText.transform.position = transform.position;
        }
    }

    private void TryDropLoot()
    {
        if (!hasDrops || drops == null || drops.Length == 0)
            return;

        foreach (DropEntry entry in drops)
        {
            if (entry == null || entry.itemPrfab == null)
                continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= entry.chance)
            {
                Instantiate(entry.itemPrfab, transform.position + dropPositionOffset, Quaternion.identity);
            }
        }
    }

    public virtual void AttackPlayer(PlayerCharacterBase playerCharacterBase)
    {
        
        if (usesWeapon && weapon != null)
        {
            playerCharacterBase.OnPlayerHit(weapon.bulletDamage);
            return;
        }

        playerCharacterBase.OnPlayerHit(damage);
    }
    #endregion
}
