using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _oldPlayerPosition;
    private Movable _movement;

    private Vector3[] _path;
    private int _targetIndex;


    private void Awake()
    {
        _movement = GetComponent<Movable>();
    }

    private void Start()
    {
        _player = GameObject.Find("Player(Clone)").GetComponent<Transform>();
    }
    private void Update()
    {
        if (_player.position != _oldPlayerPosition)
        {
            PathRequestManager.RequestPath(transform.position, _player.position, OnPathFound);
            _oldPlayerPosition = _player.position;
        }
    }
    public void OnPathFound(Vector3[] newPath, bool pathSucces)
    {
        if (pathSucces)
        {
            _path = newPath;
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        if (_path == null)
            yield return null;
        //Debug.Log("Target position " + _pla);
        Debug.Log(_path.Length);
        Debug.Log("current path nodes #========================================");
        foreach (Vector3 v in _path)
        {
            Debug.Log(v);
        }

        Vector3 currentWaypoint = _path[0];
        while (true)
        {
            StopCoroutine(_movement.MoveTo(currentWaypoint));
            if (transform.position == currentWaypoint)
            {
                _targetIndex++;
                if (_targetIndex >= _path.Length)
                {
                    yield break;
                }
                currentWaypoint = _path[_targetIndex];
            }
            StartCoroutine(_movement.MoveTo(currentWaypoint));
            yield return null;
        }
    }
}