using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Basic wandering enemy AI with wandering, staying, chasing, and simple jumping behaviors.
/// </summary>
public class WanderingEnemy : EnemyBase
{
    // -------------------------------------------------------------------------
    // MOVEMENT SETTINGS
    // -------------------------------------------------------------------------
    #region Movement Settings
    [Header("Movement")]
    [Tooltip("Horizontal movement speed while active.")]
    [SerializeField] float moveSpeed = 8f;

    [Tooltip("Acceleration applied when moving.")]
    [SerializeField] float acceleration = 0.12f;

    [Tooltip("Deceleration applied when slowing down.")]
    [SerializeField] float deceleration = 0.18f;

    [Tooltip("Stronger braking force used when changing direction on the ground.")]
    [SerializeField] float skidDeceleration = 0.4f;

    [Tooltip("Impulse applied after attacking to push the enemy away from the target.")]
    [SerializeField] private float postAttackPushForce = 2f;

    [Tooltip("Time during which deceleration is disabled after an attack.")]
    [SerializeField] private float desacelerationDisableTime = 0.25f;
    private float desacelerationDisableTimer = 0f;
    int dir = 1; // 1 = direita, -1 = esquerda
    private float movingTimer = 0f;

    [Tooltip("Suggested duration for the wandering state before stopping.")]
    [SerializeField] private float movingTime = 10f;

    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float stillTimer = 0f;

    [Tooltip("Suggested duration for the staying state before wandering again.")]
    [SerializeField] private float stillTime = 10f;

    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    private float chaseTimer = 0f;

    [Tooltip("Suggested duration for the chase state before returning to wandering.")]
    [SerializeField] private float chaseTime = 10f;
    [Description("This is a suggestion of time, cause it varries by the enemy's % of variation")]
    #endregion

    // -------------------------------------------------------------------------
    // JUMPING SETTINGS
    // -------------------------------------------------------------------------
    #region Jumping Settings
    [Header("Jump")]
    [Tooltip("Force applied when the enemy jumps.")]
    [SerializeField] float jumpForce = 12f;

    [Tooltip("Time during which jumping is blocked after a jump.")]
    [SerializeField] float jumpBlockTime = 0.08f;
    float jumpBlockTimer;
    #endregion

    // -------------------------------------------------------------------------
    // RAYCAST AND DETECTION SETTINGS
    // -------------------------------------------------------------------------
    #region Raycast and Detection Settings
    [Header("Ground Check")]
    [Tooltip("Reference point used to check if the enemy is touching the ground.")]
    [SerializeField] Transform groundCheck;

    [Tooltip("Distance of the ground raycast.")]
    [SerializeField] float groundDistance = 0.2f;

    [Tooltip("Layers considered as obstacles or walkable surfaces.")]
    [SerializeField] LayerMask obstacleLayers;

    [Tooltip("Layers that count as player targets for detection.")]
    [SerializeField] LayerMask playerLayers;

    [Tooltip("Time during which ground checks are disabled after a jump.")]
    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    bool isGrounded;

    [Header("Wall Check")]
    [Tooltip("Reference point used to check for walls or obstacles in front of the enemy.")]
    [SerializeField] Transform wallCheck;

    [Tooltip("Distance of the wall raycast.")]
    [SerializeField] float wallDistance = 0.2f;

    bool onWall;

    [Tooltip("Origin point used for player detection and line-of-sight checks.")]
    [SerializeField] Transform PlayerDetectionOrigin;
    #endregion

    // -------------------------------------------------------------------------
    // STATE AND AI REFERENCES
    // -------------------------------------------------------------------------
    #region State and AI References
    private enum State { Wander, Chase, Stay };
    [SerializeField] private State currState = State.Wander;
    float xInput;

    [Tooltip("Current target being chased by the enemy.")]
    [SerializeField] private Transform target;

    [Tooltip("Sprite transform used for flipping the enemy visually.")]
    [SerializeField] private Transform spriteTransform;

    [Tooltip("Animator component used to drive attack and movement animations.")]
    [SerializeField] private Animator animator;
    private float attackCooldownTimer;
    #endregion

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        speed *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        damage *= Random.Range(1f - variationPercentage, 1f + variationPercentage);

        moveSpeed = speed;
        movingTime *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        movingTimer = movingTime;

        target = null;

