using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using static Define;
using System.Collections;

public class UI_GameScene : UI_Scene
{
    UI_GameOver _gameOverPopupUI;
    UI_PausePopup _pausePopup;
    #region Enum
    enum GameObjects { /* 필요에 따라 객체 추가 */ }
    enum Buttons 
    {
        JumpButton,
        FlipButton ,
        PauseButton
    }
    enum Texts 
    {   
        ComboText,
        CountText
    }
    enum Images { /* 필요에 따라 이미지 추가 */ }
    #endregion

    private void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Managers.Game.OnComboChanged += UpdateComboText;
    }

    void OnDisable()
    {
        Managers.Game.OnComboChanged -= UpdateComboText;
    }


    public override bool Init()
    {
        if (!base.Init()) return false;

        // 바인딩
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        _pausePopup = Managers.UI.ShowPopupUI<UI_PausePopup>();

        _pausePopup.gameObject.SetActive(false);



        // -------- Blue 버튼: 점프 전용 (Pressed = 홀드 반복 점프) --------
        BindEvent(GetButton((int)Buttons.JumpButton).gameObject, OnJumpPressed, null, Define.EUIEvent.Pressed);

        // -------- Red 버튼: 뒤집기 전용 (PointerDown = 1회만 실행) --------
        BindEvent(GetButton((int)Buttons.FlipButton).gameObject, OnFlipDown, null, Define.EUIEvent.PointerDown);


        GetButton((int)Buttons.PauseButton).gameObject.BindEvent(OnClickPaseButton);

        GetText((int)Texts.CountText).gameObject.SetActive(false);

        return true;
    }

    private void OnClickPaseButton()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        _pausePopup.gameObject.SetActive(true);
    }


    // ===== 버튼 콜백 =====
    private void OnJumpPressed()
    {

        var mv = FindAnyObjectByType<Movement>();
        // var mv = Managers.Object.Player.GetComponent<Movement>();
        if (!mv)
            return;

        // 현재 존과 같은 색을 넣으면 Movement.OnPressColor 내부에서 '점프' 경로로 탑승
        EZoneColor current = mv._currentZone;




        mv.DoJump();
    }

    private void OnFlipDown()
    {
        if (!Managers.Game._isFlip)
            return;


        Managers.Sound.Play(ESound.Effect, "0Sound");


        var mv = FindAnyObjectByType<Movement>();
        if (!mv)
            return;
        // 반대 색을 넣어 OnPressColor가 스파이더 플립 경로로 가도록
        EZoneColor opposite = (mv._currentZone == EZoneColor.Red) ? EZoneColor.Blue : EZoneColor.Red;
        mv.OnPressFlip(opposite);
    }

    private void Update()
    {
        //컴퓨터 test용
        var mv = FindAnyObjectByType<Movement>();

        if (!mv)
            return;

        // ===== 키보드 =====
        // A -> Flip(1회) 
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!Managers.Game._isFlip)
                return;


            Managers.Sound.Play(ESound.Effect, "1Sound");
            EZoneColor opposite = (mv._currentZone == EZoneColor.Red) ? EZoneColor.Blue : EZoneColor.Red;
            mv.OnPressFlip(opposite);
        }

        // ' (apostrophe) 또는 L -> Jump(홀드로 반복)
        bool jumpHeld = Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.L);
        if (jumpHeld)
        {
            mv.DoJump();
        }

        // (옵션) 키보드로 Blue 버튼 눌림 비주얼 흉내
        if (Input.GetKeyDown(KeyCode.Quote) || Input.GetKeyDown(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.JumpButton), true);
        if (Input.GetKeyUp(KeyCode.Quote) || Input.GetKeyUp(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.JumpButton), false);
    }


    public void RestartGame()
    {
        StartCoroutine(CoRestartCountdown());
    }
    private IEnumerator CoRestartCountdown()
    {


        GetText((int)Texts.CountText).gameObject.SetActive(true);

        // 3-2-1
        for (int n = 3; n >= 1; n--)
        {
            GetText((int)Texts.CountText).text = n.ToString();

            // 살짝 커지는 연출 (언스케일드 업데이트)
            GetText((int)Texts.CountText).rectTransform.localScale = Vector3.one;
            GetText((int)Texts.CountText).rectTransform
                .DOScale(1.25f, 0.18f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            // 효과음 쓰면 여기에
            // Managers.Sound.Play(ESound.Effect, "beep");

            yield return new WaitForSecondsRealtime(0.82f);
        }

        // GO!
        GetText((int)Texts.CountText).text = "GO!";
        GetText((int)Texts.CountText).rectTransform.localScale = Vector3.one * 0.9f;
        GetText((int)Texts.CountText).rectTransform
            .DOScale(1.35f, 0.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        // Managers.Sound.Play(ESound.Effect, "go");

        yield return new WaitForSecondsRealtime(0.35f);

        // 로딩 직전 숨김
        GetText((int)Texts.CountText).gameObject.SetActive(false);

        // 재생속도 원복 후 씬 리로드
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // (옵션) 키보드로도 버튼 '눌림' 상태 연출
    private void SimulatePress(Button btn, bool isDown)
    {
        if (btn == null || EventSystem.current == null) return;
        var data = new PointerEventData(EventSystem.current);
        if (isDown)
            ExecuteEvents.Execute(btn.gameObject, data, ExecuteEvents.pointerDownHandler);
        else
            ExecuteEvents.Execute(btn.gameObject, data, ExecuteEvents.pointerUpHandler);
    }

    public void UpdateComboText(int coin)
    {
        GetText((int)Texts.ComboText).text = coin.ToString();
        AnimateComboText();
    }

    public float comboScaleFactor = 1.5f;  // 콤보 UI 커질 비율
    public float animationDuration = 0.1f; // 애니메이션 지속 시간
    private void AnimateComboText()
    {
        // 텍스트 크기를 커졌다가 원래 크기로 돌아가는 애니메이션
        GetText((int)Texts.ComboText).gameObject.transform.DOScale(comboScaleFactor, animationDuration).OnKill(() =>
        {
            // 애니메이션이 끝난 후 원래 크기로 돌아가도록 설정
            GetText((int)Texts.ComboText).gameObject.transform.DOScale(1f, animationDuration);
        });
    }

}
