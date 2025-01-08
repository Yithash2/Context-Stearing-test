using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    [SerializeField]
    GameObject[] _ennemy;

    [SerializeField]
    int _ennemyCount;

    void Start()
    {
        List<IEnnemy> Ennemies = new List<IEnnemy>();
        for(int j = 0; j < _ennemy.Length; j++)
            for(int i = 0; i < _ennemyCount/_ennemy.Length; i++){
                GameObject gm = Instantiate(_ennemy[j]);
                IEnnemy _ennemyController = gm.GetComponent<IEnnemy>();
                Ennemies.Append(_ennemyController);
            }

        for(int i = 0; i < Ennemies.Count; i++){
            if( i != 0){
                Ennemies[i].Target = Ennemies[i-1].gmObj;
            }else{
                Ennemies[i].Target = Ennemies[Ennemies.Count].gmObj;
            }
        }

        
    }
}
