using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DavidFDev.DevConsole;

public class GameManager : MonoBehaviour
{
    #region Variables

    // Reference to the singleton instance
    public static GameManager instance;

    // Reference to the player transform
    private Transform playerTransform;

    // Is the game currently paused?
    private bool isPaused = false;

    // Is the game currently in low grav?
    public bool isLowGrav = false;

    // The number of broken windows
    private List<Window> brokenWindowPool = new List<Window>();

    public delegate void GravChange();
    public static event GravChange OnLowGrav;

    public delegate void ClearAll();
    public static event ClearAll OnClear;

    #endregion

    #region Methods

    private void OnEnable()
    {
        DevConsole.OnConsoleOpened += ConsolePause;
    }

    private void OnDisable()
    {
        DevConsole.OnConsoleOpened -= ConsolePause;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Initial player grab (can be changed later for a more logical approach)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (playerTransform == null)
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
                brokenWindowPool.Add(window);

                if (brokenWindowPool.Count == 1 && !isLowGrav)
                {
                    isLowGrav = true;
                    OnLowGrav?.Invoke();
                }

                return;
            }

            isLowGrav = true;
            OnLowGrav?.Invoke();
        }
        else
        {
            if (window)
            {
                brokenWindowPool.Remove(window);
            }
        }
    }

    public void ResetGravity()
    {
        foreach (Window window in brokenWindowPool)
        {
            window.ForceClose();
        }

        brokenWindowPool.Clear();

        isLowGrav = false;
        OnLowGrav?.Invoke();
    }

    public Window GetPullingWindow()
    {
        return brokenWindowPool.Count > 0 ? brokenWindowPool[0] : null;
    }

    /// <summary>
    /// Getter for player transform reference
    /// </summary>
    /// <returns>Player transform</returns>
    public Transform GetPlayerReference()
    {
        return playerTransform;
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
        isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        playerTransform.GetComponent<PlayerController>().DisableCamera();
    }

    /// <summary>
    /// Handles game aspects of unpausing (timescale, cursor, etc.)
    /// </summary>
    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        playerTransform.GetComponent<PlayerController>().EnableCamera();
    }

    #region Dev Console

    public void ConsolePause()
    {
        if (!isPaused)
        {
            UIManager.instance.PauseGame();
        }
    }

    [DevConsoleCommand(
      name: "EndRound",
      aliases: "end",
      helpText: "Ends the current round"
    )]
    private static void EndRound()
    {
        Debug.Log("Force Round Quit...".Bold().Color("purple"));
        RoundManager.instance.ForceQuit();

        OnClear?.Invoke();

        DevConsole.CloseConsole();
    }

    [DevConsoleCommand(
      name: "NextRound",
      aliases: "nextround",
      helpText: "Sets the next round",
      parameterHelpText: new string[2] {"The index of the next round", 
          "Whether to force end the current round"}
    )]
    private static void NextRound(int nextIndex, bool endCurrent = false)
    {
        string nextRound = RoundManager.instance.SetOverride(nextIndex);
        Debug.Log($"Next round will be ... {nextRound}".Bold().Color("purple"));

        if (endCurrent)
        {
            EndRound();
        }

        DevConsole.CloseConsole();
    }

    [DevConsoleCommand(
      name: "NextEvent",
      aliases: "nextevent",
      helpText: "Sets the next event",
      parameterHelpText: new string[2] {"The index of the next event",
              "Whether to force end the current round"}
    )]
    private static void NextEvent(int nextIndex, bool endCurrent = false)
    {
        string nextEvent = EventManager.instance.SetOverride(nextIndex);
        Debug.Log($"Next event will be ... {nextEvent}".Bold().Color("purple"));

        if (endCurrent)
        {
            EndRound();
        }

        DevConsole.CloseConsole();
    }

    [DevConsoleCommand(
      name: "GodMode",
      aliases: "god",
      helpText: "Sets god mode for the player"
    )]
    private static void GodMode()
    {
        Debug.Log("ENTERING GOD MODE".Bold().Color("purple"));

        GameManager.instance.playerTransform.GetComponent<PlayerHealth>().godMode = true;

        DevConsole.CloseConsole();
    }

    #endregion

    #endregion
}
