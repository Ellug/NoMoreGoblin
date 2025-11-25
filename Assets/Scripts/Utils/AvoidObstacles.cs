using UnityEngine;

public static class ObstacleAvoidance
{
    public static Vector2 GetAvoidDirection(
        Transform self,
        Vector2 desiredDir,
        float radius,
        float checkDistance,
        float avoidStrength
        )
    {
        int layers = (1 << 6) | (1 << 7);

        // 직진 방지용 CircleCast
        RaycastHit2D hit = Physics2D.CircleCast(self.position, radius, desiredDir, checkDistance, layers);

        // 장애물 없으면 기존 방향 유지
        if (!hit) return desiredDir;

        // 충돌 지점에서 벗어날 방향 설정
        Vector2 away = ((Vector2)self.position - hit.point).normalized;

        // desired 방향과 회피 방향 블렌딩
        Vector2 final = (desiredDir + away * avoidStrength).normalized;

        return final;
    }
}
