using UnityEngine;

public class PortalGunItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 대상이 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어 오브젝트에 PlayerStatus 스크립트가 확실히 있는지 검사 후 실행
            if (other.TryGetComponent(out PlayerStatus playerStatus))
            {
                // 포탈건 획득 함수 호출
                playerStatus.AcquirePortalGun();
                // 획득 처리 후 아이템 오브젝트 파괴
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("PlayerStatus 스크립트를 찾을 수 없습니다");
            }
        }
    }
}