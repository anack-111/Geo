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
    enum GameObjects
    {
        // 필요에 따라 객체 추가
    }

    enum Buttons
    {
        BlueButton,
        RedButton
    }

    enum Texts
    {
        CoinText
    }

    enum Images
    {
        // 필요에 따라 이미지 추가
    }
    #endregion



    private void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Managers.Game.OnCoinChanged += UpdateCoinText;
    }

    public void UpdateCoinText(int coin)
    {
        GetText((int)Texts.CoinText).text = coin.ToString();
    }

    void OnDisable()
    {
        Managers.Game.OnCoinChanged -= UpdateCoinText;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        #endregion

        // BlueButton 이벤트 처리
        BindEvent(GetButton((int)Buttons.BlueButton).gameObject, OnBlueButtonPressed, null, Define.EUIEvent.Pressed);

        // RedButton 이벤트 처리
        BindEvent(GetButton((int)Buttons.RedButton).gameObject, OnRedButtonPressed, null, Define.EUIEvent.Pressed);

        return true;
    }

    private void Update()
    {
        // --- 키보드 홀드 입력도 동일하게 처리 ---
        // A -> Red
        if (Input.GetKey(KeyCode.A))
            OnRedButtonPressed();

        // ' (apostrophe) -> Blue
        // Unity 버전에 따라 Quote / Apostrophe 중 하나만 존재할 수 있으니 둘 다 체크
        if (Input.GetKey(KeyCode.Quote) || Input.GetKey(KeyCode.L))
            OnBlueButtonPressed();

        // (선택) 버튼 눌림 비주얼까지 흉내내기
        // KeyDown/KeyUp에서 UI의 PointerDown/Up 이벤트를 날려 하이라이트/Pressed 색상 반영


        if (Input.GetKeyDown(KeyCode.Quote) || Input.GetKeyDown(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.BlueButton), true);
        if (Input.GetKeyUp(KeyCode.Quote) || Input.GetKeyUp(KeyCode.L))
            SimulatePress(GetButton((int)Buttons.BlueButton), false);
    }


    private void OnBlueButtonPressed()
    {
        FindObjectOfType<Movement>().OnPressColor(EZoneColor.Blue);
    }

    private void OnRedButtonPressed()
    {
        FindObjectOfType<Movement>().OnPressColor(EZoneColor.Red);
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
