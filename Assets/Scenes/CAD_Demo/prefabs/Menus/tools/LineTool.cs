using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

namespace SenmagHaptic
{
    public class LineTool : MonoBehaviour
    {

        public GameObject sketchPointPrefab;
        private int lineState = 0;
        public Material lineMaterial;

        private bool leftButtonLatch = false;
		private bool rightButtonLatch = false;

		private LineRenderer line;
        private Vector3 startPos;
        private sketchPlane activePlane;

        private sketchPlane[] sketchPlanes;

        private List<GameObject> tempLine;


        public float lineWidth = 0.01f;

		// Start is called before the first frame update
		void Start()
        {
			line = gameObject.AddComponent<LineRenderer>();
            line.enabled = false;

			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
            line.material = lineMaterial;

			sketchPlanes = FindObjectsOfType<sketchPlane>();



            tempLine = new List<GameObject>();
	}

        // Update is called once per frame
        void Update()
        {
            if(lineState >= 1)
            {
				line.SetPosition(0, startPos);
				line.SetPosition(1, activePlane.snapPos);
			}


            if(GetComponentInParent<Senmag_stylusControl>()!= null)
			{
                if (GetComponentInParent<Senmag_stylusControl>().Input_isHeld(Stylus_Action.leftClick))
                {
                    if (leftButtonLatch == false)
                    {
                        leftButtonLatch = true;

                        

                        if (lineState == 0)
                        {
							sketchPlanes = FindObjectsOfType<sketchPlane>();

							bool pointFound = false;
                            for (int x = 0; x < sketchPlanes.Length; x++)
                            {
                                if (sketchPlanes[x].active == true)
                                {
                                    if (pointFound == false)
                                    {
                                        pointFound = true;

                                        GameObject newPoint = Instantiate(sketchPointPrefab);
                                        newPoint.transform.position = sketchPlanes[x].snapPos;
                                        tempLine.Add(newPoint);
										startPos = newPoint.transform.position;
                                        activePlane = sketchPlanes[x];
                                        line.enabled = true;
                                        lineState = 1;

                                        GetComponentInParent<Senmag_HapticCursor>().blockMenuSpawn = true;

									}
                                    else sketchPlanes[x].transform.parent.gameObject.SetActive(false);
                                }
                                else
                                {
                                    sketchPlanes[x].transform.parent.gameObject.SetActive(false);
                                }
                            }
                            if(pointFound == false)
                            {
								for (int x = 0; x < sketchPlanes.Length; x++) sketchPlanes[x].transform.parent.gameObject.SetActive(true);
							}

                        }
                        else
                        {

                            GameObject line = new GameObject();
                            line.AddComponent<LineRenderer>();
                            line.GetComponent<LineRenderer>().startWidth = lineWidth;
							line.GetComponent<LineRenderer>().endWidth = lineWidth;
							line.GetComponent<LineRenderer>().material = lineMaterial;
							line.GetComponent<LineRenderer>().SetPosition(0, startPos);
							line.GetComponent<LineRenderer>().SetPosition(1, activePlane.snapPos);
                            tempLine.Add(line);

                            startPos = activePlane.snapPos;
							GameObject newPoint = Instantiate(sketchPointPrefab);
                            newPoint.transform.position = activePlane.snapPos;
							tempLine.Add(newPoint);


                            lineState++;
							//line.enabled = false;
							//lineState = 0;
							//for (int x = 0; x < sketchPlanes.Length; x++) sketchPlanes[x].transform.parent.gameObject.SetActive(true);

						}

                    }
                    
                }
				else leftButtonLatch = false;


				if (GetComponentInParent<Senmag_stylusControl>().Input_isHeld(Stylus_Action.rightClick))
                {
                    if(rightButtonLatch == false)
                    {
                        rightButtonLatch = true;
                        if(lineState != 0)
                        {
                            if (lineState <= 1)
                            {
                                foreach (GameObject obj in tempLine)
                                {
                                    Destroy(obj);
                                }

                            }
							tempLine.Clear();
							line.enabled = false;
							lineState = 0;
							for (int x = 0; x < sketchPlanes.Length; x++) sketchPlanes[x].transform.parent.gameObject.SetActive(true);
							GetComponentInParent<Senmag_HapticCursor>().blockMenuSpawn = false;
						}
					}
                }
				else rightButtonLatch = false;
			}
        }
    }
};
