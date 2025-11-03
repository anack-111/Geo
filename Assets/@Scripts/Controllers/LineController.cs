using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : TileController
{
    float lineY;
    public float LineY => lineY;
    public float yOffset = 0f;
    void Awake()
    {
        // 에디터에서 배치한 오브젝트의 현재 y를 기본값으로
        lineY = transform.position.y;
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }




}
