using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLocker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(transform.parent.GetComponentInChildren<Senmag_radialMenu>() != null)
		{
			if(transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject != null)
			{
				if(transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponent<Rigidbody>() != null){
					if(transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponent<Rigidbody>().isKinematic == true) transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponent<Rigidbody>().isKinematic = false;
					else transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponent<Rigidbody>().isKinematic = true;
				}
				else if (transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponentInChildren<Rigidbody>() != null)
				{
					if (transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponentInChildren<Rigidbody>().isKinematic == true) transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponentInChildren<Rigidbody>().isKinematic = false;
					else transform.parent.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject.GetComponentInChildren<Rigidbody>().isKinematic = true;
				}
				else
				{
					UnityEngine.Debug.Log("No rigidBody found on target");
				}
			}
			else UnityEngine.Debug.Log("No target object");
		}
		else{
			UnityEngine.Debug.Log("No radiam menu found in parent");
		}
		Destroy(this.gameObject);
    }
}
