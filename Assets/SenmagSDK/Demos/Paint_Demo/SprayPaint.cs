using UnityEngine;
using System.Collections;

public class SprayPaint : MonoBehaviour
{
    public GameObject Nozzle;
    public Color paintColor;
    public bool shouldSpray = false;
    public float radius = 0.05f;
    public float strength = 0.5f;
    public float hardness = 0.001f;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            shouldSpray = true;
            StartCoroutine(Spray());
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            shouldSpray = false;
        }
    }
    IEnumerator Spray()
    {
        while (shouldSpray)
        {

            Ray ray = new Ray(Nozzle.transform.position, Nozzle.transform.forward);
            RaycastHit hit;
            print("spray1");
            if (Physics.Raycast(ray, out hit, 0.5f))
            {
                Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.red);

                Paintable p = hit.collider.GetComponent<Paintable>();
                if (p != null)
                {
                    radius = Vector3.Distance(ray.origin, hit.point) * 0.25f;
                    
                    print("spray2");
                    PaintManager.instance.paint(p, hit.point, radius, hardness, strength, paintColor);
                }
            }
            yield return new WaitForSeconds(0.03f);
        }
        yield return null;
    }
}