using System;
using UnityEngine;

/// <summary>
/// Movimento 2D baseado em Rigidbody2D.
///
/// Recursos: aceleração linear (chão / ar / gelo), corrida, pulo definido por
/// ALTURA e TEMPO até o ápice (independe da massa), pulo de altura variável,
/// queda com gravidade progressiva, coyote time, jump buffer, wall slide /
/// wall jump (sem congelar o input), rampas (para ou desliza), plataformas
/// atravessáveis, recuo (knockback) e API de upgrades.
///
/// Todos os números de "feel" são campos serializados, então cada prefab de
/// personagem pode ter o seu próprio perfil sem herança.
/// </summary>
[AddComponentMenu("Player/Advanced Rigidbody 2D Movement")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    #region Variáveis

    // -------------------------------------------------------------------------
    // REFERÊNCIAS
    // -------------------------------------------------------------------------
    #region Referências
    [Header("Referências")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D col;
    [SerializeField] private Transform spriteTransform;
    #endregion

    // -------------------------------------------------------------------------
    // MOVIMENTO HORIZONTAL
    // Todos os valores de aceleração estão em unidades/s² (independem da massa).
    // -------------------------------------------------------------------------
    #region Movimento
    [Header("Movimento (aceleração em unidades/s²)")]
    [SerializeField] float moveSpeed = 10f;

    [Tooltip("Acelerar no chão")]
    [SerializeField] float groundAccel = 90f;
    [Tooltip("Frear no chão sem input")]
    [SerializeField] float groundDecel = 70f;
    [Tooltip("Virar de direção no chão (freia mais forte)")]
    [SerializeField] float groundSkid = 160f;

    [Tooltip("Acelerar no ar")]
    [SerializeField] float airAccel = 60f;
    [Tooltip("Frear no ar sem input (menor = mais momentum)")]
    [SerializeField] float airDecel = 30f;
    [Tooltip("Virar de direção no ar")]
    [SerializeField] float airSkid = 90f;

    [Header("Corrida")]
    [Tooltip("Tecla de correr (segurar)")]
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;
    [Tooltip("Multiplicador da velocidade máxima correndo")]
    [SerializeField] float runSpeedMultiplier = 1.4f;
    [Tooltip("true: a corrida é decidida no chão e mantida durante o pulo (estilo Mario/Meat Boy)")]
    [SerializeField] bool keepRunInAir = true;

    bool runHeld;
    #endregion

    // -------------------------------------------------------------------------
    // GELO
    // -------------------------------------------------------------------------
    #region Gelo
    [Header("Gelo (unidades/s²)")]
    [SerializeField] private string iceTag = "Ice";
    [SerializeField] private float iceAccel = 12f;
    [SerializeField] private float iceDecel = 4f;
    [SerializeField] private float iceSkid = 20f;

    private bool isOnIce = false;
    #endregion

    // -------------------------------------------------------------------------
    // GRAVIDADE
    // A gravidade de subida vem do pulo (jumpHeight / timeToApex).
    // Caindo ela vai aumentando até fallGravityMultiplier.
    // -------------------------------------------------------------------------
    #region Gravidade
    [Header("Gravidade")]
    [Tooltip("gravityScale usado enquanto está no chão")]
    [SerializeField] float groundGravity = 3f;
    [Tooltip("Multiplicador FINAL da gravidade caindo (1 = igual à subida)")]
    [SerializeField] float fallGravityMultiplier = 1.8f;
    [Tooltip("Tempo (s) caindo até a gravidade chegar no multiplicador final")]
    [SerializeField] float fallRampTime = 0.5f;
    [Tooltip("Limite de velocidade de queda (unidades/s)")]
    [SerializeField] float maxFallSpeed = 30f;

    float fallTimer;
    #endregion

    // -------------------------------------------------------------------------
    // PULO
    // -------------------------------------------------------------------------
    #region Pulo
    [Header("Pulo (independe da massa)")]
    [Tooltip("Altura máxima do pulo, em unidades do mundo")]
    [SerializeField] float jumpHeight = 3f;
    [Tooltip("Tempo (s) do chão até o ápice. Menor = pulo mais seco/rápido.")]
    [SerializeField] float timeToApex = 0.38f;
    [Tooltip("Ao soltar o botão subindo, multiplica a velocidade vertical por este valor (menor = pulo mais curto)")]
    [SerializeField, Range(0f, 1f)] float jumpCutMultiplier = 0.5f;

    [SerializeField] float coyoteTimeMax = 0.12f;
    float coyoteTimer;

    [SerializeField] float jumpBufferTime = 0.1f;
    float jumpBufferTimer;

    [SerializeField] float jumpBlockTime = 0.08f;
    float jumpBlockTimer;

    bool jumpCutRequested;   // botão foi solto, corte pendente
    bool isJumpRising;       // está subindo por causa de um pulo (não de recuo)

    // Física derivada de altura + tempo (independe da massa do Rigidbody2D)
    float JumpTime => Mathf.Max(0.01f, timeToApex);
    float JumpGravity => (2f * jumpHeight) / (JumpTime * JumpTime);   // unidades/s²
    float JumpVelocity => (2f * jumpHeight) / JumpTime;               // unidades/s
    float GravityToScale(float g) => g / Mathf.Max(0.0001f, Mathf.Abs(Physics2D.gravity.y));
    #endregion

    // -------------------------------------------------------------------------
    // DETECÇÃO DE CHÃO
    // -------------------------------------------------------------------------
    #region Chão
    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    bool isGrounded;
    #endregion

    // -------------------------------------------------------------------------
    // ATRAVESSAR PLATAFORMAS
    // -------------------------------------------------------------------------
    #region Drop Through
    [Header("Drop Through Platforms")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float dropThroughTime = 0.25f;
    [SerializeField] private float dropThroughVelocity = 3f;

    private bool droppingThrough = false;
    #endregion

    // -------------------------------------------------------------------------
    // PAREDE: SLIDE E WALL JUMP
    // -------------------------------------------------------------------------
    #region Parede
    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;

    [Header("Wall Slide")]
    [Tooltip("Velocidade máxima de descida deslizando na parede (segurando em direção a ela)")]
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] float wallSlideGravity = 3f;

    [Header("Wall Jump")]
    [Tooltip("Velocidade horizontal do chute (unidades/s), para longe da parede")]
    [SerializeField] float wallJumpHorizontalSpeed = 11f;
    [Tooltip("Multiplica a velocidade vertical do pulo normal")]
    [SerializeField] float wallJumpVerticalMultiplier = 1f;
    [Tooltip("Tempo sem interagir com a parede depois do wall jump (evita grudar de novo)")]
    [SerializeField] float wallJumpBlockTime = 0.2f;
    [Tooltip("Tempo com controle aéreo reduzido (NÃO bloqueia o input)")]
    [SerializeField] float wallJumpControlTime = 0.2f;
    [Tooltip("Fração do controle aéreo durante esse tempo (0 = nenhum, 1 = total)")]
    [SerializeField, Range(0f, 1f)] float wallJumpAirControl = 0.35f;
    [Header("Facing pela mira")]
    [Tooltip("Tempo (s) após um wall jump em que a mira NÃO vira o personagem")]
    [SerializeField] float wallJumpAimFacingLockTime = 0.35f;

    float wallJumpAimLockTimer;
    float aimFacingTimer;
    int aimFacingDir;

    [Header("Wall Jump correndo (bônus)")]
    [Tooltip("Multiplica a força HORIZONTAL do wall jump quando está correndo")]
    [SerializeField] float wallJumpRunHorizontalMultiplier = 1.25f;
    [Tooltip("Multiplica a força VERTICAL do wall jump quando está correndo (a altura cresce com o quadrado disso)")]
    [SerializeField] float wallJumpRunVerticalMultiplier = 1.5f;

    float wallJumpBlockTimer;
    float wallJumpControlTimer;

    // Bloqueio total de movimento: usado só pelo recuo (knockback)
    float moveBlockTimer;

    bool onWall;
    int wallDir;
    bool isWallSliding;
    #endregion

    // -------------------------------------------------------------------------
    // RAMPAS
    // -------------------------------------------------------------------------
    #region Rampas
    [Header("Slope Settings")]
    [Tooltip("Acima deste ângulo a superfície é tratada como PAREDE")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [Tooltip("false (padrão): o personagem PARA na rampa. true: escorrega pela física.")]
    [SerializeField] private bool canSlideOnSlopes = false;

    private Vector2 slopeNormalPerp;
    private float slopeDownAngle;
    private bool isOnSlope;
    private bool wasOnSlope;
    #endregion

    // -------------------------------------------------------------------------
    // ESTADO INTERNO
    // -------------------------------------------------------------------------
    #region Estado interno
    [Header("Outros")]
    [SerializeField] float waitStartTime = 0.5f;
    float waitStartTimer;

    [HideInInspector] public bool moving = false;

    float xInput;
    float yInput;
    int dir = 1;

    // Modificadores dinâmicos (upgrades)
    private float speedMultiplier = 1f;
    private float desacelerationMultipliyer = 1f;
    private float bonusSpeed = 0f;

    public float CurrentMaxSpeed => (moveSpeed + bonusSpeed) * speedMultiplier * (runHeld ? runSpeedMultiplier : 1f);
    public float CurrentDessacelerationSpeed => groundDecel * desacelerationMultipliyer;
    #endregion

    // -------------------------------------------------------------------------
    // ESTADO DOS UPGRADES
    // -------------------------------------------------------------------------
    #region Estado dos upgrades
    private int maxJumps = 1;               // inclui o pulo do chão: extras = maxJumps - 1
    private int currentJumpsRemaining = 1;
    private bool wallStickEnabled = false;
    private bool wallClimbEnabled = false;
    private bool disableWallJump = false;
    #endregion

    #endregion // Variáveis

    // =========================================================================
    // CALLBACKS DA UNITY
    // =========================================================================
    #region Unity Callbacks

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        waitStartTimer = waitStartTime;
    }

    void Update()
    {
        // Sensores
        CheckGround();
        CheckSlope();   // depende do CheckGround
        CheckWall();

        // Input e tempo
        ReadInput();
        TickTimers();

        if (waitStartTimer <= 0f) moving = true;

        // Estado visual / gravidade / pulo
        UpdateGravityAndWallState();
        UpdateFacing();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Move();
        TryJump();
        HandleJumpCut();
        LimitFallSpeed();
    }

    #endregion

    // =========================================================================
    // INPUT E TIMERS
    // =========================================================================
    #region Input e Timers

    void ReadInput()
    {
        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));
        yInput = Mathf.Round(Input.GetAxisRaw("Vertical"));

        // Corrida: no chão acompanha o botão; no ar mantém o estado do último frame no chão
        if (isGrounded || !keepRunInAir)
            runHeld = Input.GetKey(runKey);
    }

    void TickTimers()
    {
        float dt = Time.deltaTime;
        waitStartTimer -= dt;
        moveBlockTimer -= dt;
        jumpBlockTimer -= dt;
        jumpBufferTimer -= dt;
        wallJumpBlockTimer -= dt;
        wallJumpControlTimer -= dt;
        groundDisableTimer -= dt;
        aimFacingTimer -= dt;
        wallJumpAimLockTimer -= dt;
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonUp("Jump"))
            jumpCutRequested = true;

        if (yInput < 0 && Input.GetButtonDown("Jump"))
        {
            TryDropThroughPlatform();
        }
        else if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    void UpdateFacing()
    {
        if (moveBlockTimer > 0) return;

        int desired = 0;

        // Mira manda enquanto o gatilho está "quente", exceto logo após wall jump
        if (aimFacingTimer > 0f && wallJumpAimLockTimer <= 0f)
            desired = aimFacingDir;
        else if (xInput > 0) desired = 1;
        else if (xInput < 0) desired = -1;

        if (desired != 0) ApplyFacing(desired);
    }

    /// <summary>Chamado pelo script de arma: vira o personagem pro lado da mira por holdTime segundos.</summary>
    public void SetAimFacing(int side, float holdTime)
    {
        if (side == 0) return;
        aimFacingDir = side > 0 ? 1 : -1;
        aimFacingTimer = holdTime;
    }

    void ApplyFacing(int side)
    {
        dir = side;
        spriteTransform.localScale = new Vector3(side, spriteTransform.localScale.y, 1f);
    }

    #endregion

    // =========================================================================
    // GRAVIDADE E ESTADOS DE PAREDE
    // =========================================================================
    #region Gravidade e Parede

    void UpdateGravityAndWallState()
    {
        isWallSliding = false;

        // ---- NO CHÃO ----
        if (isGrounded)
        {
            coyoteTimer = coyoteTimeMax;
            currentJumpsRemaining = maxJumps; // recarrega os pulos ao tocar o chão
            fallTimer = 0f;

            if (!Input.GetButtonDown("Jump"))
                rb.gravityScale = groundGravity;

            return;
        }

        // ---- NO AR ----
        bool isMovingUpward = rb.linearVelocity.y > 0.05f;
        bool hasHorizontalInput = xInput != 0;
        bool pressingIntoWall = hasHorizontalInput && Mathf.Sign(xInput) == wallDir;

        // Parede "livre": encostado, fora do bloqueio pós wall jump e sem pular agora
        bool wallFree = onWall && wallJumpBlockTimer <= 0f && !Input.GetButtonDown("Jump");

        bool wantsToClimb = wallFree && wallClimbEnabled && yInput > 0 && hasHorizontalInput;
        bool wantsToStick = wallFree && wallStickEnabled && pressingIntoWall && !isMovingUpward;
        bool wantsToSlide = wallFree && pressingIntoWall && !isMovingUpward;

        if (wantsToClimb)
        {
            // 1. Wall climb (segurando CIMA)
            fallTimer = 0f;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveSpeed * 0.75f);
        }
        else if (wantsToStick)
        {
            // 2. Wall stick (grudado, sem cair)
            fallTimer = 0f;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        else if (wantsToSlide)
        {
            // 3. Wall slide: segurando em direção à parede, desce devagar.
            // O limite de velocidade é aplicado no FixedUpdate (LimitFallSpeed).
            fallTimer = 0f;
            isWallSliding = true;
            rb.gravityScale = wallSlideGravity;
        }
        else
        {
            // 4. Ar livre
            if (rb.linearVelocity.y > 0f)
            {
                // Subindo: gravidade "leve" derivada do pulo
                fallTimer = 0f;
                rb.gravityScale = GravityToScale(JumpGravity);
            }
            else
            {
                // Caindo: a gravidade vai aumentando até fallGravityMultiplier
                fallTimer += Time.deltaTime;
                float t = Mathf.Clamp01(fallTimer / Mathf.Max(0.01f, fallRampTime));
                float multiplier = Mathf.Lerp(1f, fallGravityMultiplier, t);
                rb.gravityScale = GravityToScale(JumpGravity) * multiplier;
            }
        }

        coyoteTimer -= Time.deltaTime;

        // Saiu da borda sem pular: gasta o "pulo do chão" quando o coyote acaba.
        // (Sem isso, maxJumps = 1 permitia um pulo extra no ar depois de andar para fora da borda.)
        if (coyoteTimer <= 0f && currentJumpsRemaining >= maxJumps)
            currentJumpsRemaining = maxJumps - 1;
    }

    void LimitFallSpeed()
    {
        float limit = isWallSliding ? wallSlideSpeed : maxFallSpeed;

        if (rb.linearVelocity.y < -limit)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -limit);
    }

    #endregion

    // =========================================================================
    // MOVIMENTO HORIZONTAL
    // =========================================================================
    #region Movimento Horizontal

    void Move()
    {
        if (moveBlockTimer > 0) return;

        // Saiu do topo de uma rampa: remove o vy positivo para não dar "pulinho"
        if (wasOnSlope && !isOnSlope && isGrounded && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        wasOnSlope = isOnSlope;

        float targetSpeed = CurrentMaxSpeed;
        bool onSlopeGround = isGrounded && isOnSlope;

        // "Para frente" acompanha a superfície quando está em rampa
        Vector2 moveDirection = onSlopeGround ? -slopeNormalPerp : Vector2.right;

        float currentAlong = Vector2.Dot(rb.linearVelocity, moveDirection);
        float targetAlong = xInput * targetSpeed;

        bool reversing = xInput != 0
            && Mathf.Sign(xInput) != Mathf.Sign(currentAlong)
            && Mathf.Abs(currentAlong) > 0.5f;

        float rate = GetMoveRate(reversing);

        // Logo depois de um wall jump o controle aéreo é REDUZIDO (não bloqueado),
        // para o chute não ser cancelado no mesmo instante.
        if (!isGrounded && wallJumpControlTimer > 0f)
            rate *= wallJumpAirControl;
        

        bool idleOnSlope = onSlopeGround && xInput == 0;

        // PARA NA RAMPA (padrão): freia ao longo da superfície, sem gravidade
        // e sem componente perpendicular (descarta a gravidade acumulada).
        if (idleOnSlope && !canSlideOnSlopes && jumpBufferTimer <= 0f)
        {
            float stopped = Mathf.MoveTowards(currentAlong, 0f, rate * Time.fixedDeltaTime);
            rb.gravityScale = 0f;
            rb.linearVelocity = moveDirection * stopped;
            return;
        }

        // PERSONAGEM DE DESLIZE: sem input na rampa não freia, a física escorrega.
        if (!(idleOnSlope && canSlideOnSlopes))
        {
            float newAlong = Mathf.MoveTowards(currentAlong, targetAlong, rate * Time.fixedDeltaTime);
            rb.linearVelocity += moveDirection * (newAlong - currentAlong);
        }

        if (isGrounded)
            rb.gravityScale = groundGravity;
    }

    /// <summary>Escolhe a taxa (unidades/s²) conforme chão/ar/gelo e input.</summary>
    float GetMoveRate(bool reversing)
    {
        if (isGrounded)
        {
            if (isOnIce)
                return xInput == 0 ? iceDecel : (reversing ? iceSkid : iceAccel);

            return xInput == 0 ? CurrentDessacelerationSpeed : (reversing ? groundSkid : groundAccel);
        }

        return xInput == 0 ? airDecel : (reversing ? airSkid : airAccel);
    }

    #endregion

    // =========================================================================
    // PULO
    // =========================================================================
    #region Pulo

    void TryJump()
    {
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

    void Jump(bool isWallJump = false)
    {
        groundDisableTimer = groundDisableTime;
        coyoteTimer = -10f;
        isJumpRising = true;
        fallTimer = 0f;
        rb.gravityScale = GravityToScale(JumpGravity);

        OnJump?.Invoke();

        // ---- WALL JUMP ----
        if (isWallJump)
        {
            wallJumpBlockTimer = wallJumpBlockTime;
            wallJumpControlTimer = wallJumpControlTime;
            wallJumpAimLockTimer = wallJumpAimFacingLockTime;
            aimFacingTimer = 0f;

            int jumpXDir = -wallDir;

            // Repõe APENAS os pulos extras
            currentJumpsRemaining = Mathf.Max(currentJumpsRemaining, maxJumps - 1);

            // Corrida é reavaliada no wall jump: segurar o botão de correr dá o bônus
            // (e mantém a velocidade de corrida durante o voo, se keepRunInAir estiver ligado)
            runHeld = Input.GetKey(runKey);

            float horizontalMult = runHeld ? wallJumpRunHorizontalMultiplier : 1f;
            float verticalMult = runHeld ? wallJumpRunVerticalMultiplier : 1f;

            // Velocidade direta (sem congelar o input): o controle aéreo reduzido cuida do resto
            rb.linearVelocity = new Vector2(jumpXDir * wallJumpHorizontalSpeed * horizontalMult,
                                            JumpVelocity * wallJumpVerticalMultiplier * verticalMult);

            dir = jumpXDir;
            spriteTransform.localScale = new Vector3(dir, spriteTransform.localScale.y, 1f);
            return;
        }

        // ---- PULO NORMAL / PULO NO AR ----
        currentJumpsRemaining--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpVelocity);
    }

    /// <summary>
    /// Pulo de altura variável: soltar o botão enquanto sobe corta a velocidade vertical.
    /// Só vale para velocidade de pulo (recuo para cima não é cortado).
    /// </summary>
    void HandleJumpCut()
    {
        // Terminou a subida do pulo
        if (isJumpRising && rb.linearVelocity.y <= 0f && jumpBlockTimer < 0f)
            isJumpRising = false;

        if (!jumpCutRequested) return;

        if (isJumpRising && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            jumpCutRequested = false;
        }
        else if (!isJumpRising && jumpBufferTimer <= 0f)
        {
            // Soltou o botão sem nenhum pulo em andamento ou pendente
            jumpCutRequested = false;
        }
    }

    #endregion

    // =========================================================================
    // DETECÇÕES (CHÃO, RAMPA, PAREDE)
    // =========================================================================
    #region Detecções

    void CheckGround()
    {
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            isOnIce = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance, groundLayer);

        isGrounded = hit;
        isOnIce = isGrounded && hit.collider != null && hit.collider.CompareTag(iceTag);
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

            // Rampa válida: inclinada, mas não mais que o máximo
            isOnSlope = slopeDownAngle <= maxSlopeAngle && slopeDownAngle > 0.05f;
        }
        else
        {
            isOnSlope = false;
        }
    }

    /// <summary>Superfície mais inclinada que maxSlopeAngle conta como parede.</summary>
    bool IsWallHit(RaycastHit2D hit) =>
        hit && Vector2.Angle(hit.normal, Vector2.up) > maxSlopeAngle;

    void CheckWall()
    {
        RaycastHit2D rightHit = Physics2D.Raycast(wallCheck.position, Vector2.right, wallDistance, groundLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(wallCheck.position, Vector2.left, wallDistance, groundLayer);

        if (IsWallHit(rightHit))
        {
            wallDir = 1;
            onWall = true;
        }
        else if (IsWallHit(leftHit))
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

    #endregion

    // =========================================================================
    // PLATAFORMAS ATRAVESSÁVEIS
    // =========================================================================
    #region Drop Through Platforms

    void TryDropThroughPlatform()
    {
        if (!isGrounded || droppingThrough)
            return;

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance, platformLayer);

        if (hit.collider == null)
            return;

        StartCoroutine(DropThroughPlatform(hit.collider));
    }

    System.Collections.IEnumerator DropThroughPlatform(Collider2D platform)
    {
        droppingThrough = true;

        // Impede o sistema de pulo normal de disparar
        jumpBufferTimer = 0f;
        coyoteTimer = -10f;
        groundDisableTimer = dropThroughTime;

        // Ignora a colisão só com esta plataforma
        Physics2D.IgnoreCollision(col, platform, true);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -dropThroughVelocity);

        yield return new WaitForSeconds(dropThroughTime);

        Physics2D.IgnoreCollision(col, platform, false);
        droppingThrough = false;
    }

    #endregion

    // =========================================================================
    // RECUO (KNOCKBACK)
    // =========================================================================
    #region Recuo

    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao, float tempoBloqueioMovimento)
    {
        moveBlockTimer = tempoBloqueioMovimento;

        // Recuo na direção OPOSTA ao movimento: zera o vx para o tiro não "lutar" contra a velocidade
        if (Mathf.Sign(forçaEmpurrao.x) != Mathf.Sign(rb.linearVelocity.x) && Mathf.Abs(forçaEmpurrao.x) > 0.1f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(0, rb.linearVelocity.y));
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
    }

    public void AplicarEmpurraoRecuo(Vector2 forçaEmpurrao)
    {
        rb.AddForce(forçaEmpurrao, ForceMode2D.Impulse);
    }

    #endregion

    // =========================================================================
    // API DE UPGRADES (chamada ao carregar a fase)
    // =========================================================================
    #region Upgrade API

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

    // =========================================================================
    // ACESSO POR OUTROS SCRIPTS
    // =========================================================================
    #region Acesso externo

    public bool IsGrounded => isGrounded;
    public bool IsWalled => onWall;
    public bool IsWallSliding => isWallSliding;

    public Vector2 Velocity
    {
        get
        {
            if (rb == null) return Vector2.zero;
            Vector2 velocity = rb.linearVelocity;
            if (float.IsNaN(velocity.x) || float.IsInfinity(velocity.x)) velocity.x = 0f;
            if (float.IsNaN(velocity.y) || float.IsInfinity(velocity.y)) velocity.y = 0f;
            return velocity;
        }
    }

    public bool IsFacingLeft => spriteTransform.localScale.x < 0;
    public event Action OnJump;
    public bool isMoving => xInput != 0;
    public bool IsRunning => runHeld && xInput != 0;

    #endregion

    // =========================================================================
    // DEBUG
    // =========================================================================
    #region Gizmos

    void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);

        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left * wallDistance);
        }

        // Direção "para frente" quando está em rampa (ciano)
        if (Application.isPlaying && isOnSlope && groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(groundCheck.position, -slopeNormalPerp);
        }
        Debug.Log($"Movement Speed {rb.linearVelocity}");
    }

    
    #endregion
}