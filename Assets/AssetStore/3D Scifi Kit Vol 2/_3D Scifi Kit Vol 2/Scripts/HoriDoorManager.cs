using UnityEngine;
using System.Collections;

public class HoriDoorManager : MonoBehaviour {

	public DoorHori door1;
	public DoorHori door2;

	public bool isClosed = true;

    private void Awake()
    {
		door1.manager = this;
		door2.manager = this;
    }

    void OnTriggerEnter(){

		if (isClosed) {
			isClosed = false;

			if (door1 != null)
			{
				door1.OpenDoor();
			}

			if (door2 != null)
			{
				door2.OpenDoor();
			}
		}
		else
        {
			if (door1 != null)
			{
				door1.EndOpen();
			}

			if (door2 != null && isClosed)
			{
				door2.EndOpen();
			}
		}

	}
}
