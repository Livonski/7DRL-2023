using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float Damage;

    public void DealDamage(Damagable target)
    {
        if (target != null)
            target.TakeDamage(Damage);
    }
}
