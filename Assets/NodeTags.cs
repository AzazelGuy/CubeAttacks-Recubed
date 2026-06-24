using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeTag{ Walk, Jump, Edge }
public class NodeTags : MonoBehaviour
{
    public NodeTag nodeTag = NodeTag.Walk;
}
