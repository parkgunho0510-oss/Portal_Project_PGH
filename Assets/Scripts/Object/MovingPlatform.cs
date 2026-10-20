using TMPro;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 3f; // 움직임 거리
    [SerializeField] private float _moveSpeed = 2f;    // 속도
    [SerializeField] private Vector3 _moveDirection = Vector3.right; // 이동 방향

    private Vector3 _originPos;
    private Rigidbody _rb;

    void Start()
    {
        _originPos = transform.position;
        _rb = GetComponent<Rigidbody>();
    }

    // 물리 기반 이동은 FixedUpdate
    void FixedUpdate()
    {
        // PingPong을 이용한 거리 계산
        float offset = Mathf.PingPong(Time.time * _moveSpeed, _moveDistance);

        // 새로운 목표 위치 계산
        Vector3 localDirInWorld = transform.TransformDirection(_moveDirection.normalized);
        Vector3 targetPosition = _originPos + (localDirInWorld * offset);

        // Rigidbody를 통해 이동
        _rb.MovePosition(targetPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //f (collision.gameObject.CompareTag("Player"))
        // 플레이어를 플랫폼의 자식으로 설정하여 상대적 위치 유지
        collision.transform.SetParent(this.transform);
    }

    private void OnCollisionExit(Collision collision)
    {
       // if (collision.gameObject.CompareTag("Player"))
            // 자식 해제
            collision.transform.SetParent(null);
    }
}