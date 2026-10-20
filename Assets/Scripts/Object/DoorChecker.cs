using UnityEngine;

public class DoorChecker : MonoBehaviour
{
    [Header("렌더러 및 머티리얼 설정")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Material[] _materials;


    private void Start()
    {
        CheckerPower(false);
    }
    public void CheckerPower(bool hasPower)
    {
        // true면 1번 머티리얼(ON), false면 0번 머티리얼(OFF)
        int materialIndex = hasPower ? 1 : 0;
        _meshRenderer.material = _materials[materialIndex];
    }
}