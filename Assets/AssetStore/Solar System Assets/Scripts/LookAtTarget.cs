using UnityEngine;
using System.Collections;

public class LookAtTarget : MonoBehaviour {

	public GameObject target; // the target that the camera should look at

	void Start () {
		if (target == null) 
		{
            target = this.gameObject;
			Debug.Log ("LookAtTarget target not specified. Defaulting to parent GameObject");
		}
	}
	
	// Update is called once per frame
    // For clarity, Update happens constantly as your game is running
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray);
            if (hits.Length>0)
            {
                RaycastHit hit = hits[0];
                ChangeLookAtTarget cp = hit.collider.gameObject.GetComponent<ChangeLookAtTarget>();

                if (cp!=null)
                {
                    target = hit.collider.gameObject;
                }
            }
        }

        // This is an if statement. Code inside of it will only be run/executed if the if condition is met.
        // In this case the if statement is checking to see if target has been set to something, if it has, then the code inside of it runs
        if (target)
        {
            // transform here refers to the attached gameobject this script is on.
            // the LookAt function makes a transform point it's Z axis towards another point in space
            // In this case it is pointing towards the target.transform
            transform.LookAt(target.transform);
        }
    }
}
