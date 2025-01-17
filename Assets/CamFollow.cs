using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField]
    CameraStates _camState = CameraStates.Global;

    Camera _cam;

    [SerializeField]
    Transform _target;

    [SerializeField]
    LayerMask _fiendLayer;

    Vector3 _offset = new Vector3(0,0,-10);

    [SerializeField, Range(0.1f, 1)]
    float _smoothF;

    private GameManager _gameMan;

    void Start(){
        _cam = Camera.main;
        _gameMan = GameManager.Instance;
    }

    void Update(){
        UpdateState();
    }

    void UpdateState(){
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            _camState = CameraStates.FishSingular;
            return;
        }

        if(Input.GetKeyDown(KeyCode.Keypad1)){
            _camState = CameraStates.Global;
            return;
        }
    }


    void FixedUpdate()
    {
        switch(_camState){
            case CameraStates.Global:
                _target = null;
                Vector3 actionCenter = _gameMan.PerceivedCenterWeight();
                transform.position = Vector3.Lerp(transform.position, actionCenter + _offset, _smoothF);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, Mathf.Sqrt(Mathf.Pow(_gameMan.NumberOfFishes, 2) + Mathf.Pow(SizeAverage(), 2)) * 10f, _smoothF);
                break;
            case CameraStates.FishSingular :
                if(_target == null || Input.GetKeyDown(KeyCode.Space))
                _target = _gameMan.GetRandomActiveGameObject(transform).transform;
                transform.position = Vector3.Lerp(transform.position, _target.position + _offset, _smoothF);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize ,Mathf.Sqrt(_target.localScale.x) * 20f, _smoothF);
                break;
        }

    }

    float SizeAverage(){
        var _lstfishes = _gameMan.Fishes;

        float r = 0;

        foreach(GameObject go in _lstfishes){
            r += go.transform.localScale.x;
        }

        return r / _gameMan.NumberOfFishes;
    }

}

public enum CameraStates{
    FishSingular,
    Global
}
