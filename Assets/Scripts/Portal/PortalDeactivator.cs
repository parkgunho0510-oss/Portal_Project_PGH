using UnityEngine;

public class PortalDeactivator : MonoBehaviour
{
    [Header("비활성화할 포탈")]
    [SerializeField] private GameObject _bluePortal;
    [SerializeField] private GameObject _orangePortal;
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _source.Play();
            if (_bluePortal != null) _bluePortal.SetActive(false);
            if (_orangePortal != null) _orangePortal.SetActive(false);

            Debug.Log("포탈이 비활성화 되었습니다.");
        }
    }  
}