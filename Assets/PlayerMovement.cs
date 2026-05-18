using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // REFERÊNCIAS DE COMPONENTES
    // Públicas para performance (sem overhead de GetComponent em runtime)
    // -------------------------------------------------------------------------
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CapsuleCollider2D col;
    [HideInInspector] public SpriteRenderer spr;

    // -------------------------------------------------------------------------
    // MOVIMENTO HORIZONTAL
    // -------------------------------------------------------------------------
    [Header("Movimento")]
    [SerializeField] float moveSpeed = 8f;
    [Tooltip("Quanto o jogador acelera até a velocidade máxima (0 = instantâneo, 1 = nunca chega)")]
    [SerializeField] float acceleration = 0.12f;
    [Tooltip("Quanto o jogador desacelera ao soltar o input (cria o 'slide')")]
    [SerializeField] float deceleration = 0.18f;

    // -------------------------------------------------------------------------
    // PULO
    // -------------------------------------------------------------------------
    [Header("Pulo")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float wallJumpForce = 12f;

    [Tooltip("Tempo extra para pular após sair da borda (Coyote Time)")]
    [SerializeField] float coyoteTimeMax = 0.12f;
    float coyoteTimer;

    [Tooltip("Janela de tempo para buffering do input de pulo")]
    [SerializeField] float jumpBufferTime = 0.1f;
    float jumpBufferTimer;

    // Bloqueia o pulo por um frame para evitar pulos duplos acidentais
    [SerializeField] float jumpBlockTime = 0.08f;
    float jumpBlockTimer;

    // -------------------------------------------------------------------------
    // DETECÇÃO DE CHÃO
    // -------------------------------------------------------------------------
    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    [Tooltip("Bloqueia a detecção de chão logo após o pulo, evitando redetecção imediata")]
    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] PhysicsMaterial2D fullFriction;

    bool isGrounded;

    // Snap cola o jogador no chão ao descer rampas, evitando que ele "voe"
    bool snap;
    [SerializeField] float snapBlockTime = 0.1f;
    float snapBlockTimer;

    // -------------------------------------------------------------------------
    // PAREDE / WALL JUMP
    // -------------------------------------------------------------------------
    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;
    [SerializeField] float wallSlideSpeed = 2f;

    [Tooltip("Bloqueia novo wall jump por um tempo para o jogador subir sem re-colar na parede")]
    [SerializeField] float wallJumpBlockTime = 0.2f;
    float wallJumpBlockTimer;

    [Tooltip("Bloqueia input horizontal brevemente após wall jump para manter a trajetória")]
    [SerializeField] float moveBlockTime = 0.15f;
    float moveBlockTimer;

    bool onWall;

    // -------------------------------------------------------------------------
    // ÁGUA / ESTADO ESPECIAL
    // -------------------------------------------------------------------------
    [Header("Outros")]
    [SerializeField] bool inWater;

    [Tooltip("Delay inicial antes do jogador poder se mover (ex: cutscene de início)")]
    [SerializeField] float waitStartTime = 0.5f;
    float waitStartTimer;

    // -------------------------------------------------------------------------
    // ESTADO INTERNO
    // -------------------------------------------------------------------------
    [HideInInspector] public bool moving = false;

    float xInput;
    int dir = 1; // 1 = direita, -1 = esquerda

    // =========================================================================
    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponentInChildren<CapsuleCollider2D>();
        spr = GetComponentInChildren<SpriteRenderer>();

        waitStartTimer = waitStartTime;
    }

    // =========================================================================
    void Update()
    {
        // --- Física e detecção ---
        CheckGround();
        CheckWall();

        // Input horizontal: -1, 0 ou 1
        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));

        // --- Tick de timers ---
        waitStartTimer     -= Time.deltaTime;
        snapBlockTimer     -= Time.deltaTime;
        moveBlockTimer     -= Time.deltaTime;
        jumpBlockTimer     -= Time.deltaTime;
        jumpBufferTimer    -= Time.deltaTime;
        wallJumpBlockTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;

        // Libera movimento após o delay inicial
        if (waitStartTimer <= 0f) moving = true;

        // --- Material de fricção ---
        // Fricção total apenas quando parado no chão; fora disso, sem fricção (slide)
        rb.sharedMaterial = (xInput == 0 && isGrounded) ? fullFriction : noFriction;

        // --- Lógica de gravidade e estado ---
        if (isGrounded)
        {
            coyoteTimer = coyoteTimeMax;

            if (!Input.GetButtonDown("Jump") && !inWater)
            {
                rb.gravityScale = 3f;
                snap = true;
            }
        }
        else
        {
            // Wall slide: limita a velocidade de queda enquanto encostado na parede
            bool canWallSlide = xInput != 0 && onWall
                             && !Input.GetButtonDown("Jump")
                             && wallJumpBlockTimer <= 0;

            if (canWallSlide)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    Mathf.Max(rb.velocity.y, -wallSlideSpeed)
                );
            }
            else if (!Input.GetButtonDown("Jump") && !inWater)
            {
                rb.gravityScale = 10f;
            }

            coyoteTimer -= Time.deltaTime;

            // Desativa snap quando não há mais coyote time nem buffer
            if (coyoteTimer <= 0 || jumpBufferTimer <= 0)
                snap = false;
        }

        // --- Flip do sprite ---
        if (xInput > 0 && moveBlockTimer <= 0)
        {
            dir = 1;
            spr.transform.localScale = new Vector3(1f, spr.transform.localScale.y, 1f);
        }
        else if (xInput < 0 && moveBlockTimer <= 0)
        {
            dir = -1;
            spr.transform.localScale = new Vector3(-1f, spr.transform.localScale.y, 1f);
        }

        // --- Buffer de pulo ---
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
    }

    // =========================================================================
    void FixedUpdate()
    {
        if (!moving) return;

        Move();

        // Pulo com Coyote Time + buffer + wall jump
        bool canJump = jumpBufferTimer > 0
                    && (coyoteTimer > 0 || onWall)
                    && jumpBlockTimer < 0;

        if (canJump)
        {
            jumpBufferTimer = 0f;
            jumpBlockTimer  = jumpBlockTime;
            coyoteTimer     = 0f;
            Jump();
        }

        // Snap: mantém o jogador "colado" ao chão em rampas
        if (snap && jumpBufferTimer <= 0 && snapBlockTimer <= 0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Lerp(rb.velocity.y, -rb.gravityScale, 0.5f)
            );
        }
    }

    // =========================================================================
    // MOVIMENTO HORIZONTAL COM LERP (gera o "slide" suave)
    // =========================================================================
    void Move()
    {
        if (moveBlockTimer > 0) return;

        float targetVelocity = xInput * moveSpeed;
        float speedDiff = targetVelocity - rb.velocity.x;
        float forceFactor = (xInput != 0) ? acceleration : deceleration;

        rb.AddForce(Vector2.right * speedDiff * forceFactor, ForceMode2D.Force);

        // Clamp para não ultrapassar o moveSpeed
        rb.velocity = new Vector2(
            Mathf.Clamp(rb.velocity.x, -moveSpeed, moveSpeed),
            rb.velocity.y
        );

        Debug.Log(rb.velocity);
    }

    // =========================================================================
    void Jump()
    {
        groundDisableTimer = groundDisableTime;
        coyoteTimer        = -10f; // invalida coyote time imediatamente
        snap               = false;
        snapBlockTimer     = snapBlockTime;

        // --- Wall Jump ---
        if (onWall && !isGrounded && !inWater)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            moveBlockTimer     = moveBlockTime;
            wallJumpBlockTimer = wallJumpBlockTime;

            rb.AddForce(Vector2.up          * jumpForce,            ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * (-dir * wallJumpForce),     ForceMode2D.Impulse);

            // Inverte direção do sprite
            dir *= -1;
            spr.transform.localScale = new Vector3(dir, spr.transform.localScale.y, 1f);
            return;
        }

        // --- Pulo Normal ---
        if (!inWater) rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // =========================================================================
    void CheckGround()
    {
        // Se o timer de desativação está ativo, força isGrounded = false
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundDistance,
            groundLayer
        );

        isGrounded = hit;
    }

    void CheckWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * dir,
            wallDistance,
            groundLayer
        );

        onWall = hit;
    }

    // =========================================================================
    void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);

        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left  * wallDistance);
        }

    }
}
