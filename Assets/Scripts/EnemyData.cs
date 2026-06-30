using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Entities/Enemies/Enemy ")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    [Range(10f,9999f)]public float maxHp;
    public float defense;
    public float speed;
    public float detectionDistance;
    public float attackDistance;
    public enum Tier{ Alfa, Beta, Gamma, Delta};
    public enum TypeOfEnemy{ LongRange, Melee, Followers, Especial};
    public Tier tier;
    public TypeOfEnemy type;
    public bool isImortal = false;
    public WeaponData weapon;
    public float damage;
    [Range(0f,100f)]public float critChance;
    [Range(0.1f,1f)]public float variationPercentage = 0.2f; 
    [Tooltip("Utilizado para Waves")]
    public GameObject enemyPrefab;

}
