using UnityEngine;

public class PietController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float backwardSpeed = 2f; // Velocidad al retroceder
    [SerializeField] private float rotationSpeed = 120f; // Grados por segundo
    [SerializeField] private float speedThreshold = 0.1f;
    
    [Header("Configuración de Idle")]
    [SerializeField] private float idleTimeBeforeStand = 5f;
    
    private Animator animator;
    private CharacterController characterController;
    private float idleTimer = 0f;
    private float currentSpeed = 0f;
    private bool hasTriggeredStand = false;
    private float verticalMovement = 0f;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
        }
    }
    
    void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleIdleTimer();
        UpdateAnimatorParameters();
    }
    
    void HandleMovement()
    {
        // A/D para rotar
        float horizontal = Input.GetAxis("Horizontal");
        // W/S para avanzar/retroceder
        float vertical = Input.GetAxis("Vertical");
        
        // Rotación del personaje con A y D
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            float rotation = horizontal * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, rotation, 0f);
        }
        
        // Movimiento hacia adelante/atrás con W y S
        verticalMovement = vertical;
        
        if (Mathf.Abs(vertical) > 0.01f)
        {
            // Determinar velocidad
            float speed = walkSpeed;
            
            // Si está retrocediendo (S)
            if (vertical < 0)
            {
                speed = backwardSpeed;
            }
            // Si está corriendo hacia adelante con Shift
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = runSpeed;
            }
            
            // Mover en la dirección que mira el personaje
            Vector3 moveDirection = transform.forward * vertical * speed * Time.deltaTime;
            characterController.Move(moveDirection);
            
            // Actualizar velocidad para animaciones
            currentSpeed = Mathf.Abs(vertical);
            
            idleTimer = 0f;
            hasTriggeredStand = false;
        }
        else
        {
            currentSpeed = 0f;
        }
        
        // Aplicar gravedad
        ApplyGravity();
    }
    
    void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            Vector3 gravity = new Vector3(0, -9.81f * Time.deltaTime, 0);
            characterController.Move(gravity);
        }
    }
    
    void HandleCombat()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Debug.Log("¡Madrazo!");
            animator.SetTrigger("Attack");
            idleTimer = 0f;
            hasTriggeredStand = false;
        }
    }
    
    void HandleIdleTimer()
    {
        if (currentSpeed < 0.01f && !IsInCombat())
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= idleTimeBeforeStand && !hasTriggeredStand)
            {
                Debug.Log($"Activando Stand después de {idleTimer} segundos");
                hasTriggeredStand = true;
            }
            
            if (hasTriggeredStand && IsStandFinished())
            {
                Debug.Log("Stand terminado, reiniciando contador");
                idleTimer = 0f;
                hasTriggeredStand = false;
            }
        }
    }
    
    void UpdateAnimatorParameters()
    {
        // Actualizar velocidad para las animaciones
        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("idleTime", idleTimer);
        
        // Opcional: parámetro para saber si va hacia atrás
        animator.SetFloat("VerticalMovement", verticalMovement);
        
        // Para compatibilidad
        animator.SetBool("isWalking", currentSpeed > speedThreshold);
    }
    
    bool IsInCombat()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Madrazo");
    }
    
    bool IsStandFinished()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Stand") && stateInfo.normalizedTime >= 0.95f;
    }
}