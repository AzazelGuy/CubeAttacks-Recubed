using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Entities/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Atributos Base")]
    public string nomeInimigo;
    [Range(100f,9999f)]public float hpMaximo;
    public float defesa;
    public float velocidade;
    public enum Tier
    {
        Alfa,
        Beta,
        Gamma,
        Delta
    };

    public bool isImortal = false;

    public Tier tier;
    public enum Type
    {
        LongRange,
        Melee,
        Followers,
        Especial
    };

    public Type type;

    public WeaponData weapon;

    [Header("Combate")]
    public float danoBase;
    public float chanceCritico;

    [Header("Visual")]
    public GameObject prefabInimigo;
}
