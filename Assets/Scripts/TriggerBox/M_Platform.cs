using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M_Platform : MonoBehaviour {

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy"))
            collision.transform.parent = transform;
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy"))
            collision.transform.parent = null;
    }
}
