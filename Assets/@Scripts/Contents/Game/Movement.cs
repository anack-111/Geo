using DG.Tweening;
using System.Collections.Generic;
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
    public ParticleSystem _jumpEffect;
    public ParticleSystem _LineParticle;


    Rigidbody2D _rb;
    bool _wasOnGround = false;
    public int _gravity = 1;
    public bool _isClickProcessed = false;

    Sequence _landSeq;
    Vector3 _baseScale;

    // 입력 상태
    [HideInInspector] public bool _inputHeld;
    [HideInInspector] public bool _inputPressed;
    [HideInInspector] public bool _inputReleased;

    // 라인 관련
    LineController _lineCtrl;
    float _savedGravity;
    bool _linePressing;
    const float _lineJumpThresholdPx = 30f;
    const float _lineJumpPower = 26f;

    bool _ptrPressing, _ptrPrevPressing, _ptrJustPressed, _ptrJustReleased;
    Vector2 _ptrPos;
    float _pressMinY;

    private Dictionary<EGameMode, System.Action> _modeActions;

    private void Awake() => _baseScale = _sprite.localScale;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _modeActions = new Dictionary<EGameMode, System.Action>
        {
            { EGameMode.Cube, Cube },
            { EGameMode.Ship, Ship },
            { EGameMode.Ball, Ball },
            { EGameMode.UFO, UFO },
            { EGameMode.Wave, Wave },
            { EGameMode.Spider, Spider },
        };
    }

    private void Update()
    {
        //#if UNITY_EDITOR || UNITY_STANDALONE
        //        _inputHeld = Input.GetMouseButton(0);
        //        _inputPressed = Input.GetMouseButtonDown(0);
        //        _inputReleased = Input.GetMouseButtonUp(0);
        //#else
        //        if (Input.touchCount > 0)
        //        {
        //            var t = Input.GetTouch(0);
        //            _inputHeld = t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
        //            _inputPressed = t.phase == TouchPhase.Began;
        //            _inputReleased = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        //        }
        //        else
        //        {
        //            _inputHeld = false;
        //            _inputPressed = false;
        //            _inputReleased = false;
        //        }
        //#endif
    }

    private void FixedUpdate()
    {
        SamplePointer();
        transform.position += Vector3.right * SPEED_VALUE[(int)_currentSpeed] * Time.deltaTime;

        if (_modeActions.TryGetValue(_currentGameMode, out var action))
            action?.Invoke();

        LineControl();
        PaticleControl();
    }

    // ---------------- 라인 충돌 ----------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        var lc = other.GetComponent<LineController>();
        if (lc != null)
        {

            _lineCtrl = lc;
            _savedGravity = _rb.gravityScale;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _LineParticle.Stop();

        var lc = other.GetComponent<LineController>();

        if (_lineCtrl != null && lc == _lineCtrl)
        {

            _lineCtrl = null;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = _savedGravity;
        }
    }

    // ---------------- Cube ----------------
    void Cube() => Util.CreateGamemode(_rb, this, true, 18f, 9.057f, true, false, 409.1f);

    void Ship()
    {
        _sprite.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * 2);
        _rb.gravityScale = _inputHeld ? -2.314969f : 2.314969f;
        _rb.gravityScale *= _gravity;
    }

    void Wave()
    {
        _rb.gravityScale = 0;
        _rb.velocity = new Vector2(0, SPEED_VALUE[(int)_currentSpeed] * (_inputHeld ? 1 : -1) * _gravity);
    }

    void Ball() => Util.CreateGamemode(_rb, this, true, 0, 6.2f, false, true);
    void UFO() => Util.CreateGamemode(_rb, this, false, 10.841f, 4.1483f, false, false, 0, 10.841f);
    void Spider()
    {
        Util.CreateGamemode(_rb, this, true, 238.29f, 6.2f, false, true, 0, 238.29f);
        _sprite.GetComponent<SpriteRenderer>().flipY = _gravity != 1;
    }

    // ---------------- 라인 제어 ----------------
    void LineControl()
    {
        if (_lineCtrl == null)
            return;

        if (_ptrPressing)
        {
            _LineParticle.Play();
            if (_rb.bodyType != RigidbodyType2D.Kinematic)
            {
                _savedGravity = _rb.gravityScale;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.velocity = Vector2.zero;
                _linePressing = true;
            }

            float targetY = _lineCtrl.LineY + _lineCtrl.yOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            return;
        }

        if (_ptrJustReleased && _linePressing)
        {
            float dyUp = _ptrPos.y - _pressMinY;
            if (dyUp > _lineJumpThresholdPx)
            {
                DoLineJump(+1);
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = _savedGravity;
                _lineCtrl = null;
                _linePressing = false;
                _isClickProcessed = true;
            }
            return;
        }

        if (_rb.bodyType != RigidbodyType2D.Dynamic)
            _rb.bodyType = RigidbodyType2D.Dynamic;

        _rb.gravityScale = _savedGravity;
        _lineCtrl = null;
        _linePressing = false;
        _LineParticle.Stop();
    }

    void DoLineJump(int verticalDir)
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _savedGravity;

        var v = _rb.velocity;
        v.y = _lineJumpPower * (verticalDir > 0 ? 1f : -1f);
        _rb.velocity = v;

        _lineCtrl = null;
        _linePressing = false;
        _isClickProcessed = true;

        if (_jumpEffect) _jumpEffect.Play();
    }

    // ---------------- 포인터 샘플링 ----------------
    void SamplePointer()
    {
        bool pressing = false;
        Vector2 pos = _ptrPos;

#if UNITY_EDITOR || UNITY_STANDALONE
        pressing = Input.GetMouseButton(0);
        if (pressing || Input.GetMouseButtonUp(0))
            pos = (Vector2)Input.mousePosition;
#else
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pressing = true;
                pos = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                pressing = false;
                pos = t.position;
            }
        }
