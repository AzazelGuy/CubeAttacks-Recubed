using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorldBackground : MonoBehaviour
{
    [SerializeField] private Transform[] followY;
    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newPos = Camera.main.transform.position;
        foreach (Transform t in followY)
        {
            t.position = new Vector3(t.position.x,newPos.y, t.position.z);
        }
        newPos.y = 0;
        transform.position = newPos;
    }
}
