using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class InputController : MonoBehaviour
{
    private Movable movable;
    private void Awake()
    {
        movable = GetComponent<Movable>();
    }

    void Update()
    {
        int y = (int)Mathf.Clamp(Input.GetAxis("Vertical"),-1,1);
        int x = (int)Mathf.Clamp(Input.GetAxis("Horizontal"),-1,1);

        if (Mathf.Abs(x) > 0 | Mathf.Abs(y) > 0)
        {
            Debug.Log(x + " " + y);
            Vector3Int moveDirection = new Vector3Int(x,0, y);
            StartCoroutine(movable.Move(moveDirection));
            //movable.Move(moveDirection);
        }
    }
}