#endif

        _ptrJustPressed = pressing && !_ptrPrevPressing;
        _ptrJustReleased = !pressing && _ptrPrevPressing;

        if (_ptrJustPressed) _pressMinY = pos.y;
        if (pressing) _pressMinY = Mathf.Min(_pressMinY, pos.y);

        _ptrPos = pos;
        _ptrPrevPressing = _ptrPressing;
        _ptrPressing = pressing;
    }

    // ---------------- 포털 전환 ----------------
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
                _sprite.GetComponent<SpriteRenderer>().flipY = gravity != 1;
                Camera.main.GetComponent<FollowPlayer>().DoGravityTilt(_gravity);
                MoveParticleOffset(_gravity);
                break;
        }
    }

    void ChangeAnim(EGameMode gameMode)
    {
        // Animator anim = _sprite.GetComponent<Animator>();
        //anim.Play(gameMode == EGameMode.Cube ? "Idle" : "Ship1");
    }

    void UpdateEffects()
    {
        bool isCube = _currentGameMode == EGameMode.Cube;
        _trailRenderer.enabled = !isCube;
        _moveParticle.gameObject.SetActive(isCube);
    }

    void MoveParticleOffset(int gravity)
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

    // ---------------- 이펙트 / 애니 ----------------
    void PaticleControl()
    {
        if (_currentGameMode != EGameMode.Cube) return;
        bool isGrounded = OnGround();

        if (!_wasOnGround && isGrounded)
        {
            LandAnim();
            _sprite.GetComponent<Animator>().Play("Idle");
            _moveParticle.Play();
            isButtonFlipping = false;
            _isClickProcessed = false;
        }
        else if (_wasOnGround && !isGrounded)
        {
            _sprite.GetComponent<Animator>().Play("Jump");
            _moveParticle.Stop();
        }

        _wasOnGround = isGrounded;
    }

    public bool OnGround()
    {
        if (_rb.velocity.y * _gravity > 0.05f)
            return false;

        Collider2D hit = Physics2D.OverlapBox(
            transform.position + Vector3.down * _gravity * 0.5f,
            new Vector2(0.8f, _groundCheckRadius),
            0,
            _groundMask
        );

        if (!hit) return false;
        return _gravity == 1
            ? hit.bounds.center.y < transform.position.y - 0.05f
            : hit.bounds.center.y > transform.position.y + 0.05f;
    }

    void LandAnim()
    {
        _landSeq?.Kill();
        _sprite.localScale = _baseScale;
        _landSeq = DOTween.Sequence()
            .Append(_sprite.DOScaleY(0.8f * _baseScale.y, 0.1f))
            .Append(_sprite.DOScaleY(1.0f * _baseScale.y, 0.05f).SetEase(Ease.OutQuad));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = transform.position + Vector3.down * _gravity * 0.5f;
        Vector2 size = Vector2.right * 0.85f + Vector2.up * _groundCheckRadius;
        Gizmos.DrawWireCube(center, size);
    }
#endif

    public EZoneColor _currentZone = EZoneColor.Red; // 현재 구역 색상


    public bool isButtonFlipping = false; // 버튼 클릭 중인지 체크하는 플래그
    public void OnPressColor(EZoneColor color)
    {
        if (color == _currentZone)
        {
            // 같은 색이면 점프
            DoJump();
        }
        else
        {
            // 다른 색이면 Spider처럼 반전 이동
            isButtonFlipping = true;
            DoSpiderFlip();
        }
    }


    void DoJump()
    {
        if (OnGround())
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 18f * _gravity);
            if (_jumpEffect) _jumpEffect.Play();
        }
    }

    void DoSpiderFlip()
    {
        Camera.main.GetComponent<FollowPlayer>().DoGravityTilt(_gravity);
        _currentZone = _currentZone == EZoneColor.Blue ? EZoneColor.Red : EZoneColor.Blue;

        // Spider 세팅
        _rb.gravityScale = 6.2f * _gravity;

        // Spider 클릭 효과 그대로
        _isClickProcessed = true;
        _rb.velocity = Vector2.up * 238.29f * _gravity;
        _gravity *= -1;
        _rb.gravityScale = Mathf.Abs(_rb.gravityScale) * _gravity;

        _sprite.GetComponent<SpriteRenderer>().flipY = _gravity != 1;
        if (_jumpEffect) _jumpEffect.Play();

    }


    public void DoFlip()
    {

        if (isButtonFlipping)
        {
            // 버튼 클릭 시에는 색상 반전만 하고, LineZone 충돌은 무시
            isButtonFlipping = false;
            return;
        }

        // 현재 색상과 버튼 색상이 일치하는 경우에는 반전하지 않음
        if ((_currentZone == EZoneColor.Blue && _gravity == 1) || (_currentZone == EZoneColor.Red && _gravity == -1))
            return;

        // 색상 전환
        _currentZone = _currentZone == EZoneColor.Blue ? EZoneColor.Red : EZoneColor.Blue;

        _gravity *= -1;
        _rb.gravityScale = Mathf.Abs(_rb.gravityScale) * _gravity;

        // flipY 스프라이트 반전
        _sprite.GetComponent<SpriteRenderer>().flipY = _gravity != 1;

        // 파티클 / 화면 틸트
        if (_LineParticle) _LineParticle.Play();
    }


}
