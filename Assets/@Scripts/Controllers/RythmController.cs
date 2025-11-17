using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class RythmController : BaseController
{
    public GameObject _star;

    public float comboScaleFactor = 1.2f;  // 콤보 UI 커질 비율
    public float animationDuration = 0.2f; // 애니메이션 지속 시간



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Managers.Game.ComboAdd(1);
            StarAnim();
        }
    }

    private void StarAnim()
    {
        // 텍스트 크기를 커졌다가 원래 크기로 돌아가는 애니메이션
        _star.transform.DOScale(comboScaleFactor, animationDuration).OnKill(() =>
        {
            // 애니메이션이 끝난 후 원래 크기로 돌아가도록 설정
            _star.transform.DOScale(1f, animationDuration);
        });
    }
}