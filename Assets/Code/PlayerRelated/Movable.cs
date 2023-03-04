using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using Unity.VisualScripting;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private bool moving;
    private Grid grid;

    private void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }

    public void SetStartPosition(Vector3 startPosition)
    {
        if (startPosition != null)
            currentPosition = startPosition;
    }

    public IEnumerator Move(Vector3 direction)
    {
        if (!moving)
        {
            bool pathClear = true;

            GameObject targetTile = grid.GetTile(currentPosition + direction);
            if (targetTile != null)
            {
                for (int i = 0; i < targetTile.transform.childCount; i++)
                {
                    if (targetTile.transform.GetChild(i).tag == "Unwalkable" | targetTile.transform.GetChild(i).tag == "Entity")
                    {
                        pathClear = false;
                    }
                    if (targetTile.transform.GetChild(i).GetComponent<Damagable>() != null && GetComponent<DamageDealer>() != null)
                    {
                        DamageDealer damageDealer = GetComponent<DamageDealer>();
                        damageDealer.DealDamage(targetTile.transform.GetChild(i).GetComponent<Damagable>());
                    }
                }
            }
            if (grid.GetTile(currentPosition + direction) != null)
            {
                if (pathClear)
                {
                    moving = true;
                    transform.position += direction;
                    currentPosition += direction;
                    transform.parent = grid.GetTile(currentPosition).transform;
                }
            }
            yield return new WaitForSeconds(.5f);
            moving = false;
        }
        
    }

    public IEnumerator MoveTo(Vector3 targetPosition)
    {
        Vector3 targetGridPosition = grid.TileIndexFromWorldPoint(targetPosition);
        Vector3 direction = Vector3Clamp(targetGridPosition - currentPosition,-1,1);
        if (direction != Vector3.zero)
        {
            //Debug.Log("Current position " + transform.position + "Moving to " + targetPosition + "Direction " + direction + "Tile coordinates " + targetGridPosition);
            StartCoroutine(Move(direction));
        }


        yield return null;
    }

    private Vector3 Vector3Clamp(Vector3 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);
        value.z = Mathf.Clamp(value.z, min, max);

        return value;
    }
}
