using UnityEngine;
using UnityEngine.Rendering;

public class PortalCamera : MonoBehaviour
{
    [Header("연결할 포탈")]
    [SerializeField] private PortalCamera _targetPortal;
    public PortalCamera TargetPortal => _targetPortal;
    public Collider PlacedWallCollider { get; private set; }

    [Header("머티리얼 설정")]
    [SerializeField] private Material _conMat;
    [SerializeField] private Material _disMat;

    [Header("포탈 표면 스크린")]
    [SerializeField] private MeshRenderer _portalRenderer;

    private Camera _virtualCam;
    private Camera _mainCam;
    private RenderTexture _portalTexture;
    private bool _isConnected = false;

    private AudioSource _sfxSource;
    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
        //캐싱
        _mainCam = Camera.main;
        _virtualCam = GetComponentInChildren<Camera>(true);
        SetRenderTexture();
    }

    private void Start()
    {
        // 미리 배치해둔 포탈일 경우 부모 벽의 콜라이더 탐색
        if (PlacedWallCollider == null && transform.parent != null)
        {
            if (transform.parent.TryGetComponent(out Collider parentCol))
            {
                SetWallCollider(parentCol);
            }
        }
    }

    private void SetRenderTexture()
    {
        // 화면 해상도에 딱 맞춰서 포탈에 띄워줄 렌더 텍스처 생성
        if (_virtualCam != null && _portalRenderer != null)
        {
            _portalTexture = new RenderTexture(Screen.width, Screen.height, 24);
            _virtualCam.targetTexture = _portalTexture;
            _portalRenderer.material.SetTexture("_PortalTexture", _portalTexture);
        }
    }

    private void OnDestroy()
    {
        // 텍스처 다 쓰고 파괴할 때 꼭 풀어줘야 함
        if (_portalTexture != null)
        {
            _portalTexture.Release();
            Destroy(_portalTexture);
        }
    }
    private void Update()
    {
        // 실제 렌더링에 사용되는 _mainCam의 픽셀 크기를 기준
        if (_portalTexture != null && _mainCam != null &&
           (_portalTexture.width != _mainCam.pixelWidth || _portalTexture.height != _mainCam.pixelHeight))
        {
            _portalTexture.Release();
            SetRenderTexture();
        }

        if (_portalRenderer == null) return;

        bool checkConnected = _targetPortal != null && _targetPortal.gameObject.activeInHierarchy;

        // 상태가 달라졌을 때만 갱신
        if (checkConnected != _isConnected)
        {
            PortalState();
        }
    }


    //머티리얼과 카메라 상태를 갱신하는 전용 메서드 생성
    private void PortalState()
    {
        if (_portalRenderer == null) return;

        bool checkConnected = _targetPortal != null && _targetPortal.gameObject.activeInHierarchy;

        // 현재 상태에 맞는 머티리얼 적용
        _portalRenderer.material = checkConnected ? _conMat : _disMat;

        // 카메라 활성화 상태도 여기서 같이 제어
        if (_virtualCam != null)
        {
            _virtualCam.enabled = checkConnected;
        }

        if (checkConnected && _portalTexture != null)
        {
            _portalRenderer.material.SetTexture("_PortalTexture", _portalTexture);
        }

        _isConnected = checkConnected;
    }

    private void OnEnable()
    {
        // URP 렌더링 타이밍에 우리 카메라 업데이트 로직 끼워넣기
        RenderPipelineManager.beginCameraRendering += UpdateCamera;
        PortalState();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateCamera;
    }

    private void UpdateCamera(ScriptableRenderContext context, Camera camera)
    {
        // 메인 카메라가 화면 그릴 때만, 그리고 타겟 포탈 정상 작동할 때만 연산
        if (_mainCam == null || _virtualCam == null || camera != _mainCam) return;
        if (_targetPortal == null || !_targetPortal.gameObject.activeInHierarchy) return;


        // 가상 카메라 위치랑 회전 잡는 핵심 로직

        // 내 카메라랑 포탈 사이의 거리를 계산해서, 반대쪽 포탈 기준 위치로 가상 카메라 이동
        Vector3 worldOffset = _mainCam.transform.position - transform.position;
        Quaternion relativeRot = _targetPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(transform.rotation);
        _virtualCam.transform.position = _targetPortal.transform.position + relativeRot * worldOffset;

        // 가상 카메라가 반대쪽 포탈 바깥을 쳐다보게 180도 꺾어서 회전 동기화
        Quaternion reRot = Quaternion.Inverse(transform.rotation) * _mainCam.transform.rotation;
        _virtualCam.transform.rotation = _targetPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * reRot;


        // 포탈 벽 뒤에 있는 오브젝트까지 화면에 렌더링돼서 시야가 다 깨져버림
        // 절단면 설정
        Vector3 clipPoint = _targetPortal.transform.position - _targetPortal.transform.forward * 0.01f;
        //위치 - 이동 = 새로운 위치    //0.01f의 오프셋을 두는 이유 깜빡이는 현상을 방지
        Vector3 clipNormal = _targetPortal.transform.forward;
        Plane portalPlane = new Plane(clipNormal, clipPoint);
        //타겟 포탈의 앞까지만 절단면 설정

        Vector4 clipPlaneWorld = new Vector4(portalPlane.normal.x, portalPlane.normal.y, portalPlane.normal.z, portalPlane.distance);
        //Plane 평면의 수직 방향을 나타내는 법선 벡터 그리고 공간의 원점으로부터 평면까지의 거리 가져와서 저장
        Matrix4x4 inverseTranspose = Matrix4x4.Transpose(_virtualCam.worldToCameraMatrix.inverse);
        //가상 카메라의 worldToCameraMatrix(월드 공간을 카메라 렌즈 기준의 공간으로 바꾸는 행렬)의 역행렬을 Matrix4x4로 
        Vector4 clipPlaneCameraSpace = inverseTranspose * clipPlaneWorld;
        //계산한 역전치 행렬과 월드 4차원 절단면 벡터를 곱셈 연산 =
        //가상 카메라의 렌즈 입장에서 볼 때, 정확히 어느 각도와 어느 거리에 놓여 있는지

        // 잘라서 카메라에 적용
        _virtualCam.projectionMatrix = _mainCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    public void SetWallCollider(Collider col)
    {
        PlacedWallCollider = col;

        _sfxSource.Play();

    }
}