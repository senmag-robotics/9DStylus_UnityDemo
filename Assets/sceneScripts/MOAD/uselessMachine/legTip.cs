using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class legTip : MonoBehaviour
{
	public bool tipCollided;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnCollisionEnter(Collision collision)
	{
		UnityEngine.Debug.Log("leg collided");
		if (collision.gameObject.name == "Big table") tipCollided = true;
	}
	private void OnCollisionStay(Collision collision)
	{
		UnityEngine.Debug.Log("leg collided");
		if (collision.gameObject.name == "Big table") tipCollided = true;
	}
	private void OnCollisionExit(Collision collision)
	{
		tipCollided = false;
	}
}
