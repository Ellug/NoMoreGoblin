using UnityEngine;

public class NpcMoveController
{
    private readonly Rigidbody2D _rb;
    private readonly ContactFilter2D _obstacleFilter;
    private readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];

    private Rigidbody2D.SlideMovement _slideMovement;

    private const float CHECK_DIST = 1.5f;
    private const float TARGET_BUFFER = 0.25f;
    private const float UPDATE_INTERVAL = 0.1f;

    private const int BLOCKED_THRESHOLD = 2;
    private const int HARD_BLOCK_THRESHOLD = 5;

    private const int DIR_COUNT = 16;
    private const float DIR_ANGLE = 360f / DIR_COUNT;

    private const float AVOID_LOCK_TIME = 0.35f;
    private const float MIN_CLEARANCE = 0.05f;
    private const float CLEARANCE_EPSILON = 0.02f;

    private float _nextCheckTime = 0;
    private bool _isAvoiding = false;
    private Vector2 _avoidDir;
    private int _blockedCount = 0;

    private float _avoidLockTimer = 0;   // 방향 유지 타이머

    public NpcMoveController(Rigidbody2D rb)
    {
        _rb = rb;
        int obstacleMask = BuildObstacleMask();

        _obstacleFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = obstacleMask,
            useTriggers = false
        };

        _slideMovement = new Rigidbody2D.SlideMovement
        {
            layerMask = obstacleMask,
            useAttachedTriggers = false,
            useSimulationMove = true,
            gravity = Vector2.zero,
            gravitySlipAngle = 0f,
            surfaceSlideAngle = 90f,
            maxIterations = 8,
            surfaceAnchor = Vector2.zero
        };
    }

    public void MoveTo(Vector2 target, float speed)
    {
        Vector2 pos = _rb.position;
        Vector2 toTarget = target - pos;
        float remainingDist = toTarget.magnitude;

        if (remainingDist < 0.18f)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 forward = toTarget.normalized;
        float checkDist = Mathf.Min(CHECK_DIST, Mathf.Max(remainingDist - TARGET_BUFFER, 0f));

#if UNITY_EDITOR
        Debug.DrawLine(pos, target, Color.yellow);
#endif

        // 회피 방향 락 시간 체크
        if (_avoidLockTimer > 0)
        {
            _avoidLockTimer -= Time.deltaTime;
        }
        else
        {
            _isAvoiding = false;
        }

        // 회피 중이어도 정면이 열리면 즉시 원래 경로로 복귀
        if (_isAvoiding && !IsDirectionBlocked(forward, checkDist * 0.75f))
        {
            _isAvoiding = false;
            _avoidLockTimer = 0f;
            _blockedCount = 0;
        }

        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + UPDATE_INTERVAL;

            bool blocked = IsDirectionBlocked(forward, checkDist);

            if (blocked)
            {
                _blockedCount++;

                // 일정 시간 막히면 회피 시작
                if (_blockedCount >= BLOCKED_THRESHOLD && _avoidLockTimer <= 0f)
                {
                    _isAvoiding = true;
                    _avoidDir = FindEscapeDirection(forward, checkDist);
                    if (_avoidDir == Vector2.zero)
                        _avoidDir = -forward;
                    _avoidLockTimer = AVOID_LOCK_TIME;
                }
                else if (_isAvoiding && IsDirectionBlocked(_avoidDir, checkDist * 0.75f))
                {
                    // 회피 중인 방향도 막히면 즉시 재탐색
                    _avoidDir = FindEscapeDirection(forward, checkDist);
                    if (_avoidDir == Vector2.zero)
                        _avoidDir = -forward;
                    _avoidLockTimer = AVOID_LOCK_TIME;
                }

                // 강제 탈출
                if (_blockedCount >= HARD_BLOCK_THRESHOLD)
                {
                    _isAvoiding = true;
                    _avoidDir = FindEscapeDirection(forward, CHECK_DIST);
                    if (_avoidDir == Vector2.zero)
                        _avoidDir = Random.insideUnitCircle.normalized;
                    _avoidLockTimer = AVOID_LOCK_TIME * 0.75f;
                    _blockedCount = BLOCKED_THRESHOLD;
                }
            }
            else if (!blocked)
            {
                _blockedCount = 0;
            }
        }

        Vector2 finalDir = _isAvoiding ? _avoidDir : forward;
        finalDir.Normalize();

        _slideMovement.startPosition = pos;
        _rb.Slide(finalDir * speed, Time.fixedDeltaTime, _slideMovement);

