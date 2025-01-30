using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelFish : Fish
{
    protected override void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(IsCharging){
            if(playerDistance > 10){
                _realSpeed = MaxSpeed;
                IsCharging = false;
            }
            return;
        }

        if(playerDistance < 5){
            _state = EnnemyStates.Defensive;
            if(!GameMan.BadPC)Instantiate(ExplosionPrefab, transform.position, Quaternion.identity).transform.localScale = transform.localScale;
            Target.TryGetComponent<IKnockbakable>(out IKnockbakable _kb); 
            _kb?.SetKnockBack(Rb.velocity*2); 
            if(Target.TryGetComponent<IKillable>(out IKillable killable)){
                killable.SubbHealthEvent(1);
                //Health += 1;
                transform.localScale = transform.localScale + new Vector3(1,1,1);
            }
            _realSpeed = MaxSpeed;
            IsCharging = true;
        }else{
            _realSpeed = MaxSpeed * 2;
            _state = EnnemyStates.Offensive;
        }
    }

    protected override void ChangeTarget(){
        GameObject obj = GameMan.GetRandomChildGameObject(out Egg egg);
        if(obj){
            Target = obj;
        }else{
            Target = GameMan.GetRandomActiveGameObject(transform);
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

    protected override void AfterStart(){}
}
