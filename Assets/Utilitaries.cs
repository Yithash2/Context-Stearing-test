using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum EnnemyStates{
    Defensive,
    Offensive,
    Isometric
}

[System.Serializable]
public struct Pattern{
    public string Name;
    public float[] PatternWeights;
}

public interface IKnockbakable{
    Rigidbody2D Rb {get;}
    float KnockBackDuration {get;}
    float KnockBackTimer {get; set;}
    bool IsKnockBacked {get; set;}

    public void SetKnockBack(Vector2 knockback){
        Rb.velocity += knockback;
        IsKnockBacked = true;
        KnockBackTimer = 0;
    }
}
