using UnityEngine;
using static Define;

public class Portal : MonoBehaviour
{
    public EGameMode _gameMode;   // ← 인스펙터에서 Center로 설정하면 센터 포털로 동작
    public ESpeed _speed;
    public bool _gravity;         // true=+1, false=-1  (센터 외 포털에서만 사용)
    public int _state;            // 기존 state 로직 유지

    // (충돌 방식 그대로 쓰는 버전)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var movement = collision.gameObject.GetComponent<Movement>();
        if (movement == null) return;

     
         //  일반 포털: 기존처럼 모드/속도/중력/state 전달
         movement.ChangeThroughPortal(_gameMode, _speed, _gravity ? 1 : -1, _state, transform.position.y);
        
    }
}

