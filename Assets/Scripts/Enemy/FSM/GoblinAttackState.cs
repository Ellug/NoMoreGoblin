using UnityEngine;

public class GoblinAttackState : GoblinState
{
    private float _attackTimer;

    public GoblinAttackState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        _attackTimer = 0f;
        _goblin.targetPos = null;
        _goblin.Anim.SetBool("IsRunning", false);
    }

    public override void UpdateLogic()
    {
        // 대상이 없으면 Idle State
        if (_goblin.target == null ||
            !_goblin.target.gameObject.activeInHierarchy ||
            (_goblin.target.TryGetComponent<IDamageable>(out var t) && !t.IsAlive)
        )
        {
            _goblin.SetIdleState();
            return;
        }

        float dist = Vector2.Distance(_goblin.transform.position, _goblin.target.position);

        // 사거리 밖으로 나가면 다시 추적
        if (dist > _goblin.AttackRange * 1.1f)
        {
            _fsm.ChangeState(_goblin.ChaseState);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _goblin.AttackCooldown)
        {
            _attackTimer = 0f;
            DoAttack();
        }
    }

    private void DoAttack()
    {
        if (_goblin.target == null) return;
        _goblin.Anim.SetTrigger("AttackTrigger");

        if (_goblin.target.TryGetComponent<IDamageable>(out var dmged))
        {         
            switch (dmged.Type)
            {
                case DamageableType.Player:
                    dmged.TakeDamage(_goblin.Dmg, null);
                    break;
                case DamageableType.Guard:
                    dmged.TakeDamage(_goblin.Dmg, _goblin.gameObject);
                    break;
            }
        }
    }
}
