using System;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class PathActor : MonoBehaviour {
    [SerializeField] float moveSpeed = 1;
    [SerializeField] List<Path> paths;
    
    int curPathIndex;
    Path curPath;
    Vector3 curEndPoint;
    bool isMoving;
    
    float distanceTraveled;

    // NOTE: a bit silly passing index, but works for simple cases - see station idea for extension
    public event Action<int> OnPathEnd; // index of path that this actor just finished

    void Awake() {
        if (paths.Count == 0) return;
        curPathIndex = 0;
        curPath = paths[curPathIndex];
        curEndPoint = curPath.path.GetPoint(curPath.path.NumPoints - 1);
        
        isMoving = true;
    }

    void Update() {
        FollowPath();
    }

    public void StartNextPath() {
        curPathIndex++;
        curPath = paths[curPathIndex];
        curEndPoint = curPath.path.GetPoint(curPath.path.NumPoints - 1);
        
        isMoving = true;
    }

    void FollowPath() {
        if (isMoving) {
            if (transform.position == curEndPoint) { // positions match exactly with EndOfPathInstruction.Stop
                OnPathEnd?.Invoke(curPathIndex);
                isMoving = false;
            }
            
            distanceTraveled += moveSpeed * Time.deltaTime * GlobalClock.TimeScale;
            transform.position = curPath.path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Stop);
            transform.rotation = curPath.path.GetRotationAtDistance(distanceTraveled, EndOfPathInstruction.Stop);
        }
    }
}