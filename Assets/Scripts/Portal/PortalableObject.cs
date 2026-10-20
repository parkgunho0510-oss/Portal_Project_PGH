using UnityEngine;

// Rigidbody랑 Collider는 무조건 있어야 됨 (없으면 알아서 붙여줌)
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PortalableObject : MonoBehaviour
{

    [SerializeField] private AudioClip _actionSound; // 재생할 특정 소리
    private AudioSource _sfxSource;

    private GameObject _cloneObj;
    private Rigidbody _rb;
    private Collider[] _cols;
    private bool _isPlayer;

    // 어느 포탈에 들어가 있는지 기억할 변수
    private PortalCamera _inPortal;
    public bool IsInPortal => _inPortal != null;

    private void Awake()
    {
        //캐싱
        _rb = GetComponent<Rigidbody>();
        _cols = GetComponentsInChildren<Collider>();
        _isPlayer = CompareTag("Player");
        if(_actionSound != null)
          TryGetComponent<AudioSource>(out _sfxSource);

        CreateClone();
    }

    private void CreateClone()
    {
        if (_isPlayer) return;

        // 클론 생성
        _cloneObj = new GameObject(name + "_PortalClone");
        _cloneObj.SetActive(false);

        _cloneObj.transform.localScale = transform.localScale;

        // 외형 투입
        // 나 자신을 포함한 모든 자식 오브젝트의 MeshRenderer를 다 찾아냄
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var rend in renderers)
        {
            MeshFilter filter = rend.GetComponent<MeshFilter>();
            if (filter != null)
            {
                // 렌더러마다 클론의 자식으로 빈 껍데기 오브젝트를 생성
                GameObject meshObj = new GameObject(rend.name + "_Mesh");
                meshObj.transform.SetParent(_cloneObj.transform);

                // 원본 껍데기의 지역 위치와 회전값을 클론 껍데기에 완벽히 똑같이 맞춰줌
                meshObj.transform.localPosition = rend.transform.localPosition;
                meshObj.transform.localRotation = rend.transform.localRotation;
                meshObj.transform.localScale = rend.transform.localScale;

                // 외형과 재질(Material) 복사
                meshObj.AddComponent<MeshFilter>().mesh = filter.mesh;
                meshObj.AddComponent<MeshRenderer>().materials = rend.materials;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 닿은게 포탈 카메라고, 그 포탈에 타겟 포탈이 정상적으로 켜져 있다면
        if (other.TryGetComponent(out PortalCamera portal) && portal.TargetPortal != null &&
        portal.TargetPortal.gameObject.activeInHierarchy)
        {
            Vector3 offset = transform.position - portal.transform.position;

            // 살짝 빗겨맞아도 탄 걸로 침
            if (Vector3.Dot(offset, portal.transform.forward) > -0.6f)
            {
                _inPortal = portal; // 현재 들어간 포탈 기억
                InPortal();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 포탈에서 완전히 빠져나왔으면 상태 초기화
        if (other.TryGetComponent(out PortalCamera portal) && _inPortal == portal)
        {
            ExitPortal();
            _inPortal = null;
        }
    }

    private void LateUpdate()
    {
        // 포탈에 안 들어가 있거나 반대쪽 포탈 꺼져있으면 걍 리턴
        if (_inPortal == null || _inPortal.TargetPortal == null || !_inPortal.TargetPortal.gameObject.activeInHierarchy) return;

        UpdateClone(); // 클론은 계속 반대쪽에서 움직이게 둠

       

    }

   private void FixedUpdate() {
        Funneling();

        if (_inPortal == null || _inPortal.TargetPortal == null || !_inPortal.TargetPortal.gameObject.activeInHierarchy) return;
        
        Vector3 offset = transform.position - _inPortal.transform.position;

        // 내적(Dot) 계산해서 포탈 중심선 넘어가면(< 0f) 바로 텔레포트 쏴버림
        if (Vector3.Dot(offset, _inPortal.transform.forward) < 0f)
        {
            PortalCamera nextPortal = _inPortal.TargetPortal;
            //포착할 포탈 미리 저장 
            Teleport();
            ExitPortal();

            _inPortal = nextPortal;
            InPortal();
        }
    }

    private void InPortal()
    {
        //포탈에 들어가 있을때
        if (_cloneObj != null)
        {
            _cloneObj.SetActive(true);
        }


        Collider wallCol = _inPortal.PlacedWallCollider;
        if (wallCol != null)
        {
            foreach (var col in _cols)
            {
                Physics.IgnoreCollision(col, wallCol, true);
            }
        }
    }

    private void ExitPortal()
    {
        if (_cloneObj != null)
        {
            _cloneObj.SetActive(false);
        }// 클론 숨김

        Collider wallCol = _inPortal.PlacedWallCollider;
        if (wallCol != null)
        {
            // 포탈 다 탔으니까 벽이랑 다시 부딪히게 원상복구
            foreach (var col in _cols)
            {
                Physics.IgnoreCollision(col, wallCol, false);
            }
        }
    }

    private void UpdateClone()
    {
       
        if ( _cloneObj == null || !_cloneObj.activeSelf ) return;

        // 내 현재 위치랑 회전값을 반대쪽 포탈 기준으로 변환해서 클론한테 덮어씌움 (시각적 훼이크)
        Vector3 rePos = _inPortal.transform.InverseTransformPoint(transform.position);
        Vector3 exitPos = new Vector3(-rePos.x, rePos.y, -Mathf.Abs(rePos.z)); // z는 무조건 타겟 포탈 뒤

        Quaternion reRot = Quaternion.Inverse(_inPortal.transform.rotation) * transform.rotation;

        _cloneObj.transform.position = _inPortal.TargetPortal.transform.TransformPoint(exitPos);
        _cloneObj.transform.rotation = _inPortal.TargetPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * reRot;
    }

    private void Teleport()
    {
        // 진짜 몸통을 반대쪽 포탈로 넘김
        Vector3 rePos = _inPortal.transform.InverseTransformPoint(transform.position);
        //InverseTransformPoint : 월드 좌표(현재 실제 위치)를 진입 포탈 기준의 **로컬 좌표(상대 위치)**로 변환

        // 다시 타버리는 무한 텔포 버그 막으려고 앞쪽으로 살짝(0.5f) 밀어줌
        Vector3 exitPos = new Vector3(-rePos.x, rePos.y, -rePos.z + 0.5f);
        Vector3 finalPosition = _inPortal.TargetPortal.transform.TransformPoint(exitPos);
        //TransformPoint : 계산된 로컬 좌표를 출구 포탈 기준의 월드 좌표(최종 텔레포트 위치)로 변경

        Quaternion reRot = Quaternion.Inverse(_inPortal.transform.rotation) * transform.rotation;
        Quaternion targetRot = _inPortal.TargetPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * reRot;

        Vector3 incomingVelocity = _rb.linearVelocity; //켜기 '전'에 현재 속도를 미리 저장

        //transform.position 대신 Rigidbody 사용
        _rb.isKinematic = true; //isKinematic을 잠깐 켜서 유니티 물리 연산 끔

        _rb.position = finalPosition; // 연산이 꺼진 안전한 상태에서 오브젝트를 출구 좌표로 즉시 이동

        Physics.SyncTransforms();//물리 엔진에 콜라이더 위치가 완전히 변경되었음을 즉시 갱신

        // 좌우 회전만 적용. 오브젝트는 상관 없음
        if (_isPlayer)
        {
            _rb.rotation = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);
        }
        else
        {
            _rb.rotation = targetRot;
        }

        if(_sfxSource != null && _actionSound != null)
        {
            _sfxSource.PlayOneShot(_actionSound, 0.3f);

        }

        _rb.isKinematic = false;

        // 들어올 때의 속도(linearVelocity) 방향을 꺾어서 반대쪽 포탈 밖으로 쏴줌
        Vector3 reVelocity = _inPortal.transform.InverseTransformDirection(incomingVelocity);
        Vector3 exitVel = new Vector3(reVelocity.x, reVelocity.y, -reVelocity.z);
        _rb.linearVelocity = _inPortal.TargetPortal.transform.TransformDirection(exitVel);
    }

    //새롭게 추가된 깔때기(Funneling) 함수
    private void Funneling()
    {
        // 아래로 꽤 빠르게 떨어지는 중(예: y속도 -3 이하)이 아니면 무시
        if (_rb.linearVelocity.y > -20f) return;

        // 플레이어가 WASD를 누르고 있다면 개입 안 함 (유저 조작 우선)
        if (_isPlayer)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) return;
        }
        // 발밑 5미터 아래까지 반경 1f짜리 구체를 쏴서 포탈이 있는지 넉넉하게 탐지
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1.5f, Vector3.down, 5f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            // 맞은 것들 중에 포탈이 있는지 확인
            if (hit.collider.TryGetComponent(out PortalCamera floorPortal))
            {
                // 포탈이 바닥에 깔려 위를 보고 있는지 확인 (내적이 0.8 이상)
                if (Vector3.Dot(floorPortal.transform.forward, Vector3.up) > 0.8f)
                {
                    // 포탈 중심을 향한 방향 벡터 계산 
                    Vector3 centerDir = floorPortal.transform.position - transform.position;
                    centerDir.y = 0; // (높이 Y는 당기면 안 되므로 0으로 무시)

                    // 중심과 거리가 살짝 어긋나 있을 때만 당겨줌
                    if (centerDir.sqrMagnitude > 0.01f)
                    {  // sqrMagnitude : 거리 값을 구하는 연산

                        // 수평 속도를 포탈 중심 쪽(targetVel)으로
                        Vector3 targetVel = centerDir.normalized * 10f; // 끌어당기는 수평 속도 강도

                        // 현재 오브젝트의 수평 속도만 추출 (Y축 무시)
                        Vector3 currentHorizontalVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

                        //  얼마나 빨리 빨려들어갈지를 결정
                        Vector3 newHorizontalVel = Vector3.MoveTowards(currentHorizontalVel, targetVel, 300f * Time.fixedDeltaTime);

                        //빨려듦
                        _rb.linearVelocity = new Vector3(newHorizontalVel.x, _rb.linearVelocity.y, newHorizontalVel.z);

                        return;

                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 원본 오브젝트가 파괴될 때 생성했던 클론도 파괴
        if (_cloneObj != null)
        {
            Destroy(_cloneObj);
        }
    }
}