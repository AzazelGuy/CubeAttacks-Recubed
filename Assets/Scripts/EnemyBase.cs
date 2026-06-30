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
    #endregion ESTADO ESPECIAL

    #endregion

    
    protected virtual void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"[ERROR!] The object: {gameObject.name} has no data atached!");
            Destroy(gameObject);
            return;
        }

        if (rb == null) rb  = GetComponent<Rigidbody2D>();
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

        waitStartTimer = waitStartTime;
    }
    
    public virtual void OnHit(float recievedDamage)
    {
        
        float critPlayerChance = .2f;
        bool playerCrit = (Random.Range(0f, 1f) <= critPlayerChance);

        if (playerCrit)
        {
            recievedDamage *= 2;
            Debug.Log("critico!");
        }
        float damageFinal = recievedDamage * (255f/(255+defense));
        if (!isImortal) currHp -= damageFinal;
        Debug.Log(currHp);
        Vector3 offset = new Vector3 (Random.Range(-2f,2f), Random.Range(-2f, 2f)); ;
        var damagepopUp = Instantiate(popUpDamage);
        damagepopUp.transform.position = transform.position + offset;
        damagepopUp.GetComponent<DamagePopUp>().Setup(damageFinal);
        if (playerCrit) damagepopUp.GetComponent<DamagePopUp>().ONCRIT();
    }

    public virtual float AttackPlayer()
    {
        return damage;
    }
}
