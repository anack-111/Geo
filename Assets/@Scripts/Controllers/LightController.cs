using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : TileController
{
    LightBeam _lb;

    private void Awake()
    {
        _lb = Camera.main.GetComponent<LightBeam>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        _lb.FlashOnce();
    }
}
