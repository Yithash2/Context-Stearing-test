using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum EnnemyStates{
    Defensive,
    Offensive,
    Isometric
}

[System.Serializable]
public struct Pattern{
    public string Name;
    public float[] PatternWeights;
}

public class AngryFishes : MonoBehaviour, IKnockbakable, IEnnemy
{
    public int Health {get {return _heath;} set {_heath = value;}}
    [SerializeField]
    int _heath;
    public GameObject gmObj {get {return gameObject;}}

    public Action subbHealthEvent {get {return null;}}

    public float KnockBackDuration {get {return _knockBackDuration;}}
    public float KnockBackTimer {get; set;}
    public Rigidbody2D Rb {get {return _rb;}}

    [SerializeField] float _knockBackDuration = 0.5f;

    public bool IsKnockBacked {get; set;}
    
    public GameObject Target {get {return _target;} set {_target = value;}}
    private GameObject _target;

    [SerializeField]
    EnnemyStates _state;

    [SerializeField]
    Pattern[] _patterns; // Bias for each direction

    [SerializeField]
    int RayCount = 16; // Number of rays cast

    [SerializeField]
    LayerMask _obstacleLayer;

    [SerializeField]
    LayerMask _fiendLayer;

    [SerializeField]
    Rigidbody2D _rb;

    [SerializeField]
    float MaxSpeed = 5f; // Max speed of the enemy

    [SerializeField]
    float RepulsionForceMult = 1f; // Max speed of the enemy

    float _realSpeed;

    [SerializeField]
    GameObject clonePrefab;

    [ReadOnly]
    public bool _isCharging;

    [SerializeField, Range(0.001f, 1f)]
    float _smoothF;

    void Start()
    {
        UpdatePatterns();
        _realSpeed = MaxSpeed;
        Target = GetRandomActiveGameObject();
        
    }

    void Update()
    {
        if(Health <= 0){
            Destroy(gameObject);
        }
        
        if(IsKnockBacked){
            if(KnockBackTimer < KnockBackDuration){
                KnockBackTimer += Time.deltaTime;
                return;
            }else{
                IsKnockBacked = false;
            }
        }

        if(Target == null){
            Target = GetRandomActiveGameObject();
        }

        UpdatePatterns();
        
        Vector2[] directions;
        Vector2 possiblePlayerPos = (Vector2)Target.transform.position;
        
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
        
        float[] weights = EvaluateDirections(possiblePlayerPos, (Vector2)transform.position, out directions, _curPattern.PatternWeights);

        Vector2 steeringDirection = ComputeSteeringDirection(weights, directions);
        steeringDirection += RepulsionForce() * RepulsionForceMult;
        ApplySteering(steeringDirection);
    }

    Vector2 RepulsionForce()
{
    Collider2D[] fiends = Physics2D.OverlapCircleAll(transform.position, 50f, _fiendLayer);
    Vector2 finalForce = Vector2.zero;

    foreach (Collider2D col in fiends)
    {
        Vector2 directionToFiend = transform.position - col.transform.position;
        float distance = directionToFiend.magnitude;

        // Avoid division by zero and ensure meaningful repulsion at very close distances
        if (distance > 0)
        {
            float repulsionForce = 1 / (distance * distance);
            finalForce += directionToFiend.normalized * repulsionForce; // Normalize direction and apply force magnitude
        }
    }

    return finalForce;
}

    public GameObject GetRandomActiveGameObject()
    {
        Collider2D[] fiends = Physics2D.OverlapCircleAll(transform.position, 1000000f, _fiendLayer);
        List<GameObject> fiend = new List<GameObject>();

        foreach (Collider2D co in fiends)
        {
            float Dist = Vector2.Distance(co.transform.position, transform.position);
            if (Dist != 0)
            {
                fiend.Add(co.gameObject);
            }
        }

        // Ensure there are valid fiends in the list
        if (fiend.Count == 0)
        {
            Debug.LogWarning("No active game objects found within range.");
            return null; // Or handle this case differently
        }

        // Safely get a random game object
        return fiend[UnityEngine.Random.Range(0, fiend.Count)];
    }



