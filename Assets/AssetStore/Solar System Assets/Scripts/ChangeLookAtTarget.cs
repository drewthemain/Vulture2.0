using UnityEngine;
using System.Collections;

public class ChangeLookAtTarget : MonoBehaviour {

    [Tooltip("This is the object the camera will look at when this object is clicked, defaults to the object this script is attached to")]
	public GameObject target; // the target that the camera should look at

    // Start is a Unity Specific function that runs when the script is created in the actual game, so on startup, or when loading into a scene
    // or when an object with a script on it is created while the game is playing, etc.
    //
    // This start script handles the defaulting of the target variable if it has not been set
	void Start() {
		if (target == null) 
		{
			target = this.gameObject;
			Debug.Log ("ChangeLookAtTarget target not specified. Defaulting to parent GameObject");
		}
	}

	// Called when MouseDown on this gameObject (The game object is clicked on in the game)
	void OnMouseDown () {
        // change the target of the LookAtTarget script to be this gameobject or whatever the target was set to.
        LookAtTarget cmp = Camera.main.GetComponent<LookAtTarget>();
        cmp.target = target;
        Camera.main.fieldOfView = 60;
	}
}
