using UnityEngine;
using System.Collections.Generic;
using Pathfinding; // Namespace do A*

public class InimigoWander : MonoBehaviour
{
    private AIPath ai;
    private Rigidbody2D rb;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 4f;
    [SerializeField] private float forcaPulo = 8f;

    [Header("Verificação de Chão")]
    [SerializeField] private Transform checadorChao;
    [SerializeField] private LayerMask layerChao;

    void Start()
    {
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();

        // Desliga o movimento nativo para a nossa física assumir
        ai.canMove = false;

        // Começa o ciclo escolhendo o primeiro nó
        EscolherProximoNoDeWander();
    }

    void Update()
    {
        // O próprio AIPath sabe se chegou no destino final do caminho atual.
        // Se chegou, escolhemos o próximo nó vizinho para continuar o Wander.
        if (ai.reachedEndOfPath && !ai.pathPending)
        {
            EscolherProximoNoDeWander();
        }
    }

    void FixedUpdate()
    {
        // Se o A* tiver uma rota ativa para seguir
        if (ai.hasPath && !ai.reachedEndOfPath)
        {
            // O steeringTarget é o ponto intermediário gerado pelo A* para onde o inimigo DEVE ir agora
            Vector3 proximoPonto = ai.steeringTarget;

            // 1. CHECAGEM DE PULO (Via Raycast curto para frente/baixo mirando o Nó)
            // Olhamos se há um NoCustomizado exatamente na posição do próximo ponto visual
            Collider2D hit = Physics2D.OverlapCircle(proximoPonto, 0.6f);
            if (hit != null && hit.TryGetComponent<NodeTags>(out var no))
            {
                if (no.nodeTag == NodeTag.Jump)
                {
                    
                    Pular();
                }
            }
            else
            {
                Debug.Log("Don't Wanna!");
            }

                // 2. MOVIMENTAÇÃO HORIZONTAL
                float direcaoX = proximoPonto.x - transform.position.x;

            // Se o inimigo não estiver exatamente em cima do ponto X, ele anda
            if (Mathf.Abs(direcaoX) > 0.2f)
            {
                rb.velocity = new Vector2(Mathf.Sign(direcaoX) * velocidade, rb.velocity.y);
            }
            else
            {
                // Se ele estiver alinhado em X com o nó intermediário, reduz a velocidade para não passar direto
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        else
        {
            // Para o movimento se não houver caminho ou se já tiver chegado
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void EscolherProximoNoDeWander()
    {
        // Pega o nó mais próximo da posição do inimigo usando o Grafo ativo
        GraphNode noAtual = AstarPath.active.GetNearest(transform.position).node;

        if (noAtual == null) return;

        // Cria uma lista para armazenar os nós conectados por linhas (edges) no Point Graph
        List<GraphNode> vizinhos = new List<GraphNode>();
        noAtual.GetConnections(vizinho => vizinhos.Add(vizinho));

        if (vizinhos.Count > 0)
        {
            // Escolhe um vizinho aleatório
            int indiceAleatorio = Random.Range(0, vizinhos.Count);
            Vector3 destinoSeguro = (Vector3)vizinhos[indiceAleatorio].position;

            // Aplica o novo destino ao AIPath e força ele a escanear a nova rota imediatamente
            ai.destination = destinoSeguro;
            ai.SearchPath();
        }
    }

    bool EstiverNoChao()
    {
        Debug.Log("Estã no chão!");
        return Physics2D.OverlapCircle(checadorChao.position, 0.2f, layerChao);
    }

    void Pular()
    {
        rb.velocity = new Vector2(rb.velocity.x, forcaPulo);
    }

    void OnDrawGizmosSelected()
    {
        if (checadorChao != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checadorChao.position, 0.2f);
        }
    }
}