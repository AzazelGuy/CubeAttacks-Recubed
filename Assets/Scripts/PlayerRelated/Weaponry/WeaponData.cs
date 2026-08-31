using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Entities/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Dados das armas")]
    public string WeaponName;
    public Sprite WeaponImage; // THIS IS THE WEAPONS SPRITE (which in the future will bbe cahnged for the animator BUT YOU GET THE POINT)
    public int WeaponIndex;
    public int BulletsMax;
    public float coolDownTime = 0.06f;
    [Range(0f, 100f)] public float recoilStrenght = 0f;
    [Header("Configura��es de Precis�o")]
    [Range(0f, 20f)] public float imprecisao = 0.5f; // O qu�o "torta" a bala pode sair
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

    [ShowIf(nameof(firingMethod), firingType.Spread)][Range(1, 20)] public int NumOfBullets;

    public enum Effect
    {
        None,
        Explosive,
        Eletric,
        Piercing,
        AutoAim
    }

    public bool isAutomatic;
}
