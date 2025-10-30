using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : TileController
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Managers.Sound.Play(Define.ESound.Effect, "Sound_Gate_Open");
            

        }
    }
}
