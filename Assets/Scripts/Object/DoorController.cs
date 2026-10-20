using UnityEngine;
using UnityEngine.Rendering;


public class DoorController : MonoBehaviour
{
    [SerializeField] private GameObject _left;
    [SerializeField] private GameObject _right;
    private Collider _lCol;
    private Collider _rCol;

    private float _moveDistance = 2.5f; //이동 거리 = 속도 * 시간
    private float _moveTime = 1f;    //이동하는데 걸리는 시간
    [SerializeField] private int _ButtonPressed = 1;
    private int _ButtonCount = 0;
    bool _isOpen = false;
    private bool _wasOpen = false; // 이전 문의 상태를 저장할 변수

    Vector3 _originPosR; //시작점
    Vector3 _destPosR; //목적지
    Vector3 _originPosL; //시작점
    Vector3 _destPosL; //목적지
    // 현재 진행도 (0 = 닫힘, 1 = 완전히 열림)

    float _timer = 0f;

    private AudioSource myAudio;

    private void Awake()
    {
        _lCol = _left.GetComponent<Collider>();
        _rCol = _right.GetComponent<Collider>();

        myAudio = GetComponent<AudioSource>();
    }
    void Start()
    {
        _lCol.enabled = true;
        _rCol.enabled = true;

        _originPosR = transform.position;
        _destPosR = _originPosR + transform.right * _moveDistance;
        _originPosL = transform.position;
        _destPosL = _originPosL - transform.right * _moveDistance;
    }

    void Update()
    {
        _isOpen = (_ButtonCount >= _ButtonPressed);

        if (_isOpen != _wasOpen)
        {
            if (_isOpen)
            {
                myAudio.Play();
                _lCol.enabled = false;
                _rCol.enabled = false;
            }
            else
            {
                _lCol.enabled = true;
                _rCol.enabled = true;
                myAudio.Play();

            }
            _wasOpen = _isOpen;
        }
        float targetOpen = _isOpen ? 1f : 0f;

        // 시간 = (1 / 목표 시간) * 델타타임
        float t = 1f / _moveTime * Time.deltaTime;

        _timer = Mathf.MoveTowards(_timer, targetOpen, t);
        //결과값 = Mathf.MoveTowards(현재위치, 목표 (0 ~ 1), 움직인 시간)

        _right.transform.position = Vector3.Lerp(_originPosR, _destPosR, _timer);
        _left.transform.position = Vector3.Lerp(_originPosL, _destPosL, _timer);

        //목표 위치로 이동 _timer가 다시 줄어듦
    }



    public void AddPower()
    {
        _ButtonCount++;
    }

    public void RemovePower()
    {
        _ButtonCount = Mathf.Max(0, _ButtonCount - 1);
        //0아래론 안 내려가게
    }
}