using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowFish : Fish
{
    protected override void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(IsCharging && _state == EnnemyStates.Isometric){
            if(playerDistance < 11 + Target.transform.localScale.x - 1){
                IsCharging = false;
                if(!GameMan.BadPC)Instantiate(ExplosionPrefab, transform.position, Quaternion.identity).transform.localScale = transform.localScale;
                Target.TryGetComponent<IKnockbakable>(out IKnockbakable _kb); 
                _kb?.SetKnockBack(Rb.velocity*2);

                if(Target.TryGetComponent<IKillable>(out IKillable killable)){
                    killable.SubbHealthEvent(2);
                    //Health += 1;
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
        }else if (playerDistance < 400 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Isometric;
            IsCharging = true;
            _realSpeed = 4*MaxSpeed*Mathf.Sqrt(transform.localScale.x);
        }else{
            _state = EnnemyStates.Offensive;
            _realSpeed = MaxSpeed * 4 ;
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

    protected override void AfterStart(){
        _patterns = new Pattern[3];
        for(int i = 0; i < 3; i++){
            _patterns[i].PatternWeights = new float[8];
            for(int j = 0; j < 8; j++){
                if(Random.Range(0, 2) == 0)
                _patterns[i].PatternWeights[j] = Random.Range(-10f, 10f);
            }
        }

        gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer _sp);
        _sp.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
    }
}
