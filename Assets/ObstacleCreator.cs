using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCreator : MonoBehaviour
{
    [SerializeField]
    GameObject[] _objPool;

    [SerializeField]
    float _objDensity;

    [SerializeField]
    float _spawnPerimeter;

    void Start()
    {
        int _objCount = (int)(_objDensity*Mathf.PI*Mathf.Pow(_spawnPerimeter/2, 2));
        for(int o = 0; o < _objCount; o++){
            GameObject go = _objPool[Random.Range(0, _objPool.Length)];
            Vector3 _pos = Random.insideUnitCircle * _spawnPerimeter;
            float randomRotation = Random.Range(0f, 360f);
            Instantiate(go, _pos, Quaternion.Euler(0f, 0f, randomRotation));
        }
    }
}