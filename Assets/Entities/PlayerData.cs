using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Info", menuName = "Entities/Player/Player Data")]
public class PlayerData : ScriptableObject
{
    public float playerHealthCurrent = 100.0f;
    public float playerHealthMax = 100.0f;
}
