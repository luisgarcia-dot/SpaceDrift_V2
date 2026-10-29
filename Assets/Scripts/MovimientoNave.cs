using UnityEngine;
using UnityEngine.InputSystem;

// Nave 3D que se mueve solo en el plano XY (jugabilidad 2D en proyecto 3D).
[RequireComponent(typeof(Rigidbody))]
public class ShipMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float brakeMultiplier = 1.8f;

    [Header("Inercia")]
    [SerializeField] private float thrustDamping = 0.1f;
    [SerializeField] private float coastingDamping = 0.7f;

    [Header("Límites")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -4.15f;
    [SerializeField] private float maxY = 6.21f;

    [Header("Rotación visual")]
    [SerializeField] private Transform visualNave;
    [SerializeField] private float rotationSmoothTime = 0.18f;
    [SerializeField] private float spriteAngleOffset = -90f;

    private Rigidbody rb;
    private Vector2 input;
    private float rotationVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Bloquea el eje Z y toda rotación física: la nave solo se mueve en XY
        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        input = ReadInput();
    }

    private void LateUpdate()
    {
        RotateVisual();
    }

    private void FixedUpdate()
    {
        bool isMoving = input.sqrMagnitude > 0.01f;

        rb.linearDamping = isMoving ? thrustDamping : coastingDamping;

        if (isMoving)
        {
            float forceMultiplier = 1f;

            Vector2 velocity2D = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);

            if (Vector2.Dot(velocity2D, input) < 0f)
            {
                forceMultiplier = brakeMultiplier;
            }

            Vector3 force = new Vector3(input.x, input.y, 0f) * acceleration * forceMultiplier;
            rb.AddForce(force, ForceMode.Force);
        }

        if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        Vector3 pos = rb.position;
        Vector3 vel = rb.linearVelocity;

        if (pos.x < minX) { pos.x = minX; if (vel.x < 0f) vel.x = 0f; }
        else if (pos.x > maxX) { pos.x = maxX; if (vel.x > 0f) vel.x = 0f; }

        if (pos.y < minY) { pos.y = minY; if (vel.y < 0f) vel.y = 0f; }
        else if (pos.y > maxY) { pos.y = maxY; if (vel.y > 0f) vel.y = 0f; }

        rb.position = pos;
        rb.linearVelocity = vel;
    }

    private void RotateVisual()
    {
        // Si no hay hijo visual, rota el propio objeto (la rotación física está congelada,
        // así que se hace por transform)
        Transform target = visualNave != null ? visualNave : transform;

        Vector2 velocity2D = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);

        if (velocity2D.sqrMagnitude < 0.04f)
        {
            return;
        }

        Vector2 direction = velocity2D.normalized;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
            + spriteAngleOffset;

        float smoothAngle = Mathf.SmoothDampAngle(
            target.eulerAngles.z,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );

        target.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }

    private Vector2 ReadInput()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            vertical += 1f;

        return new Vector2(horizontal, vertical).normalized;
    }
}
