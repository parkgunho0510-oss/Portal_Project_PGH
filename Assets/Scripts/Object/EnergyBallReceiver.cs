using UnityEngine;

public class EnergyBallReceiver : MonoBehaviour
{
    [Header("연결할 장치")]
    [SerializeField] private DoorController _targetDoor;
    [SerializeField] private EnergyBallLauncher _linkedLauncher;
    [SerializeField] private DoorChecker _targetChecker;

    private bool _isActivated = false;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated) return;

        if (other.TryGetComponent(out EnergyBall energyBall))
        {
            _isActivated = true;
            Debug.Log("에너지볼 수신");

            // 발사기에게 정지 명령
            if (_linkedLauncher != null)
            {
                _linkedLauncher.StopLauncher();
            }

            // 문에 파워 공급
            if (_targetDoor != null)
            {
                _audioSource.Play();
                _targetDoor.AddPower();

                if (_targetChecker != null)
                {
                    _targetChecker.CheckerPower(true);
                }
            }

            energyBall.CatchBall();
        }
    }
}