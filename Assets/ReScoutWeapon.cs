using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReScoutWeapon : MonoBehaviour
{

    [Header("Gun Configurations")]
    public Transform gunPivot;
    public Transform canoDaArma; // Crie um Empty Object na ponta da arma e arraste aqui
    public SpriteRenderer gunsprite;
    [SerializeField] private float rotateSpeed;

    private void Start()
    {

    }

    private void Update()
    {


        GunRotation();
        handleFire();
    }
    

    public void GunRotation()
    {
        
    }

    // Cuidar dos tiros da arma
    private void handleFire()
    {
       
    }

    private void Atirar(WeaponData arma)
    {
       
    }

    private Vector2 AplicarImprecisao(Vector2 direcaoOriginal, float anguloImprecisao)
    {
        if (anguloImprecisao <= 0) return direcaoOriginal;

        float desvioAleatorio = Random.Range(-anguloImprecisao, anguloImprecisao);
        return Quaternion.Euler(0, 0, desvioAleatorio) * direcaoOriginal;
    }
}
