using Unity.VisualScripting;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [SerializeField] private DoorController targetDoor;
    private int _pressCount = 0; //누르고 있는 물체 수
                                 // 밟았을 때
    [SerializeField] private DoorChecker _targetChecker;

    private AudioSource  myAudio;

   [SerializeField]  private Animator _anim;
    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box") || other.CompareTag("Player"))
        {
            if (_pressCount == 0)
            { //처음 눌린 순간에
                _anim.SetBool("Pressed", true);
                targetDoor.AddPower();
                myAudio.Play();
                if (_targetChecker != null)
                {
                    _targetChecker.CheckerPower(true);
                }
            }
            _pressCount++;
        }
    }

    // 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box") || other.CompareTag("Player"))
        {
            _pressCount--;
            if (_pressCount == 0)
            { //모튼 물체가 지워질때만
                targetDoor.RemovePower();
                _anim.SetBool("Pressed", false);
                if (_targetChecker != null)
                {
                    _targetChecker.CheckerPower(false);
                }
            }
        }
    }
}