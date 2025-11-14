using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTest : MonoBehaviour
{
    AudioSource _as;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            _as.Play();
        }

        //if(collision.CompareTag("Portal"))
        //{
            
        //}

    }

}
