using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
//using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class Movement : MonoBehaviour
{
    public EGameMode _currentGameMode;
    public ESpeed _currentSpeed;
    public float _groundCheckRadius;
    [System.NonSerialized] public float _yLastPortal = 2.3f;
    public LayerMask _groundMask;
    public Transform _sprite;
    public ParticleSystem _moveParticle;
    public TrailRenderer _trailRenderer;
    Rigidbody2D _rb;

    bool _wasOnGround = false;
    public int _gravity = 1;
    public bool _isClickProcessed = false;

    #region 착치 트윈
    Sequence _landSeq; // 착지 스케일 트윈 재사용/중복방지
    Vector3 _baseScale; // 기본 스케일 캐시

    #endregion

    LineController _lineCtrl;   // 현재 들어간 라인
    float _savedGravity;        // 복구용

    const float _lineJumpThresholdPx = 30f; // 드래그 임계(픽셀)
    const float _lineJumpPower = 26f;       // Cube 초기 점프(18)와 동일하게(원하면 조절) 
    bool _linePressing;

    // 포인터 상태(PC/모바일 공통)
    bool _ptrPressing, _ptrPrevPressing, _ptrJustPressed, _ptrJustReleased;
    Vector2 _ptrPos;         // 항상 '마지막으로 알려진' 좌표 (릴리즈 프레임에도 보존)
    float _pressMinY;        // 누른 뒤 내려간 최저 Y (위 드래그 관대판정)

    public  ParticleSystem _jumpEffect;
    private void Awake()
    {
        _baseScale = _sprite.localScale;
    }
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        SamplePointer();

        transform.position += Vector3.right * SPEED_VALUE[(int)_currentSpeed] * Time.deltaTime;
        Invoke(_currentGameMode.ToString(), 0);

        LineControl();

        PaticleControl();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        var lc = other.GetComponent<LineController>();
        if (lc != null)
        {
            _lineCtrl = lc;
            _savedGravity = _rb.gravityScale; // 들어올 때 중력 저장
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 나간 트리거가 현재 붙어 있는 라인일 때만 해제
        var lc = other.GetComponent<LineController>();
        if (_lineCtrl != null && lc == _lineCtrl)
        {
            _lineCtrl = null;
            _rb.gravityScale = Mathf.Abs(_savedGravity) > 0.0001f ? _savedGravity : _rb.gravityScale;
            _rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    void PaticleControl()
    {

        //  Cube 모드일 때 파티클 제어
        if (_currentGameMode == EGameMode.Cube)
        {
            bool isGrounded = OnGround();

            if (!_wasOnGround && isGrounded)
            {
                LandAnim();
                _sprite.GetComponent<Animator>().Play("Idle");
                // 착지했을 때 다시 켜기
                _moveParticle.Play();


            }
            else if (_wasOnGround && !isGrounded)
            {
                // 점프 순간에 끄기
                //_jumpEffect.Play();
                 _sprite.GetComponent<Animator>().Play("Jump");
                _moveParticle.Stop();
            }

            _wasOnGround = isGrounded;
        }
    }


    public bool OnGround()
    {
        // 상승(혹은 중력 방향 반대쪽)으로 움직이는 중이면 false
        if (_rb.velocity.y * _gravity > 0.05f)
            return false;

        // 중력 방향으로 살짝 아래(또는 위) 쪽 박스 감지
        Collider2D hit = Physics2D.OverlapBox(
            transform.position + Vector3.down * _gravity * 0.5f,
            new Vector2(0.8f, _groundCheckRadius),
            0,
            _groundMask
        );

        // 중력 방향에 따라 "내 아래쪽" or "내 위쪽" 판정 다르게
        if (hit)
        {
            if (_gravity == 1)
                return hit.bounds.center.y < transform.position.y - 0.05f; // 아래쪽만
            else
                return hit.bounds.center.y > transform.position.y + 0.05f; // 위쪽만
        }

        return false;
    }


    bool TouchingWall()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + (Vector2.right * 0.55f), Vector2.up * 0.85f + (Vector2.right * _groundCheckRadius), 0, _groundMask);
    }


    #region PlayerState
    void Cube()
    {
        Util.CreateGamemode(_rb, this, true, 18f, 9.057f, true, false, 409.1f);
    }

    void Ship()
    {
        _sprite.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * 2);

        if (Input.GetMouseButton(0))
            _rb.gravityScale = -2.314969f;
        else
            _rb.gravityScale = 2.314969f;

        _rb.gravityScale = _rb.gravityScale * _gravity;
    }
    void Wave()
    {
        _rb.gravityScale = 0;
        _rb.velocity = new Vector2(0, SPEED_VALUE[(int)_currentSpeed] * (Input.GetMouseButton(0) ? 1 : -1) * _gravity);
    }
    void Ball()
    {
        Util.CreateGamemode(_rb, this, true, 0, 6.2f, false, true);
    }

    void UFO()
    {
        Util.CreateGamemode(_rb, this, false, 10.841f, 4.1483f, false, false, 0, 10.841f);
    }

    void Spider()
    {
        Util.CreateGamemode(_rb, this, true, 238.29f, 6.2f, false, true, 0, 238.29f);

        //일단 캐싱 안함
        if (_gravity == 1)
            _sprite.gameObject.GetComponent<SpriteRenderer>().flipY = false;
        else
            _sprite.gameObject.GetComponent<SpriteRenderer>().flipY = true;
    }

    #endregion
    public void ChangeThroughPortal(EGameMode gameMode, ESpeed speed, int gravity, int State, float yPortal)
    {
        Managers.Game.OnModeChanged?.Invoke(gameMode);

        ChangeAnim(gameMode);
        Managers.Game.Flash();

        switch (State)
        {
            case 0:
                _currentSpeed = speed;
                break;
            case 1:
                yPortal = _yLastPortal;
                _currentGameMode = gameMode;
                UpdateEffects();

                break;
            case 2:
                _gravity = gravity;
                _rb.gravityScale = Mathf.Abs(_rb.gravityScale) * gravity;
                
                //일단 캐싱 안함 
                _sprite.gameObject.GetComponent<SpriteRenderer>().flipY = gravity == 1 ? false : true;
                Camera.main.GetComponent<FollowPlayer>().DoGravityTilt(_gravity);

                MoveParticleOffset(_gravity);
               
                break;
        }

    }

    void ChangeAnim(EGameMode gameMode)
    {
        Animator anim = _sprite.gameObject.GetComponent<Animator>();

        switch (gameMode)
        {
            case EGameMode.Cube:
                anim.Play("Idle");
                break;
            case EGameMode.Ship:
                anim.Play("Ship1");

                break;
            case EGameMode.Ball:
                anim.Play("Ship1");
                break;
            case EGameMode.UFO:
                anim.Play("Ship1");
                break;
            case EGameMode.Wave:
                anim.Play("Ship1");
                break;
            case EGameMode.Spider:
                anim.Play("Ship1");
                break;
            default:
                break;
        }
    }


    private void UpdateEffects()
    {
        

        if (_currentGameMode == EGameMode.Cube)
        {
            _trailRenderer.enabled = false;
            _moveParticle.gameObject.SetActive(true);
        }
        else
        {
            // 다른 모드가 생길 때 기본값
            _trailRenderer.enabled = true;
            _moveParticle.gameObject.SetActive(false);

        }
    }


    void MoveParticleOffset(int gravity) //뒤집혔을 때 파티클 오프셋 설정
    {
        var shape = _moveParticle.shape;
        GameObject jump = _jumpEffect.gameObject;
        Vector3 localPos = jump.transform.localPosition;
        if (_gravity == 1)
        {
            shape.position = new Vector3(shape.position.x, -0.5f, shape.position.z);
            localPos.y = -0.5f;
        }
            
        else
        {
            shape.position = new Vector3(shape.position.x, 0.5f, shape.position.z);
            localPos.y = 0.5f;
        }
        jump.transform.localPosition = localPos;
    }
    //-0.03
