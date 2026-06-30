using System.ComponentModel;
using UnityEngine;

public class WanderingEnemy : EnemyBase
{

    // -------------------------------------------------------------------------
    // MOVIMENTO HORIZONTAL (Com Aceleração e Derrapagem)
    // -------------------------------------------------------------------------
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 0.12f;
    [SerializeField] float deceleration = 0.18f;
    [SerializeField] float skidDeceleration = 0.4f;
    private float movingTimer = 0;
    [SerializeField] private float movingTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float stillTimer = 0;
    [SerializeField] private float stillTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float chaseTimer = 0;
    [SerializeField] private float chaseTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]


    // -------------------------------------------------------------------------
    // PULO
    // -------------------------------------------------------------------------
    [Header("Jump")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float jumpBlockTime = 0.08f;
    float jumpBlockTimer;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] LayerMask playerLayers;

    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    bool isGrounded;

    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;

    bool onWall;

    [SerializeField] Transform PlayerDetectionOrigin;

    /// <summary>
    /// Perseguir jogador e outras coisas
    /// </summary>

    private enum State { Wander, Chase, Stay};
    [SerializeField]private State currState = State.Wander;
    float xInput;
    int dir = 1; // 1 = direita, -1 = esquerda

    [SerializeField] private Transform target;
    [SerializeField] private Transform spriteTransform;
    private bool playerIsInRange;
    
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

    protected void Update()
    {
        
        CheckGround();
        CheckWall();
        DetectPlayer();

        if (currState == State.Chase && (target != null) && (Mathf.Abs(target.position.x - transform.position.x) > 0.03)) 
        {
            xInput = Mathf.Sign(target.position.x - transform.position.x);
        }
        else if (currState == State.Stay)
        {
            xInput = 0; // Garante que fique parado no frame atual
        }
        else 
        {
            xInput = dir * 0.4f; // Wander usa a direção padrão
        }
        
        // 1. Define para qual direção o sprite deve olhar de forma limpa
        float lookDirection = (currState == State.Chase) ? xInput : dir;

        // 2. Aplica o flip sem acumular multiplicação (usando o valor absoluto da escala atual)
        if (lookDirection != 0)
        {
            float escalaXAbsoluta = Mathf.Abs(spriteTransform.localScale.x);
            spriteTransform.localScale = new Vector3(escalaXAbsoluta * Mathf.Sign(lookDirection), spriteTransform.localScale.y, spriteTransform.localScale.z);
        }
        
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

        waitStartTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;
        jumpBlockTimer -= Time.deltaTime;

        if (onWall && currState != State.Chase) dir *= -1;

        if (currHp <= 0 && !isImortal) Destroy(gameObject);
    }

    
    protected void FixedUpdate()
    {
        Move();
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("EnemyJump"))
        {
            if (isGrounded && jumpBlockTimer <= 0) Jump();
        }
    }

    void Jump()
    {
        groundDisableTimer = groundDisableTime;
        jumpBlockTimer = jumpBlockTime;

        rb.gravityScale = 10f;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void Move()
    {
        if (waitStartTimer > 0) return;

        float targetVelocity = xInput * moveSpeed;
        float speedDiff = targetVelocity - rb.velocity.x;
        float forceFactor;

        if (xInput != 0 )
        {
            // DERRAPAGEM: Só acontece se estiver no CHÃO (isGrounded), mudando de direção e com velocidade
            if (isGrounded && Mathf.Sign(xInput) != Mathf.Sign(rb.velocity.x) && Mathf.Abs(rb.velocity.x) > 0.5f)
            {
                forceFactor = skidDeceleration; // Frena forte no chão (Drift)
            }
            else
            {
                forceFactor = acceleration; // Aceleração normal (tanto correndo em linha reta quanto no ar)
            }
        }
        else
        {
            forceFactor = deceleration; // Desaceleração passiva ao soltar o botão
        }

        rb.AddForce(Vector2.right * speedDiff * forceFactor, ForceMode2D.Force);

        // Mantém o Clamp original
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
        RaycastHit2D hit = Physics2D.CircleCast(PlayerDetectionOrigin.position, (col.bounds.size.y * 0.9f), Vector2.right * dir, detectionDistance, obstacleLayers); //Adiciona a layer do player como obstaclo

        if (hit.collider != null && hit.collider.CompareTag("PlayerHurtbox") && hit.collider.gameObject.layer == playerLayers)
        {   
            target = hit.transform.root;
            ChangeState(State.Chase);
        }
    }
    void CheckGround()
    {
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundDistance,
            obstacleLayers
        );

        isGrounded = hit;
    }

    void CheckWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * dir,
            wallDistance,
            obstacleLayers
        );

        onWall = hit;
    }

    private void ChangeState(State newState)
{
    currState = newState;

    // Reseta o timer específico do estado que está entrando
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

    void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);

        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left  * wallDistance);
        }
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}