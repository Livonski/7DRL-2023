using System.Collections.Generic;
using System;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    PathRequest _currentPathRequest;

    static PathRequestManager instance;
    Pathfinding _pathfinding;

    bool _isProcessingPath;

    private void Awake()
    {
        instance = this;
        _pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance._pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinding.StartFindPath(_currentPathRequest._pathStart, _currentPathRequest._pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool succes)
    {
        _currentPathRequest._callback(path, succes);
        _isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 _pathStart;
        public Vector3 _pathEnd;
        public Action<Vector3[], bool> _callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
        {
            _pathStart = start;
            _pathEnd = end;
            _callback = callback;
        }
    }
}