    void UpdatePatterns(){
        float playerDistance = Vector2.Distance(Target.transform.position, transform.position);
        if(_isCharging && _state == EnnemyStates.Isometric){
            if(playerDistance < 11 + Target.transform.localScale.x - 1){
                _isCharging = false;
                Instantiate(clonePrefab, transform.position, Quaternion.identity);
                Target.GetComponent<IKnockbakable>().SetKnockBack(_rb.velocity*2); 

                IEnnemy ennemy;
                if(Target.TryGetComponent<IEnnemy>(out ennemy)){
                    ennemy.SubbHealth();
                    Health ++;
                    transform.localScale = transform.localScale + new Vector3(1,1,1);
                }
                
                
                Target = GetRandomActiveGameObject();
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
        }else{
            _state = EnnemyStates.Isometric;
            _isCharging = true;
            _realSpeed = 4*MaxSpeed*Mathf.Sqrt(transform.localScale.x);
        }
    }


    /// <summary>
    /// Evaluates all directions and assigns weights based on alignment, obstacles, and patterns.
    /// </summary>
    float[] EvaluateDirections(Vector2 playerPosition, Vector2 selfPosition, out Vector2[] directions, float[] _patterns)
    {
        Vector2[] resultDirections = new Vector2[RayCount];
        float[] weights = new float[RayCount];
        Vector2 targetDirection = (playerPosition - selfPosition).normalized;
        

        float angleStep = 360f / RayCount; // Angle step for each ray

        float closestAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        closestAngle = (closestAngle + 360) % 360; // Ensure angle is positive
        int startIndex = Mathf.RoundToInt(closestAngle / angleStep) % RayCount;


        for (int i = 0; i < RayCount; i++)
        {
            int index = (startIndex + i) % RayCount;
            // Calculate direction for the ray
            float angle = Mathf.Deg2Rad * (index * angleStep);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            resultDirections[index] = direction;

            // Check for obstacles in the direction
            bool obstacle = Physics2D.Raycast(selfPosition, direction, 5, _obstacleLayer);
            if (obstacle)
            {
                weights[index] = 0f; // Obstacle blocks this direction
                Debug.DrawLine(selfPosition, selfPosition + direction * 1, Color.red);
                continue;
            }

            // Compute alignment with target direction
            float alignment = Vector2.Dot(direction, targetDirection);

            // Apply pattern and alignment to calculate weight
            float patternBias = _patterns[i % _patterns.Length]; // Handle pattern wrap-around
            weights[index] = Mathf.Max(0,alignment) * patternBias + patternBias;

            // Debug visual for valid directions
            Debug.DrawLine(selfPosition, selfPosition + direction * weights[index], Color.green);
        }

        directions = resultDirections;
        return weights;
    }

    /// <summary>
    /// Computes the final steering direction by blending all weighted directions.
    /// </summary>
    Vector2 ComputeSteeringDirection(float[] weights, Vector2[] directions)
    {
        Vector2 resultantDirection = Vector2.zero;
        float totalWeight = 0f;

        for (int i = 0; i < RayCount; i++)
        {
            resultantDirection += directions[i] * weights[i];
            totalWeight += weights[i];
        }

        return resultantDirection.normalized; // Ensure direction is normalized
    }

    /// <summary>
    /// Applies the computed steering direction to the Rigidbody2D.
    /// </summary>
    void ApplySteering(Vector2 steeringDirection)
    {
        if (steeringDirection != Vector2.zero)
        {
            float dist = Vector2.Distance(transform.position, Target.transform.position);

            // Apply a smoother curve (e.g., exponential decay)
            float speedFactor = Mathf.Exp(-dist / 1000f); // Adjust the divisor for scaling effect
            _rb.velocity = Vector2.Lerp(_rb.velocity, steeringDirection * (_realSpeed * speedFactor), _smoothF);

        }
        else
        {
            _rb.velocity = Vector2.zero; // Stop if no valid direction
        }
    }

    void OnCollisionEnter2D(Collision2D collision){
        if(collision.gameObject.CompareTag("Player")){
            GetComponent<IKnockbakable>().SetKnockBack(collision.rigidbody.velocity*4);
            Health --;
        }
        
        Target = collision.gameObject;
    }
}

public interface IKnockbakable{
    Rigidbody2D Rb {get;}
    float KnockBackDuration {get;}
    float KnockBackTimer {get; set;}
    bool IsKnockBacked {get; set;}

    public void SetKnockBack(Vector2 knockback){
        Rb.velocity += knockback;
        IsKnockBacked = true;
        KnockBackTimer = 0;
    }
}
