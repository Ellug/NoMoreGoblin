using UnityEngine;

public enum DamageableType
{
    Player,
    Guard,
    Citizen,
    Building,
    Tree,
    Enemy
}

public interface IDamageable
{
    DamageableType Type { get; }
    public bool IsAlive { get; }
    void TakeDamage(float dmg, GameObject attacker);
}