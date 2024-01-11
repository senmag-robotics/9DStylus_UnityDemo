using UnityEngine;
using UnityEngine.AI;

public class CreeperBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject particles;

    private void Start()
    {
        Invoke("BigBoom", 12);
    }

    void Update()
    {
        agent.SetDestination(new Vector3(0, 0, -0.6f));

    }

    public void BigBoom()
    {
        Vector3 pos = GameObject.Find("/CreeperObjectPrefab(Clone)/Nav/CreepBody").GetComponent<Transform>().position;
        Instantiate(particles, pos, Quaternion.identity);
        particles.GetComponent<AudioSource>().Play();
        Collider[] Colliders = Physics.OverlapSphere(pos, 0.7f);
        foreach (var hitCollider in Colliders)
        {
            hitCollider.SendMessage("BreakLoose", pos);
        }
        SendMessage("HapticBoom");

        GameObject.Find("/CreeperObjectPrefab(Clone)/Nav/CreepBody").GetComponent<MeshRenderer>().enabled = false;
        Invoke("Death", 2);
    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
