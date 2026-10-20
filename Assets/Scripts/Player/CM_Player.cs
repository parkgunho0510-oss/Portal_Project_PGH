using UnityEngine;

public class CM_Player : MonoBehaviour
{
    [Header("Camera Settings")]
    public float turnSpeed = 60f;
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 90f;
    private float tempY;

    private void Awake()
    {
        // 마우스 커서를 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // 위아래로 회전 로직
        float mouseY = Input.GetAxis("Mouse Y");

        tempY -= mouseY * turnSpeed * Time.deltaTime;

        tempY = Mathf.Clamp(tempY, minPitch, maxPitch);

        transform.localEulerAngles = Vector3.right * tempY;
    }
}