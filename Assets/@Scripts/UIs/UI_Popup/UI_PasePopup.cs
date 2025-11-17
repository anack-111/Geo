using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // ★ 추가
using UnityEngine.UI;

public class UI_PasePopup : UI_Popup
{
    #region Enum
    enum GameObjects { Background }
    enum Buttons
    {
        LobbyButton,
        RestartButton,
        PlayButton
    }
    enum Texts { }
    enum Images { /* 필요에 따라 이미지 추가 */ }
    #endregion

    Sequence _seq; // 애니메이션 시퀀스

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        var bg = GetObject((int)GameObjects.Background);
        PaseOpenAnimation(bg);
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.LobbyButton).gameObject.BindEvent(OnClickLobbyButton);
        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(OnClickRestratButton);
        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnClickPlayButton);

        return true;
    }

    private void OnClickPlayButton()
    {
        if (Managers.UI.SceneUI is UI_GameScene)
            (Managers.UI.SceneUI as UI_GameScene).RestartGame();

        gameObject.SetActive(false);
    }

    private void OnClickRestratButton()
    {
        Managers.Scene.LoadScene(Define.EScene.GameScene);
    }

    private void OnClickLobbyButton()
    {
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }

    // ==========================
    // Background 떨어지고 흔들리기
    // ==========================
    void PaseOpenAnimation(GameObject background)
    {
        if (!background) return;

        RectTransform rt = background.GetComponent<RectTransform>();
        if (!rt) return;

        // ---- 하드코딩 파라미터 ----
        Vector2 HANG_POS = rt.anchoredPosition; // 최종 매달릴 위치(현재 배치 위치)
        float DROP_FROMY = 600f;                // 시작 오프셋(위)
        float DROP_TIME = 0.2f;

        float SQUASH_Y = 18f;                 // 착지 눌림 양
        float SQUASH_TIME = 0.12f;
        // --------------------------

        _seq?.Kill(true);

        // 시작 위치(위에서)
        rt.anchoredPosition = HANG_POS + Vector2.up * DROP_FROMY;
        rt.localRotation = Quaternion.identity;

        _seq = DOTween.Sequence().SetUpdate(true);

        // 1) 낙하
        _seq.Append(rt.DOAnchorPos(HANG_POS, DROP_TIME).SetEase(Ease.InQuad));

        // 2) 착지 스쿼시(아래로 눌렸다가 복귀)
        _seq.Append(rt.DOAnchorPos(HANG_POS + Vector2.down * SQUASH_Y, SQUASH_TIME * 0.5f).SetEase(Ease.OutQuad));
        _seq.Append(rt.DOAnchorPos(HANG_POS, SQUASH_TIME * 0.5f).SetEase(Ease.OutQuad));

        // 스윙(좌우 흔들림) 제거됨
    }

}