#if UNITY_EDITOR
        Debug.DrawRay(pos, forward * checkDist, Color.red);
        Debug.DrawRay(pos, finalDir * 0.4f, Color.green);
#endif
    }

    private static int BuildObstacleMask()
    {
        // 요구사항: 건축물(Wall), 나무(Tree), NPC 유닛(Enemy/NPC)만 충돌 처리
        return LayerMask.GetMask("Wall", "Tree", "Enemy", "NPC");
    }

    private bool IsDirectionBlocked(Vector2 dir, float distance)
    {
        if (distance <= 0f || dir.sqrMagnitude < 0.0001f)
            return false;

        int hitCount = _rb.Cast(dir.normalized, _obstacleFilter, _castHits, distance);
        return hitCount > 0;
    }

    private float GetClearance(Vector2 dir, float distance)
    {
        if (distance <= 0f || dir.sqrMagnitude < 0.0001f)
            return 0f;

        int hitCount = _rb.Cast(dir.normalized, _obstacleFilter, _castHits, distance);
        if (hitCount <= 0)
            return distance;

        float minDist = distance;
        for (int i = 0; i < hitCount; i++)
        {
            float hitDist = _castHits[i].distance;
            if (hitDist < minDist)
                minDist = hitDist;
        }

        return minDist;
    }

    // 16방향 ShapeCast 기반 탈출
    private Vector2 FindEscapeDirection(Vector2 forward, float checkDist)
    {
        // 정면 180도 우선 탐색
        Vector2 dir1 = ScanAngles(forward, checkDist, -90f, 90f);
        if (dir1 != Vector2.zero) return dir1;

        // 전체 360도 탐색
        Vector2 dir2 = ScanAngles(forward, checkDist, -180f, 180f);
        if (dir2 != Vector2.zero) return dir2;

        // 완전 차단
        return Vector2.zero;
    }

    private Vector2 ScanAngles(Vector2 forward, float checkDist, float minAngle, float maxAngle)
    {
        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        Vector2 bestDir = Vector2.zero;
        float bestClearance = -1f;

        int half = DIR_COUNT / 2;
        for (int i = 0; i <= half; i++)
        {
            if (EvaluateAngle(i * DIR_ANGLE, baseAngle, checkDist, minAngle, maxAngle, ref bestDir, ref bestClearance))
                return bestDir;

            if (i == 0)
                continue;

            if (EvaluateAngle(-i * DIR_ANGLE, baseAngle, checkDist, minAngle, maxAngle, ref bestDir, ref bestClearance))
                return bestDir;
        }

        if (bestClearance <= MIN_CLEARANCE)
            return Vector2.zero;

        return bestDir;
    }

    private bool EvaluateAngle(
        float deltaAngle,
        float baseAngle,
        float checkDist,
        float minAngle,
        float maxAngle,
        ref Vector2 bestDir,
        ref float bestClearance
    )
    {
        if (deltaAngle < minAngle || deltaAngle > maxAngle)
            return false;

        float worldAngle = baseAngle + deltaAngle;
        float rad = worldAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float clearance = GetClearance(dir, checkDist);
        if (clearance >= checkDist - CLEARANCE_EPSILON)
        {
            bestDir = dir.normalized;
            return true;
        }

        // 완전히 열린 방향이 없으면, 가장 여유 공간이 큰 방향 채택
        if (clearance > bestClearance)
        {
            bestClearance = clearance;
            bestDir = dir.normalized;
        }

        return false;
    }
}
