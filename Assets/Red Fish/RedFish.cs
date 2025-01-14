using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedFish : Ennemy
{
    protected override void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(IsCharging && _state == EnnemyStates.Isometric){
            if(playerDistance < 11 + Target.transform.localScale.x - 1){
                IsCharging = false;
                Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
                Target.GetComponent<IKnockbakable>().SetKnockBack(Rb.velocity*2); 

                Ennemy ennemy;
                if(Target.TryGetComponent<Ennemy>(out ennemy)){
                    ennemy.SubbHealthEvent(1);
                    Health += 1;
                    transform.localScale = transform.localScale + new Vector3(1,1,1);
                }
                
                
                Target = GameMan.GetRandomActiveGameObject(transform);
                }
            else return;
        }

        if(playerDistance < 10 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Offensive;
            _realSpeed = MaxSpeed*Mathf.Sqrt(transform.localScale.x);
        }else if (playerDistance < 40 + Target.transform.localScale.x - 1)
        {
            _state = EnnemyStates.Defensive; 
            _realSpeed = MaxSpeed*Mathf.Sqrt(transform.localScale.x);
        }else{
            _state = EnnemyStates.Isometric;
            IsCharging = true;
            _realSpeed = 4*MaxSpeed*Mathf.Sqrt(transform.localScale.x);
        }
    }

    protected override Pattern GetPatterns(){
        Pattern _curPattern = _patterns[2];
        switch (_state){
            case EnnemyStates.Defensive :
                _curPattern = _patterns[0];
                break;
            case EnnemyStates.Offensive :
                _curPattern = _patterns[1];
                break;
            case EnnemyStates.Isometric :
                _curPattern = _patterns[2];
                break;
        }
        return _curPattern;
    }
}
