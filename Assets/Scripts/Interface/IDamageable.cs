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
    void TakeDamage(float dmg, GameObject attacker);
}