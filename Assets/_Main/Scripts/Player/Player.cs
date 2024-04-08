using System;
using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour {
    [field:SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field:SerializeField] public PlayerInteract PlayerInteract { get; private set; }
    [field:SerializeField] public PlayerDrag PlayerDrag { get; private set; }
}