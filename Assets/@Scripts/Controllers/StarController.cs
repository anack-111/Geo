using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : TileController
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 180f); // �ʴ� ȸ�� �ӵ�(��/��)

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
