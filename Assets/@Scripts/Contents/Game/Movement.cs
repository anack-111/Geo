using DG.Tweening;
using System.Collections;
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
    Rigidbody2D _rb;

    bool _wasOnGround = false;
    public int _gravity = 1;
    public bool _isClickProcessed = false;

    #region 착치 트윈
    Sequence _landSeq; // 착지 스케일 트윈 재사용/중복방지
    Vector3 _baseScale; // 기본 스케일 캐시

    #endregion

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

        transform.position += Vector3.right * SPEED_VALUE[(int)_currentSpeed] * Time.deltaTime;
        Invoke(_currentGameMode.ToString(), 0);


        PaticleControl();
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
                // 착지했을 때 다시 켜기
                _moveParticle.Play();
            }
            else if (_wasOnGround && !isGrounded)
            {
                // 점프 순간에 끄기
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
        return Physics2D.OverlapBox((Vector2)transform.position + (Vector2.right * 0.55f), Vector2.up * 0.8f + (Vector2.right * _groundCheckRadius), 0, _groundMask);
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
            _rb.gravityScale = -4.314969f;
        else
            _rb.gravityScale = 4.314969f;

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
                _sprite.gameObject.GetComponent<SpriteRenderer>().flipY = gravity == 1 ? false : true;
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
                anim.Play("Ship2");

                break;
            case EGameMode.Ball:
                anim.Play("Idle");
                break;
            case EGameMode.UFO:
                anim.Play("Idle");
                break;
            case EGameMode.Wave:
                anim.Play("Idle");
                break;
            case EGameMode.Spider:
                anim.Play("Idle");
                break;
            default:
                break;
        }
    }


    private void UpdateEffects()
    {
        var trail = _sprite.gameObject.GetComponent<TrailRenderer>();

        if (_currentGameMode == EGameMode.Cube)
        {
            trail.enabled = false;
            _moveParticle.gameObject.SetActive(true);
        }
        else
        {
            // 다른 모드가 생길 때 기본값
            trail.enabled = true;
            _moveParticle.gameObject.SetActive(false);

        }
    }


    void MoveParticleOffset(int gravity) //뒤집혔을 때 파티클 오프셋 설정
    {
        var shape = _moveParticle.shape;
        if (_gravity == 1)
            shape.position = new Vector3(shape.position.x, -0.5f, shape.position.z);
        else
            shape.position = new Vector3(shape.position.x, 0.5f, shape.position.z);
    }
    //-0.03
#if UNITY_EDITOR
    // Scene 뷰에서 확인용 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // OverlapBox와 같은 위치/크기로 박스 그림
        Vector2 center = transform.position + Vector3.down * _gravity * 0.5f;
        Vector2 size = Vector2.right * 0.8f + Vector2.up * _groundCheckRadius;

        Gizmos.DrawWireCube(center, size);
    }
#endif

    void LandAnim()
    {
        //  착지 스쿼시-스트레치
        _landSeq?.Kill();               // 중복 방지
        _sprite.localScale = _baseScale; // 기본값으로 리셋(안전)
        _landSeq = DOTween.Sequence()
            .Append(_sprite.DOScaleY(0.80f * _baseScale.y, 0.05f))   // 순간 스쿼시
            .Append(_sprite.DOScaleY(1.00f * _baseScale.y, 0.05f)
                .SetEase(Ease.OutQuad));                             // 빠르게 복귀
    }
}


