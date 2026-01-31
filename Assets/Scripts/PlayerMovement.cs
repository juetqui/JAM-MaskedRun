using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLaneMovement : MonoBehaviour
{
    [Header("Carriles")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 12f;
    public float laneArriveThreshold = 0.02f;

    [Header("Salto")]
    public float jumpForce = 6f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Animator animator;

    private int currentLane = 1; // 0=Left, 1=Mid, 2=Right
    public bool isGrounded = true;

    // Para no spamear animaciones mientras todavía estás yendo al carril
    private bool isChangingLane;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();
        animator = GetComponentInChildren<Animator>();

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

        // Si estás cerca del carril objetivo, terminamos el "lane change"
        float targetX = (currentLane - 1) * laneDistance;
        if (isChangingLane && Mathf.Abs(transform.position.x - targetX) <= laneArriveThreshold)
        {
            isChangingLane = false;
            // Si usás un bool tipo "IsDashing", acá sería buen lugar para apagarlo.
        }
    }

    void FixedUpdate()
    {
        CheckGround();

        // si usás parámetro en animator:
        animator.SetBool("Grounded", isGrounded);
    }

    // ===============================
    // MOVIMIENTO ENTRE CARRILES
    // ===============================
    void OnMove(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();

        if (direction > 0.5f) ChangeLane(+1);
        else if (direction < -0.5f) ChangeLane(-1);
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane < 0 || targetLane > 2) return;

        currentLane = targetLane;
        isChangingLane = true;

        // Animator: trigger + dirección
        animator.SetFloat("LaneDir", direction);
        animator.SetTrigger("LaneChange");
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
        if (isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Animator: trigger de salto
        animator.SetTrigger("Jump");
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }
}
