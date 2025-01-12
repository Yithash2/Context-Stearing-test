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
        StartCoroutine(FindTarget());
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
            case CameraStates.Global :
                _target = null;
                transform.position = Vector3.Lerp(transform.position, (Vector3)_gameMan.PerceivedCenter() + _offset, _smoothF);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize ,_gameMan.NumberOfFishes * 10f, _smoothF);
                break;
            case CameraStates.FishSingular :
                if(_target == null || Input.GetKeyDown(KeyCode.Space))
                _target = GetRandomActiveGameObject().transform;
                transform.position = Vector3.Lerp(transform.position, _target.position + _offset, _smoothF);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize ,_target.localScale.x * 20f, _smoothF);
                break;
        }

        

    }

    public GameObject GetRandomActiveGameObject()
    {
        List<GameObject> fiends = _gameMan.Fishes;
        List<IEnnemy> fiend = new List<IEnnemy>();

        foreach (GameObject go in fiends)
        {
            if(go.TryGetComponent<IEnnemy>(out IEnnemy friend)){
                fiend.Add(friend);
            }
        }

        // Ensure there are valid fiends in the list
        if (fiend.Count == 0)
        {
            Debug.LogWarning("No active game objects found within range.");
            return null; // Or handle this case differently
        }
        
        int maxHealth = fiend.Max(e => e.Health);
        // Safely get a random game object
        return fiend.FirstOrDefault(e => e.Health == maxHealth).gmObj;
    }

    IEnumerator FindTarget(){
        yield return new WaitForSeconds(1);
        _target = GetRandomActiveGameObject().transform;
    }
}

public enum CameraStates{
    FishSingular,
    Global
}
