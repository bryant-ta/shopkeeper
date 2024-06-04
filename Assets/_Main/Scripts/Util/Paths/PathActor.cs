using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Paths {
public class PathActor : MonoBehaviour {
    [SerializeField] float moveSpeed = 1;
    [SerializeField] List<Path> paths;

    int curPathIndex = -1;
    Path curPath;
    Vector3 curEndPoint;
    bool isMoving;

    float distanceTraveled;

    public event Action OnPathEnd;

    public void StartPath(int pathIndex) {
        if (curPathIndex == paths.Count - 1) return;

        curPathIndex = pathIndex;
        curPath = paths[curPathIndex];
        curEndPoint = curPath.path.GetPoint(curPath.path.NumPoints - 1);

        distanceTraveled = 0;
        isMoving = true;
    }

    public void StartNextPath() {
        StartPath(curPathIndex + 1);
    }

    void Update() { FollowPath(); }

    void FollowPath() {
        if (isMoving) {
            if (transform.position == curEndPoint) { // positions match exactly with EndOfPathInstruction.Stop
                isMoving = false;
                OnPathEnd?.Invoke();
            }

            distanceTraveled += moveSpeed * Time.deltaTime * GlobalClock.TimeScale;
            transform.position = curPath.path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Stop);
            transform.rotation = curPath.path.GetRotationAtDistance(distanceTraveled, EndOfPathInstruction.Stop);
        }
    }

    public void AddPath(Path path) {
        paths.Add(path);
    }
}
}