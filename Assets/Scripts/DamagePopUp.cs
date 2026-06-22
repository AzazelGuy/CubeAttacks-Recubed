using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 1f;
    [SerializeField] private Color textColor;
    [SerializeField] private Gradient CRITICALColor;

    [SerializeField] private float moveYSpeed = 2f;
    [SerializeField] private float disappearSpeed = 3f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // Função para configurar o valor do dano
    public void Setup(float damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
        textColor = textMesh.color;
    }
    public void ONCRIT()
    {
        Color top = CRITICALColor.Evaluate(0f);
        Color bottom = CRITICALColor.Evaluate(0f);
        textMesh.fontSize = 14f;
        textMesh.colorGradient = new VertexGradient(top, top, bottom, bottom);
    }

    private void Update()
    {
        // Faz o texto subir
        transform.position += new Vector3(0, moveYSpeed, 0) * Time.deltaTime;

        // Controla o tempo para começar a sumir
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Efeito de Fade Out (diminuir o Alpha do texto)
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            // Destrói o objeto quando estiver totalmente invisível
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}