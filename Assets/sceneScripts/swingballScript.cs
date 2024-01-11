using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swingballScript : MonoBehaviour
{
	// Start is called before the first frame update
	LineRenderer line;
	void Start()
    {
		line = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

		Vector3 position = transform.parent.transform.position + this.GetComponent<SpringJoint>().connectedAnchor;
		//Debug.Log(this.transform.position);
		line.enabled = true;
		line.SetWidth(0.05f, 0.05f);
		line.positionCount = 2;
		line.SetPosition(0, position);
		line.SetPosition(1, transform.position);
		line.startColor = new Color(0.5f, 0.5f, 0.5f);
		line.endColor = new Color(1, (94f / 255f), (31f / 255f));

		//line.SetColors(Color.white, Color.white);
		Material whiteDiffuseMat = new Material(Shader.Find("Unlit/Texture"));
		line.material = whiteDiffuseMat;

		//Debug.DrawLine(position, this.transform.position, Color.white, 0.1f, true);
	}
}
