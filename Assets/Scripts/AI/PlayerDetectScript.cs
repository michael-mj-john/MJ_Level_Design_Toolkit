using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetectScript : MonoBehaviour {

    public AIController aiController;
   

    void OnTriggerEnter(Collider col)
    {
        aiController.PlayerDetectTriggerEnter(col);
    }

    private void OnTriggerStay(Collider col)
    {
        aiController.PlayerDetectTriggerStay(col);
    }

    private void OnTriggerExit(Collider col)
    {
        aiController.PlayerDetectTriggerExit(col);
    }

    public void setRadius(float radius)
    {
        GetComponent<SphereCollider>().radius = radius;
    }
}
