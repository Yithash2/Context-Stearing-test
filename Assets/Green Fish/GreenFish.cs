using UnityEngine;

public class GreenFish : Ennemy{
    float _forgetTimer = 5f;

    protected override void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(IsCharging && (_state == EnnemyStates.Defensive)){
            if(playerDistance > 29 + Target.transform.localScale.x - 1){
                IsCharging = false;

                }
        
            else return;
        }else if(IsCharging && _forgetTimer < 0){
            if(playerDistance < 1 + Target.transform.localScale.x - 1){
                IsCharging = false;
            }else return;
        }

        if(playerDistance < 1 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Defensive;
            _realSpeed = 4*MaxSpeed*Mathf.Sqrt(transform.localScale.x);

            Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
                Target.GetComponent<IKnockbakable>().SetKnockBack(Rb.velocity*2); 
                Ennemy ennemy;
                if(Target.TryGetComponent<Ennemy>(out ennemy)){
                    ennemy.SubbHealthEvent(1);
                    Health ++;
                    transform.localScale = transform.localScale + new Vector3(1,1,1);
                }

            IsCharging = true;
        }else if(playerDistance < 30 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Offensive;
            _realSpeed = MaxSpeed*Mathf.Sqrt(transform.localScale.x);
            _forgetTimer = 5;
        }else{
            _state = EnnemyStates.Isometric;    
            _forgetTimer -= Time.deltaTime; 
            if(_forgetTimer < 0){
                Target = GameMan.GetRandomActiveGameObject(transform);
                IsCharging = true;
                _state = EnnemyStates.Offensive;
            }      
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