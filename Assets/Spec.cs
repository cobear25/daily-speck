using UnityEngine;
using UnityEngine.InputSystem;

public class Spec : MonoBehaviour
{
    [SerializeField] float maxUpwardVelocity = 1f;
    [SerializeField] float maxFallVelocity = 1f;
    [SerializeField] float verticalAcceleration = 500f;
    public GameController gameController;
    int deathCount = 0;
    float speed = 1.0f;
    public Vector2 initialPosition;

    public bool movingRight = true;
    public bool canMove = true;
    bool acceleratingUp;
    Rigidbody2D specRigidbody;
    float originalGravityScale;
    bool wasMoving;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        specRigidbody = GetComponent<Rigidbody2D>();
        originalGravityScale = specRigidbody.gravityScale;
        wasMoving = !canMove;
        ApplyMovementState();
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyMovementState();

        if (!canMove)
        {
            GetComponent<TrailRenderer>().Clear();
            acceleratingUp = false;
            return;
        }

        if (movingRight)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.World);
        }
        else
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed, Space.World);
        }

        // if holding on the screen or pressing x key, move upwards
        acceleratingUp = (Keyboard.current != null && Keyboard.current.xKey.isPressed)
            || (Mouse.current != null && Mouse.current.leftButton.isPressed)
            || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            specRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 velocity = specRigidbody.linearVelocity;

        if (acceleratingUp)
        {
            velocity.y += verticalAcceleration * Time.fixedDeltaTime;
        }

        velocity.y = Mathf.Clamp(velocity.y, -maxFallVelocity, maxUpwardVelocity);

        specRigidbody.linearVelocity = velocity;
    }

    void ApplyMovementState()
    {
        if (canMove == wasMoving)
        {
            return;
        }

        wasMoving = canMove;
        specRigidbody.gravityScale = canMove ? originalGravityScale : 0f;

        if (!canMove)
        {
            specRigidbody.linearVelocity = Vector2.zero;
            specRigidbody.angularVelocity = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            movingRight = !movingRight;
        }
        else if (collision.gameObject.CompareTag("goal"))
        {
            gameController.LevelBeat();
        }
        else
        {
            ResetPosition();
            GetComponent<TrailRenderer>().Clear();
        }
    }

    void ResetPosition()
    {
        GetComponent<TrailRenderer>().Clear();
        transform.position = initialPosition;
        specRigidbody.linearVelocity = Vector2.zero;
        specRigidbody.angularVelocity = 0f;
        acceleratingUp = false;
        movingRight = true;
        if (gameController.currentLevel == 1)
        {
            movingRight = false;
        }
        gameController.RecordAttempt();
        deathCount++;
        Debug.Log("Death count: " + deathCount);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("jam"))
        {
            gameController.RevealJam(collider.gameObject);
        }
    }
}
