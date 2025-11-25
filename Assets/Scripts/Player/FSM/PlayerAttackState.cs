using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float _timer = 0f;
    private const float BaseAttackDuration = 0.75f;
    private float _attackCoolDown;
    private float _attackDuration;
    private bool _hasAppliedDamage = false;

    public PlayerAttackState(PlayerController player, PlayerFSM fsm) : base(player, fsm) { }

    public override void Enter()
    {
        _timer = 0f;
        _hasAppliedDamage = false;

        // 비례 계산
        _attackDuration = BaseAttackDuration / _player.AttackSpeed;
        _attackCoolDown = _player.AttackCoolDown / (_player.AttackSpeed * _player.AttackSpeed);

        _player.CanAttack = false;
        _player.AttackPressed = false;

        // 애니메이션 속도 조정 및 처리
        _player.Anim.SetFloat("AttackSpeed", _player.AttackSpeed);
        _player.Anim.SetTrigger("Attack1Trigger");
    }
    
    public override void Exit()
    {
        _player.AttackPressed = false;
    }

    public override void UpdateLogic()
    {
        _timer += Time.deltaTime;

        // 공격 타이밍(타격 프레임) — AttackSpeed에 따라 자동 조절됨
        if (!_hasAppliedDamage && _timer >= _attackDuration * 0.4f)
        {
            ApplyAttackDamage();
            _hasAppliedDamage = true;
        }

        // 공격 모션 끝나면 MoveState 복귀
        if (_timer >= _attackDuration)
        {
            _player.Invoke(nameof(_player.ResetAttack), _attackCoolDown);
            _fsm.ChangeState(_player.MoveState);
        }
    }

    private void ApplyAttackDamage()
    {
        Vector2 origin = _player.Rb.position;
        Vector2 dir = _player.IsFacingRight ? Vector2.right :  Vector2.left;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, _player.AttackRange, dir, 0.1f, LayerMask.GetMask("Enemy", "Tree"));

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmged))
                dmged.TakeDamage(_player.AttackDamage, _player.gameObject);
        }
    }
}
