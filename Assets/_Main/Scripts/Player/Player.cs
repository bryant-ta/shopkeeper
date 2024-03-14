using System;
using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour {
    [field:SerializeField] public PlayerInput PlayerInput { get; private set; }
}