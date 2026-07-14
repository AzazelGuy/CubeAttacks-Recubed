using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Scout-style enemy AI with wandering, staying, attacking, and simple jumping behaviors.
/// </summary>
public class ReScoutAI : EnemyBase
{
    #region Serialized Fields
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
    private bool previousOnWall;

    [Tooltip("Time the enemy keeps chasing after losing sight of the player.")]
    [SerializeField] private float targetLoseTime = 5f;
    private float targetLostTimer = 0f;

    [Tooltip("Random speed multiplier range applied per wander segment.")]
    [SerializeField] private Vector2 speedVariationRange = new Vector2(0.85f, 1.15f);

    [Tooltip("Chance to stop and stay still after a wander segment ends.")]
    [SerializeField][Range(0f, 1f)] private float stopChance = 0.4f;

    [Tooltip("Chance to flip direction after staying still.")]
    [SerializeField][Range(0f, 1f)] private float flipChanceAfterStop = 0.5f;

    [Tooltip("Origin point used for player detection and line-of-sight checks.")]
    [SerializeField] Transform PlayerDetectionOrigin;

    [Header("Cover")]
    [Tooltip("Tag used to find cover objects in the scene.")]
    [SerializeField] private string coverTag = "Cover";

    [Tooltip("Optional manual cover points to search for.")]
    [SerializeField] private Transform[] coverPoints;

    [Tooltip("Maximum distance to search for cover.")]
    [SerializeField] private float coverSearchRadius = 25f;

    [Tooltip("How close the AI must get to a cover point to be considered in cover.")]
    [SerializeField] private float coverReachDistance = 1f;

    [Tooltip("Point used to spawn bullets when peeking.")]
    [SerializeField] private Transform shootPoint;

    [Tooltip("Weapon component used for aiming and bullet spawning.")]
    [SerializeField] private ReScoutWeapon enemyWeapon;

    [Tooltip("Time the enemy stays hidden before peeking.")]
    [SerializeField] private float peekInterval = 3f;

    [Tooltip("How long the enemy stays peeking/shooting.")]
    [SerializeField] private float peekDuration = 1f;
    [Tooltip("Number of shots fired while peeking.")]
    [SerializeField] private int shotsPerPeek = 3;
    [Tooltip("Delay between peek shots.")]
    [SerializeField] private float peekShotInterval = 0.25f;
    [Tooltip("Offset of the player's Origin Point.")]
    [SerializeField] private Vector3 Offset;

    private Transform selectedCover;
    private bool isCrouched;
    private bool isPeeking;
    private int shotsFired;
    private float peekTimer;
    private float peekShotTimer;
    private float peekDurationTimer;
    #endregion

    // -------------------------------------------------------------------------
    // STATE AND AI REFERENCES
    // -------------------------------------------------------------------------
    #region State and AI References
    private enum State { Wander, LookingForCover, Hidding, Shooting, Stay };
    [SerializeField] private State currState = State.Wander;
    float xInput;

    [Tooltip("Current detected target for attack behavior.")]
    [SerializeField] private Transform target;

    [Tooltip("Sprite transform used for flipping the enemy visually.")]
    [SerializeField] private Transform spriteTransform;

    [Tooltip("Animator component used to drive attack and movement animations.")]
    [SerializeField] private Animator animator;
    private float attackCooldownTimer;
    #endregion
    #endregion

    #region Unity Callbacks
    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        speed *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        damage *= Random.Range(1f - variationPercentage, 1f + variationPercentage);

        moveSpeed = speed * Random.Range(speedVariationRange.x, speedVariationRange.y);
        movingTime *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        movingTimer = movingTime;

        target = null;
        if (enemyWeapon == null)
            enemyWeapon = GetComponentInChildren<ReScoutWeapon>();

        if (shootPoint == null && enemyWeapon != null)
            shootPoint = enemyWeapon.canoDaArma;

        peekTimer = peekInterval;

