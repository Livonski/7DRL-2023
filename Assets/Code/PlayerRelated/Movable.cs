using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
    private bool moving;

    public IEnumerator Move(Vector3Int direction)
    {
        if (!moving)
        {
            moving = true;
            transform.position += direction;
            yield return new WaitForSeconds(1f);
            moving = false;
        }
        
    }
}
