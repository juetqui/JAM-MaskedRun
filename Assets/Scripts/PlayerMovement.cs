using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLaneMovement : MonoBehaviour
{
    [Header("Carriles")]
    public float laneDistance = 3f; // Distancia entre carriles
    public float laneChangeSpeed = 12f;

    [Header("Salto")]
    public float jumpForce = 6f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private PlayerInputActions inputActions;

    private int currentLane = 1; // 0 = Left | 1 = Mid | 2 = Right
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        rb.constraints = RigidbodyConstraints.FreezeRotation |
                         RigidbodyConstraints.FreezePositionZ;
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Jump.performed += OnJump;
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Disable();
    }

    void Update()
    {
        MoveToLane();
    }

    void FixedUpdate()
    {
        CheckGround();
    }

    // ===============================
    // MOVIMIENTO ENTRE CARRILES
    // ===============================
    void OnMove(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();

        if (direction > 0.5f)
            ChangeLane(1);
        else if (direction < -0.5f)
            ChangeLane(-1);
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;

        // Evita saltar carriles o salir del rango
        if (targetLane < 0 || targetLane > 2)
            return;

        currentLane = targetLane;
    }

    void MoveToLane()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.x = (currentLane - 1) * laneDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            laneChangeSpeed * Time.deltaTime
        );
    }

    // ===============================
    // SALTO
    // ===============================
    void OnJump(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            1.1f,
            groundLayer
        );
    }
}
