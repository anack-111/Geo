using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FollowPlayer : MonoBehaviour
{
    public Vector2 cameraOffset;

    public float interpolationTime = 0.1f;

    public Transform _player;
    public Transform _groundCamera;
    public Transform _topGround;
    public Transform _backGround;
    public Transform _bottomGround;

    [Header("Background Layer 설정")]
    public Transform _background;   // 구름 앞 배경



    public float background = 0.5f;      // 구름 배경 이동 속도 (느림)


    public Movement _movement;
    Vector3 newVector;
    bool firstFrame = true;

    public SpriteRenderer _globalSprite;
    public GameObject _backgroundLight;

    public void Init()
    {
        _player = Managers.Object.Player.gameObject.transform;
        _movement = Managers.Object.Player.GetComponent<Movement>();
    }

    void FixedUpdate()
    {
        newVector = new Vector3(_player.position.x + cameraOffset.x, newVector.y + _tiltYOffset, -10);

        //if (Define.SCREEN_HEIGHT_VALUES[(int)_movement._currentGameMode] > 10)
        //    FreeCam(firstFrame);
        //else
        StaticCam(firstFrame, _movement._yLastPortal, Define.SCREEN_HEIGHT_VALUES[(int)_movement._currentGameMode]);

        //// 배경을 각각 다르게 이동
        MoveBackground(_background, background);
        transform.position = newVector;
        firstFrame = false;
    }

    void MoveBackground(Transform background, float speed)
    {
        // 배경을 특정 속도로 이동시킴
        background.localPosition = new Vector3(background.localPosition.x - speed * Time.deltaTime, background.localPosition.y, background.localPosition.z);

        // 배경이 화면 밖으로 나가면 반복되도록 처리
        if (background.localPosition.x < -background.GetComponent<Renderer>().bounds.size.x)
        {
            background.localPosition = new Vector3(0, background.localPosition.y, background.localPosition.z);
        }
    }

    void FreeCam(bool doInstantly)
    {

        _groundCamera.position = InterpolateVec3(new Vector3(0, _groundCamera.position.y), Vector3.up * cameraOffset, 20) + Vector3.right * (Mathf.Floor(_player.transform.position.x / 5) * 5);

        if (Vector2.Distance(_topGround.localPosition, Vector3.up * 20f) < 0.3f)
            _topGround.gameObject.SetActive(false);

        if (_topGround)
            _topGround.localPosition = InterpolateVec3(_topGround.localPosition, Vector3.up * 20f, 100);

        if (!doInstantly)
            newVector += Vector3.up * (Mathf.Lerp(_player.transform.position.y + 1.7f - newVector.y, 2.4f - newVector.y, (_player.position.y <= 4.2f) ? 1 : 0)) * Time.deltaTime / interpolationTime;
        else
            newVector += Vector3.up * (_player.transform.position.y + 1.7f);
    }

    Vector3 InterpolateVec3(Vector3 current, Vector3 target, float speed)
    {
        float maxDelta = speed * Time.deltaTime;
        return Vector3.MoveTowards(current, target, maxDelta);

        //return current + Vector3.Normalize(target - current) * Time.deltaTime * speed * Mathf.Clamp(Vector3.Distance(current, target), 0, 1);
    }

    void StaticCam(bool doInstantly, float yLastPortal, int screenHeight)
    {

        _groundCamera.position = InterpolateVec3(new Vector3(0, _groundCamera.position.y), Vector3.up * Mathf.Clamp(yLastPortal - screenHeight * 0.5f, cameraOffset.y, float.MaxValue), 20) + Vector3.right * (Mathf.Floor(_player.transform.position.x / 5) * 5);

        if (!doInstantly)
            newVector += Vector3.up * (5 + Mathf.Clamp(yLastPortal - screenHeight * 0.5f, cameraOffset.y, 2048) - newVector.y - ((11 - screenHeight) * 0.5f)) * Time.deltaTime / interpolationTime;
        else
            newVector += Vector3.up * (5 + Mathf.Clamp(yLastPortal - screenHeight * 0.5f, cameraOffset.y, 2048) - ((11 - screenHeight) * 0.5f));
    }



    [SerializeField] private float _shockWaveTime = 0.75f;

    private Coroutine _shockWaveCoroutine;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    public Material _mtrl;


    public void CallShockWave()
    {
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        _backgroundLight.SetActive(true);

        _mtrl.SetFloat(_waveDistanceFromCenter, startPos);

        float lerpedAmount = 0f;

        float elapsedTime = 0f;

        while (elapsedTime < _shockWaveTime)
        {
            elapsedTime += Time.deltaTime;

            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / _shockWaveTime));
            _mtrl.SetFloat(_waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
        //  _backgroundLight.SetActive(false);

    }


    public void HandleBackgroundSizeChange(float targetSize)
    {
        // Adjust background based on camera size
        float targetTop = targetSize >= 7f ? 11.5f : 11f;
        float targetBottom = targetSize >= 7f ? -2.5f : -1f;

        // Smooth background transition based on camera size change
        _topGround.localPosition = Vector3.Lerp(_topGround.localPosition, new Vector3(_topGround.localPosition.x, targetTop, _topGround.localPosition.z), 0.1f);
        _bottomGround.localPosition = Vector3.Lerp(_bottomGround.localPosition, new Vector3(_bottomGround.localPosition.x, targetBottom, _bottomGround.localPosition.z), 0.1f);
    }


    // ▼ Gravity 틸트용
    [Header("Gravity Tilt")]
    [SerializeField] float _tiltAngle = 2f;     // 기울기 크기(+/-Z)
    [SerializeField] float _tiltUp = 0.08f;     // 기울이는 시간
    [SerializeField] float _tiltReturn = 0.18f; // 복귀 시간
    [SerializeField] float _pressDistance = 0.1f; // 눌림 거리(중력 1=아래, -1=위로)

    Sequence _tiltSeq;
    float _tiltYOffset = 0f; // ← Y 오프셋(트윈으로 관리)

    public void DoGravityTilt(int gravitySign)
    {
        // 회전 피크
        float zPeak = (gravitySign == 1) ? -_tiltAngle : +_tiltAngle;

        // 눌림 방향: 중력 1(바닥) => 아래(-1), 중력 -1(천장) => 위(+1)
        float pressDir = (gravitySign == 1) ? 1f : -1f;
        float pressTarget = _pressDistance * pressDir;

        _tiltSeq?.Kill();

        // 시퀀스: (회전 + Y오프셋) 동시에 올렸다가, 제로로 복귀
        _tiltSeq = DOTween.Sequence()
            // 올라가기(기울이기 + 눌림)
            .Append(
                DOTween.Sequence()
                    .Join(transform.DOLocalRotate(new Vector3(0f, 0f, zPeak), _tiltUp).SetEase(Ease.OutQuad))
                    .Join(DOTween.To(() => _tiltYOffset, v => _tiltYOffset = v, pressTarget, _tiltUp).SetEase(Ease.OutQuad))
            )
            // 돌아오기(원위치)
            .Append(
                DOTween.Sequence()
                    .Join(transform.DOLocalRotate(Vector3.zero, _tiltReturn).SetEase(Ease.InOutQuad))
                    .Join(DOTween.To(() => _tiltYOffset, v => _tiltYOffset = v, 0f, _tiltReturn).SetEase(Ease.InOutQuad))
            );
    }


}
