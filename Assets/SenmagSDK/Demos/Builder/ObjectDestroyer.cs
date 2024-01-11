using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		if (transform.parent.GetComponentInChildren<Senmag_radialMenu>() != null)
		{
			if (transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject != null)
			{
				Destroy(transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject);
			}
			else UnityEngine.Debug.Log("No target object");
		}
		else
		{
			UnityEngine.Debug.Log("No radiam menu found in parent");
		}
		Destroy(this.gameObject);
	}
}
