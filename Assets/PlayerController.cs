using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKnockbakable
{
    public Vector2 CurrentDirection {get {return _rb.velocity.normalized;}}

    public float Speed {get {return _speed;}}
    public Rigidbody2D Rb {get {return _rb;}}

    public static PlayerController Instance {get; protected set;}

    public float KnockBackDuration {get {return _knockBackDuration;}}
    public float KnockBackTimer {get; set;}

    [SerializeField] float _knockBackDuration = 0.5f;

    public bool IsKnockBacked {get; set;}
    
    
    void Awake()
    {
        if(Instance != null){
            Debug.Log("Found more then one DialogueManager, and destoyed it");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputs(out _xInput,out _yInput);
    }

    void FixedUpdate(){
        if(IsKnockBacked){
            if(KnockBackTimer < KnockBackDuration){
                KnockBackTimer += Time.deltaTime;
                return;
            }else{
                IsKnockBacked = false;
            }
        }

        Vector2 _direction = new Vector2(_xInput, _yInput).normalized;
        _direction = ChoseDirection(_direction);

        _rb.velocity = _direction * _speed;
    }

    void UpdateInputs(out float XInput, out float YInput){
        XInput = Input.GetAxisRaw("Horizontal");
        YInput = Input.GetAxisRaw("Vertical");
    }

    Vector2 ChoseDirection(Vector2 _direct){
        Vector2 closestVector = Vector2.zero;
        float maxDot = float.MinValue;

        foreach (Vector2 vector in _possibleDirections)
        {
            float dotProduct = Vector2.Dot(vector.normalized, _direct);

            if (dotProduct > maxDot)
            {
                maxDot = dotProduct;
                closestVector = vector;
            }
        }

        return closestVector;
    }
    
    
    [SerializeField]
    Rigidbody2D _rb;

    [SerializeField]
    float _speed;

    private float _xInput;
    private float _yInput;

    readonly Vector2[] _possibleDirections =  {new Vector2(0, 0),
                                                new Vector2(1, 0).normalized,
                                                new Vector2(0, 1).normalized,
                                                new Vector2(-1, 0).normalized,
                                                new Vector2(0, -1).normalized,
                                                new Vector2(1, 1).normalized,
                                                new Vector2(-1, 1).normalized,
                                                new Vector2(-1, -1).normalized,
                                                new Vector2(1, -1).normalized
                                        };

}
