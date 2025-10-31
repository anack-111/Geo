using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Vector2 cameraOffset;

    public float interpolationTime = 0.1f;

    public Transform _player;
    public Transform _groundCamera;
    public Transform _topGround;
    public Transform _backGround;

    [Header("Background Layer ����")]
    public Transform _background;   // ���� �� ���
    public Transform _cloudFrontBackground;   // ���� �� ���
    public Transform _cloudBackBackground;   // ���� �� ���
    public Transform _moonBackground;    // �� ���
    public Transform _buildingBackground; // �� �ǹ� ���
    public Transform _buildingFrontBackground; // �� �ǹ� ���
    public Transform _BuildingSecondBackground; // �� �ǹ� ���
    public Transform _fogBackground; // �� �ǹ� ���

    public float background = 0.5f;      // ���� ��� �̵� �ӵ� (����)
    public float frontcloudSpeed = 0.5f;      // ���� ��� �̵� �ӵ� (����)
    public float backcloudSpeed = 0.5f;      // ���� ��� �̵� �ӵ� (����)
    public float moonSpeed = 1.0f;       // �� ��� �̵� �ӵ� (�߰�)
    public float backbuildingSpeed = 2.0f;   // �ǹ� ��� �̵� �ӵ� (����)
    public float secondBuildingSpeed = 2.0f;
    public float frontbuildingSpeed = 2.0f;   // �ǹ� ��� �̵� �ӵ� (����)
    public float fogSpeed = 1.0f;       // �� ��� �̵� �ӵ� (�߰�)

    public Movement _movement;
    Vector3 newVector;
    bool firstFrame = true;

    public SpriteRenderer _globalSprite;

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

        // ����� ���� �ٸ��� �̵�
        MoveBackground(_background, background);
        MoveBackground(_cloudBackBackground, backcloudSpeed);
        MoveBackground(_cloudFrontBackground, frontcloudSpeed);
        MoveBackground(_moonBackground, moonSpeed);
        MoveBackground(_buildingBackground, backbuildingSpeed);
        MoveBackground(_BuildingSecondBackground, secondBuildingSpeed);
        MoveBackground(_buildingFrontBackground, frontbuildingSpeed);
        MoveBackground(_fogBackground, fogSpeed);


        transform.position = newVector;
        firstFrame = false;
    }

    void MoveBackground(Transform background, float speed)
    {
        // ����� Ư�� �ӵ��� �̵���Ŵ
        background.localPosition = new Vector3(background.localPosition.x - speed * Time.deltaTime, background.localPosition.y, background.localPosition.z);

        // ����� ȭ�� ������ ������ �ݺ��ǵ��� ó��
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
        return current + Vector3.Normalize(target - current) * Time.deltaTime * speed * Mathf.Clamp(Vector3.Distance(current, target), 0, 1);
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



}
