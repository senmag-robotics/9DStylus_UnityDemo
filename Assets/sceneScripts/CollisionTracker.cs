using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTracker : MonoBehaviour
{
    public GameObject collidedObject;
    public bool colliding = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log("collided!");
        colliding = true;
        collidedObject = collision.other.gameObject;
    }
    void OnCollisionStay(Collision collision)
    {
        collidedObject = collision.other.gameObject;
    }
    void OnCollisionExit(Collision collision)
    {
        UnityEngine.Debug.Log("stopped colliding!");
        collidedObject = null;
        colliding = false;
    }

    void OnTriggerEnter(Collider other)
    {
        colliding = true;
        collidedObject = other.gameObject;
    }
    void OnTriggerStay(Collider other)
    {
        collidedObject = other.gameObject;
    }
    void OnTriggerExit(Collider other)
    {
        collidedObject = null;
        colliding = false;
    }
}
