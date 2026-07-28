using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Info", menuName = "Entities/Player/New Character")]
public class PlayerData : ScriptableObject
{
    //Main Player Info
    public string playerName = "Fulano";
    public int id; 
    public enum movementTypePlayer
    {
        slipperyMovement,
        hardMovement
    }

    public movementTypePlayer movementType;

    public enum attackTypePlayer
    {
        shootingBased,
        meleeBased,
        pyshicalBased
    }

    public attackTypePlayer attackType;
    public bool usesWeapon;

    [ShowIf(nameof(usesWeapon), false)]
    public float baseMeleedamage;

    [ShowIf(nameof(usesWeapon), false)]
    public float baseMeleeRange;


    public bool hasAcceleration;

    #region Acceleration movement variables
    [ShowIf(nameof(hasAcceleration))]
    public float moveSpeed = 8f;
    [ShowIf(nameof(hasAcceleration))]
    public float acceleration = 0.12f;
    [ShowIf(nameof(hasAcceleration))]
    public float deceleration = 0.18f;
    [ShowIf(nameof(hasAcceleration))]
    public float skidDeceleration = 0.4f;
    #endregion

    [ShowIf(nameof(hasAcceleration), false)] //if the movement isn't speed based;
    public float movementSpeedBase;
    //Health Related
    public float playerHealthMax = 100.0f;

    //Health Related
    public float playerShieldMax = 100.0f;

    public PlayerVisualData myVisualData;

    public GameObject Prefab;
}
