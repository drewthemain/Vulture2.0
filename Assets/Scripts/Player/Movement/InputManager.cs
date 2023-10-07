using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance = null;

    private PlayerInput input;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }
        input = new PlayerInput();
    }

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return input.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return input.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerIsJumping()
    {
        if (Time.timeScale == 0) return false;
        float inputVal = input.Player.Jump.ReadValue<float>();
        if (inputVal == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool PlayerIsSprinting()
    {
        if (Time.timeScale == 0) return false;
        float inputVal = input.Player.Sprint.ReadValue<float>();
        if (inputVal == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool PlayerIsCrouching()
    {
        if (Time.timeScale == 0) return false;
        float inputVal = input.Player.Crouch.ReadValue<float>();
        if (inputVal == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool PlayerStartedSprinting()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.Sprint.triggered && input.Player.Sprint.ReadValue<float>() > 0;
    }

    public bool PlayerStartedCrouching()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.Crouch.triggered && input.Player.Crouch.ReadValue<float>() > 0;
    }

    public bool PlayerStartedJumping()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.Jump.triggered && input.Player.Jump.ReadValue<float>() > 0;
    }

    public bool PlayerIsFiring()
    {
        if (Time.timeScale == 0) return false;
        float inputVal = input.Player.Fire.ReadValue<float>();
        if (inputVal == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool PlayerStartedFiring()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.Fire.triggered && input.Player.Fire.ReadValue<float>() > 0;
    }

    public bool PlayerStartedReloading()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.Reload.triggered && input.Player.Reload.ReadValue<float>() > 0;
    }

    public bool PlayerIsHoldingForward()
    {
        if (GetPlayerMovement().y > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayerIsHoldingLeft()
    {
        if (GetPlayerMovement().x < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayerIsHoldingRight()
    {
        if (GetPlayerMovement().x > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayerPressedEscape()
    {
        return input.Player.Pause.triggered && input.Player.Pause.ReadValue<float>() > 0;
    }

    public bool PlayerIsAiming()
    {
        if (Time.timeScale == 0) return false;
        float inputVal = input.Player.ADS.ReadValue<float>();
        if (inputVal == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool PlayerStartedAiming()
    {
        if (Time.timeScale == 0) return false;
        return input.Player.ADS.triggered && input.Player.ADS.ReadValue<float>() > 0;
    }
}
