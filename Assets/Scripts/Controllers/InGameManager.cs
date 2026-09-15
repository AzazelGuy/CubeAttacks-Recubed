using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [Header("Estatísticas da Fase")]
    [SerializeField] private int inimigosMortos;
    public int InimigosMortos => inimigosMortos;

    // Guarda os IDs dos inimigos destruídos durante a execução desta fase
    private HashSet<string> inimigosDerrotados = new HashSet<string>();

    private void Awake()
    {
        // Singleton simples escopado apenas para o tempo de vida desta fase
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "HubWorld" || SceneManager.GetActiveScene().name == "MainMenu")
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
    public bool InimigoFoiMorto(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return inimigosDerrotados.Contains(id);
    }

    public void RegistrarMorteInimigo(string id)
    {
        if (!string.IsNullOrEmpty(id) && inimigosDerrotados.Add(id))
        {
            inimigosMortos++;
            Debug.Log($"Inimigos mortos na fase: {inimigosMortos}");
        }
    }
}