using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFloor : MonoBehaviour
{
    [Tooltip("Point for the player to teleport to when touching the killbox collider")]
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.GetPlayerReference().position = respawnPoint.position;
    }
}
