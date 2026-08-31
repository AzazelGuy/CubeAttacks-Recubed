using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private Camera miniMapCam;
    private Transform player;

    private void OnEnable()
    {
        PlayerInGameDataHolder.OnPlayerSpawned += SetTarget;
    }

    private void OnDisable()
    {
        PlayerInGameDataHolder.OnPlayerSpawned -= SetTarget;
    }

    private void Start()
    {
        // Se o Player já foi instanciado em uma cena anterior (persistent via DontDestroyOnLoad),
        // resgatamos a referência dele imediatamente ao carregar a nova cena.
        if (PlayerInGameDataHolder.Instance != null)
        {
            GameObject currentPlayer = PlayerInGameDataHolder.Instance.currentPlayer;
            if (currentPlayer != null)
            {
                SetTarget(currentPlayer.transform);
            }
        }
    }

    private void SetTarget(Transform target)
    {
        if (target == null) return;

        player = target;

        if (cam != null)
        {
            cam.Follow = target;
            // Se for Cinemachine 2D / 3D com LookAt, vale a pena setar também por garantia:
            cam.LookAt = target;
        }
    }

    private void LateUpdate()
    {
        // Checagem de segurança para evitar erro se o Player ainda não tiver sido atribuído
        if (player != null && miniMapCam != null)
        {
            miniMapCam.transform.position = new Vector3(player.position.x, player.position.y, -10f);
        }
    }
}