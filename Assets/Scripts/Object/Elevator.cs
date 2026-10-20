using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int _nextScene = 2;

    private Animator _animator;

    private readonly int _isOpen = Animator.StringToHash("is_open");

    private bool _isTriggered = false;


    [SerializeField] private CinemachineCamera _playerCam; // 플레이어 카메라 할당
    private CinemachineBasicMultiChannelPerlin _camNoise;

    [SerializeField] private AudioClip[] _actionSound; // 재생할 특정 소리
    private AudioSource _sfxSource;
    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();

        if (_playerCam != null)
        {
            _camNoise = _playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

            _camNoise.AmplitudeGain = 0f;
            _camNoise.FrequencyGain = 0f;
        }
    }

    private void Start()
    {
        // 1. 처음에는 무조건 false
        _animator.SetBool(_isOpen, false);

        StartCoroutine(OpenElevatorRoutine());
    }

    private IEnumerator OpenElevatorRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        _sfxSource.PlayOneShot(_actionSound[1]);
        yield return new WaitForSeconds(2f);
        _sfxSource.PlayOneShot(_actionSound[0]);
        _animator.SetBool(_isOpen, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 아니면 무시
        if (!other.CompareTag("Player")) return;

        //  중복 실행 방지
        if (_isTriggered) return;

        _isTriggered = true; // 트리거 잠금
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        // 트리거에 닿자마자 백그라운드에서 다음 씬 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_nextScene);
        asyncLoad.allowSceneActivation = false;
        // 씬 로딩이 완료되어도 자동으로 넘어가지 않도록 막아둠 (90%에서 대기)

        //// === 엘리베이터 문 닫힘 연출 시작 ===
        _sfxSource.PlayOneShot(_actionSound[0]);
        _animator.SetBool(_isOpen, false);

        _sfxSource.PlayOneShot(_actionSound[1]);
        yield return new WaitForSeconds(2f);

        // === 엘리베이터 흔들림 연출 시작 ===
        _camNoise.AmplitudeGain = 0.2f;
        _camNoise.FrequencyGain = 1.0f;

        _sfxSource.PlayOneShot(_actionSound[2]);
        yield return new WaitForSeconds(3f);

        // 연출 종료 후 카메라 초기화
        _camNoise.AmplitudeGain = 0f;
        _camNoise.FrequencyGain = 0f;

        // 만약 연출이 끝났는데도 다음 씬 로딩이 덜 끝났다면 마저 기다림
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        // 씬 이동
        asyncLoad.allowSceneActivation = true;
    }
}