#if UNITY_EDITOR
    // Scene 뷰에서 확인용 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // OverlapBox와 같은 위치/크기로 박스 그림
        Vector2 center = transform.position + Vector3.down * _gravity * 0.5f;
        Vector2 size = Vector2.right * 0.85f + Vector2.up * _groundCheckRadius;

        Gizmos.DrawWireCube(center, size);
    }
#endif

    void LandAnim()
    {
        //  착지 스쿼시-스트레치
        _landSeq?.Kill();               // 중복 방지
        _sprite.localScale = _baseScale; // 기본값으로 리셋(안전)
        _landSeq = DOTween.Sequence()
            .Append(_sprite.DOScaleY(0.80f * _baseScale.y, 0.1f))   // 순간 스쿼시
            .Append(_sprite.DOScaleY(1.00f * _baseScale.y, 0.05f)
                .SetEase(Ease.OutQuad));                             // 빠르게 복귀
    }


    void LineControl()
    {
        if (_lineCtrl == null) return;

        if (_ptrPressing)
        {
            // 처음 붙는 순간: Kinematic 전환 + 속도 0 + 기준점 초기화
            if (_rb.bodyType != RigidbodyType2D.Kinematic)
            {
                _savedGravity = _rb.gravityScale;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.velocity = Vector2.zero;

                _linePressing = true;
                // 시작 좌표는 SamplePointer에서 처리됨(_pressMinY 세팅)
            }

            // 라인 중앙 Y에 고정
            float targetY = _lineCtrl.LineY + _lineCtrl.yOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            return; // 점프 판정은 손을 '뗄 때'만
        }

        // 손을 뗀 프레임에만: '최저점' 기준 위 드래그 판정
        if (_ptrJustReleased && _linePressing)
        {
            float dyUp = _ptrPos.y - _pressMinY;   // 누른 뒤 최저점 대비 얼마나 위로 올렸나
            if (dyUp > _lineJumpThresholdPx)
            {
                // 위로 충분히 끌어올리고 릴리즈 → 위 점프
                DoLineJump(+1);
            }
            else
            {
                // 아래/미세 드래그 → 점프 없이 해제(자연 낙하)
                if (_rb.bodyType != RigidbodyType2D.Dynamic)
                    _rb.bodyType = RigidbodyType2D.Dynamic;

                _rb.gravityScale = _savedGravity;
                _isClickProcessed = true; // 직후 기본 점프 중복 방지(선택)
                _lineCtrl = null;
                _linePressing = false;
            }
            return;
        }

        // 안전망: 라인인데 지금은 누르지 않고 릴리즈도 아님 → 해제 유지
        if (_rb.bodyType != RigidbodyType2D.Dynamic)
            _rb.bodyType = RigidbodyType2D.Dynamic;

        _rb.gravityScale = _savedGravity;
        _lineCtrl = null;
        _linePressing = false;
    }

    void SamplePointer()
    {
        bool pressing = false;
        Vector2 pos = _ptrPos; // 기본: 직전 좌표 유지

#if UNITY_EDITOR || UNITY_STANDALONE
        pressing = Input.GetMouseButton(0);
        if (pressing || Input.GetMouseButtonUp(0))
            pos = (Vector2)Input.mousePosition;
#else
    // 모바일: Ended/Canceled 프레임에서도 마지막 좌표를 pos에 남김
    if (Input.touchCount > 0)
    {
        // 첫 번째 유효 터치만 사용(원하면 특정 fingerId로 확장)
        var t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            pressing = true;
            pos = t.position;
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            pressing = false;
            pos = t.position; // 릴리즈 좌표 보존
        }
    }
    else
    {
        pressing = false;
        // 좌표는 유지(직전 pos 사용)
    }
