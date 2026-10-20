using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class EnergyBall : MonoBehaviour
{
    [Header("에너지볼 설정")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private int _maxBounces = 8;
    [SerializeField] private float _lifeTime = 30f;

    private int _currentBounces = 0;
    private Rigidbody _rb;
    private bool _isDestroyed = false; // 중복 파괴 방지용
    private PortalableObject _portalObj;
    // 발사기나 수신기가 이 이벤트를 구독해서 파괴 시점을 알 수 있음
    public Action OnBallDestroyed;

    [SerializeField] private AudioClip _actionSound; // 재생할 특정 소리
    private AudioSource _sfxSource;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _portalObj = GetComponent<PortalableObject>();
        _sfxSource = GetComponent<AudioSource>();

        // 중력 무시, 회전 고정
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;


    }

    private void Start()
    {
        Destroy(gameObject, _lifeTime); //30초후 삭제
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_portalObj != null && _portalObj.IsInPortal)
        {
            // 포탈에 진입 중이라면 벽에 부딪혀도 튕기지 않고 무시합니다.
            return;
        }
        // 플레이어와 닿았을 때
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out PlayerStatus player))
            {
                player.TakeDamage(100f); // 즉사 데미지
            }
            TriggerDestroy();
            return;
        }

        // 벽이나 바닥과 닿았을 때
        Bounce(collision.contacts[0].normal);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = transform.forward * _speed;
    }

    private void Bounce(Vector3 hitNormal)
    {
        _sfxSource.PlayOneShot(_actionSound);
        _currentBounces++;

        if (_currentBounces > _maxBounces)
        {
            TriggerDestroy();
            return;
        }

        // 공이 아예 그 방향을 바라보도록 100% 회전
        Vector3 reflectDir = Vector3.Reflect(transform.forward, hitNormal).normalized;
        transform.rotation = Quaternion.LookRotation(reflectDir);
    }

    // 수신기에서 볼을 잡았을 때 호출할 함수
    public void CatchBall()
    {
        TriggerDestroy();
    }

    // 실제 파괴 처리를 담당하는 내부 함수
    private void TriggerDestroy()
    {
        if (_isDestroyed) return;

        _isDestroyed = true;
        OnBallDestroyed?.Invoke(); //+= 로 구독해 둔 모든 함수들이 자동으로 일제히 실행
        Destroy(gameObject);
    }

    // 30초 뒤에 Destroy가 알아서 실행될 때를 대비한 예외 처리
    private void OnDestroy()
    {
        if (!_isDestroyed)
        {
            _isDestroyed = true;
            OnBallDestroyed?.Invoke();
            //+=로 구독해 둔 모든 함수들이 자동으로 일제히 실행
        }
    }
}