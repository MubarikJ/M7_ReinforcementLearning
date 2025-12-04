using UnityEngine;

public class CatchZone : MonoBehaviour
{
    [SerializeField] private FallingTrashAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            agent.AddReward(+1.5f); // slightly bigger than perfect landing reward
            agent.EndEpisode();
        }
    }
}