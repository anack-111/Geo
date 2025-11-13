using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using static Define;

public class UI_GameScene : UI_Scene
{
    UI_GameOver _gameOverPopupUI;

    #region Enum
    enum GameObjects { /* 필요에 따라 객체 추가 */ }
    enum Buttons { JumpButton, FlipButton }
    enum Texts { CoinText }
    enum Images { /* 필요에 따라 이미지 추가 */ }
    #endregion

    private void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Managers.Game.OnCoinChanged += UpdateCoinText;
    }

    void OnDisable()
    {
        Managers.Game.OnCoinChanged -= UpdateCoinText;
    }

    public void UpdateCoinText(int coin)
    {
        GetText((int)Texts.CoinText).text = coin.ToString();
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        // 바인딩
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        // -------- Blue 버튼: 점프 전용 (Pressed = 홀드 반복 점프) --------
        BindEvent(GetButton((int)Buttons.JumpButton).gameObject, OnBlueJumpPressed, null, Define.EUIEvent.Pressed);

        // -------- Red 버튼: 뒤집기 전용 (PointerDown = 1회만 실행) --------
        BindEvent(GetButton((int)Buttons.FlipButton).gameObject, OnFlipDown, null, Define.EUIEvent.PointerDown);


        return true;
    }


    // ===== 버튼 콜백 =====
    private void OnBlueJumpPressed()
    {
        var mv = FindAnyObjectByType<Movement>();
       // var mv = Managers.Object.Player.GetComponent<Movement>();
        if (!mv)
            return;

        // 현재 존과 같은 색을 넣으면 Movement.OnPressColor 내부에서 '점프' 경로로 탑승
        EZoneColor current = mv._currentZone;
        mv.OnPressColor(current);
    }

    private void OnFlipDown()
    {
       // var mv = Managers.Object.Player.GetComponent<Movement>();

        var mv = FindAnyObjectByType<Movement>();
        if (!mv)
            return;
        // 반대 색을 넣어 OnPressColor가 스파이더 플립 경로로 가도록
        EZoneColor opposite = (mv._currentZone == EZoneColor.Red) ? EZoneColor.Blue : EZoneColor.Red;
        mv.OnPressColor(opposite);
    }

    private void Update()
    {
        //컴퓨터 test용
        //var mv = Managers.Object.Player.GetComponent<Movement>();

        var mv = FindAnyObjectByType<Movement>();

        if (!mv) return;

        // ===== 키보드 =====
        // A -> Flip(1회) 
        if (Input.GetKeyDown(KeyCode.A))
        {

            EZoneColor opposite = (mv._currentZone == EZoneColor.Red) ? EZoneColor.Blue : EZoneColor.Red;
            mv.OnPressColor(opposite);
        }

        // ' (apostrophe) 또는 L -> Jump(홀드로 반복)
        bool jumpHeld = Input.GetKey(KeyCode.Quote) ||  Input.GetKey(KeyCode.L);
        if (jumpHeld)
        {
            mv.OnPressColor(mv._currentZone);
        }

        // (옵션) 키보드로 Blue 버튼 눌림 비주얼 흉내
        if (Input.GetKeyDown(KeyCode.Quote) ||  Input.GetKeyDown(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.JumpButton), true);
        if (Input.GetKeyUp(KeyCode.Quote) ||  Input.GetKeyUp(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.JumpButton), false);
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
}
