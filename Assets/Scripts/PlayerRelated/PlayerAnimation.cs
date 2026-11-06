using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Liga o PlayerMovement ao Animator, usando o contrato PlayerAnimParams.
///
/// - Preenche os parâmetros de locomoção sozinho (os scripts filhos não precisam).
/// - A velocidade de reprodução de Walk/Run acompanha a velocidade REAL do personagem.
/// - Só mexe em parâmetros que EXISTEM no Controller: um personagem sem animação de
///   wall slide, por exemplo, não gera spam de erro.
/// - Oferece SetFloat/SetBool/SetTrigger seguros para os scripts filhos usarem.
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] PlayerMovement movement;
    [SerializeField] Animator animator;

    [Header("Velocidade da animação")]
    [Tooltip("Velocidade (unidades/s) em que o ciclo de ANDAR casa com o chão, sem patinar")]
    [SerializeField] float walkReferenceSpeed = 10f;
    [Tooltip("Velocidade (unidades/s) em que o ciclo de CORRER casa com o chão")]
    [SerializeField] float runReferenceSpeed = 14f;
    [Tooltip("Mínimo e máximo da velocidade de reprodução (1 = normal)")]
    [SerializeField] Vector2 playbackRange = new Vector2(0.6f, 2f);
    [Tooltip("Suavização (maior = responde mais rápido)")]
    [SerializeField] float smooth = 12f;

    readonly HashSet<int> available = new HashSet<int>();
    float playback = 1f;

    public Animator Animator => animator;

    void Reset()
    {
        FindReferences();
    }

    void Awake()
    {
        FindReferences();
        CacheParameters();
    }

    // O Animator fica em um objeto FILHO (o que controla os sprites).
    // Campos arrastados no Inspector têm prioridade; isto só preenche o que estiver vazio.
    void FindReferences()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
    }

    void OnEnable()
    {
        if (movement != null) movement.OnJump += HandleJump;
    }

    void OnDisable()
    {
        if (movement != null) movement.OnJump -= HandleJump;
    }

    void Update()
    {
        float speedX = Mathf.Abs(movement.Velocity.x);

        SetFloat(PlayerAnimParams.Speed, speedX);
        SetFloat(PlayerAnimParams.VerticalSpeed, movement.Velocity.y);
        SetBool(PlayerAnimParams.Grounded, movement.IsGrounded);
        SetBool(PlayerAnimParams.Running, movement.IsRunning);
        SetBool(PlayerAnimParams.WallSliding, movement.IsWallSliding);

        // Velocidade real / velocidade de referência = ritmo da animação
        float reference = movement.IsRunning ? runReferenceSpeed : walkReferenceSpeed;
        float target = Mathf.Clamp(speedX / Mathf.Max(0.01f, reference), playbackRange.x, playbackRange.y);
        playback = Mathf.Lerp(playback, target, smooth * Time.deltaTime);
        SetFloat(PlayerAnimParams.PlaybackSpeed, playback);
    }

    void HandleJump() => SetTrigger(PlayerAnimParams.Jump);

    // -------------------------------------------------------------------------
    // API SEGURA (usada também pelos scripts filhos dos personagens)
    // -------------------------------------------------------------------------
    public bool Has(int hash) => available.Contains(hash);

    public void SetFloat(int hash, float value)  { if (Has(hash)) animator.SetFloat(hash, value); }
    public void SetBool(int hash, bool value)    { if (Has(hash)) animator.SetBool(hash, value); }
    public void SetTrigger(int hash)             { if (Has(hash)) animator.SetTrigger(hash); }
    public void ResetTrigger(int hash)           { if (Has(hash)) animator.ResetTrigger(hash); }

    // -------------------------------------------------------------------------
    // INTERNO
    // -------------------------------------------------------------------------
    void CacheParameters()
    {
        available.Clear();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] PlayerAnimation: Animator sem Controller.", this);
            return;
        }

        foreach (AnimatorControllerParameter p in animator.parameters)
            available.Add(p.nameHash);

#if UNITY_EDITOR
        // Avisa UMA vez quais parâmetros do contrato faltam neste personagem
        List<string> missing = null;
        foreach (string paramName in PlayerAnimParams.Required)
        {
            if (!available.Contains(Animator.StringToHash(paramName)))
            {
                missing ??= new List<string>();
                missing.Add(paramName);
            }
        }

        if (missing != null)
            Debug.LogWarning($"[{name}] Controller sem parâmetros do contrato: {string.Join(", ", missing)}", this);
#endif
    }
}