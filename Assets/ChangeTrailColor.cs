using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer)), RequireComponent(typeof(SpriteRenderer))]
public class ChangeTrailColor : MonoBehaviour
{
    [SerializeField] TrailRenderer _trailRen;
    [SerializeField] SpriteRenderer _spriteRen;
    // Start is called before the first frame update
    void Start()
    {
        _trailRen.startColor = _spriteRen.color; // The color at the start of the trail
    }
}
