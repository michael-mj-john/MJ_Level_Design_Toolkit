using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnDestroy : MonoBehaviour {

    public string playOnDestroy;

    private void OnDestroy()
    {
        AudioManager.PlaySound(playOnDestroy);
    }
  
}