        if (!isImortal)
        {
            // Randomizes extra amount of health (in %) and applies it.
            maxHp *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
            currHp = maxHp;
        }
    }

    protected override void Update()
    {
        base.Update();
        CheckActivation();

        CheckGround();
        CheckWall();
        DetectPlayer();

        DirectionBehavior();
        StateUpdater();
        SpriteFlip();

        if (isGrounded && groundDisableTimer <= 0)
        {
            rb.gravityScale = 10f;
        }
        else
        {
            if (rb.velocity.y > 5f)
            {
                rb.gravityScale = 10f;
            }
            else
            {
                rb.gravityScale = 20f;
            }
        }

        waitStartTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;
        jumpBlockTimer -= Time.deltaTime;
        desacelerationDisableTimer -= Time.deltaTime;

        if (onWall && currState != State.Chase) dir *= -1;

        if (currHp <= 0 && !isImortal) Destroy(gameObject);
    }

    protected void FixedUpdate()
    {
        Move();
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("EnemyJump") && currState != State.Stay)
        {
            if (isGrounded && jumpBlockTimer <= 0) Jump();
        }
    }

    // -------------------------------------------------------------------------
    // MOVEMENT AND STATE LOGIC
    // -------------------------------------------------------------------------
    #region Movement and State Logic
    void DirectionBehavior()
    {
        if (currState == State.Chase && target != null)
        {
            float distanceToTarget = Mathf.Abs(target.position.x - transform.position.x);

            if (distanceToTarget > attackDistance && attackCooldownTimer <= 0f)
            {
                xInput = Mathf.Sign(target.position.x - transform.position.x);
                attackCooldownTimer = 0f;
            }
            else
            {
                xInput = 0f;
                attackCooldownTimer += Time.deltaTime;
                float heightDistanceToTarget = Mathf.Abs(target.position.y - transform.position.y);
                if (attackCooldownTimer >= attackSpeed && heightDistanceToTarget <= 2f)
                {
                    TriggerAttackAnimation();
                    attackCooldownTimer = 0f;
                    float direction = Mathf.Sign(target.position.x - transform.position.x);
                    desacelerationDisableTimer = desacelerationDisableTime;
                    rb.AddForce(Vector2.right * direction * postAttackPushForce, ForceMode2D.Impulse);
                }
            }
        }
        else if (currState == State.Stay)
        {
            xInput = 0; // Garante que fique parado no frame atual.
        }
        else
        {
            xInput = dir * 0.4f; // Wander usa a direção padrão.
        }
    }

    void StateUpdater()
    {
        switch (currState)
        {
            case State.Wander:
                movingTimer -= Time.deltaTime;
                if (movingTimer <= 0) ChangeState(State.Stay); // Cansei de andar, vou parar.
                break;

            case State.Stay:
                stillTimer -= Time.deltaTime;
                if (stillTimer <= 0) ChangeState(State.Wander); // Cansei de ficar parado, vou andar.
                break;

            case State.Chase:
                chaseTimer -= Time.deltaTime;
                if (chaseTimer <= 0)
                {
                    ChangeState(State.Wander); // Perdi o player de vista, vou voltar a passear.
                    target = null;
                }
                break;
        }
    }

    void SpriteFlip()
    {
        float lookDirection = (currState == State.Chase && target != null)
            ? Mathf.Sign(target.position.x - transform.position.x)
            : dir;

        if (lookDirection != 0)
        {
            float absoluteXScale = Mathf.Abs(spriteTransform.localScale.x);
            spriteTransform.localScale = new Vector3(absoluteXScale * Mathf.Sign(lookDirection), spriteTransform.localScale.y, spriteTransform.localScale.z);
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
        if (waitStartTimer > 0 || desacelerationDisableTimer > 0) return;

        float targetVelocity = xInput * moveSpeed;
        float speedDiff = targetVelocity - rb.velocity.x;
        float forceFactor;

        if (xInput != 0)
        {
            // DERRAPAGEM: só acontece se estiver no chão, mudando de direção e com velocidade.
            if (isGrounded && Mathf.Sign(xInput) != Mathf.Sign(rb.velocity.x) && Mathf.Abs(rb.velocity.x) > 0.5f)
            {
                forceFactor = skidDeceleration; // Frena forte no chão (drift).
            }
            else
            {
                forceFactor = acceleration; // Aceleração normal (tanto correndo em linha reta quanto no ar).
            }
        }
        else
        {
            forceFactor = deceleration; // Desaceleração passiva ao soltar o botão.
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

        Vector2 direction = (player.transform.position - PlayerDetectionOrigin.position).normalized;
        float distance = Vector2.Distance(PlayerDetectionOrigin.position, player.transform.position);

        RaycastHit2D wall = Physics2D.Raycast(
            PlayerDetectionOrigin.position,
            direction,
            distance,
            obstacleLayers
        );

        if (wall.collider == null)
        {
            target = player.transform.root;
            ChangeState(State.Chase);
        }
    }

    void FindPlayerAfterHit()
    {
        Collider2D player = Physics2D.OverlapCircle(
            PlayerDetectionOrigin.position,
            detectionDistance * 2,
            playerLayers
        );

        if (player == null)
            return;

        target = player.transform.root;
        ChangeState(State.Chase);
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
        if (currState == newState || !isGrounded) return;

        currState = newState;
        attackCooldownTimer = 0f;
        UpdateAnimationState();

        // Reseta o timer específico do estado que está entrando.
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
        FindPlayerAfterHit();
    }

    private void UpdateAnimationState()
    {
        if (animator == null) return;

        animator.SetBool("isChasing", currState == State.Chase);

        switch (currState)
        {
            case State.Wander:
                animator.SetTrigger("Move");
                break;
            case State.Stay:
                animator.SetTrigger("Rest");
                break;
            case State.Chase:
                animator.SetTrigger("Move");
                break;
        }
    }

    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void StopAttack()
    {
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);

        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left * wallDistance);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}