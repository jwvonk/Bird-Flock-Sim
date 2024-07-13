using UnityEngine;

public class Bird : MonoBehaviour
{
    [Tooltip("Maximum speed limit.")]
    public float maxSpeed = 48f;

    [Tooltip("Minimum speed limit.")]
    public float minSpeed = 24f;

    [Tooltip("Turning speed factor for boundary avoidance.")]
    public float boundsFactor = .2f;

    [Tooltip("Range to see other birds for flocking.")]
    public float visualRange = 40f;

    [Tooltip("Range to avoid other birds to prevent collision.")]
    public float protectedRange = 8f;

    [Tooltip("Factor to move towards the average position of nearby birds.")]
    public float centeringFactor = 0.0005f;

    [Tooltip("Factor to move away from nearby birds to avoid collisions.")]
    public float avoidFactor = 0.05f;

    [Tooltip("Factor to align velocity with nearby birds.")]
    public float matchingFactor = 0.05f;

    [Tooltip("Determines how quickly birds rotate to face movement vector.")]
    public float rotationFactor = .5f;

    [Tooltip("Rate at which bias increases/decreases.")]
    public float biasIncrement = 0.00004f;

    [Tooltip("Maximum limit for bias strength.")]
    public float maxBias = 0.01f;

    [HideInInspector] public int group;

    private Vector3 currentVelocity;
    private float biasStrength = 0;

    private BoxCollider boundingBox; // Bounding box collider for the flock area

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

    void EnforceBounds()
    {
        if (!boundingBox) return;
        Bounds bounds = boundingBox.bounds;
        inBounds = bounds.Contains(transform.position);

        // If outside bounds, accelerate towards bounding box
        if (!inBounds)
        {
            Vector3 closestPoint = bounds.ClosestPoint(transform.position);
            currentVelocity += (closestPoint - transform.position).normalized * boundsFactor;
        }
    }

    // Boids Algorithm
    // Reference: https://vanhunteradams.com/Pico/Animal_Movement/Boids-algorithm.html
    void CalculateFlocking()
    {
        // Zero accumulator vars
        Vector3 closeDifferential = Vector3.zero;
        Vector3 averageVelocity = Vector3.zero;
        Vector3 averagePosition = Vector3.zero;
        int neighborCount = 0;

        // Find birds within protected range
        Collider[] closeBirdColliders = Physics.OverlapSphere(transform.position, protectedRange);
        foreach (Collider collider in closeBirdColliders)
        {
            Bird bird = collider.GetComponentInParent<Bird>();
            if (bird == this || bird == null) continue; // Filter out ourselves && non-birds

            // Accumulate movement vector to move away from close birds
            closeDifferential += transform.position - bird.transform.position;
        }

        // Find birds within visible range
        Collider[] visibleBirdColliders = Physics.OverlapSphere(transform.position, visualRange);
        foreach (Collider collider in visibleBirdColliders)
        {
            Bird bird = collider.GetComponentInParent<Bird>();
            if (bird == this || bird == null) continue;// Filter out ourselves && non-birds

            averageVelocity += bird.currentVelocity;
            averagePosition += bird.transform.position;
            neighborCount++;
        }

        // Move away from close birds
        currentVelocity += closeDifferential * avoidFactor;

        if (neighborCount > 0)
        {
            averageVelocity /= neighborCount;
            averagePosition /= neighborCount;

            // Align flight direction with visible birds
            currentVelocity += (averageVelocity - currentVelocity) * matchingFactor;

            // Move towards center point of visible birds
            currentVelocity += (averagePosition - transform.position) * centeringFactor;
        }
    }
    // Update the bias based on the group and velocity
    // Reference: https://vanhunteradams.com/Pico/Animal_Movement/Boids-algorithm.html#Bias
    void UpdateBias()
    {
        if (group == 0) // Biased to positive x
        {
            // Bias strength increases if x velocity is positive, decreases if negative
            biasStrength += currentVelocity.x > 0 ? biasIncrement : -biasIncrement;
        }
        else if (group == 1) // biased to negative x
        {
            // bias strength increases if x velocity is negative, decreases if positive
            biasStrength += currentVelocity.x < 0 ? biasIncrement : -biasIncrement;
        }
        Mathf.Clamp(biasStrength, biasIncrement, maxBias);

        // Apply bias
        currentVelocity.x = (1 - biasStrength) * currentVelocity.x + (group == 0 ? biasStrength : -biasStrength);

    }

    // Constrain speed to within user-defined min and max
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
        // Apply velocity
        transform.position += currentVelocity * Time.deltaTime;

        // rotate towards motion vector
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentVelocity), rotationFactor * Time.deltaTime);
    }
}
