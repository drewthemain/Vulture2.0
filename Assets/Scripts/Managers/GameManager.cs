using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables

    // Reference to the singleton instance
    public static GameManager instance;

    // Reference to the player transform
    private Transform _playerTransform;

    // Is the game currently paused?
    private bool _isPaused = false;

    public bool _isLowGrav = false;

    private int _brokenCount = 0;

    public delegate void GravChange();
    public static event GravChange OnLowGrav;

    #endregion

    #region Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Initial player grab (can be changed later for a more logical approach)
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (_playerTransform == null)
            {
                Debug.LogWarning("Missing a player in the scene!");
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ToggleGravity(bool toLow)
    {
        if (toLow)
        {
            _brokenCount++;

            if (_brokenCount == 1)
            {
                _isLowGrav = true;
                OnLowGrav?.Invoke();
            }
        }
        else
        {
            _brokenCount--;

            if (_brokenCount <= 0)
            {
                _isLowGrav = false;
                OnLowGrav?.Invoke();
            }
        }
    }

    /// <summary>
    /// Getter for player transform reference
    /// </summary>
    /// <returns>Player transform</returns>
    public Transform GetPlayerReference()
    {
        return _playerTransform;
    }

    /// <summary>
    /// Handles the losing of the game
    /// </summary>
    public void LoseGame()
    {
        Pause();
        UIManager.instance.ToggleOnScreenUI(UIManager.UIType.End);
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Handles game aspects of pausing (timescale, cursor, etc.)
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        _playerTransform.GetComponent<PlayerController>().DisableCamera();
    }

    /// <summary>
    /// Handles game aspects of unpausing (timescale, cursor, etc.)
    /// </summary>
    public void Unpause()
    {
        _isPaused = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        _playerTransform.GetComponent<PlayerController>().EnableCamera();
    }

    #endregion
}
