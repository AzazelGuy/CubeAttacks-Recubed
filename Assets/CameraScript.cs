using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cam;
    [SerializeField] Camera MiniMapCam;
    private Transform Player;
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
    Player = target;
    }

    void LateUpdate()
    {
        if (TransitionScene.Instance.state == TransitionState.Stoped){
        MiniMapCam.transform.position = new Vector3(Player.position.x, Player.position.y, -10f);
        }else
        {
            cam.Follow = null;
            Player = null;
        }
    }

    
}
