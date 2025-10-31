using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoinController : TileController
{
    SpriteRenderer _sr;
    public ParticleSystem _particle;
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        
    }   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Managers.Game.CoinAdd(1);
            Managers.Sound.Play(Define.ESound.Effect, "Sound_Coin_1");
            DestroyEffect();
            Destroy(gameObject , 1);
        }
    }

    void DestroyEffect()
    {
        _particle.Play();
        var c = _sr.color;
        c.a = 0f;
        _sr.color = c;
    }
}
