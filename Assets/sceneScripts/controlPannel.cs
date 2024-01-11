using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SenmagHaptic
{
	public class controlPannel : MonoBehaviour
	{
		GameObject panelBase;
		GameObject buttonText;
		GameObject []buttons = new GameObject[5];
		int allReleased = 0;

		GameObject mantisWorkspace;
		// Start is called before the first frame update
		void Start()
		{

			panelBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
			panelBase.name = "panel base";
			panelBase.transform.parent = transform;
			panelBase.transform.localPosition = new Vector3(2.5f, 0, .5f);
			panelBase.transform.localScale = new Vector3(6, 1, 2);
			panelBase.AddComponent<Rigidbody>();
			panelBase.GetComponent<Rigidbody>().isKinematic = true;
			panelBase.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));

			buttonText = new GameObject();

			//buttonText.AddComponent<Transform>();
			buttonText.AddComponent<MeshRenderer>();
			buttonText.AddComponent<TextMesh>();
			buttonText.GetComponent<TextMesh>().text = "Scene Loader";
			buttonText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
			buttonText.transform.parent = panelBase.transform;

			buttonText.transform.localScale = new Vector3(.1f, .2f, .2f);
			buttonText.transform.localPosition = new Vector3(0, 0, 0.2f);
			buttonText.transform.Rotate(new Vector3(90, 0, 0));

			buttonText.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));

			for (int x = 0; x < 5; x++)
			{
				buttons[x] = GameObject.CreatePrimitive(PrimitiveType.Cube);
				buttons[x].name = "button" + x;
				buttons[x].transform.parent = transform;
				buttons[x].transform.localScale = new Vector3(.8f, .8f, .8f);
				buttons[x].transform.localPosition = new Vector3(1.2f * x, 0.2f, 0);
				buttons[x].GetComponent<MeshRenderer>().enabled = false;
				buttons[x].AddComponent<button>();
				buttons[x].GetComponent<button>().scaleX = 1;
				buttons[x].GetComponent<button>().scaleY = 1;
				buttons[x].GetComponent<button>().scaleZ = 1;
				buttons[x].GetComponent<button>().ButtonText = ""+x;
			}

		}

		// Update is called once per frame
		void Update()
		{
			int count = 0;
			for(int x = 0; x < 5; x++)
			{
				if (buttons[x].GetComponent<button>().checkClicked() == 1)
				{

					
						mantisWorkspace = GameObject.Find("/Workspace");
						//mantisWorkspace.GetComponent<MantisWorkspace>().sendForce(1);

						SceneManager.LoadScene("test" + x, LoadSceneMode.Single);
						Debug.Log("Loading Scene: test" + x);
					
				}
				else count++;
			}
			if(count == 4)
			{
				allReleased++;
			}
		}
		

	}
}