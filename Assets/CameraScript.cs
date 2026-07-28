using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cam;
    private void OnEnable()
    {
        PlayerInGameDataHolder.OnPlayerSpawned += SetTarget;
    }

    private void OnDisable()
    {
        PlayerInGameDataHolder.OnPlayerSpawned -= SetTarget;
    }

    private void SetTarget(Transform target)
    {
    cam.Follow = target;
    }
}
