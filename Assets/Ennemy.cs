using System;
using UnityEngine;

public interface IEnnemy {
    GameObject gmObj {get;}
    int Health {get; set;}
    GameObject Target {get; set;}

    Action subbHealthEvent {get;}

    public void SubbHealth(){
        Health --;

        subbHealthEvent?.Invoke();
    }
}