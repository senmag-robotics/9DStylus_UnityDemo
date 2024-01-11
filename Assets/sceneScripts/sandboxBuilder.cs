using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{

	public class sandboxBuilder : MonoBehaviour
	{
		public List<GameObject>		palleteObjects;
		public List<GameObject>		tools;

		private GameObject			butonToolbox;
		private GameObject			buttonItemPallete;
		private GameObject			buttonScrollLeft;
		private GameObject			buttonScrollRight;
		private GameObject			buttonCancel;
		private GameObject			buttonCancelText;
		private GameObject			panelBase;

		private List<GameObject>	toolIcons = new List<GameObject>();
		private List<GameObject>	palleteIcons = new List<GameObject>();

		private int					menuPage;
		private int					menuScroll;
		private int					minScroll;
		private int					maxScroll;


		// Start is called before the first frame update
		void Start()
		{
			menuPage = 0;
			menuScroll = 0;
			minScroll = 0;
			
			

			panelBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
			panelBase.name = "base";
			panelBase.transform.parent = transform;
			panelBase.transform.localScale = new Vector3(16, 1, 8);
			panelBase.transform.localPosition = new Vector3(5, -.4f, -2.5f);
			panelBase.AddComponent<Rigidbody>();
			panelBase.GetComponent<Rigidbody>().isKinematic = false;
			panelBase.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
			panelBase.GetComponent<Renderer>().material.SetColor("_Color",new Color(.5f, .5f, .5f));


			buttonCancel = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonCancel.name = "buttonCancel";
			buttonCancel.transform.parent = transform;
			buttonCancel.transform.localScale = new Vector3(3, 1.5f, 1.5f);
			buttonCancel.transform.localPosition = new Vector3(5, .9f, 0f);
			buttonCancel.AddComponent<Rigidbody>();
			buttonCancel.GetComponent<Rigidbody>().isKinematic = false;
			buttonCancel.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
			buttonCancel.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, (94f / 255f), (31f/255f)));
			buttonCancel.AddComponent<CursorSpawner>();

			buttonCancelText = new GameObject();
			buttonCancelText.AddComponent<TextMesh>();
			buttonCancelText.GetComponent<TextMesh>().text = "Cancel";
			buttonCancelText.GetComponent<TextMesh>().characterSize = 1f;
			buttonCancelText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
			buttonCancelText.transform.parent = buttonCancel.transform;
			
			//buttonText.transform.localScale = new Vector3(.8f, textSize, .8f);
			buttonCancelText.transform.localScale = new Vector3(0.2f, 0.5f, 0.5f);
			buttonCancelText.transform.localPosition = new Vector3(0, 0.5f, 0);
			buttonCancelText.transform.Rotate(new Vector3(90, 0, 0));

			buttonCancelText.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0, 0));

			butonToolbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonItemPallete = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonScrollLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonScrollRight = GameObject.CreatePrimitive(PrimitiveType.Cube);

			butonToolbox.name = "ButtonToolbox";
			butonToolbox.transform.parent = transform;
			butonToolbox.transform.localScale = new Vector3(.8f, .8f, .8f);
			butonToolbox.transform.localPosition = new Vector3(0, 0, 0);
			butonToolbox.GetComponent<MeshRenderer>().enabled = false;
			butonToolbox.AddComponent<button>();
			butonToolbox.GetComponent<button>().scaleX = 5;
			butonToolbox.GetComponent<button>().scaleY = 1.5f;
			butonToolbox.GetComponent<button>().scaleZ = 2;
			butonToolbox.GetComponent<button>().ButtonText = "Tools";
			butonToolbox.GetComponent<button>().strength = 20;
			butonToolbox.GetComponent<button>().textScale = new Vector3(0.3f, 1f, 1f);


			buttonItemPallete.name = "ButtonItemPallete";
			buttonItemPallete.transform.parent = transform;
			buttonItemPallete.transform.localScale = new Vector3(.8f, .8f, .8f);
			buttonItemPallete.transform.localPosition = new Vector3(10f, 0, 0);
			buttonItemPallete.GetComponent<MeshRenderer>().enabled = false;
			buttonItemPallete.AddComponent<button>();
			buttonItemPallete.GetComponent<button>().scaleX = 5;
			buttonItemPallete.GetComponent<button>().scaleY = 1.5f;
			buttonItemPallete.GetComponent<button>().scaleZ = 2;
			buttonItemPallete.GetComponent<button>().ButtonText = "Items";
			buttonItemPallete.GetComponent<button>().strength = 20;
			buttonItemPallete.GetComponent<button>().textScale = new Vector3(0.3f, 1f, 1f);

			buttonScrollLeft.name = "ButtonScrollLeft";
			buttonScrollLeft.transform.parent = transform;
			buttonScrollLeft.transform.localScale = new Vector3(.8f, .8f, .8f);
			buttonScrollLeft.transform.localPosition = new Vector3(-2f, 0, -4.5f);
			buttonScrollLeft.GetComponent<MeshRenderer>().enabled = false;
			buttonScrollLeft.AddComponent<button>();
			buttonScrollLeft.GetComponent<button>().scaleX = 1.5f;
			buttonScrollLeft.GetComponent<button>().scaleY = 1.5f;
			buttonScrollLeft.GetComponent<button>().scaleZ = 1.5f;
			buttonScrollLeft.GetComponent<button>().ButtonText = "<";
			buttonScrollLeft.GetComponent<button>().strength = 20;
			buttonScrollLeft.GetComponent<button>().textScale = new Vector3(1f, 1f, 1f);

			buttonScrollRight.name = "ButtonScrollRight";
			buttonScrollRight.transform.parent = transform;
			buttonScrollRight.transform.localScale = new Vector3(.8f, .8f, .8f);
			buttonScrollRight.transform.localPosition = new Vector3(12f, 0, -4.5f);
			buttonScrollRight.GetComponent<MeshRenderer>().enabled = false;
			buttonScrollRight.AddComponent<button>();
			buttonScrollRight.GetComponent<button>().scaleX = 1.5f;
			buttonScrollRight.GetComponent<button>().scaleY = 1.5f;
			buttonScrollRight.GetComponent<button>().scaleZ = 1.5f;
			buttonScrollRight.GetComponent<button>().ButtonText = ">";
			buttonScrollRight.GetComponent<button>().strength = 20;
			buttonScrollRight.GetComponent<button>().textScale = new Vector3(1f, 1f, 1f);

			buttonScrollLeft.GetComponent<button>().show = false;
			buttonScrollRight.GetComponent<button>().show = false;
		}

		// Update is called once per frame
		void Update()
		{
			if (butonToolbox.GetComponent<button>().checkClicked() == 1)
			{
				if(menuPage == 1) menuPage = 0;
				else menuPage = 1;

				maxScroll = tools.Count / 6;
				menuScroll = 0;
				updateMenu(1);
				
			}
			if (buttonItemPallete.GetComponent<button>().checkClicked() == 1)
			{
				if(menuPage == 2) menuPage = 0;
				else menuPage = 2;

				maxScroll = palleteObjects.Count / 6;
				menuScroll = 0;
				updateMenu(1);
			}
			if(buttonScrollLeft.GetComponent<button>().checkClicked() == 1)
			{
				if(menuScroll > minScroll) menuScroll -= 1;
				updateMenu(1);

			}
			if(buttonScrollRight.GetComponent<button>().checkClicked() == 1)
			{
				if(menuScroll < maxScroll) menuScroll += 1;
				updateMenu(1);
			}
		}

		private void updateMenu(int wipeMenu)
		{
			//Debug.Log("going to menu: " + menuPage);
			if(wipeMenu == 1)
			{
				for(int x = 0; x < toolIcons.Count; x++){
					Destroy(toolIcons[x].gameObject);
				}
				toolIcons.Clear();

				for(int x = 0; x < palleteIcons.Count; x++){
					Destroy(palleteIcons[x].gameObject);
				}
				palleteIcons.Clear();
				
				butonToolbox.GetComponent<button>().setColour(0);
				buttonItemPallete.GetComponent<button>().setColour(0);
			}
			//Debug.Log("menu scroll: " + menuScroll);

			if(menuScroll > minScroll) buttonScrollLeft.GetComponent<button>().setColour(0);
			else buttonScrollLeft.GetComponent<button>().setColour(2);
			if(menuScroll >= maxScroll) buttonScrollRight.GetComponent<button>().setColour(2);
			else buttonScrollRight.GetComponent<button>().setColour(0);

			if(menuPage == 0)
			{
				buttonScrollLeft.GetComponent<button>().show = false;
				buttonScrollRight.GetComponent<button>().show = false;
			}

			if(menuPage == 1)		//toolbox
			{

				buttonScrollLeft.GetComponent<button>().show = true;
				buttonScrollRight.GetComponent<button>().show = true;

				butonToolbox.GetComponent<button>().setColour(1);
				buttonItemPallete.GetComponent<button>().setColour(0);

				int count = 0;
				for(int y = 0; y < 2; y++) {
					for(int x = 0; x < 3; x++)
					{
						
						if(tools.Count > count + menuScroll*6) {
							toolIcons.Add(Instantiate(tools[count+ menuScroll*6], new Vector3(0, 0, 0), transform.rotation));
							toolIcons[count].transform.localScale *= 2f;
							toolIcons[count].transform.parent = transform;
							toolIcons[count].transform.localPosition = new Vector3(1f + x * 4, 7, -2.5f-y*3);
							toolIcons[count].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
							toolIcons[count].AddComponent<CursorSpawner>();
							toolIcons[count].GetComponent<CursorSpawner>().objectToSpawn = tools[count+ menuScroll*6];
						}
						count += 1;
					}
				}

			}
			else if(menuPage == 2)		//item pallete
			{
				buttonScrollLeft.GetComponent<button>().show = true;
				buttonScrollRight.GetComponent<button>().show = true;

				butonToolbox.GetComponent<button>().setColour(0);
				buttonItemPallete.GetComponent<button>().setColour(1);
				int count = 0;

				for(int y = 0; y < 2; y++) {
					for(int x = 0; x < 3; x++)
					{
						if(palleteObjects.Count > count + menuScroll*6) {
							palleteIcons.Add(Instantiate(palleteObjects[count+ menuScroll*6], new Vector3(0, 0, 0), palleteObjects[count+ menuScroll*6].transform.rotation));

							
						
							//palleteIcons[count].transform.localScale *= 0.5f;
							//palleteIcons[count].transform.rotation = transform.rotation;
							palleteIcons[count].transform.parent = transform;
							

							float maxSize = palleteIcons[count].GetComponent<Renderer> ().bounds.size.x;
							if(palleteIcons[count].GetComponent<Renderer> ().bounds.size.y > maxSize) maxSize = palleteIcons[count].GetComponent<Renderer> ().bounds.size.y;
							if(palleteIcons[count].GetComponent<Renderer> ().bounds.size.z > maxSize) maxSize = palleteIcons[count].GetComponent<Renderer> ().bounds.size.z;

							Debug.Log(palleteIcons[count].name + "max size = " + maxSize);

							float scale = 0.2f / maxSize;
							palleteIcons[count].transform.localScale *= scale;
							
							palleteIcons[count].transform.localPosition = new Vector3(1f + x * 4, 7, -2.5f-y*3);
							if(palleteIcons[count].GetComponent<Rigidbody>()) palleteIcons[count].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
							palleteIcons[count].AddComponent<CursorSpawner>();
							palleteIcons[count].GetComponent<CursorSpawner>().objectToSpawn = palleteObjects[count+ menuScroll*6];
						}
						count += 1;
					}
				}
			}
		}
	}
}