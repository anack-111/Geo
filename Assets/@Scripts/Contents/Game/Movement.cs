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

    Rigidbody2D _rb;
    bool _wasOnGround = false;
    public int _gravity = 1;
    public bool _isClickProcessed = false;

    Sequence _landSeq;
    Vector3 _baseScale;

    // 입력 상태 저장용
    [HideInInspector] public bool _inputHeld;
    [HideInInspector] public bool _inputPressed;
    [HideInInspector] public bool _inputReleased;


    private Dictionary<EGameMode, System.Action> _modeActions;

    private void Awake() => _baseScale = _sprite.localScale;


    //  Update: 모바일/PC 모두에서 입력 감지
    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        _inputHeld = Input.GetMouseButton(0);
        _inputPressed = Input.GetMouseButtonDown(0);
        _inputReleased = Input.GetMouseButtonUp(0);
#else
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            _inputHeld = t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            _inputPressed = t.phase == TouchPhase.Began;
            _inputReleased = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }
        else
        {
            _inputHeld = false;
            _inputPressed = false;
            _inputReleased = false;
        }
#endif
    }


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

    private void FixedUpdate()
    {
        transform.position += Vector3.right * SPEED_VALUE[(int)_currentSpeed] * Time.deltaTime;

        if (_modeActions.TryGetValue(_currentGameMode, out var action))
            action?.Invoke();

        PaticleControl();
    }
    void PaticleControl()
    {
        if (_currentGameMode != EGameMode.Cube)
            return;

        bool isGrounded = OnGround();

        if (!_wasOnGround && isGrounded)
        {
            LandAnim();
            _sprite.GetComponent<Animator>().Play("Idle");
            _moveParticle.Play();
            _isClickProcessed = false; // 착지 시 클릭 리셋
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

        if (hit)
        {
            if (_gravity == 1)
                return hit.bounds.center.y < transform.position.y - 0.05f;
            else
                return hit.bounds.center.y > transform.position.y + 0.05f;
        }

        return false;
    }

    void Cube()
    {
        Util.CreateGamemode(_rb, this, true, 18f, 9.057f, true, false, 409.1f);
    }

    void Ship()
    {
        _sprite.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * 2);
        if (_inputHeld)
            _rb.gravityScale = -2.314969f;
        else
            _rb.gravityScale = 2.314969f;
        _rb.gravityScale *= _gravity;
    }

    void Wave()
    {
        _rb.gravityScale = 0;
        _rb.velocity = new Vector2(0, SPEED_VALUE[(int)_currentSpeed] * (_inputHeld ? 1 : -1) * _gravity);
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
        _sprite.GetComponent<SpriteRenderer>().flipY = _gravity != 1;
    }

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
        Animator anim = _sprite.GetComponent<Animator>();
        anim.Play(gameMode == EGameMode.Cube ? "Idle" : "Ship1");
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = transform.position + Vector3.down * _gravity * 0.5f;
        Vector2 size = Vector2.right * 0.85f + Vector2.up * _groundCheckRadius;
        Gizmos.DrawWireCube(center, size);
    }
#endif

    void LandAnim()
    {
        _landSeq?.Kill();
        _sprite.localScale = _baseScale;
        _landSeq = DOTween.Sequence()
            .Append(_sprite.DOScaleY(0.8f * _baseScale.y, 0.1f))
            .Append(_sprite.DOScaleY(1.0f * _baseScale.y, 0.05f).SetEase(Ease.OutQuad));
    }
}
