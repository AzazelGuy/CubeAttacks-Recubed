using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TransitionState
{
    Stoped,
    Entering,   // Executando animação de entrada (ex: escurecendo)
    Loading, // Processando a cena/posição em segundo plano
    Exiting      // Executando animação de saída (ex: clareando)
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

    [Header("Config")]

    [Header("Configurações de Segurança")]
    [Tooltip("Tempo máximo de espera da animação caso o Animator falhe ou não tenha Animation Event.")]
    [SerializeField] private float animationTimeLimit = 1.5f;

    public TransitionState CurrState { get; private set; } = TransitionState.Stoped;
    private TypeOfTransition type = TypeOfTransition.ChangeScene;
    // Controla se o evento da animação realmente respondeu
    private bool hasAnimationStarted = false;

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
        
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        
        CurrState = TransitionState.Entering;
        hasAnimationStarted = false;

        // --- 1. SEGURANÇA: Verificar se o Animator existe e está ativo ---
        bool animatorExists = animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null;

        if (animatorExists)
        {
            // Tenta disparar a animação de entrada
            animator.SetTrigger("Enter");

            // Aguarda o Animation Event disparar OU estoura o tempo limite de segurança
            float runTime = 0f;
            while (!hasAnimationStarted && runTime < animationTimeLimit)
            {
                runTime += Time.deltaTime;
                yield return null; // Pausa até o próximo frame
            }

            if (!hasAnimationStarted)
            {
                Debug.LogWarning("[TransitionManager] O Animation Event não foi chamado a tempo! Pulando animação por segurança.");
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
        if (animatorExists)
        {
            animator.SetTrigger("Exit");
            // Espera o tempo da animação de saída sem travar o código
            yield return new WaitForSeconds(animationTimeLimit);
        }

        // Finaliza o processo com segurança total
        CurrState = TransitionState.Stoped;
    }

    public TransitionState state => CurrState;
    public void setTransition(TypeOfTransition type)
    {
        this.type = type;
    }
}