#endif

        _ptrJustPressed = pressing && !_ptrPrevPressing;
        _ptrJustReleased = !pressing && _ptrPrevPressing;

        if (_ptrJustPressed)
            _pressMinY = pos.y;              // 시작시 최저점 초기화
        if (pressing)
            _pressMinY = Mathf.Min(_pressMinY, pos.y); // 누르는 동안 내려간 최저 Y 갱신

        _ptrPos = pos;
        _ptrPrevPressing = _ptrPressing;
        _ptrPressing = pressing;
    }

    bool IsLinePressing()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#else
    if (Input.touchCount == 0) return false;
    var p = Input.GetTouch(0).phase;
    return p == TouchPhase.Began || p == TouchPhase.Moved || p == TouchPhase.Stationary;
#endif
    }

    bool ReleasedThisFrame()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonUp(0);
#else
    // 첫 손가락이 떨어진 프레임 감지
    if (Input.touchCount == 0) return false;
    var p = Input.GetTouch(0).phase;
    return p == TouchPhase.Ended || p == TouchPhase.Canceled;
#endif
    }

    Vector2 GetPointerScreenPos()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#else
    return (Input.touchCount > 0) ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
#endif
    }


    void DoLineJump(int verticalDir) // +1 = 화면 위로, -1 = 화면 아래로
    {
        // 물리 복귀
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _savedGravity;

        // Util.CreateGamemode의 점프 감각과 맞춤(속도만 직접 셋)
        // 세계 기준 "위/아래"로 즉시 가속
        var v = _rb.velocity;
        v.y = _lineJumpPower * (verticalDir > 0 ? 1f : -1f);
        _rb.velocity = v;

        // 라인 상태 해제 및 입력 소모(더블 점프 방지)
        _lineCtrl = null;
        _linePressing = false;
        _isClickProcessed = true;   // ← 다음 프레임의 CreateGamemode 점프 중복 방지  :contentReference[oaicite:1]{index=1}
    }
}


