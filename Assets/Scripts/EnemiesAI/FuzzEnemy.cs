using System.ComponentModel;
using UnityEngine;

public class FuzzEnemy : EnemyBase
{
    #region Variable Declarations
        #region Horizontal Movement
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 0.12f;
    [SerializeField] float deceleration = 0.18f;
    [SerializeField] float skidDeceleration = 0.4f;
    int dir = 1; // 1 = direita, -1 = esquerda
    private float movingTimer = 0;
    [SerializeField] private float movingTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float stillTimer = 0;
    [SerializeField] private float stillTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float chaseTimer = 0;
    [SerializeField] private float chaseTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
        #endregion

        #region Detection and States
    [Header("Detection")]
    [SerializeField] LayerMask playerLayers;
    [SerializeField] Transform PlayerDetectionOrigin;

    private enum State { Wander, Chase, Stay};
    [SerializeField]private State currState = State.Wander;
    float xInput;

    [SerializeField] private Transform target;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private float spinSpeed = 720f;
        #endregion
    
    #endregion

    protected override void Start()
    {
        base.Start();
        speed *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        damage *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        
        moveSpeed = speed;
        movingTime *= Random.Range(1f - variationPercentage, 1f + variationPercentage); 
        movingTimer = movingTime;

        target = null;

        if (!isImortal){
            // Randomizes extra amount of health (in %) and applies it
            maxHp *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
            currHp = maxHp;
        }

    }

    protected override void Update()
    {
        CheckActivation();
        
        DetectPlayer();
        DirectionBehavior();
        StateUpdater();
        SpinSprite();
        
        waitStartTimer -= Time.deltaTime;
    }
    
    protected void FixedUpdate()
    {
        Move();
    }

    #region Movement Logic

    void DirectionBehavior()
    {
        if (currState == State.Chase && target != null)
        {
            xInput = Mathf.Sign(target.position.x - transform.position.x);
        }
        else if (currState == State.Stay)
        {
            xInput = 0f;
        }
        else 
        {
            xInput = dir * 0.4f;
        }
    }

    void StateUpdater()
    {
        switch (currState)
        {
            case State.Wander:    
                movingTimer -= Time.deltaTime;
                if (movingTimer <= 0) ChangeState(State.Stay); // Cansei de andar, vou parar
            break;

            case State.Stay:
                stillTimer -= Time.deltaTime;
                if (stillTimer <= 0) ChangeState(State.Wander); // Cansei de ficar parado, vou andar
            break;

            case State.Chase:
                chaseTimer -= Time.deltaTime;
                if (chaseTimer <= 0) {
                    ChangeState(State.Wander); // Perdi o player de vista, vou voltar a passea
                    target = null;
                    }
            break;
        }
    }
    
    void SpinSprite()
    {
        if (spriteTransform == null) return;

        float currentSpeed = Mathf.Abs(rb.velocity.x);
        float direction = currentSpeed > 0.01f
            ? Mathf.Sign(rb.velocity.x)
            : Mathf.Sign(xInput);

        if (direction != 0f)
        {
            float speedRatio = Mathf.Clamp01(currentSpeed / Mathf.Max(moveSpeed, 0.0001f));
            float spinRate = spinSpeed * speedRatio;
            spriteTransform.Rotate(0f, 0f, spinRate * Time.deltaTime * direction);
        }
    }

    void Move()
    {
        if (waitStartTimer > 0) return;
        
        float targetVelocity = xInput * moveSpeed;
        float speedDiff = targetVelocity - rb.velocity.x;
        float forceFactor;

        if (xInput != 0 )
        {
            if (Mathf.Sign(xInput) != Mathf.Sign(rb.velocity.x) && Mathf.Abs(rb.velocity.x) > 0.5f)
            {
                forceFactor = skidDeceleration;
            }
            else
            {
                forceFactor = acceleration;
            }
        }
        else
        {
            forceFactor = deceleration;
        }

        rb.AddForce(Vector2.right * speedDiff * forceFactor, ForceMode2D.Force);

        if (xInput != 0)
        {
            rb.velocity = new Vector2(
                Mathf.Clamp(rb.velocity.x, -moveSpeed, moveSpeed),
                rb.velocity.y
            );
        }
    }

    void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(
            PlayerDetectionOrigin.position,
            detectionDistance,
            playerLayers
        );

        if (player == null)
            return;

        target = player.transform.root;
        ChangeState(State.Chase);
    }

    private void ChangeState(State newState)
    {
        if (currState == newState) return;

        currState = newState;

        switch (newState)
        {
            case State.Wander:
                movingTimer = movingTime; 
                dir = Random.Range(0, 2) == 0 ? 1 : -1;
                break;

            case State.Stay:
                stillTimer = stillTime;
                xInput = 0;
                break;

            case State.Chase:
                chaseTimer = chaseTime;
                break;
        }
    }
    #endregion
    
    public override void OnHit(float recievedDamage)
    {
        base.OnHit(recievedDamage);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}