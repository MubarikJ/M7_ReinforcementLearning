using UnityEngine;

public class TrashFloor : MonoBehaviour
{
    [SerializeField] private FallingTrashAgent agent;
    [SerializeField] private float maxRewardDistance = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Trash"))
        {
            
            Vector3 tp = collision.transform.position;
            Vector3 ap = agent.transform.position;

            Vector2 t2 = new Vector2(tp.x, tp.z);
            Vector2 a2 = new Vector2(ap.x, ap.z);
            float dist = Vector2.Distance(t2, a2);

            float t = Mathf.Clamp01(dist / maxRewardDistance);
            float reward = Mathf.Lerp(1f, -1f, t); 

            agent.AddReward(reward);
            agent.EndEpisode();
        }
    }
}
