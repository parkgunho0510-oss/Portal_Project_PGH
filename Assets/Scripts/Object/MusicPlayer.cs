using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private GameObject _radio;
    private AudioSource _music;
    [SerializeField] private DoorController _targetDoor;

    private bool _isPlaying;

    private void Awake()
    {
        _music = _radio.GetComponent<AudioSource>();
    }

    private void Start()
    {
        _isPlaying = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && _isPlaying == false)
        {
            _music.Play();
            _targetDoor.AddPower();
            _isPlaying = true;
        }
    }
}
