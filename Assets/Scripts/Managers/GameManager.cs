using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Didn't have time to make this into a game I guess smh
// Reset scene from a button
public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;
    private void Awake()
    {
        if (instance)
        {
            Debug.Log("Warning! More than one instance of GameManager found!");
            Destroy(gameObject);
            return;
        }
        //DoNotDestroyOnLoad(this);
        instance = this;
    }
    #endregion


    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name.ToString());
    }
}
