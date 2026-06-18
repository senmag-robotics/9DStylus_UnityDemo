using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Senmag_toolIcon : MonoBehaviour
{

    private Vector3 relativePos;


    // Start is called before the first frame update
    void Start()
    {
        relativePos = transform.position - gameObject.GetComponentInParent<Transform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = gameObject.GetComponentInParent<Transform>().position + relativePos;
		this.transform.LookAt(Camera.main.transform.position);
	}
}
