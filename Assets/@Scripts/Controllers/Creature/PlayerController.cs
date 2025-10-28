    using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : BaseController
{

    bool _isHack = false;

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
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if(other.CompareTag("Obstacle"))
            {
                //Managers.Game.GameOver();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }


        }

      
    }
}
