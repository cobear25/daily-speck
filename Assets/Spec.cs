using UnityEngine;
using UnityEngine.InputSystem;

public class Spec : MonoBehaviour
{
    [SerializeField] float maxUpwardVelocity = 3f;
    [SerializeField] float maxFallVelocity = 3f;
    [SerializeField] float verticalAcceleration = 500f;
    public GameController gameController;
    int deathCount = 0;
    float speed = 1.5f;
    public Vector2 initialPosition;

    public bool movingRight = true;
    bool acceleratingUp;
    Rigidbody2D specRigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        specRigidbody = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
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
        Vector2 velocity = specRigidbody.linearVelocity;

        if (acceleratingUp)
        {
            velocity.y += verticalAcceleration * Time.fixedDeltaTime;
        }

        velocity.y = Mathf.Clamp(velocity.y, -maxFallVelocity, maxUpwardVelocity);

        specRigidbody.linearVelocity = velocity;
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
        }
    }

    void ResetPosition()
    {
        transform.position = initialPosition;
        specRigidbody.linearVelocity = Vector2.zero;
        specRigidbody.angularVelocity = 0f;
        acceleratingUp = false;
        movingRight = true;
        deathCount++;
        Debug.Log("Death count: " + deathCount);
    }
}
