using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class UI_GameOver : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        Content,
        TotalGroup,
        MatchGroup,
        AttemptsObject
    }
    enum Buttons { }
    enum Texts
    {
        TotalCountText,
        MatchCountText,
        AttemptCountText
    }
    #endregion

    bool _animating;

    // 옵션
    [SerializeField] float _stepDelay = 0.02f;
    [SerializeField] int _maxSteps = 50;
    [SerializeField] float _bumpScale = 1.15f;
    [SerializeField] float _bumpTime = 0.08f;

    // ★ 진입 애니용 캐시
    RectTransform _rtTotal, _rtMatch, _rtAttempts;
    Vector2 _posTotal, _posMatch, _posAttempts;
    bool _enterCached = false;

    // ★ 진입 애니 옵션
    [SerializeField] float _enterDuration = 0.35f;
    [SerializeField] float _enterStagger = 0.08f;
    [SerializeField] float _enterOffsetX = 900f; // 오른쪽 바깥으로 밀어둘 X 오프셋
    [SerializeField] float _enterDelay = 1f;   //  팝업 후 지연 시간
    private void Awake()
    {
        Init();
    }
    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        //  RectTransform 캐시
        _rtTotal = GetObject((int)GameObjects.TotalGroup).GetComponent<RectTransform>();
        _rtMatch = GetObject((int)GameObjects.MatchGroup).GetComponent<RectTransform>();
        _rtAttempts = GetObject((int)GameObjects.AttemptsObject).GetComponent<RectTransform>();

        return true;
    }

    int _matchCoins;
    int _totalCoinsStart;
    int _totalCoinsTarget;
    Coroutine _enterCo; //  코루틴 핸들

    private void OnEnable()
    {
        GetText((int)Texts.MatchCountText).text = Managers.Game.Combo.ToString();

        PopupOpenAnimation(GetObject((int)GameObjects.Content));
        // 네가 쓰는 다른 처리 유지
        Camera.main.GetComponent<FollowPlayer>()._lenderCamera.SetActive(true);

        //  첫 활성화 시 원래 위치 캐시
        if (!_enterCached)
        {
            _posTotal = _rtTotal.anchoredPosition;
            _posMatch = _rtMatch.anchoredPosition;
            _posAttempts = _rtAttempts.anchoredPosition;
            _enterCached = true;
        }

        PrepositionOffscreen();


        if (_enterCo != null)
            StopCoroutine(_enterCo);

        _enterCo = StartCoroutine(Co_DelayedEnter());
    }

    IEnumerator Co_DelayedEnter()
    {
        yield return new WaitForSeconds(_enterDelay);

        var seq = PlayEnterSlideIn();
        yield return seq.WaitForCompletion(); //  슬라이드 인 끝날 때까지 대기


        yield return new WaitForSeconds(1f);
       // 슬라이드 끝난 후 숫자 전환 시작
       ShowAndAnimate(Managers.Game.Combo, Managers.Game.TotalCoins);
        _enterCo = null;
    }
    void PrepositionOffscreen()
    {
        if (!_rtTotal || !_rtMatch || !_rtAttempts)
            return;

        _rtTotal.DOKill(); _rtMatch.DOKill(); _rtAttempts.DOKill();

        _rtTotal.anchoredPosition = _posTotal + new Vector2(_enterOffsetX, 0f);
        _rtMatch.anchoredPosition = _posMatch + new Vector2(_enterOffsetX, 0f);
        _rtAttempts.anchoredPosition = _posAttempts + new Vector2(_enterOffsetX, 0f);
    }

    //  오른쪽에서 원래 위치로 들어오는 슬라이드 인
    Sequence PlayEnterSlideIn()
    {
        // 시작 위치
        _rtTotal.anchoredPosition = _posTotal + new Vector2(_enterOffsetX, 0f);
        _rtMatch.anchoredPosition = _posMatch + new Vector2(_enterOffsetX, 0f);
        _rtAttempts.anchoredPosition = _posAttempts + new Vector2(_enterOffsetX, 0f);

        _rtTotal.DOKill(); _rtMatch.DOKill(); _rtAttempts.DOKill();

        // 하나의 시퀀스로 묶어서 반환
        var seq = DOTween.Sequence();
        seq.Append(_rtTotal.DOAnchorPos(_posTotal, _enterDuration).SetEase(Ease.OutCubic));
        seq.Join(_rtMatch.DOAnchorPos(_posMatch, _enterDuration).SetEase(Ease.OutCubic).SetDelay(_enterStagger));
        seq.Join(_rtAttempts.DOAnchorPos(_posAttempts, _enterDuration).SetEase(Ease.OutCubic).SetDelay(_enterStagger * 2));

        return seq;
    }
    // ===== 외부에서 값 주입 후 팝업 띄울 때 호출 =====
    public void ShowAndAnimate(int matchCoins, int totalCoinsBefore)
    {
        _matchCoins = Mathf.Max(0, matchCoins);
        _totalCoinsStart = Mathf.Max(0, totalCoinsBefore);
        _totalCoinsTarget = _totalCoinsStart + _matchCoins;

        SetText((int)Texts.MatchCountText, _matchCoins.ToString());
        SetText((int)Texts.TotalCountText, _totalCoinsStart.ToString());

        gameObject.SetActive(true);
        StartTransfer();
    }

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

        int step = Mathf.Max(1, Mathf.CeilToInt((float)remain / _maxSteps));

        var totalTxt = GetText((int)Texts.TotalCountText);
        var matchTxt = GetText((int)Texts.MatchCountText);

        while (remain > 0)
        {
            int move = Mathf.Min(step, remain);
            remain -= move;
            total += move;

            matchTxt.text = remain.ToString();
            totalTxt.text = total.ToString();

            Bump(totalTxt.transform);
            Bump(matchTxt.transform);

            yield return new WaitForSeconds(_stepDelay);
        }

        matchTxt.text = "0";
        totalTxt.text = _totalCoinsTarget.ToString();

        _animating = false;
    }

    void Bump(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOScale(_bumpScale, _bumpTime).SetEase(Ease.OutQuad)
         .OnComplete(() => t.DOScale(1f, _bumpTime).SetEase(Ease.OutQuad));
    }

    void SetText(int idx, string value)
    {
        var tmp = GetText(idx) as TextMeshProUGUI;
        if (tmp != null) tmp.text = value;
    }
}
