using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Movement m = collision.GetComponent<Movement>();
        if (m != null && !m.isButtonFlipping) // 버튼 클릭 중일 때는 충돌 무시
        {
            m.DoFlip(); // 라인 닿으면 중력 반전
        }
    }
}
