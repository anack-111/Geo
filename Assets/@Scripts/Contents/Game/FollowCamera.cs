using System.Collections;
using Unity.VisualScripting;
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

    [Header("Background Layer 설정")]
    public Transform _background;   // 구름 앞 배경
    public Transform _img1;   // 구름 앞 배경
    public Transform _img2;   // 구름 뒤 배경


    public float background = 0.5f;      // 구름 배경 이동 속도 (느림)
    public float _img1Speed = 0.5f;      // 구름 배경 이동 속도 (느림)
    public float _img2Speed = 0.5f;      // 구름 배경 이동 속도 (느림)


    public Movement _movement;
    Vector3 newVector;
    bool firstFrame = true;

    public SpriteRenderer _globalSprite;
    public Light2D _globalLight;
    public GameObject _backgroundLight;

    public void Init()
    {
        _player = Managers.Object.Player.gameObject.transform;
        _movement = Managers.Object.Player.GetComponent<Movement>();
    }

    void FixedUpdate()
    {
        newVector = new Vector3(_player.position.x + cameraOffset.x, newVector.y, -10);

        if (Define.SCREEN_HEIGHT_VALUES[(int)_movement._currentGameMode] > 10)
            FreeCam(firstFrame);
        else
            StaticCam(firstFrame, _movement._yLastPortal, Define.SCREEN_HEIGHT_VALUES[(int)_movement._currentGameMode]);

        //// 배경을 각각 다르게 이동
        MoveBackground(_background, background);
        MoveBackground(_img1, _img1Speed);
        MoveBackground(_img2, _img2Speed);

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
            newVector += Vector3.up * (Mathf.Lerp(_player.transform.position.y + 1.7f - newVector.y, -0.6f - newVector.y, (_player.position.y <= 4.2f) ? 1 : 0)) * Time.deltaTime / interpolationTime;
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

        _topGround.gameObject.SetActive(true);

        _groundCamera.position = InterpolateVec3(new Vector3(0, _groundCamera.position.y), Vector3.up * Mathf.Clamp(yLastPortal - screenHeight * 0.5f, cameraOffset.y, float.MaxValue), 20) + Vector3.right * (Mathf.Floor(_player.transform.position.x / 5) * 5);
        _topGround.localPosition = InterpolateVec3(_topGround.localPosition, Vector3.up * (4.5f + screenHeight), 30);

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
        _globalLight.gameObject.SetActive(true);
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
        _globalLight.gameObject.SetActive(false);
        _backgroundLight.SetActive(false);

    }

}
