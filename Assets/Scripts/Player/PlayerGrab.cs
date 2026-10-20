using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    private float _grabDis = 2f;
    [SerializeField] private Transform _holdPoint;  // 물체를 들고 있을 기준 위치
    [SerializeField] private LayerMask _grabLayer;

    [SerializeField] private AudioClip[] _actionSound; // 재생할 특정 소리
    private AudioSource _sfxSource;

    public bool _IsHoldingObj => _obj != null; //외부에서 물체를 들고 있는지 확인하기 위한 프로퍼티

    private float _dropDistance = 3f; //손이랑 멀어지면 놓을 용도

    //들고 있을 오브젝트
    private Rigidbody _obj;
    private Collider _playerCollider;
    private Collider _objCollider;

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        _playerCollider = GetComponentInParent<Collider>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_obj == null) Grab();
            else Drop();
        }
    }

    void FixedUpdate()
    {
        if (_obj != null) //오브젝트 집은 상태일때
        {
            MoveObjectToHoldPos();
            CheckDistanceDrop();
        }
    }

    private void Grab()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _grabDis, _grabLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                _obj = rb;
                _objCollider = _obj.GetComponent<Collider>();
                _sfxSource.PlayOneShot(_actionSound[0], 0.3f);

                // 플레이어와 물체 간의 충돌 연산 무시 (밀림 방지)
                if (_playerCollider != null && _objCollider != null)
                {
                    Physics.IgnoreCollision(_playerCollider, _objCollider, true);
                }

                _obj.useGravity = false;
                _obj.freezeRotation = true;
            }
        }
    }

    public void Drop()
    {
        if (_obj != null)
        {
            _sfxSource.PlayOneShot(_actionSound[1], 0.3f);

            // 충돌 무시복구
            if (_playerCollider != null && _objCollider != null)
            {
                Physics.IgnoreCollision(_playerCollider, _objCollider, false);
            }

            _obj.useGravity = true;
            _obj.freezeRotation = false;

            _obj = null;
            _objCollider = null;
        }
    }

    private void MoveObjectToHoldPos()
    {
        if (_obj == null) return;

        Vector3 holdPosition = _holdPoint.position + (_holdPoint.forward * 1f);
        //물체를 플레이어 앞 + 1f(화면 가림 방지)

        //Vector3 targetPosition = Vector3.Lerp(_obj.position, holdPosition, 10f * Time.deltaTime);
        //_obj.MovePosition(targetPosition);

        Vector3 direction = holdPosition - _obj.position;

        // MovePosition 대신 velocity(속도)를 사용하여 물리 엔진이 충돌을 더 정확히 계산하게 합니다.
        _obj.linearVelocity = Vector3.ClampMagnitude(direction * 30f, 15f);
        // 일정 속도 이상 빨라지지 않게 막음

    }

    private void CheckDistanceDrop() 
    {
        //물체가 멀어지거나 벽에 걸릴때 놓는 코드
        Vector3 holdPosition = _holdPoint.position + (_holdPoint.forward * 1f);
        float distance = Vector3.Distance(_obj.position, holdPosition);

        if (distance > _dropDistance)
        {
            _obj.linearVelocity = Vector3.zero;
            Drop();
        }
    }
}