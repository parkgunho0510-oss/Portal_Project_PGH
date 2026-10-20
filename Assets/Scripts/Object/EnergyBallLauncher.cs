using System.Collections;
using UnityEngine;

public class EnergyBallLauncher : MonoBehaviour
{
    [Header("발사기 설정")]
    [SerializeField] private EnergyBall _energyBallPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _reloadTime = 2f;

    [SerializeField] private AudioClip[] _actionSound;
    private AudioSource _sfxSource;

    private bool _isBallActive = false;
    private bool _isStopped = false; // 추가: 영구 정지 플래그

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        ShootBall();
    }

    private void ShootBall()
    {
        // 발사기가 정지되었거나, 이미 공이 있으면 무시
        if (_isStopped || _isBallActive) return;

        _sfxSource.PlayOneShot(_actionSound[0]);
        _isBallActive = true;
        EnergyBall spawnedBall = Instantiate(_energyBallPrefab, _firePoint.position, _firePoint.rotation);
        //에너지볼 생성
        spawnedBall.OnBallDestroyed += IsBallDestroyed;
        //에너지볼이 꺼졌는지 구독(연결)
    }

    private void IsBallDestroyed()
    {
        //예외처리
        if (this == null || _isStopped ) return;
        //볼이 파괴된 상태면 다시 장전후 발사
        _isBallActive = false;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _sfxSource.PlayOneShot(_actionSound[1]);
        yield return new WaitForSeconds(_reloadTime);
        ShootBall();
    }

    // 수신기에서 호출할 정지 명령 함수
    public void StopLauncher()
    {
        _isStopped = true;
        StopAllCoroutines();
        enabled = false;
    }

    private void OnDestroy()
    {
        // 씬이 넘어가거나 발사기가 파괴될 때, 코루틴을 모두 끄기
        StopAllCoroutines();
    }
}