using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Entities/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Dados das armas")]
    public string WeaponName;
    public Sprite WeaponImage;
    public int WeaponIndex;
    public int BulletsMax;
    public float coolDownTime = 0.06f;
    [Range(0f, 100f)] public float recoilStrenght = 0f;
    [Header("Configurações de Precisão")]
    [Range(0f, 20f)] public float imprecisao = 0.5f; // O quão "torta" a bala pode sair
    public enum firingType
    {
        Spread,
        Single,
        Double,
        Projectile
    }

    public firingType firingMethod;

    [Header("Dados das Balas")]
    public GameObject bulletPrefab;
    public float bulletDamage;
    public Sprite bulletImage;

    public enum Effect
    {
        None,
        Explosive,
        Eletric,
        Piercing,
        AutoAim
    }

    public Effect bulletEffect;

    public bool isAutomatic;
}