        // Randomizes extra amount of health (in %) and applies it.
        maxHp *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        currHp = maxHp;
    }

    protected void Update()
    {
        CheckActivation();
        if (!isActive)
            return;

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

        if (currHp <= 0 && !isImortal) Destroy(gameObject);
    }

    protected void FixedUpdate()
    {
        Move();
    }

    #endregion

    #region Physics Callbacks
    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("EnemyJump") && currState != State.Stay)
        {
            if (isGrounded && jumpBlockTimer <= 0) Jump();
        }
    }

    private bool IsTriggerInsideCover(Collider2D collision)
    {
        if (selectedCover == null) return false;

        Vector2 coverPosition = selectedCover.position;
        Vector2 triggerPosition = collision.bounds.center;
        float distance = Vector2.Distance(coverPosition, triggerPosition);
        return distance <= coverReachDistance;
    }
    #endregion

    // -------------------------------------------------------------------------
    // MOVEMENT AND STATE LOGIC
    // -------------------------------------------------------------------------
    #region Movement and State Logic
    #region Decision and Movement
    void DirectionBehavior()
    {
        if (currState == State.LookingForCover)
        {
            if (selectedCover != null)
            {
                xInput = Mathf.Sign(selectedCover.position.x - transform.position.x);
                return;
            }

            xInput = 0f;
            return;
        }

        if (currState == State.Hidding || currState == State.Shooting)
        {
            xInput = 0f;
            return;
        }

        if (target != null)
        {
            float distanceToTarget = Mathf.Abs(target.position.x - transform.position.x);

            if (distanceToTarget > attackDistance && attackCooldownTimer <= 0f)
            {
                xInput = Mathf.Sign(target.position.x - transform.position.x);
                attackCooldownTimer = 0f;
            }
        }

        if (onWall && !previousOnWall && currState != State.Hidding && currState != State.Shooting)
        {
            dir *= -1;
            xInput = dir * 0.4f;
        }

        previousOnWall = onWall;

        if (currState == State.Stay)
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
                if (movingTimer <= 0)
                {
                    if (Random.value < stopChance)
                    {
                        ChangeState(State.Stay);
                    }
                    else
                    {
                        ChangeState(State.Wander);
                    }
                }
                break;

            case State.LookingForCover:
                if (target == null)
                {
                    ChangeState(State.Wander);
                    break;
                }

                if (selectedCover == null)
                {
                    FindCover();
                }

                if (selectedCover != null)
                {
                    float distanceToCover = Vector2.Distance(transform.position, selectedCover.position);
                    if (distanceToCover <= coverReachDistance)
                    {
                        ChangeState(State.Hidding);
                    }
                }
                break;

            case State.Hidding:
                if (target == null)
                {
                    ChangeState(State.Wander);
                    break;
                }

                SetCrouchState(true);
                peekTimer -= Time.deltaTime;
                if (peekTimer <= 0f)
                {
                    ChangeState(State.Shooting);
                }
                break;

            case State.Shooting:
                if (target == null)
                {
                    ChangeState(State.Wander);
                    break;
                }

                if (!isPeeking)
                {
                    StartPeek();
                }

                peekShotTimer -= Time.deltaTime;
                if (peekShotTimer <= 0f && shotsFired < shotsPerPeek)
                {
                    ShootAtTarget();
                    shotsFired++;
                    peekShotTimer = peekShotInterval;
                }

                if (shotsFired >= shotsPerPeek)
                {
                    isPeeking = false;
                    SetCrouchState(true);
                    peekTimer = peekInterval;
                    ChangeState(State.Hidding);
                }
                break;

            case State.Stay:
                stillTimer -= Time.deltaTime;
                if (stillTimer <= 0)
                {
                    if (Random.value < flipChanceAfterStop)
                    {
                        dir *= -1;
                    }
                    ChangeState(State.Wander);
                }
                break;

        }
    }

    void SpriteFlip()
    {
        float lookDirection = 0f;
        if (currState == State.LookingForCover)
        {
            lookDirection = Mathf.Sign(xInput);
        }
        else if (target != null)
        {
            lookDirection = Mathf.Sign(target.position.x - transform.position.x);
        }
        else
        {
            lookDirection = dir;
        }

        if (lookDirection != 0)
        {
            float absoluteXScale = Mathf.Abs(spriteTransform.localScale.x);
            spriteTransform.localScale = new Vector3(absoluteXScale * Mathf.Sign(lookDirection), spriteTransform.localScale.y, spriteTransform.localScale.z);
        }
    }

    void Jump()
    {
        if (isCrouched || isPeeking) return;
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
    #endregion
    #endregion

    #region Detection
    void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(
            PlayerDetectionOrigin.position,
            detectionDistance,
            playerLayers
        );

        bool playerDetected = false;
        if (player != null)
        {
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
                targetLostTimer = 0f;
                playerDetected = true;
                if (currState != State.Hidding && currState != State.Shooting)
                {
                    ChangeState(State.LookingForCover);
                }
            }
        }

        if (target != null && !playerDetected)
        {
            targetLostTimer += Time.deltaTime;
            if (targetLostTimer >= targetLoseTime)
            {
                ClearTarget();
            }
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
        targetLostTimer = 0f;
        if (currState != State.Hidding && currState != State.Shooting)
        {
            ChangeState(State.LookingForCover);
        }
    }

    private void ClearTarget()
    {
        target = null;
        targetLostTimer = 0f;
        selectedCover = null;
        if (currState != State.Wander)
        {
            ChangeState(State.Wander);
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

    private void FindCover()
    {
        selectedCover = null;
        float closestDistance = float.MaxValue;

        if (coverPoints != null)
        {
            foreach (Transform cover in coverPoints)
            {
                if (cover == null) continue;
                float dist = Vector2.Distance(transform.position, cover.position);
                if (dist <= coverSearchRadius && dist < closestDistance)
                {
                    closestDistance = dist;
                    selectedCover = cover;
                }
            }
        }

        if (selectedCover == null && !string.IsNullOrWhiteSpace(coverTag))
        {
            GameObject[] covers = GameObject.FindGameObjectsWithTag(coverTag);
            foreach (GameObject coverObject in covers)
            {
                if (coverObject == null) continue;
                float dist = Vector2.Distance(transform.position, coverObject.transform.position);
                if (dist <= coverSearchRadius && dist < closestDistance)
                {
                    closestDistance = dist;
                    selectedCover = coverObject.transform;
                }
            }
        }
    }

    private void SetCrouchState(bool crouched)
    {
        isCrouched = crouched;
        if (animator == null) return;

        animator.SetBool("IsCrouched", crouched);
    }

    private void StartPeek()
    {
        isPeeking = true;
        shotsFired = 0;
        peekShotTimer = 0f;
        SetCrouchState(false);
        animator.SetTrigger("Peek");
    }

    private void ShootAtTarget()
    {
        if (weapon == null || weapon.bulletPrefab == null || target == null) return;

        Vector2 direction = ((target.position + Offset) - (shootPoint != null ? shootPoint.position : PlayerDetectionOrigin.position)).normalized;

        if (enemyWeapon != null && weapon != null)
        {
            enemyWeapon.Shoot(weapon, direction);
            return;
        }

        Transform spawnPoint = shootPoint != null ? shootPoint : PlayerDetectionOrigin;
        if (spawnPoint == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(weapon.bulletPrefab, spawnPoint.position, Quaternion.Euler(0f, 0f, angle));

        EnemyBulletProjectile projectile = bullet.GetComponent<EnemyBulletProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, weapon.bulletDamage);
        }
    }
    #endregion

    #region State Transitions
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
                movingTimer = Random.Range(movingTime * 0.8f, movingTime * 1.2f);
                moveSpeed = speed * Random.Range(speedVariationRange.x, speedVariationRange.y);
                dir = Random.Range(0, 2) == 0 ? 1 : -1;
                selectedCover = null;
                isPeeking = false;
                SetCrouchState(false);
                break;

            case State.LookingForCover:
                FindCover();
                xInput = 0;
                break;

            case State.Hidding:
                xInput = 0;
                peekTimer = peekInterval;
                isPeeking = false;
                SetCrouchState(true);
                break;

            case State.Shooting:
                xInput = 0;
                break;

            case State.Stay:
                stillTimer = Random.Range(stillTime * 0.8f, stillTime * 1.2f);
                xInput = 0;
                break;
        }
    }
    #endregion

    #region Attack Helpers
    public override void OnHit(float recievedDamage)
    {
        base.OnHit(recievedDamage);
        FindPlayerAfterHit();
    }

    private void UpdateAnimationState()
    {
        if (animator == null) return;

        animator.SetBool("IsCrouched", currState == State.Hidding || currState == State.Shooting);
        animator.SetBool("IsPeeking", currState == State.Shooting);

        switch (currState)
        {
            case State.Wander:
                animator.SetTrigger("Move");
                break;
            case State.LookingForCover:
                animator.SetTrigger("Move");
                break;
            case State.Hidding:
                animator.SetTrigger("Crouch");
                break;
            case State.Shooting:
                animator.SetTrigger("Peek");
                break;
            case State.Stay:
                animator.SetTrigger("Rest");
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
    #endregion

    #region Debug
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

        if (shootPoint != null && target != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 aimDirection = (target.position + Offset - shootPoint.position).normalized;
            Gizmos.DrawLine(shootPoint.position, shootPoint.position + aimDirection * 2f);
            Gizmos.DrawSphere(shootPoint.position + aimDirection * 2f, 0.1f);
        }
    }
    #endregion
}