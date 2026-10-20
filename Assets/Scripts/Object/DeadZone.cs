using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 닿은 물체가 무엇이든 상관없이 씬에 있는 PlayerStatus를 찾습니다.
        // FindAnyObjectByType은 유니티의 최신 내장 API로, 씬 전체에서 해당 컴포넌트를 찾아냅니다.
        PlayerStatus player = FindAnyObjectByType<PlayerStatus>();

        if (player != null && !player.IsDead)
        {
            player.TakeDamage(10000);
        }
    }
}