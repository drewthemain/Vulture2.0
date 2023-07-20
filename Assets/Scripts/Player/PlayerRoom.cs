using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoom : MonoBehaviour
{
    #region Variables

    private Room _currentRoom;

    #endregion

    #region Methods

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            // If was in a previous room, leave it!
            if (_currentRoom != null && (_currentRoom.GetRoomID() != other.GetComponent<Room>().GetRoomID()))
            {
                _currentRoom.PlayerToggle(false);
            }

            _currentRoom = other.GetComponent<Room>();
            _currentRoom.PlayerToggle(true);
        }
    }

    #endregion
}
