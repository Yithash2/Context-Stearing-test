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

    private GameManager _gameMan;

    void Start()
    {
        _gameMan = GameManager.Instance;

        for(int j = 0; j < _ennemy.Length; j++){
            for(int i = 0; i < _ennemyCount/_ennemy.Length; i++){
                GameObject go = Instantiate(_ennemy[j]);
                _gameMan.AddFish(go);  
            }
        }
            
    }

}
