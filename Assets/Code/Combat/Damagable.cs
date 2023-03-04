using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [SerializeField] private float HP;

    public void TakeDamage(float damage)
    {

        HP -= damage;
        Debug.Log("Taken " + damage + " current HP" + HP);
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
