using UnityEngine;

public class Bird : MonoBehaviour
{
    public float maxSpeed = 48f;
    public float minSpeed = 24f;
    public float turnFactor = .2f;
    public float visualRange = 40f;
    public float protectedRange = 8f;
    public float centeringFactor = 0.0005f;
    public float avoidFactor = 0.05f;
    public float matchingFactor = 0.05f;
    public float rotationFactor = .5f;
    public float biasIncrement = 0.00004f;
    public float maxBias = 0.01f;

    [HideInInspector] public int group;

    private Vector3 currentVelocity;
    private float biasStrength;

    private BoxCollider boundingBox;

    private bool inBounds = true;

    private void Start()
    {
        boundingBox = GameObject.FindWithTag("Respawn").GetComponent<BoxCollider>();
        currentVelocity = Random.onUnitSphere * minSpeed;
    }

    void Update()
    {
        EnforceBounds();
        if (inBounds)
        {
            CalculateFlocking();
            UpdateBias();
        }   
        EnforceSpeedLimits();
        MoveAndRotate();
    }

    // Boids Algorithm
    // https://vanhunteradams.com/Pico/Animal_Movement/Boids-algorithm.html
    void CalculateFlocking()
    {
        // Zero accumulator vars
        Vector3 closeDifferential = Vector3.zero;
        Vector3 averageVelocity = Vector3.zero;
        Vector3 averagePosition = Vector3.zero;
        int neighborCount = 0;

        Collider[] visibleBirdColliders = Physics.OverlapSphere(transform.position, visualRange);
        foreach (Collider collider in visibleBirdColliders)
        {
            Bird bird = collider.GetComponent<Bird>();
            if (bird == this) continue;
            float distance = Vector3.Distance(transform.position, bird.transform.position);

            if (distance < protectedRange)
            {
                closeDifferential += transform.position - bird.transform.position;
            }
            averageVelocity += bird.currentVelocity;
            averagePosition += bird.transform.position;
            neighborCount++;
        }

        currentVelocity += closeDifferential * avoidFactor;

        if (neighborCount > 0)
        {
            averageVelocity /= neighborCount;
            averagePosition /= neighborCount;

            currentVelocity += (averageVelocity - currentVelocity) * matchingFactor;
            currentVelocity += (averagePosition - transform.position) * centeringFactor;
        }
    }

    void EnforceBounds()
    {
        if (!boundingBox) return;
        Bounds bounds = boundingBox.bounds;
        inBounds = bounds.Contains(transform.position);
        if (!inBounds)
        {
            Vector3 closestPoint = bounds.ClosestPoint(transform.position);
            currentVelocity += (closestPoint - transform.position).normalized * turnFactor;
        }
    }

    void UpdateBias()
    {
        if (group == 0)
        {
            biasStrength += currentVelocity.x > 0 ? biasIncrement : -biasIncrement;
        }
        else if (group == 1)
        {
            biasStrength += currentVelocity.x < 0 ? biasIncrement : -biasIncrement;
        }
        Mathf.Clamp(biasStrength, biasIncrement, maxBias);

        currentVelocity.x = (1 - biasStrength) * currentVelocity.x + (group == 0 ? biasStrength : -biasStrength);

    }

    void EnforceSpeedLimits()
    {
        var speed = currentVelocity.magnitude;

        if (speed < minSpeed)
        {
            currentVelocity = currentVelocity.normalized * minSpeed;
        }
        if (speed > maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
    }

    void MoveAndRotate()
    {
        // Move and rotate the bird
        transform.position += currentVelocity * Time.deltaTime;
        if (currentVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentVelocity), rotationFactor * Time.deltaTime);
        }
    }
}
