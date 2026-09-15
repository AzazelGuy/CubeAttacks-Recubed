using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Faz o objeto copiar exatamente a rotação da câmera
        transform.rotation = mainCamera.transform.rotation;
    }
}