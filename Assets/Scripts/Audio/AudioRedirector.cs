using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRedirector : MonoBehaviour

{
    public void AudioRedirect(string wwiseEventName)
    {
        AudioManager.instance.AnimEventPostWwiseEvent(wwiseEventName, this.gameObject);
    }
}
