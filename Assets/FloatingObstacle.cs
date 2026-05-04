using UnityEngine;

public class FloatingObstacle : MonoBehaviour
{
    public bool shouldRotate = false;
    public bool shouldMoveUpAndDown = false;
    public bool shouldMoveLeftAndRight = false;
    float rotationSpeed = 25f;
    float moveSpeed = 1f;
    float moveDistance = 0.5f;
    Vector3 startingPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed);
        }

        Vector3 movementOffset = Vector3.zero;
        float movementAmount = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        if (shouldMoveUpAndDown)
        {
            movementOffset.y = movementAmount;
        }
        if (shouldMoveLeftAndRight)
        {
            movementOffset.x = movementAmount;
        }

        transform.position = startingPosition + movementOffset;
    }
}
