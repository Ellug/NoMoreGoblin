using UnityEngine;

public class NpcMoveController
{
    private readonly Rigidbody2D _rb;

    private readonly LayerMask _wallMask = (1 << 6) | (1 << 7);
    private readonly LayerMask _allMask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9) | (1 << 10);

    private Rigidbody2D.SlideMovement _slideMovement;

    private const float CHECK_DIST = 0.02f;
    private const float UPDATE_INTERVAL = 0.35f;

    private const int BLOCKED_THRESHOLD = 40;
    private const int HARD_BLOCK_THRESHOLD = 80;

    private const int DIR_COUNT = 16;
    private const float DIR_ANGLE = 360f / DIR_COUNT;

    private const float AVOID_LOCK_TIME = 1.5f;   // 회피 방향 유지 시간

    private float _nextCheckTime = 0;
    private bool _isAvoiding = false;
    private Vector2 _avoidDir;
    private int _blockedCount = 0;

    private float _avoidLockTimer = 0;   // 방향 유지 타이머

    public NpcMoveController(Rigidbody2D rb)
    {
        _rb = rb;

        _slideMovement = new Rigidbody2D.SlideMovement
        {
            layerMask = _wallMask,
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

        if (toTarget.sqrMagnitude < 0.03f)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 forward = toTarget.normalized;

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

        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + UPDATE_INTERVAL;

            bool blocked = Physics2D.Raycast(pos, forward, CHECK_DIST, _allMask);

            if (blocked && _avoidLockTimer <= 0)
            {
                _blockedCount++;

                // 일정 시간 막히면 회피 시작
                if (_blockedCount == BLOCKED_THRESHOLD)
                {
                    _isAvoiding = true;
                    _avoidDir = FindEscapeDirection(forward, pos);
                    _avoidLockTimer = AVOID_LOCK_TIME;
                }

                // 강제 탈출
                if (_blockedCount >= HARD_BLOCK_THRESHOLD)
                {
                    _isAvoiding = true;
                    _avoidDir = Random.insideUnitCircle.normalized;
                    _avoidLockTimer = AVOID_LOCK_TIME;
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
        Debug.DrawRay(pos, forward * CHECK_DIST, Color.red);
        Debug.DrawRay(pos, finalDir * 0.4f, Color.green);
#endif
    }

    // 16방향 레이 기반 탈출
    private Vector2 FindEscapeDirection(Vector2 forward, Vector2 pos)
    {
        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        // 정면 180도 우선 탐색
        Vector2 dir1 = ScanAngles(pos, baseAngle, -90f, 90f);
        if (dir1 != Vector2.zero) return dir1;

        // 전체 360도 탐색
        Vector2 dir2 = ScanAngles(pos, baseAngle, -180f, 180f);
        if (dir2 != Vector2.zero) return dir2;

        // 완전 차단 → 뒤로
        return -forward;
    }

    private Vector2 ScanAngles(Vector2 pos, float baseAngle, float minAngle, float maxAngle)
    {
        for (int i = 0; i < DIR_COUNT; i++)
        {
            float worldAngle = baseAngle + (i * DIR_ANGLE);
            float delta = Mathf.DeltaAngle(baseAngle, worldAngle);
            if (delta < minAngle || delta > maxAngle)
                continue;

            float rad = worldAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Raycast 검사
            bool hit = Physics2D.Raycast(pos, dir, CHECK_DIST, _allMask);
            if (!hit)
                return dir.normalized;
        }

        return Vector2.zero;
    }
}
