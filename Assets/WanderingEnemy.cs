using UnityEngine;
using UnityEngine.PlayerLoop;

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
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float groundDisableTime = 0.1f;
    float groundDisableTimer;

    bool isGrounded;

    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;

    bool onWall;

    private bool onChase = false;
    float xInput;
    int dir = 1; // 1 = direita, -1 = esquerda

    [SerializeField] private Transform target;
    protected override void Start()
    {
        base.Start();
        speed *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        damage *= Random.Range(1f - variationPercentage, 1f + variationPercentage);
        
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

         if (onChase) xInput = Mathf.Sign(target.position.x - transform.position.x);
         else xInput = dir;

        waitStartTimer -= Time.deltaTime;

        if (onWall && !onChase) dir *= -1;

        if (currHp <= 0 && !isImortal) Destroy(gameObject);
    }

    protected void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (waitStartTimer > 0) return;

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