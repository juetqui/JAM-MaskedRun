using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLaneMovement : MonoBehaviour
{
    public static PlayerLaneMovement Instance;
    
    [Header("Carriles")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 12f;
    public float laneArriveThreshold = 0.02f;

    [Header("Salto")]
    public float jumpForce = 6f;
    public LayerMask groundLayer;
    private float groundCheckDistance = 0.3f;

    private Rigidbody rb;
    public InputsPlayer inputActions;
    private Animator animator;

    private int currentLane = 1; // 0=Left, 1=Mid, 2=Right
    public bool isGrounded;

    [SerializeField] private float groundCheckRadius = 0f;
    [SerializeField] private float groundCheckOffset = 0.1f;

    // Para no spamear animaciones mientras todav�a est�s yendo al carril
    private bool isChangingLane;

    public Action OnWorldChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        rb = GetComponent<Rigidbody>();
        inputActions = new InputsPlayer();
        animator = GetComponentInChildren<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation |
                         RigidbodyConstraints.FreezePositionZ;
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.ChangeEnv.performed += ChangeWorld;
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.ChangeEnv.performed -= ChangeWorld;
        inputActions.Disable();
    }

    void Update()
    {
        MoveToLane();
        CheckGround();
        animator.SetBool("Grounded", isGrounded);

        // Si est�s cerca del carril objetivo, terminamos el "lane change"
        float targetX = (currentLane - 1) * laneDistance;
        if (isChangingLane && Mathf.Abs(transform.position.x - targetX) <= laneArriveThreshold)
        {
            isChangingLane = false;
            // Si us�s un bool tipo "IsDashing", ac� ser�a buen lugar para apagarlo.
        }
    }

    void FixedUpdate()
    {

        // si us�s par�metro en animator:
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

    private void ChangeWorld(InputAction.CallbackContext context)
    {
        OnWorldChanged?.Invoke();
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane < 0 || targetLane > 2) return;

        currentLane = targetLane;
        isChangingLane = true;

        // Animator: trigger + direcci�n
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
        if (!isGrounded) return;

        // salto consistente (no se acumula)
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
    }

    void CheckGround()
    {
        // Punto desde donde chequeamos (cerca de los pies)
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;

        // SphereCast hacia abajo
        isGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;

        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
