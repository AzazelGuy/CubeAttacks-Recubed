using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int RandomSeed { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RandomSeed = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 + System.DateTime.Now.Minute * 60000;
        Random.InitState(RandomSeed);
    }
}
