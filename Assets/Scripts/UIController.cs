using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    
    public TextMeshProUGUI DebugBullets;

    private string weaponName;
    private int bulletsMax;
    private int bulletscurr;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {

        DebugBullets.text = ($"Arma: {weaponName}\n AMMO:{bulletscurr}/{bulletsMax}");
    }

    public void getWeaponInfo(string name, int bulletMax, int bulletcurr)
    {
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
    }
}
