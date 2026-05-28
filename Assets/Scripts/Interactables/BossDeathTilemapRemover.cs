using UnityEngine;
using UnityEngine.Tilemaps;

public class BossDeathTilemapRemover : MonoBehaviour
{
    [Header("Boss")]
    [Tooltip("Drag the Golem boss GameObject here")]
    [SerializeField] private EnemyControllerBase boss;

    [Header("Tilemaps to Remove on Boss Death")]
    [SerializeField] private Tilemap wallTilemap1;
    [SerializeField] private Tilemap wallTilemap2;

    private void Start()
    {
        if (boss != null)
            boss.OnDefeated += HandleBossDefeated;
        else
            Debug.LogWarning("[BossDeathTilemapRemover] Boss reference is not assigned.");
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.OnDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated(EnemyControllerBase defeated)
    {
        if (wallTilemap1 != null)
            wallTilemap1.gameObject.SetActive(false);

        if (wallTilemap2 != null)
            wallTilemap2.gameObject.SetActive(false);
    }
}
