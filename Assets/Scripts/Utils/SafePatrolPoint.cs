using UnityEngine;

public static class SafePatrolPoint
{
    public static bool TryGetSafePatrolPoint(Vector3 center, float patrolRadius, out Vector3 safePos)
    {
        const int MaxTry = 20;

        for (int i = 0; i < MaxTry; i++)
        {
            // 랜덤 포인트 생성
            Vector2 offset = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = center + (Vector3)offset;
            candidate.z = 0;

            // Confiner 내부 체크
            if (!BuildManager.Instance.IsInsideConfiner(candidate))
                continue;

            // Ground tile 체크
            if (!BuildManager.Instance.IsGround(candidate))
                continue;

            // Collision tile 체크
            if (BuildManager.Instance.IsCollisionTile(candidate))
                continue;

            // Collider 내부에 있는지 체크 (벽, 건물 등)
            if (IsInsideCollider(candidate))
                continue;

            // 유효 좌표
            safePos = candidate;
            return true;
        }

        safePos = Vector3.zero;
        return false;
    }

    /// 실제 Collider2D 내부인지 검사
    private static bool IsInsideCollider(Vector3 pos)
    {
        int mask = LayerMask.GetMask("Building", "Wall");

        Collider2D hit = Physics2D.OverlapPoint(pos, mask);
        return hit != null;
    }
}
