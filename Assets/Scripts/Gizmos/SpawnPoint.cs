using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    public Mesh targetObject; // set to arrow object in prefab
    private Vector3 arrowScale = new Vector3(0.6f, 0.8f, 0.5f); //because source object is pretty big
    public Color arrowColor = new Color(0, 0, 1, 0.5f);
    public Color arrowColorSelected = new Color(0, 0, 1, 0.8f);

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = arrowColor;
        Gizmos.DrawMesh(targetObject, transform.position, transform.rotation, arrowScale);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = arrowColorSelected;
        Gizmos.DrawMesh(targetObject, transform.position, transform.rotation, arrowScale);
    }

}
