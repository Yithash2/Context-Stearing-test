using UnityEngine;

public abstract class Fish : MonoBehaviour, IKnockbakable, IKillable{

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
    LayerMask _obstacleLayer;

    [SerializeField]
    GameObject _explosionPrefab;

    protected GameObject ExplosionPrefab{get {return _explosionPrefab;}}

    public bool IsCharging;

    private GameManager _gameMan;

    protected GameManager GameMan {get {return _gameMan;}}

    [SerializeField, Range(0.001f, 1f)]
    float _smoothF;

    [Header("Child Mechanic")]

    [SerializeField]
    int _litter;

    public int MaxNumberOfChild;

    [SerializeField]
    private int _numbOfChild = 0;

    [SerializeField]
    GameObject _egg;
 

 
    void Start()
    {
        _maxSpeed = Random.Range(_maxSpeed*0.5f, _maxSpeed*2f);
        _gameMan = GameManager.Instance;
        _realSpeed = _maxSpeed;
        ChangeTarget();
        AfterStart();
    }

    protected void CalculateChild(){
        if(_gameMan.ChildMechanic){
            if(_numbOfChild + _litter < MaxNumberOfChild){
                MakeChild(_litter);
            }else if(_numbOfChild < MaxNumberOfChild){
                MakeChild(MaxNumberOfChild - _numbOfChild);
            }
        }
    }

    void MakeChild(int n){
        Vector2 posBehind = (Vector2)gameObject.transform.position - _rb.velocity.normalized * (gameObject.transform.localScale.x + 10);
        float angleRange = 90f; // Angle in degrees for the arc (e.g., -22.5 to +22.5 degrees)
        float radius = 10f; // Distance between the object and the eggs
        for(int i = 1; i <= n; i++){
            float angle = Mathf.Lerp(-angleRange / 2, angleRange / 2, (float)i / (n - 1));
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);
            Vector2 spawnPosition = posBehind + offset;
            GameObject go = Instantiate(_egg, spawnPosition, Quaternion.identity);
            go.TryGetComponent<Egg>(out Egg egg);
            if(egg){
                egg.Fertility = Random.Range(0,MaxNumberOfChild);
                egg.StartHatching();
            }
                
            _gameMan.AddFish(go);
        }

        _numbOfChild += n;
    }

    void Update()
    {       
        if(IsKnockBacked){
            if(KnockBackTimer < KnockBackDuration){
                KnockBackTimer += Time.deltaTime;
                return;
            }else{
                IsKnockBacked = false;
            }
        }

        if(Target == null && _gameMan.NumberOfFishes > 1){
            ChangeTarget();
            CalculateChild();
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

    protected abstract void AfterStart();

    protected virtual void ChangeTarget(){
        Target = _gameMan.GetRandomActiveGameObject(transform);
        
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

    public void Die(){
        Debug.Log("Destroy Fish");
        _gameMan.RemoveFish(gameObject);
        Destroy(gameObject);   
    }

    void OnCollisionEnter2D(Collision2D collision){     
    }
}

public interface IKillable{

    public int Health {get; set;}

    public void SubbHealthEvent(int heathSubbed){
        Health -= heathSubbed;
        if(Health <= 0){
            Die();
        }
    }

    public void Die();
}