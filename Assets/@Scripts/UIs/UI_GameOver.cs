using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class UI_GameOver : UI_Popup
{
    #region Enum
    enum GameObjects { Content }
    enum Buttons {  }
    enum Texts 
    {
        TotalCountText,
        MatchCountText, 
        AttemptCountText 
    } 
    #endregion


    bool _animating;

    // 옵션
    [SerializeField] float _stepDelay = 0.02f;     // 스텝 간 딜레이
    [SerializeField] int _maxSteps = 50;         // 총 스텝 상한(많을 땐 묶음 이동)
    [SerializeField] float _bumpScale = 1.15f;     // 숫자 톡톡 효과
    [SerializeField] float _bumpTime = 0.08f;

    public override bool Init()
    {
        if (!base.Init()) return
                false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        return true;
    }

    int _matchCoins;          // 이번 판 획득 코인
    int _totalCoinsStart;     // 애니 전 Total
    int _totalCoinsTarget;    // 애니 후 Total (= start + match)

    private void OnEnable()
    {
        //PopupOpenAnimation(GetObject((int)GameObjects.Content));
        // 필요 시 자동 시작을 원하면 여기서 StartTransfer() 호출

        Camera.main.GetComponent<FollowPlayer>()._lenderCamera.SetActive(true);
    }


    // ===== 외부에서 값 주입 후 팝업 띄울 때 호출 =====
    // ex) uiGameOver.ShowAndAnimate(match, Managers.Game.TotalCoins);
    public void ShowAndAnimate(int matchCoins, int totalCoinsBefore)
    {
        _matchCoins = Mathf.Max(0, matchCoins);
        _totalCoinsStart = Mathf.Max(0, totalCoinsBefore);
        _totalCoinsTarget = _totalCoinsStart + _matchCoins;

        // 초기 표시 갱신
        SetText((int)Texts.MatchCountText, _matchCoins.ToString());
        SetText((int)Texts.TotalCountText, _totalCoinsStart.ToString());

        gameObject.SetActive(true);
        StartTransfer();
    }

    // ===== 코인 이체 애니 시작 =====
    public void StartTransfer()
    {
        if (_animating)
            return;

        StartCoroutine(Co_TransferCoins());
    }

    IEnumerator Co_TransferCoins()
    {
        _animating = true;

        int remain = _matchCoins;
        int total = _totalCoinsStart;

        // 스텝 크기 계산(많이 쌓였으면 한 번에 n개씩 이동)
        int step = Mathf.Max(1, Mathf.CeilToInt((float)remain / _maxSteps));

        var totalTxt = GetText((int)Texts.TotalCountText);
        var matchTxt = GetText((int)Texts.MatchCountText);

        while (remain > 0)
        {
            int move = Mathf.Min(step, remain);
            remain -= move;
            total += move;

            // 숫자 갱신
            matchTxt.text = remain.ToString();
            totalTxt.text = total.ToString();

            // 톡톡 애니 (스케일 펀치)
            Bump(totalTxt.transform);
            Bump(matchTxt.transform);

            // 사운드가 있다면 여기서 짧게 (묶음마다 1회) 호출
            // Managers.Sound.Play(ESound.Effect, "coin_tick");

            yield return new WaitForSeconds(_stepDelay);
        }

        // 최종 보정
        matchTxt.text = "0";
        totalTxt.text = _totalCoinsTarget.ToString();

        _animating = false;

        // 게임 매니저 반영이 필요하면 여기서 처리 (예: 토탈 갱신/매치 0 세팅)
        // Managers.Game.SetTotalCoins(_totalCoinsTarget);
        // Managers.Game.SetMatchCoins(0);
    }

    void Bump(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOScale(_bumpScale, _bumpTime).SetEase(Ease.OutQuad)
         .OnComplete(() => t.DOScale(1f, _bumpTime).SetEase(Ease.OutQuad));
    }

    // 편의 메서드
    void SetText(int idx, string value)
    {
        var tmp = GetText(idx) as TextMeshProUGUI;
        if (tmp != null) tmp.text = value;
    }
}
