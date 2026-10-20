using UnityEngine;

public class PortalShooter : MonoBehaviour
{
    [Header("포탈 설정")]
    [SerializeField] private GameObject _bluePortal;
    [SerializeField] private GameObject _orangePortal;
    [SerializeField] private float _shootRange = 100f;
   private LayerMask _wallLayer;

    [Header("참조")]
    [SerializeField] private PlayerGrab _playerGrab;

    private AudioSource _sfxSource;

    private Transform _mainCamTransform; // 카메라 호출 고정

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
        _wallLayer = LayerMask.GetMask("Wall", "Ground");
        _mainCamTransform = Camera.main.transform;
    }
    void Update()
    {

        // 객체 들고 있는중 포탈 발사 차단
        if (_playerGrab != null && _playerGrab._IsHoldingObj)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _sfxSource.Play();
            Debug.DrawRay(_mainCamTransform.position, _mainCamTransform.forward * _shootRange, Color.blue, 2f);
            ShootPortal(_bluePortal);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _sfxSource.Play();
            Debug.DrawRay(_mainCamTransform.position, _mainCamTransform.forward * _shootRange, Color.orange, 2f);
            ShootPortal(_orangePortal);
        }
    }

    private void ShootPortal(GameObject portal)
    {
        if (portal == null) return;

        // 카메라 정중앙 조준점으로 레이캐스트 발사
        Ray ray = new Ray(_mainCamTransform.position, _mainCamTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _shootRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // 맞은 곳이 Wall이나 Ground 레이어인지 확인
            if ((_wallLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
            {
                // 반대쪽 포탈이랑 너무 가까우면 (2.5f 이내) 겹침 버그 생기니까 무시
                GameObject otherPortal = (portal == _bluePortal) ? _orangePortal : _bluePortal;
                if (otherPortal.activeInHierarchy && Vector3.Distance(hit.point, otherPortal.transform.position) < 2.5f)
                {
                    Debug.Log("포탈이 너무 가까움");
                    return;
                }

                // 부모를 아예 해제해서 스케일 간섭을 완전히 끊어버림
                portal.transform.SetParent(null);

                Vector3 portalUp = Vector3.up;

                if (Mathf.Abs(hit.normal.y) > 0.9f) //바닥이나 천장인 경우(벡터의 Y 절댓값이 1에 가까운지 확인)
                {
                    // 카메라가 바라보는 전방  방향을 바닥 평면에 투영시켜 포탈의 위쪽(Up) 방향으로 설정
                    portalUp = Vector3.ProjectOnPlane(_mainCamTransform.forward, hit.normal).normalized;

                    // 플레이어가 바닥을 완전히 수직으로 내려다볼 경우 벡터가 0이 되는 문제 방지
                    if (portalUp.sqrMagnitude < 0.001f)
                    {
                        // 카메라의 위쪽 벡터를 대신 평면에 투영하여 사용
                        portalUp = Vector3.ProjectOnPlane(_mainCamTransform.up, hit.normal).normalized;
                    }
                }

                // 포탈이 바라볼 최종 회전값을 먼저 구하기
                Quaternion portalRotation = Quaternion.LookRotation(hit.normal, portalUp);
               
                
                // 4모서리 검사 로직(공간이 부족하면 설치 취소)
                float halfWidth = 0.9f; // 포탈 가로 크기의 절반
                float halfHeight = 1.35f; // 포탈 세로 크기의 절반
                // 회전값에 기반한 상하좌우 방향 벡터 추출
                Vector3 pRight = portalRotation * Vector3.right;
                Vector3 pUp = portalRotation * Vector3.up;

                // 4개의 모서리 좌표 계산
                Vector3[] corners = new Vector3[4];
                corners[0] = hit.point + (pUp * halfHeight) + (pRight * halfWidth); // 우상단
                corners[1] = hit.point + (pUp * halfHeight) - (pRight * halfWidth); // 좌상단
                corners[2] = hit.point - (pUp * halfHeight) + (pRight * halfWidth); // 우하단
                corners[3] = hit.point - (pUp * halfHeight) - (pRight * halfWidth); // 좌하단

                // 모서리에서 벽 쪽으로 쏴볼 짧은 레이저 길이
                float checkDistance = 0.2f;
                bool canPlace = true;

                foreach (Vector3 corner in corners)
                {
                    // 모서리 위치에서 벽의 수직 방향으로 살짝(0.1f) 띄운 곳에서 시작하여, 벽 쪽(-hit.normal)으로 쏩니다.
                    Vector3 rayStart = corner + (hit.normal * 0.1f);

                    // 디버그용으로 씬 뷰에서 선을 그려줍니다 (확인 후 지우셔도 됩니다)
                    Debug.DrawRay(rayStart, -hit.normal * checkDistance, Color.red, 2f);

                    if (Physics.Raycast(rayStart, -hit.normal, out RaycastHit cornerHit, checkDistance, _wallLayer))
                    {
                        // 맞은 벽이 처음에 맞은 벽(hit.collider)과 다르면 모서리가 다른 벽에 걸친 것
                        if (cornerHit.collider != hit.collider)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    else
                    {
                        // 아예 아무것도 안 맞았다면 허공으로 삐져나간 것
                        canPlace = false;
                        break;
                    }
                }

                // 4모서리 중 하나라도 조건에 안 맞으면 설치 중단
                if (!canPlace)
                {
                    Debug.Log("설치할 수 없습니다.");
                    return;
                }

                portal.transform.SetParent(null);
                portal.transform.rotation = portalRotation; // 미리 구해둔 회전값 적용
                portal.transform.position = hit.point + (hit.normal * 0.01f);

                // localScale을 원래 포탈 크기로 고정됨
                portal.transform.localScale = Vector3.one;

                portal.SetActive(true); // 세팅 끝 포탈 켜기

                // PortalCamera 쪽에 맞은 벽 콜라이더 정보 바로 넘겨줌
                PortalCamera pCam = portal.GetComponent<PortalCamera>();
                if (pCam != null)
                {
                    pCam.SetWallCollider(hit.collider);
                }

            }
        }
    }


}