using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrailEfects : MonoBehaviour
{
    [SerializeField] Transform MuzzleFLashSprite;
    void Start()
    {
        MuzzleFLashSprite.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
    }
}
