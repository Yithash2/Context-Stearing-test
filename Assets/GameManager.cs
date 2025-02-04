using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public bool ChildMechanic {get {return _childMechanic;}}
    public bool BadPC {get {return _badPC;}}

    [SerializeField]
    bool _badPC, _childMechanic = true;

    public List<GameObject> Fishes {get {return _fishes;} private set {_fishes = value;}}
    [SerializeField]
    List<GameObject> _fishes;
    public int NumberOfFishes {get {return Fishes.Count;}}

    [SerializeField]
    TextMeshPro text;
    [SerializeField]
    RectTransform textGo;

    public static GameManager Instance {get; private set;}

    void Awake(){
        if(Instance!=null){
            Debug.LogWarning("There was more then one GameManager,\n so we decided to delete the last one");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Fishes = new List<GameObject>();
    }

    void Update(){
        text.text = NumberOfFishes.ToString();
        textGo.localScale = new Vector3(1, 1,1) * NumberOfFishes;
        if(NumberOfFishes > 400)_childMechanic = false;
    }

    public GameObject GetRandomActiveGameObject(Transform fishTransform){
        List<GameObject> fiend = new List<GameObject>();

        foreach (GameObject obj in Fishes)
        {
            float Dist = Vector2.Distance(obj.transform.position, fishTransform.position);
            if (Dist != 0)
            {
                fiend.Add(obj);
            }
        }

        if (fiend.Count == 0)
        {
            Debug.LogWarning("No active game objects found within range.");
            return null; // Or handle this case differently
        }

        // Safely get a random game object
        return fiend[UnityEngine.Random.Range(0, fiend.Count)];
    }

    public GameObject GetRandomChildGameObject(out Egg p_egg){
        List<Egg> fiend = new List<Egg>();

        foreach (GameObject obj in Fishes)
        {
            
            if (obj.TryGetComponent<Egg>(out Egg egg))
            {
                fiend.Add(egg);
            }
        }

        if (fiend.Count == 0)
        {
            Debug.LogWarning("No Eggs Found.");
            p_egg = null;
            return null; // Or handle this case differently
        }

        p_egg = fiend[UnityEngine.Random.Range(0, fiend.Count)];
        return p_egg.gameObject;
    }

    public void RemoveFish(GameObject fish){
        Fishes.Remove(fish);
    }

    public void AddFish(GameObject fish){
        Fishes.Add(fish);
    }

    public Vector2 RepulsionForce(GameObject fish)
    {
        Vector2 finalForce = Vector2.zero;

        foreach (GameObject go in Fishes)
        {
            Vector2 directionToFiend = fish.transform.position - go.transform.position;
            float distance = directionToFiend.magnitude;

            // Avoid division by zero and ensure meaningful repulsion at very close distances
            if (distance > 0)
            {
                float repulsionForce = 1 / (distance * distance);
                finalForce += directionToFiend.normalized * repulsionForce; // Normalize direction and apply force magnitude
            }
        }

        return finalForce.normalized;
    }

    public Vector2 AttractionForce(GameObject fish)
    {
         Vector2 perceivedCenter = PerceivedCenter();

        Vector2 directionToCenter = perceivedCenter - (Vector2)fish.transform.position;

        return directionToCenter.normalized;
    }

    public Vector2 PerceivedCenter(){
        Vector2 perceivedCenter = Vector2.zero;

        foreach(GameObject go in Fishes){
            
            perceivedCenter += (Vector2)go.transform.position;
            
        }

        return perceivedCenter / (NumberOfFishes-1);
    }

    public Vector2 PerceivedCenterWeight(){
    Vector2 perceivedCenter = Vector2.zero;
    float totalWeight = 0f;

    // Calculate the initial perceived center as a weighted average of the fish positions
    foreach (GameObject go in Fishes)
    {
        if (go.TryGetComponent<IKillable>(out IKillable ennemy))
        {
            float distance = ((Vector2)go.transform.position - perceivedCenter).magnitude;
            
            float distanceWeight = (distance == 0f) ? 1f : 1f / distance;
            
            // The weight will now be a combination of health and distance.
            float weightedHealth = ennemy.Health * distanceWeight;
            
            perceivedCenter += (Vector2)go.transform.position * weightedHealth;
            totalWeight += weightedHealth;
        }
    }

    // Avoid division by zero
    if (totalWeight == 0)
        return Vector2.zero;

    return perceivedCenter / totalWeight;
}


}
