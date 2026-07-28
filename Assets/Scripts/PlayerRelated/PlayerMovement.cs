using System;
using UnityEngine;

[AddComponentMenu("Player/Advanced Rigidbody 2D Movement")]
[RequireComponent(typeof(Rigidbody2D))]

/// Something to note is that this PlayerMovemnt only works for CUBE ATTACKS RECUBED due to the compatibility with the upgrade system included.
/// If anyone should grab this script please be known to adapt to not cause trouble for you
/// 
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
    [SerializeField] private float iceAcceleration = 0.03f;      // Lower = harder to gain speed
    [SerializeField] private float iceDeceleration = 0.015f;     // Lower = longer slide when letting go of inputs
    [SerializeField] private float iceSkidDeceleration = 0.05f;  // Lower = slides further when changing direction

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
    int wallDir; // -1 = parede na esquerda, 1 = parede na direita, 0 = nenhuma

    // -------------------------------------------------------------------------
    // ESTADO ESPECIAL
    // -------------------------------------------------------------------------
    [Header("Outros")]
    [SerializeField] float waitStartTime = 0.5f;
    float waitStartTimer;

    // -------------------------------------------------------------------------
    // ESTADO INTERNO
    // -------------------------------------------------------------------------
    [HideInInspector] public bool moving = false;

    float xInput;
    int dir = 1; // 1 = direita, -1 = esquerda


    // Variáveis para modificadores dinâmicos
    private float speedMultiplier = 1f;
    private float bonusSpeed = 0f;

    // Propriedade calculada da velocidade final
    public float CurrentMaxSpeed => (moveSpeed + bonusSpeed) * speedMultiplier;

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

        // Tick de timers
        waitStartTimer -= Time.deltaTime;
        moveBlockTimer -= Time.deltaTime;
        jumpBlockTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        wallJumpBlockTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;

        if (waitStartTimer <= 0f) moving = true;

        // Gravidade com base no estado do pulo
        if (isGrounded)
        {
            coyoteTimer = coyoteTimeMax;
            if (!Input.GetButtonDown("Jump"))
            {
                rb.gravityScale = 3f;
            }
        }
        else
        {
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
            else
            {
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

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Move();

        // Pulo (Adicionado verificação do wallJumpBlockTimer)
        bool canJump = jumpBufferTimer > 0
            && (
                coyoteTimer > 0 ||
                (onWall && xInput != 0 && wallJumpBlockTimer <= 0)
               )
            && jumpBlockTimer < 0;

        if (canJump)
        {
            jumpBufferTimer = 0f;
            jumpBlockTimer = jumpBlockTime;
            coyoteTimer = 0f;
            Jump();
        }
    }

    #region Funcoes Diversas

    void Move()
{
    if (moveBlockTimer > 0) return;

    // Usando a velocidade final com os upgrades aplicados
    float targetSpeed = CurrentMaxSpeed;
    float targetVelocity = xInput * targetSpeed;
    float speedDiff = targetVelocity - rb.velocity.x;

    float currentAccel = isOnIce ? iceAcceleration : acceleration;
    float currentDecel = isOnIce ? iceDeceleration : deceleration;
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

    rb.AddForce(Vector2.right * speedDiff * forceFactor, ForceMode2D.Force);

    if (xInput != 0)
    {
        // Agora o limite máximo respeita tanto os Upgrades quanto impulsos de força externos
        float maxAllowedSpeed = Mathf.Max(targetSpeed, Mathf.Abs(rb.velocity.x));

        rb.velocity = new Vector2(
            Mathf.Clamp(rb.velocity.x, -maxAllowedSpeed, maxAllowedSpeed),
            rb.velocity.y
        );
    }
}

    void Jump()
    {
        groundDisableTimer = groundDisableTime;
        coyoteTimer = -10f;

        rb.gravityScale = 10f;

        OnJump?.Invoke();

        // --- Wall Jump Fix ---
        if (onWall && xInput != 0 && !isGrounded)
        {
            Debug.Log($"WallJump! Wall direction was : {wallDir}");

            // Reseta a velocidade X e Y totalmente para garantir impulso limpo
            rb.velocity = Vector2.zero;

            moveBlockTimer = moveBlockTime;
            wallJumpBlockTimer = wallJumpBlockTime;

            // Define direção oposta à parede detectada
            int jumpXDir = -wallDir;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * (jumpXDir * wallJumpForce), ForceMode2D.Impulse);

            dir = jumpXDir;
            spriteTransform.transform.localScale = new Vector3(dir, spriteTransform.transform.localScale.y, 1f);
            return;
        }

        Debug.Log("Jump!");
        // --- Pulo Normal ---
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

        // Check if the surface we hit has the Ice tag
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
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(0, rb.velocity.y));
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
    }

    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao)
    {
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
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