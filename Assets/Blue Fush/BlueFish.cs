using UnityEngine;

public class BlueFish : Ennemy{

    protected override void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(IsCharging){
            if(playerDistance > 50 + Target.transform.localScale.x - 1){
                IsCharging = false;
            } 
            else return;
        }
        

        if(playerDistance < 5 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Defensive;
            IsCharging = true;
            if(!GameMan.BadPC)Instantiate(ExplosionPrefab, transform.position, Quaternion.identity).transform.localScale = transform.localScale;
                Target.GetComponent<IKnockbakable>().SetKnockBack(Rb.velocity*2); 

                Ennemy ennemy;
                if(Target.TryGetComponent<Ennemy>(out ennemy)){
                    ennemy.SubbHealthEvent(1);
                    //Health += 1;
                    transform.localScale = transform.localScale + new Vector3(1,1,1);
                }

            _realSpeed = MaxSpeed * transform.localScale.x;
        }else if (playerDistance < 60 + Target.transform.localScale.x - 1){
            _state = EnnemyStates.Offensive; 
            _realSpeed = MaxSpeed;
        }else{
            _state = EnnemyStates.Offensive;
            _realSpeed = MaxSpeed * 8 ;
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