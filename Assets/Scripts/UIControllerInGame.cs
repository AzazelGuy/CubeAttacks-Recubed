using System;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIControllerInGame : MonoBehaviour
{
    public static UIControllerInGame Instance;
    
    [SerializeField] private TextMeshProUGUI BulletAmount;
    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private Image fillImageHealth;
    
    [SerializeField] private TextMeshProUGUI TexthealthAmnt;
    [SerializeField] private CanvasGroup hurtImage;
    [Tooltip("A ordem do array afeta sim o sistema todo, fique atento")]
    [SerializeField] private RectTransform[] WeaponPos;
    [SerializeField] private Image[] WeaponImages;

    private int weaponSlot = 0;
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
            return;
        }

        foreach(RectTransform s in WeaponPos)
        {
            WeaponImages[weaponSlot].rectTransform.position = s.position;
            weaponSlot ++;
        }
    }
    private void Update()
    {

        BulletAmount.text = $"{bulletscurr}/{bulletsMax}";
        WeaponName.text = $"{weaponName}";
        if (SceneManager.GetActiveScene().name == "HubWorld" || SceneManager.GetActiveScene().name == "MainMenu")
        {
            Instance = null;
            Destroy(gameObject);
        }

        for(int i = 0; i <= WeaponPos.Length; i++)
        {

        }
        foreach(var w in WeaponImages)
        {
            int desiredpos = weaponSlot % WeaponImages.Length;
        }
    }
    public void SetHealthEffect(float currentAmnt, float maxAmnt)
    {
        float targetFill = currentAmnt / maxAmnt;

        hurtImage.alpha = targetFill;
        
    }    
    public void getWeaponInfo(string name = "None", int bulletMax = 0, int bulletcurr = 0)
    {
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
    }
    public void SpinWheel(int index, int size)
    {
        int target = index % size;
    }

    public Image Healthfill => fillImageHealth;

    public TextMeshProUGUI TextHealth => TexthealthAmnt;
}
