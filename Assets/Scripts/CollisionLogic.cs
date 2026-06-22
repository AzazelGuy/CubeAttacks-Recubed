using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogic : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("PickUp"))
            {
                weaponController.RecarregarArmas();
                Destroy(collision.gameObject);
            }
        }
    }
}
