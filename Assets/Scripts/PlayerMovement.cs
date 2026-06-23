using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variaveis
    // -------------------------------------------------------------------------
    // REFERÊNCIAS DE COMPONENTES
    // -------------------------------------------------------------------------
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CapsuleCollider2D col;
    [HideInInspector] public SpriteRenderer spr;
    [SerializeField] private WeaponController weaponController;

    // -------------------------------------------------------------------------
    // MOVIMENTO HORIZONTAL (Com Aceleração e Derrapagem)
    // -------------------------------------------------------------------------
    [Header("Movimento")]
    [SerializeField] float moveSpeed = 8f;
    [Tooltip("Quanto o jogador acelera até a velocidade máxima")]
    [SerializeField] float acceleration = 0.12f;
    [Tooltip("Quanto o jogador desacelera ao soltar o input (slide passivo)")]
    [SerializeField] float deceleration = 0.18f;
    [Tooltip("Força da derrapagem ao tentar mudar de direção correndo (valores maiores travam o drift mais rápido)")]
    [SerializeField] float skidDeceleration = 0.4f;

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

    // -------------------------------------------------------------------------
    // ÁGUA / ESTADO ESPECIAL
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

    #endregion

    
    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        spr = GetComponentInChildren<SpriteRenderer>();

        waitStartTimer = waitStartTime;
    }

    
    void Update()
    {
        CheckGround();
        CheckWall();

        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));

        // Tick de timers
        waitStartTimer     -= Time.deltaTime;
        moveBlockTimer     -= Time.deltaTime;
        jumpBlockTimer     -= Time.deltaTime;
        jumpBufferTimer    -= Time.deltaTime;
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
                rb.gravityScale = 10f;
            }

            coyoteTimer -= Time.deltaTime;
        }

        // --- Flip do sprite ---
        if (!weaponController.alert)
        {
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
        }
        else //Aqui é paara caso ele tivese mirando com o mouse para o jogador olhar na direçã oda mira
        {
            if (weaponController.isMouseAim())
            {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;

            Vector2 mdirection = mouseWorldPosition - transform.position;
            dir = (mdirection.x < 0) ? -1 : 1;
            spr.transform.localScale = new Vector3(dir, spr.transform.localScale.y, 1f);
            }
        }

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
    }

    
    void FixedUpdate()
    {
        if (!moving) return;

        Move();

        // Pulo
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
    }

    #region Funcoes Diversas
    // MOVIMENTO HORIZONTAL MODIFICADO COM DETECÇÃO DE DERRAPAGEM
    
    void Move()
    {
        if (moveBlockTimer > 0) return;

        float targetVelocity = xInput * moveSpeed;
        float speedDiff = targetVelocity - rb.velocity.x;
        float forceFactor;

        if (xInput != 0)
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

    
    void Jump()
    {
        groundDisableTimer = groundDisableTime;
        coyoteTimer        = -10f;

        rb.gravityScale = 10f;

        // --- Wall Jump ---
        if (onWall && !isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            moveBlockTimer     = moveBlockTime;
            wallJumpBlockTimer = wallJumpBlockTime;

            rb.AddForce(Vector2.up    * jumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * (-dir * wallJumpForce), ForceMode2D.Impulse);

            if (!weaponController.alert) dir *= -1;
            spr.transform.localScale = new Vector3(dir, spr.transform.localScale.y, 1f);
            return;
        }

        // --- Pulo Normal ---
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

    
    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao, float tempoBloqueioMovimento)
    {
        moveBlockTimer = tempoBloqueioMovimento;

        // Reseta o eixo vertical se quiser saltar atirando, mas preserva o horizontal para somar o impacto
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(0, rb.velocity.y));

        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
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
    }

    public bool isFlipped()
    {
        return (spr.transform.localScale.x == -1);
    }
    #endregion
}