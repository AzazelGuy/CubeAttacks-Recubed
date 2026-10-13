using UnityEngine;

public class HubPlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] AudioClip Music;
    private Camera mainCamera;
    private Vector3 moveInput;

    void Start()
    {
        mainCamera = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(Music);
        }
    }

    void Update()
    {
        // 1. Captura os eixos de entrada
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 2. Calcula a direção baseada no espaço da CÂMERA (evita inversão ortográfica)
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // Anula a inclinação vertical da câmera para o movimento ficar só no chão (X, Z)
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveInput = (camRight * x + camForward * z).normalized;

        // 3. Flip do Sprite de Papelão
        if (x > 0.1f)
        {
            spriteRenderer.flipX = false; // Olhando pra direita
        }
        else if (x < -0.1f)
        {
            spriteRenderer.flipX = true;  // Olhando pra esquerda
        }
    }

    void FixedUpdate()
    {
        // 4. Aplica o movimento no Rigidbody preservando a gravidade/colisão Y
        Vector3 targetVelocity = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}