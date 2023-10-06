using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoom : MonoBehaviour
{
    #region Variables

    private Room currentRoom;

    #endregion

    #region Methods

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            // If was in a previous room, leave it!
            if (currentRoom != null && (currentRoom.GetRoomID() != other.GetComponent<Room>().GetRoomID()))
            {
                currentRoom.PlayerToggle(false);
            }

            currentRoom = other.GetComponent<Room>();
            currentRoom.PlayerToggle(true);
        }
    }

    #endregion
}
