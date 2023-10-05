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

    // Is the game currently in low grav?
    public bool _isLowGrav = false;

    // The number of broken windows
    private List<Window> _brokenWindowPool = new List<Window>();

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

    /// <summary>
    /// Toggles gravity for all gravity-affected objects
    /// </summary>
    /// <param name="toLow">Move to low gravity?</param>
    public void ToggleGravity(bool toLow, Window window)
    {
        if (toLow)
        {
            if (window)
            {
                _brokenWindowPool.Add(window);

                if (_brokenWindowPool.Count == 1 && !_isLowGrav)
                {
                    _isLowGrav = true;
                    OnLowGrav?.Invoke();
                }

                return;
            }

            _isLowGrav = true;
            OnLowGrav?.Invoke();
        }
        else
        {
            if (window)
            {
                _brokenWindowPool.Remove(window);
            }
        }
    }

    public void ResetGravity()
    {
        foreach (Window window in _brokenWindowPool)
        {
            window.ForceClose();
        }

        _brokenWindowPool.Clear();

        _isLowGrav = false;
        OnLowGrav?.Invoke();
    }

    public Window GetPullingWindow()
    {
        return _brokenWindowPool.Count > 0 ? _brokenWindowPool[0] : null;
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
