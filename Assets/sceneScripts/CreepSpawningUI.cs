using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{

    public class CreepSpawningUI : MonoBehaviour
    {

        private GameObject menuBase;
        private GameObject buttonSpawnCreeper;
        private GameObject buttonSwitchCreepOnOff;
        private GameObject sliderCreepFrequency;
        private GameObject descriptionPanel;
        private GameObject descriptionText;
        private GameObject titleText;
        private GameObject buttonDescriptions;

        private string willSpawnText = "will";
        private string spawningIntervalText;
        private bool willSpawn = true;

        private int timeCount = 0;

        public int spawnInterval;
        public GameObject creeperPrefab;
        public Vector3 spawnLocation;

        private bool delay = true;

        // Start is called before the first frame update
        void Start()
        {
            spawnInterval = 60;

            menuBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menuBase.name = "creepMenubBase";
            menuBase.transform.parent = transform;
            menuBase.transform.localPosition = new Vector3(0, 0, 0);
            menuBase.transform.localScale = new Vector3(9, 12, 1);
            menuBase.transform.localRotation = Quaternion.identity;
            menuBase.AddComponent<Rigidbody>();
            menuBase.GetComponent<Rigidbody>().isKinematic = false;
            menuBase.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            menuBase.GetComponent<Renderer>().material.SetColor("_Color", new Color(.7f, .7f, .7f));

            titleText = new GameObject();
            titleText.AddComponent<TextMesh>();
            titleText.GetComponent<TextMesh>().text = "<b>      Creeper Spawning       \n               Menu</b>      \n\n                            Spawn\n                            Creepers?";
            titleText.GetComponent<TextMesh>().characterSize = 0.1f;
            titleText.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
            titleText.transform.parent = transform;
            titleText.transform.localPosition = new Vector3(0, 5.5f, 0);
            titleText.transform.localScale = new Vector3(5, 5, 5);
            titleText.transform.rotation = transform.rotation;


            descriptionPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            descriptionPanel.name = "creepMenubBase";
            descriptionPanel.transform.parent = transform;
            descriptionPanel.transform.localPosition = new Vector3(-1.5f, 1.5f, -0.5f);
            descriptionPanel.transform.localScale = new Vector3(5, 4, 0.2f);
            descriptionPanel.transform.localRotation = Quaternion.identity;
            descriptionPanel.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));

            descriptionText = new GameObject();
            descriptionText.AddComponent<TextMesh>();
            descriptionText.GetComponent<TextMesh>().text = "<color=black>Creepers</color>" + willSpawnText + " <color=black>\nspawn every\n</color>" + spawningIntervalText + " <color=blue> seconds</color> " + "<color=black>\nand try to\nblow up what\nyou've built.</color>";
            descriptionText.GetComponent<TextMesh>().characterSize = 0.1f;
            descriptionText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            descriptionText.transform.parent = transform;
            descriptionText.transform.localPosition = new Vector3(-1.5f, 1.5f, -0.5f);
            descriptionText.transform.localScale = new Vector3(5, 4, 0.2f);
            descriptionText.transform.rotation = transform.rotation;

            buttonSwitchCreepOnOff = new GameObject();
            
            buttonSwitchCreepOnOff.AddComponent<button>();
            buttonSwitchCreepOnOff.GetComponent<button>().scaleX = 5f;
            buttonSwitchCreepOnOff.GetComponent<button>().scaleY = 1.5f;
            buttonSwitchCreepOnOff.GetComponent<button>().scaleZ = 2f;
            buttonSwitchCreepOnOff.GetComponent<button>().ButtonText = "Press";
            buttonSwitchCreepOnOff.GetComponent<button>().strength = 20;
            buttonSwitchCreepOnOff.GetComponent<button>().textScale = new Vector3(0.4f, 0.6f, 0.6f);
            //buttonSwitchCreepOnOff.GetComponent<button>().transform.rotation = transform.rotation;
            buttonSwitchCreepOnOff.transform.parent = transform;
            buttonSwitchCreepOnOff.transform.localPosition = new Vector3(2.8f, 0.5f, -0.5f);
            buttonSwitchCreepOnOff.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            //buttonSwitchCreepOnOff.transform.localRotation = Quaternion.identity;

            sliderCreepFrequency = new GameObject();
            sliderCreepFrequency.AddComponent<Slider>();
            sliderCreepFrequency.GetComponent<Slider>().sliderRotation = new Vector3(-90f, 34f, 0); //Set rotation
            sliderCreepFrequency.transform.parent = transform;
            sliderCreepFrequency.transform.localPosition = new Vector3(0, -3f, -0.5f);
            sliderCreepFrequency.transform.localScale = new Vector3(8, 0.5f, 0.8f);

            buttonSpawnCreeper = new GameObject();
            buttonSpawnCreeper.AddComponent<button>();
            buttonSpawnCreeper.GetComponent<button>().scaleX = 5f;
            buttonSpawnCreeper.GetComponent<button>().scaleY = 1.5f;
            buttonSpawnCreeper.GetComponent<button>().scaleZ = 2f;
            buttonSpawnCreeper.GetComponent<button>().ButtonText = "Press";
            buttonSpawnCreeper.GetComponent<button>().strength = 20;
            buttonSpawnCreeper.GetComponent<button>().textScale = new Vector3(0.4f, 0.6f, 0.6f);
            buttonSpawnCreeper.transform.parent = transform;
            buttonSpawnCreeper.transform.localPosition = new Vector3(2.8f, -4.5f, -0.5f);
            buttonSpawnCreeper.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);

            buttonDescriptions = new GameObject();
            buttonDescriptions.AddComponent<TextMesh>();
            buttonDescriptions.GetComponent<TextMesh>().text = "       Creeper Spawning Rate:          \n\n\n\n\nCreate a Creeper Now ->     ";
            buttonDescriptions.GetComponent<TextMesh>().characterSize = 0.08f;
            buttonDescriptions.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
            buttonDescriptions.transform.parent = transform;
            buttonDescriptions.transform.localPosition = new Vector3(0, -1.5f, 0);
            buttonDescriptions.transform.localScale = new Vector3(5, 5, 5);
            buttonDescriptions.transform.rotation = transform.rotation;


            StartCoroutine(time());
            
            IEnumerator time()
            {
                while(true)
                {
                    timeCount++;
                    yield return new WaitForSeconds(1);
                    delay = true;
                }
            }









        }

        // Update is called once per frame
        void Update()
        {
            buttonSwitchCreepOnOff.GetComponent<button>().transform.rotation = Quaternion.Euler(-90f, 34, 0);
            sliderCreepFrequency.transform.rotation = Quaternion.Euler(-90f, 34, 0);
            buttonSpawnCreeper.GetComponent<button>().transform.rotation = Quaternion.Euler(-90f, 34, 0);

            if (buttonSpawnCreeper.GetComponent<button>().checkClicked() == 1 && delay)
            {
                Instantiate(creeperPrefab, spawnLocation, Quaternion.identity);
                delay = false;
            }

            spawnInterval = Mathf.RoundToInt((sliderCreepFrequency.GetComponent<Slider>().position + 1) * 150);
            spawningIntervalText = "<color=blue>" + spawnInterval.ToString() + "</color>";
            descriptionText.GetComponent<TextMesh>().text = "   <color=black>Creepers</color>" + spawnSwitchCheck() + "<color=black>\n   spawn every    \n   </color>" + spawningIntervalText + "<color=blue>seconds</color> " + "<color=black>    \n   and try to    \n   blow up what    \n   you've built.</color>    ";

            

            if (spawnInterval <= timeCount && willSpawn)
            {
                Instantiate(creeperPrefab, spawnLocation, Quaternion.identity);
                timeCount = 0;
            }
        }

        private string spawnSwitchCheck()
        { 
        if (buttonSwitchCreepOnOff.GetComponent<button>().checkClicked() == 1)
            {
                willSpawn = !willSpawn;
            }

            if (willSpawn) return ("<color=green>will</color>");
            else return ("<color=red>will not</color>");
        }
    }

}