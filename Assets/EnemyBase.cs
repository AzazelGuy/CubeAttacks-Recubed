using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private GameObject popUpDamage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onHit(float damage)
    {
        Vector3 offset = new Vector3 (Random.Range(-1f,1f), Random.Range(-1f, 1f)); ;
        var enemy = Instantiate(popUpDamage);
        enemy.transform.position = transform.position + offset;
        enemy.GetComponent<DamagePopUp>().Setup(damage);
    }
}
