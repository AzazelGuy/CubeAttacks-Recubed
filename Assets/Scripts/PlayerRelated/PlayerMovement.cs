using System;
using UnityEngine;

[AddComponentMenu("Player/Advanced Rigidbody 2D Movement")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    #region Variaveis
    // -------------------------------------------------------------------------
    // REFERÊNCIAS DE COMPONENTES
    // -------------------------------------------------------------------------
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D col;
    [SerializeField] private Transform spriteTransform;

    // -------------------------------------------------------------------------
    // MOVIMENTO HORIZONTAL (Com Aceleração e Derrapagem)
    // -------------------------------------------------------------------------
    [Header("Movimento")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 0.12f;
    [SerializeField] float deceleration = 0.18f;
    [SerializeField] float skidDeceleration = 0.4f;

    // -------------------------------------------------------------------------
    // ICE / SLIPPERY GROUND
    // -------------------------------------------------------------------------
    [Header("Ice Settings")]
    [SerializeField] private string iceTag = "Ice";
    [SerializeField] private float iceAcceleration = 0.03f;
    [SerializeField] private float iceDeceleration = 0.015f;
    [SerializeField] private float iceSkidDeceleration = 0.05f;

    private bool isOnIce = false;

    // -------------------------------------------------------------------------
    // PULO
    // -------------------------------------------------------------------------
    [Header("Pulo")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float wallJumpForce = 12f;

    [SerializeField] float coyoteTimeMax = 0.12f;
    float coyoteTimer;

    [SerializeField] float jumpBufferTime = 0.1f;
    float jumpBufferTimer;

    [SerializeField] float jumpBlockTime = 0.08f;
    float jumpBlockTimer;

    // -------------------------------------------------------------------------
    // DETECÇÃO DE CHÃO
    // -------------------------------------------------------------------------
    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    bool isGrounded;

    // -------------------------------------------------------------------------
    // DROP THROUGH PLATFORMS
    // -------------------------------------------------------------------------
    [Header("Drop Through Platforms")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float dropThroughTime = 0.25f;
    [SerializeField] private float dropThroughVelocity = 3f;

    private bool droppingThrough = false;

    // -------------------------------------------------------------------------
    // PAREDE / WALL JUMP
    // -------------------------------------------------------------------------
    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;
    [SerializeField] float wallSlideSpeed = 2f;

    [SerializeField] float wallJumpBlockTime = 0.2f;
    float wallJumpBlockTimer;

    [SerializeField] float moveBlockTime = 0.15f;
    float moveBlockTimer;

    bool onWall;
    int wallDir;

    // -------------------------------------------------------------------------
    // RAMPAS / SLOPES
    // -------------------------------------------------------------------------
    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private bool canSlideOnSlopes = false; // SWITCH: Altere no Inspector ou via Scriptable

    private Vector2 slopeNormalPerp;
    private float slopeDownAngle;
    private bool isOnSlope;

    // -------------------------------------------------------------------------
    // ESTADO ESPECIAL E INTERNO
    // -------------------------------------------------------------------------
    [Header("Outros")]
    [SerializeField] float waitStartTime = 0.5f;
    float waitStartTimer;

    [HideInInspector] public bool moving = false;

    float xInput;
    float yInput;
    int dir = 1;

    // Variáveis para modificadores dinâmicos
    private float speedMultiplier = 1f;
    private float desacelerationMultipliyer = 1f;
    private float bonusSpeed = 0f;

    public float CurrentMaxSpeed => (moveSpeed + bonusSpeed) * speedMultiplier;
    public float CurrentDessacelerationSpeed => (deceleration) * desacelerationMultipliyer;

    #endregion

    #region UPGRADES DYNAMIC STATE

    [Header("Upgrades Integration State")]
    private int maxJumps = 1;
    private int currentJumpsRemaining = 1;
    private bool wallStickEnabled = false;
    private bool wallClimbEnabled = false;
    private bool disableWallJump = false;

    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        waitStartTimer = waitStartTime;
    }

    void Update()
    {
        CheckGround();
        CheckWall();

        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));
        yInput = Mathf.Round(Input.GetAxisRaw("Vertical"));

        // Tick de timers
        waitStartTimer -= Time.deltaTime;
        moveBlockTimer -= Time.deltaTime;
        jumpBlockTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        wallJumpBlockTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;

        if (waitStartTimer <= 0f) moving = true;

        // Gravidade e Modificadores de Parede (Stick / Climb / Slide)
        if (isGrounded)
        {
            coyoteTimer = coyoteTimeMax;
            currentJumpsRemaining = maxJumps; // RESET DAS CARGAS DE PULO AO TOCAR O CHÃO

            if (!Input.GetButtonDown("Jump"))
            {
                rb.gravityScale = 3f;
            }
        }
        else
        {
            // Fix: Only enter wall interact mode if falling (rb.velocity.y <= 0) OR if holding UP to Wall Climb
            bool isMovingUpward = rb.velocity.y > 0.05f;
            bool wantsToClimb = wallClimbEnabled && yInput > 0;

            bool canWallInteract = xInput != 0 && onWall && !Input.GetButtonDown("Jump")
                                && wallJumpBlockTimer <= 0 && (!isMovingUpward || wantsToClimb);

            if (canWallInteract)
            {
                // 1. WALL CLIMB UPGRADE (Hold UP)
                if (wallClimbEnabled && yInput > 0)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = new Vector2(rb.velocity.x, moveSpeed * 0.75f);
                }
                // 2. WALL STICK UPGRADE (Pressing into wall while falling)
                else if (wallStickEnabled && Mathf.Sign(xInput) == wallDir)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                }
                // 3. DEFAULT WALL SLIDE (Slide down with gravity)
                else
                {
                    rb.gravityScale = 3f;
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
                }
            }
            else
            {
                // Player is jumping UP against the wall or in open air -> Apply normal heavy gravity
                rb.gravityScale = 15f;
            }

            coyoteTimer -= Time.deltaTime;
        }
        // Flip do sprite
        if (xInput > 0 && moveBlockTimer <= 0)
        {
            dir = 1;
            spriteTransform.transform.localScale = new Vector3(1f, spriteTransform.transform.localScale.y, 1f);
        }
        else if (xInput < 0 && moveBlockTimer <= 0)
        {
            dir = -1;
            spriteTransform.transform.localScale = new Vector3(-1f, spriteTransform.transform.localScale.y, 1f);
        }

        if (yInput < 0 && Input.GetButtonDown("Jump"))
        {
            TryDropThroughPlatform();
        }
        else if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Move();

        // LÓGICA DE PULO ATUALIZADA COM SUPORTE A MULTI-JUMPS E WALL JUMP MODS
        bool hasJumpBuffer = jumpBufferTimer > 0 && jumpBlockTimer < 0;
        bool isWallJumpValid = onWall && xInput != 0 && wallJumpBlockTimer <= 0 && !disableWallJump;
        bool hasAirJumps = currentJumpsRemaining > 0;

        if (hasJumpBuffer && (coyoteTimer > 0 || isWallJumpValid || hasAirJumps))
        {
            jumpBufferTimer = 0f;
            jumpBlockTimer = jumpBlockTime;
            coyoteTimer = 0f;

            Jump(isWallJumpValid && !isGrounded);
        }
    }

    #region Funcoes Diversas

    void Move()
    {
        if (moveBlockTimer > 0) return;

        float targetSpeed = CurrentMaxSpeed;
        
        // Define o vetor de movimento: no plano usa (1,0), na rampa alinha com a superfície
        Vector2 moveDirection = Vector2.right;
        if (isOnSlope && isGrounded)
        {
            moveDirection = -slopeNormalPerp; // Projeta o impulso na inclinação correta
        }

        float targetVelocityX = xInput * targetSpeed;
        float speedDiff = targetVelocityX - rb.velocity.x;

        float currentAccel = isOnIce ? iceAcceleration : acceleration;
        float currentDecel = isOnIce ? iceDeceleration : CurrentDessacelerationSpeed;
        float currentSkid = isOnIce ? iceSkidDeceleration : skidDeceleration;

        float forceFactor;

        if (xInput != 0)
        {
            if (isGrounded && Mathf.Sign(xInput) != Mathf.Sign(rb.velocity.x) && Mathf.Abs(rb.velocity.x) > 0.5f)
            {
                forceFactor = currentSkid;
            }
            else
            {
                forceFactor = currentAccel;
            }
        }
        else
        {
            forceFactor = currentDecel;
        }

        // Aplica a força na direção projetada (preserva impulso e velocidade)
        rb.AddForce(moveDirection * (speedDiff * forceFactor), ForceMode2D.Force);

        // LÓGICA DE TRAVA DE DESLIZAMENTO (SWITCH):
        if (isGrounded && isOnSlope && xInput == 0 && jumpBufferTimer <= 0)
        {
            if (!canSlideOnSlopes)
            {
                // Personagem comum (Cubert): Zera gravidade e trava na rampa
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(rb.velocity.x * 0.85f, 0f);
            }
            else
            {
                // Personagem de Deslizamento: Restaura a gravidade para escorregar pela física
                rb.gravityScale = 3f;
            }
        }
        else if (isGrounded)
        {
            rb.gravityScale = 3f;
        }

        // Limitador de velocidade max
        if (xInput != 0)
        {
            float maxAllowedSpeed = Mathf.Max(targetSpeed, Mathf.Abs(rb.velocity.x));

            rb.velocity = new Vector2(
                Mathf.Clamp(rb.velocity.x, -maxAllowedSpeed, maxAllowedSpeed),
                rb.velocity.y
            );
        }
    }

    void TryDropThroughPlatform()
    {
        if (!isGrounded || droppingThrough)
            return;

        // Check if the ground underneath the player is a platform
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundDistance,
            platformLayer
        );

        if (hit.collider == null)
            return;

        StartCoroutine(DropThroughPlatform(hit.collider));
    }

    System.Collections.IEnumerator DropThroughPlatform(Collider2D platform)
    {
        droppingThrough = true;

        // Prevent the normal jump system from triggering
        jumpBufferTimer = 0f;
        coyoteTimer = -10f;
        groundDisableTimer = dropThroughTime;

        // Temporarily ignore collision with this specific platform
        Physics2D.IgnoreCollision(col, platform, true);

        // Force the player downward
        rb.velocity = new Vector2(rb.velocity.x, -dropThroughVelocity);

        yield return new WaitForSeconds(dropThroughTime);

        // Turn collision back on
        Physics2D.IgnoreCollision(col, platform, false);

        droppingThrough = false;
    }

    void Jump(bool isWallJump = false)
    {
        groundDisableTimer = groundDisableTime;
        coyoteTimer = -10f;
        rb.gravityScale = 10f;

        OnJump?.Invoke();

        // EXECUTA WALL JUMP SE APLICÁVEL
        if (isWallJump)
{
    rb.velocity = Vector2.zero;

    moveBlockTimer = moveBlockTime;
    wallJumpBlockTimer = wallJumpBlockTime;

    int jumpXDir = -wallDir;

    // Replenish ONLY extra jumps.
    currentJumpsRemaining = Mathf.Max(currentJumpsRemaining, maxJumps - 1);

    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    rb.AddForce(Vector2.right * (jumpXDir * wallJumpForce), ForceMode2D.Impulse);

    dir = jumpXDir;
    spriteTransform.transform.localScale = new Vector3(
        dir,
        spriteTransform.transform.localScale.y,
        1f
    );

    return;
}

        // PULO NORMAL OU PULO NO AR (MULTI-JUMP)
        currentJumpsRemaining--; // Consome uma carga de pulo
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void CheckGround()
    {
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            isOnIce = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundDistance,
            groundLayer
        );

        isGrounded = hit;

        if (isGrounded && hit.collider != null)
        {
            isOnIce = hit.collider.CompareTag(iceTag);
        }
        else
        {
            isOnIce = false;
        }
    }

    void CheckWall()
    {
        RaycastHit2D rightHit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right,
            wallDistance,
            groundLayer
        );

        RaycastHit2D leftHit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.left,
            wallDistance,
            groundLayer
        );

        if (rightHit)
        {
            wallDir = 1;
            onWall = true;
        }
        else if (leftHit)
        {
            wallDir = -1;
            onWall = true;
        }
        else
        {
            wallDir = 0;
            onWall = false;
        }
    }

    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao, float tempoBloqueioMovimento)
    {
        moveBlockTimer = tempoBloqueioMovimento;

        // Se o recoil estiver empurrando na direção OPOSTA ao movimento atual,
        // zeramos a velocidade horizontal para que o tiro não "lute" contra a patinação.
        if (Mathf.Sign(forçaEmpurrao.x) != Mathf.Sign(rb.velocity.x) && Mathf.Abs(forçaEmpurrao.x) > 0.1f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(0, rb.velocity.y));
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
    }

    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao)
    {
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
    }

    void CheckSlope()
    {
        if (!isGrounded)
        {
            isOnSlope = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance + 0.2f, groundLayer);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Confirma se está em uma superfície inclinada válida
            isOnSlope = slopeDownAngle <= maxSlopeAngle && slopeDownAngle > 0.05f;
        }
        else
        {
            isOnSlope = false;
        }
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
    }

    #endregion

    #region UPGRADE API (Called on Stage Load)

    public void ApplySpeedUpgrade(float speedAdd, float speedMult, float desacelerationMult)
    {
        bonusSpeed += speedAdd;
        speedMultiplier *= speedMult;
        desacelerationMultipliyer *= desacelerationMult;
    }

    public void AddExtraJumps(int amount)
    {
        maxJumps += amount;
        currentJumpsRemaining = maxJumps;
    }

    public void EnableWallStick() => wallStickEnabled = true;

    public void EnableWallClimb(bool replacesJump)
    {
        wallClimbEnabled = true;
        disableWallJump = replacesJump;
    }

    #endregion

    #region Access in other scripts

    public bool IsGrounded => isGrounded;
    public bool IsWalled => onWall;
    public Vector2 Velocity
    {
        get
        {
            if (rb == null) return Vector2.zero;
            Vector2 velocity = rb.velocity;
            if (float.IsNaN(velocity.x) || float.IsInfinity(velocity.x)) velocity.x = 0f;
            if (float.IsNaN(velocity.y) || float.IsInfinity(velocity.y)) velocity.y = 0f;
            return velocity;
        }
    }
    public bool IsFacingLeft => spriteTransform.transform.localScale.x < 0;
    public event Action OnJump;
    public bool isMoving => xInput != 0;

    #endregion
}