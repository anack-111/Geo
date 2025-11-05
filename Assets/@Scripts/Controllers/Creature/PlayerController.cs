    using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : BaseController
{

    public bool _isHack = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            _isHack = true;
        
        if (Input.GetKeyDown(KeyCode.D))
            _isHack = false;
        


    }
    void Awake()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!_isHack)
        {
            if (other.TryGetComponent<EdgeCollider2D>(out var edge))
            {
                //Managers.Game.GameOver();
                Managers.Scene.LoadScene(Define.EScene.GameScene);
            }

            if(other.CompareTag("Obstacle"))
            {
                //Managers.Game.GameOver();
                Managers.Scene.LoadScene(Define.EScene.GameScene);
            }


        }
    }

    #region UI Test 


    public void ToggleGod()
    {
        _isHack = !_isHack;
    }


    #endregion

}
