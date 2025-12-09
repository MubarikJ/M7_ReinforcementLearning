using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FallingTrashAgent : Agent
{
    [Header("Scene References")]
    [SerializeField] private Transform ground;        
    [SerializeField] private GameObject trashPrefab;  
    [SerializeField] private Transform catchZone;     


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float trackingRewardScale = 0.025f;

    [Header("Spawn Settings")]
    [SerializeField] private float areaHalfSize = 4f;
    [SerializeField] private float spawnHeight = 4f;
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private Transform areaCenter;


    [SerializeField] private Transform[] eyes;   
    [SerializeField] private float eyeRayLength = 5f;
   


    private float _prevHorizontalDist;
    private Rigidbody _rb;
    private Rigidbody _trashRb;
    private Transform _trash;

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        Vector3 center = areaCenter != null ? areaCenter.position : ground.position;

        // Reset agent position on the floor near center
        Vector3 startPos = new Vector3(
            center.x + Random.Range(-areaHalfSize * 0.5f, areaHalfSize * 0.5f),
            ground.position.y + 0.5f,
            center.z + Random.Range(-areaHalfSize * 0.5f, areaHalfSize * 0.5f)
        );
        transform.position = startPos;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Destroy old trash
        if (_trash != null)
        {
            Destroy(_trash.gameObject);
        }

        // --- Spawn trash on an edge, thrown across the arena ---

        // Choose a random edge: 0=+X,1=-X,2=+Z,3=-Z
        int edge = Random.Range(0, 4);
        Vector3 spawnPos = center;
        switch (edge)
        {
            case 0:
                spawnPos.x = center.x + areaHalfSize;
                spawnPos.z = center.z + Random.Range(-areaHalfSize, areaHalfSize);
                break;
            case 1:
                spawnPos.x = center.x - areaHalfSize;
                spawnPos.z = center.z + Random.Range(-areaHalfSize, areaHalfSize);
                break;
            case 2:
                spawnPos.z = center.z + areaHalfSize;
                spawnPos.x = center.x + Random.Range(-areaHalfSize, areaHalfSize);
                break;
            case 3:
                spawnPos.z = center.z - areaHalfSize;
                spawnPos.x = center.x + Random.Range(-areaHalfSize, areaHalfSize);
                break;
        }
        spawnPos.y = ground.position.y + spawnHeight;

        GameObject trashObj = Instantiate(trashPrefab, spawnPos, Quaternion.identity);
        _trash = trashObj.transform;
        _trashRb = trashObj.GetComponent<Rigidbody>();

        // Throw towards center with some upward angle
        Vector3 dirXZ = (center - spawnPos);
        dirXZ.y = 0f;
        dirXZ.Normalize();

        float throwAngleDeg = Random.Range(20f, 60f); // vary arc
        float throwAngleRad = throwAngleDeg * Mathf.Deg2Rad;

        float vy = horizontalSpeed * Mathf.Tan(throwAngleRad);
        Vector3 velocity = dirXZ * horizontalSpeed + Vector3.up * vy;

        _trashRb.linearVelocity = velocity;

        // For tracking-reward
        _prevHorizontalDist = HorizontalDistanceToTrash();
    }

    private float HorizontalDistanceToTrash()
    {
        if (_trash == null) return 0f;
        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(_trash.position.x, _trash.position.z);
        return Vector2.Distance(a, b);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_trash == null || _trashRb == null)
        {
            // If something went wrong, fill with zeros so we don't crash
            sensor.AddObservation(new float[8]);
            return;
        }

        // Relative position (bin -> trash)
        Vector3 rel = _trash.position - transform.position;

        // Normalize roughly by arena size / height
        sensor.AddObservation(rel.x / areaHalfSize);
        sensor.AddObservation(rel.y / spawnHeight);
        sensor.AddObservation(rel.z / areaHalfSize);

        // Trash velocity (normalized)
        sensor.AddObservation(_trashRb.linearVelocity.x / 10f);
        sensor.AddObservation(_trashRb.linearVelocity.y / 10f);
        sensor.AddObservation(_trashRb.linearVelocity.z / 10f);

        // Agent horizontal velocity (normalized)
        sensor.AddObservation(_rb.linearVelocity.x / 10f);
        sensor.AddObservation(_rb.linearVelocity.z / 10f);

        bool trashAbove = IsTrashAbove();
        sensor.AddObservation(trashAbove ? 1f : 0f);
    }

    private bool TrashOutOfBounds()
    {
        if (_trash == null) return true;

        Vector3 center = areaCenter != null ? areaCenter.position : ground.position;
        Vector3 p = _trash.position;

        float margin = 1f; // small extra padding outside walls

        bool outsideXZ =
            Mathf.Abs(p.x - center.x) > areaHalfSize + margin ||
            Mathf.Abs(p.z - center.z) > areaHalfSize + margin;

        bool tooLow = p.y < ground.position.y - 1f; // fell way below floor

        return outsideXZ || tooLow;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        Vector3 dir = Vector3.zero;
        switch (action)
        {
            case 0: dir = Vector3.forward; break;
            case 1: dir = Vector3.back; break;
            case 2: dir = Vector3.left; break;
            case 3: dir = Vector3.right; break;
        }

        Vector3 move = dir * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(move.x, 0f, move.z);

        // Tracking reward: reward getting closer in XZ
        if (_trash != null)
        {
            float dist = HorizontalDistanceToTrash();
            float diff = _prevHorizontalDist - dist; // >0 if got closer
            float r = Mathf.Clamp(diff * trackingRewardScale, -0.03f, 0.03f);
            AddReward(r);
            _prevHorizontalDist = dist;
        }

        if (IsTrashAbove())
        {
            AddReward(0.001f); 
        }

        
        AddReward(-0.0005f);

        if (TrashOutOfBounds())
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    private bool IsTrashAbove()
    {
        if (eyes == null || eyes.Length == 0) return false;

        foreach (var eye in eyes)
        {
            if (eye == null) continue;

            Vector3 origin = eye.position;
            Vector3 dir = Vector3.up; // straight up

            if (Physics.Raycast(origin, dir, out RaycastHit hit, eyeRayLength))
            {
                if (hit.collider.CompareTag("Trash"))
                {
                    Debug.DrawRay(origin, dir * hit.distance, Color.green);
                    return true;
                }
            }
        }

        return false;
    }
    





    
    public void OnCaughtTrash()
    {
        AddReward(+1.5f);
        EndEpisode();
    }

    
    public void OnMissedTrash()
    {
        AddReward(-1.0f);
        EndEpisode();
    }
}
