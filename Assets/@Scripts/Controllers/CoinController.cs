using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoinController : TileController
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Managers.Sound.Play(Define.ESound.Effect, "Sound_Coin_1");
            //Managers.Game.CoinAdd(1);
            Destroy(gameObject);
        }
    }
}
