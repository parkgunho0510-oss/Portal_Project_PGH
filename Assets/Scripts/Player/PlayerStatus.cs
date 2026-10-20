using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위한 네임스페이스

public class PlayerStatus : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private GameObject _deadBall;
    private float _currentHealth;

    [Header("포탈건 설정")]
    [SerializeField] private GameObject _portalGunObject; // 포탈 슈터 스크립트가 있는 오브젝트
    [SerializeField] private bool _hasPortalGun;

    public bool IsDead => _isDead; // 외부에서 죽었는지 확인할 수 있는 프로퍼티
    private bool _isDead = false;

    [SerializeField] private AudioClip[] _actionSound; // 재생할 특정 소리
    private AudioSource _sfxSource;

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _deadBall.SetActive(false);
        _currentHealth = _maxHealth;
        UpdatePortalGunState();
    }

    // 플레이어에게 데미지를 입힐 때 호출할 함수
    public void TakeDamage(float damage)
    {
        // 이미 죽은 상태라면 추가 데미지 및 사망 로직 무시
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log("현재 체력: "+ _currentHealth);

        if (_currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }
    // 사망 연출
    IEnumerator DieRoutine()
    {
        _isDead = true;
        _portalGunObject.SetActive(false);
        _deadBall.SetActive(true);

        Debug.Log("플레이어를 폐기");

        _sfxSource.PlayOneShot(_actionSound[1], 0.2f);


        yield return new WaitForSeconds(2f); // 2초 대기

        // 씬 다시 불러오기
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);

    }

    // 포탈건을 얻었을 때 호출할 함수
    public void AcquirePortalGun()
    {
        _hasPortalGun = true;
        UpdatePortalGunState();
    }

    private void UpdatePortalGunState()
    {
        if (_portalGunObject != null)
        {
            _portalGunObject.SetActive(_hasPortalGun);
            if(_hasPortalGun)
            _sfxSource.PlayOneShot(_actionSound[0], 0.5f);

        }
        else
        {
            Debug.LogWarning("포탈건 오브젝트가 인스펙터에 할당 없음");
        }
    }
}