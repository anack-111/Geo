using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : TileController
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 180f); // 초당 회전 속도(도/초)

    void Update()
    {
        //transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
