using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

// Util class for obstacles instantiated in scene during playback, updates A* graph

public class RescanOnStart : MonoBehaviour
{
    Collider2D collider;
    private void Start() {
        collider = GetComponent<Collider2D>();
        var guo = new GraphUpdateObject(collider.bounds);
        guo.updatePhysics = true;
        AstarPath.active.UpdateGraphs(guo);
    }


}
