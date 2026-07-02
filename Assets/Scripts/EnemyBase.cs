using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    #region Variaveis
    // -------------------------------------------------------------------------
    // REFERÊNCIAS DE COMPONENTES
    // -------------------------------------------------------------------------
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spr;

    /// <summary>
    /// DADOS DOS INIMIGOS
    /// </summary>
    #region  DADOS DE INIMIGOS
    [Header("Informações do Inimigo")]
    [SerializeField] protected EnemyData enemyData;
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

    #endregion DADOS DE INIMIGOS

    // -------------------------------------------------------------------------
    // ESTADO ESPECIAL
    // -------------------------------------------------------------------------
    #region ESTADO ESPECIAL
    [Header("Outros")]
    [SerializeField] protected float waitStartTime = 0.5f;
    protected float waitStartTimer;
    private float checkTimer = 0.2f;

    #endregion ESTADO ESPECIAL

    protected bool isActive;
    [SerializeField] private float activationDistance = 45f;
    [SerializeField] private float deactivationDistance = 50;

    private Transform cam;

    #endregion


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

        // Setting up the enemy data
        enemyName = enemyData.enemyName;
        maxHp = enemyData.maxHp;
        currHp = maxHp;
        defense = enemyData.defense;
        speed = enemyData.speed;
        detectionDistance = enemyData.detectionDistance;
        attackDistance = enemyData.attackDistance;
        attackSpeed = enemyData.attackSpeed;
        isImortal = enemyData.isImortal;
        tier = enemyData.tier;
        type = enemyData.type;
        weapon = enemyData.weapon;
        damage = enemyData.damage;
        critChance = enemyData.critChance;
        variationPercentage = enemyData.variationPercentage;
        cam = Camera.main.transform;
        waitStartTimer = waitStartTime;
    }

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
        float distance = Vector2.Distance(
            transform.position,
            cam.position
        );

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

    public virtual void OnHit(float recievedDamage)
    {
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
        float damageFinal = recievedDamage * (255f / (255 + defense));
        if (!isImortal) currHp -= damageFinal;
        Debug.Log(currHp);
        damagepopUp.transform.position = transform.position + offset;
        damagepopUp.GetComponent<DamagePopUp>().Setup(damageFinal);
        if (playerCrit) {
            damagepopUp.GetComponent<DamagePopUp>().ONCRIT();
            var criticalText = Instantiate(popUpDamage);
            criticalText.GetComponent<DamagePopUp>().SetupCrit();
            criticalText.transform.position = transform.position;
            }
    }

    public virtual float AttackPlayer()
    {
        return damage;
    }
}
