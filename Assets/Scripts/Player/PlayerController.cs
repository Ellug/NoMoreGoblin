using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _moveSpeed = 5f;
    // [SerializeField] private float _hp = 100f;
    // [SerializeField] private float _atk = 1f;
    // [SerializeField] private float _atkDelay = 1f;
    
    private Vector2 _moveInput;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (_moveInput == Vector2.zero) return;

        Debug.Log("인풋감지");
        Vector2 next = _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + next);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Attack();
        }
    }

    private void Attack()
    {
        Debug.Log("Player Attack");
    }
}
