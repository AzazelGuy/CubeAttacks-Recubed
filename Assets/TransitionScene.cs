using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TransitionState
{
    Stoped,
    Entering,   // Executando animação de entrada (ex: escurecendo)
    Loading,    // Processando a cena/posição em segundo plano
    Exiting     // Executando animação de saída (ex: clareando)
}

public enum TypeOfTransition
{
    ChangeScene,
    TeleportInScene,
    ChangeSceneAndTP
}

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance;

    [Header("UI")]
    [SerializeField] private Animator animator;

    [Header("Configurações de Segurança")]
    [Tooltip("Tempo máximo de espera da animação caso o Animator falhe ou não tenha Animation Event.")]
    [SerializeField] private float animationTimeLimit = 1.5f;

    public TransitionState CurrState { get; private set; } = TransitionState.Stoped;
    private TypeOfTransition type = TypeOfTransition.ChangeScene;

    // Controla se a animação finalizou
    private bool isEnterAnimationFinished = false;
    private bool isExitAnimationFinished = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        // Se a transição já estiver acontecendo (Entering, Loading ou Exiting), ignora chamadas duplicadas
        if (CurrState != TransitionState.Stoped)
        {
            Debug.LogWarning("[TransitionScene] Transição já em andamento. Chamada duplicada ignorada.");
            return;
        }

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // Reseta as flags para a nova transição
        isEnterAnimationFinished = false;
        isExitAnimationFinished = false;

        CurrState = TransitionState.Entering;

        // --- 1. SEGURANÇA: Verificar se o Animator existe e está ativo ---
        bool animatorExists = animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null;

        if (animatorExists)
        {
            // Tenta disparar a animação de entrada
            animator.SetTrigger("Enter");

            // Aguarda o Animation Event disparar OU estourar o tempo limite de segurança
            float runTime = 0f;
            while (!isEnterAnimationFinished && runTime < animationTimeLimit)
            {
                runTime += Time.deltaTime;
                yield return null; // Pausa até o próximo frame
            }

            if (!isEnterAnimationFinished)
            {
                Debug.LogWarning("[TransitionManager] O Animation Event de ENTER não foi chamado a tempo! Pulando animação por segurança.");
            }
        }
        else
        {
            Debug.LogWarning("[TransitionManager] Animator ausente ou desativado. Pulando animação de entrada.");
        }

        // --- 2. CARREGAMENTO DA CENA (Executa SEMPRE) ---
        CurrState = TransitionState.Loading;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // Pausa e espera até que a cena seja 100% carregada
        yield return new WaitUntil(() => op.isDone);

        if (type == TypeOfTransition.ChangeSceneAndTP)
        {
            PlayerInGameDataHolder.Instance.Teleport();
            setTransition(TypeOfTransition.ChangeScene);
        }

        // --- 3. ANIMAÇÃO DE SAÍDA (EXIT) ---
        if (animatorExists)
        {
            CurrState = TransitionState.Exiting;
            animator.SetTrigger("Exit");

            float runTime = 0f;
            while (!isExitAnimationFinished && runTime < animationTimeLimit)
            {
                runTime += Time.deltaTime;
                yield return null;
            }
        }

        // Finaliza o processo com segurança total
        CurrState = TransitionState.Stoped;
    }

    // ========================================================================
    // MÉTODOS CHAMADOS PELOS ANIMATION EVENTS NO UNITY
    // ========================================================================

    /// <summary>
    /// Coloque este método no Animation Event no ÚLTIMO frame da animação de ENTER.
    /// </summary>
    public void OnEnterAnimationComplete()
    {
        isEnterAnimationFinished = true;
    }

    /// <summary>
    /// Coloque este método no Animation Event no ÚLTIMO frame da animação de EXIT.
    /// </summary>
    public void OnExitAnimationComplete()
    {
        isExitAnimationFinished = true;
    }

    // ========================================================================

    public TransitionState state => CurrState;

    public void setTransition(TypeOfTransition type)
    {
        this.type = type;
    }
}