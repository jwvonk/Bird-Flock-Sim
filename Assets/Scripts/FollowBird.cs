using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBird : MonoBehaviour
{
    private Bird bird;
    // Start is called before the first frame update
    void Start()
    {
        bird = FindObjectOfType<Bird>();
        transform.parent = bird.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

}
