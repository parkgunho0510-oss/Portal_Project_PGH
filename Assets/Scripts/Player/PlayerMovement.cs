using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Animator _anim;
    Rigidbody _rb;

    [SerializeField] float _jumpPower = 10f;
    [SerializeField] float _moveSpeed = 5f; // 초당 이동 거리

    float rotateSpeed;
    private CM_Player _cm_Player;
    [Header("점프")]
    private bool groundHit = false;
    private int _jumpCount = 1;
    [SerializeField] int _jumpMaxCount = 1;
    [SerializeField] Transform _rayPosition;
    [SerializeField] LayerMask _groundLayer; 

    private float _h;
    private float _v;
    private float _mouseX;
    private bool _isJumpPressed;

    PlayerStatus status;
    void Start()
    {
        _cm_Player = GetComponentInChildren<CM_Player>();
        status = GetComponent<PlayerStatus>();
        Transform _myTransform = transform;
        _rb = GetComponent<Rigidbody>();
        rotateSpeed = _cm_Player.turnSpeed;
    }

    void Update()
    {
        if (status != null && status.IsDead)
        {
            return;
        }

        _mouseX = Input.GetAxis("Mouse X");
        _h = Input.GetAxisRaw("Horizontal");
        _v = Input.GetAxisRaw("Vertical");

       
        if (Input.GetKeyDown(KeyCode.Space) && _jumpCount > 0)
        {
            _isJumpPressed = true;
        }

        Anim();
    }

    void FixedUpdate()
    {
        if (status != null && status.IsDead)
        {
            return;
        }
        CheckGround();
        RotatePlayer();
        Move();
        Jump();

        //떨어지는 속도 유지
        //FallSpeed();
        // 미끄러짐 방지 로직 실행
        PreventSliding();
    }

    void PreventSliding()
    {
        if (groundHit)
        {
            // Y축(중력) 속도는 유지
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }
    }

    void CheckGround()
    {
        groundHit = Physics.Raycast(_rayPosition.position, Vector3.down, out RaycastHit hit, 0.1f, _groundLayer);
         //Debug.DrawRay(_rayPosition.position, Vector3.down * 0.1f, Color.red);

        // 바닥 판정을 하여 점프 횟수를 초기화
        if (groundHit && _rb.linearVelocity.y <= 0.1f)
        {
            _jumpCount = _jumpMaxCount;
        }
    }

    void Jump()
    {
        if (_isJumpPressed)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);

            _jumpCount--;
            _isJumpPressed = false; 
        }
    }

    void RotatePlayer()
    {
        //좌우 회전
        Quaternion turnOffset = Quaternion.Euler(0, _mouseX * rotateSpeed * Time.fixedDeltaTime, 0);
        _rb.MoveRotation(_rb.rotation * turnOffset);
    }

    void Move()
    {
        Vector3 dir = new Vector3(_h, 0, _v);
        dir.Normalize();

        dir = transform.TransformDirection(dir);
        _rb.MovePosition(_rb.position + (dir * _moveSpeed * Time.fixedDeltaTime));
    }

    void Anim()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        _anim.SetFloat("Y", y);
        _anim.SetFloat("X", x);
    }
}