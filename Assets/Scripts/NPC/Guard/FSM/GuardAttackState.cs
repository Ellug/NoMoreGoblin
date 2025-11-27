using UnityEngine;

public class GuardAttackState : GuardState
{
    private float _attackTimer;

    public GuardAttackState(GuardController guard, GuardFSM fsm) : base(guard, fsm) { }

    public override void Enter()
    {
        _attackTimer = 0f;
        _guard.targetPos = null;
        _guard.Anim.SetBool("IsRunning", false);
    }

    public override void UpdateLogic()
    {
        // 대상이 없으면 Idle State
        if (_guard.target == null ||
            !_guard.target.gameObject.activeInHierarchy ||
            (_guard.target.TryGetComponent<IDamageable>(out var t) && !t.IsAlive)
        )
        {
            _guard.SetIdleState();
            return;
        }

        float dist = Vector2.Distance(_guard.transform.position, _guard.target.position);

        // 사거리 밖으로 나가면 다시 추적
        if (dist > _guard.AttackRange * 1.1f)
        {
            _fsm.ChangeState(_guard.ChaseState);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _guard.AttackCoolDown)
        {
            _attackTimer = 0f;
            DoAttack();
        }
    }

    private void DoAttack()
    {
        if (_guard.target == null) return;
        _guard.Anim.SetTrigger("AttackTrigger");

        if (_guard.target.TryGetComponent<IDamageable>(out var dmged))
            dmged.TakeDamage(_guard.AttackDamage, _guard.gameObject);
    }
}
