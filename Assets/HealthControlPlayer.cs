using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthControlPlayer : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] float timeUntilRegen = 15f;
    float timerUntilRegen;
    [SerializeField] float regenMult = 0.01f;

    void Update()
    {
        timerUntilRegen -= Time.deltaTime;
        playerData.playerHealthCurrent = Mathf.Clamp(playerData.playerHealthCurrent, 0f, playerData.playerHealthMax);
        
        if (timerUntilRegen > 0) return;

        playerData.playerHealthCurrent = Mathf.Lerp(playerData.playerHealthCurrent, playerData.playerHealthMax, regenMult);
        if (playerData.playerHealthCurrent >= (playerData.playerHealthMax - 0.5f)) playerData.playerHealthCurrent  = playerData.playerHealthMax;
    }

    public void ResetTimer()
    {
        timerUntilRegen = timeUntilRegen;
    }
}
