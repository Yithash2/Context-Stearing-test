using UnityEngine;

public abstract class Ennemy : MonoBehaviour, IKnockbakable{

    public int Health {get {return _heath;} set {_heath = value;}}

    [SerializeField]
    int _heath;

    public float KnockBackDuration {get {return _knockBackDuration;}}
    public float KnockBackTimer {get; set;}
    public Rigidbody2D Rb {get {return _rb;}}

    [SerializeField] float _knockBackDuration = 0.5f;

    public bool IsKnockBacked {get; set;}

    public GameObject Target {get {return _target;} set {_target = value;}}
    [SerializeField]
    private GameObject _target;

    [SerializeField]
    protected EnnemyStates _state;

    [SerializeField]
    protected Pattern[] _patterns; // Bias for each direction

    [SerializeField]
    int RayCount = 16; // Number of rays cast

    [SerializeField]
    protected LayerMask _obstacleLayer;

    [SerializeField]
    protected LayerMask _fiendLayer;

    [SerializeField]
    Rigidbody2D _rb;

    [SerializeField]
    float _maxSpeed = 5f; // Max speed of the enemy

    protected float MaxSpeed {get {return _maxSpeed;}}

    [SerializeField]
    float RepulsionForceMult = 1f; // Max speed of the enemy

    [SerializeField]
    float AttractionForceMult = 1f;

    protected float _realSpeed;

    [SerializeField]
    GameObject _explosionPrefab;

    protected GameObject ExplosionPrefab{get {return _explosionPrefab;}}

    public bool IsCharging;

    private GameManager _gameMan;

    protected GameManager GameMan {get {return _gameMan;}}

    [SerializeField, Range(0.001f, 1f)]
    float _smoothF;

    void Start()
    {
        _maxSpeed = Random.Range(_maxSpeed*0.5f, _maxSpeed*2f);
        _gameMan = GameManager.Instance;
        UpdatePatterns();
        _realSpeed = _maxSpeed;
        Target = _gameMan.GetRandomActiveGameObject(transform);
        
    }

    void Update()
    {
        if(Health <= 0){
            Destroy(gameObject);
            _gameMan.RemoveFish(gameObject);
            return;
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
            Target = _gameMan.GetRandomActiveGameObject(transform);
        }

        
        UpdatePatterns();

        Pattern _curPattern = GetPatterns();
        Vector2[] directions;
        Vector2 possiblePlayerPos = (Vector2)Target.transform.position;
        
        float[] weights = EvaluateDirections(possiblePlayerPos, (Vector2)transform.position, out directions, _curPattern.PatternWeights);

        Vector2 steeringDirection = ComputeSteeringDirection(weights, directions);
        steeringDirection += _gameMan.RepulsionForce(gameObject) * RepulsionForceMult;
        steeringDirection += _gameMan.AttractionForce(gameObject) * AttractionForceMult;
        ApplySteering(steeringDirection);
    }

    protected abstract void UpdatePatterns();

    protected abstract Pattern GetPatterns();

    public void SubbHealthEvent(int heathSubbed){
        Health = Mathf.Clamp(Health - heathSubbed, 0, int.MaxValue);
    }


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

    void ApplySteering(Vector2 steeringDirection)
    {
        if (steeringDirection != Vector2.zero)
        {
            float dist = Vector2.Distance(transform.position, Target.transform.position);

            _rb.velocity = Vector2.Lerp(_rb.velocity.normalized * _realSpeed, steeringDirection.normalized * _realSpeed, _smoothF);

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