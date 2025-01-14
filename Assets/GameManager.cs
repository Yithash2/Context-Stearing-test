using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public List<GameObject> Fishes {get; private set;}
    public int NumberOfFishes {get {return Fishes.Count;}}

    [SerializeField]
    TextMeshPro text;

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
        text.fontSize = Camera.main.orthographicSize * 10;
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

        return finalForce;
    }

    public Vector2 AttractionForce(GameObject fish)
    {
         Vector2 perceivedCenter = PerceivedCenter();

        Vector2 directionToCenter = perceivedCenter - (Vector2)fish.transform.position;

        float force = Vector2.Distance(perceivedCenter, fish.transform.position);

        Vector2 finalForce = directionToCenter * Mathf.Sqrt(force);

        return finalForce;
    }

    public Vector2 PerceivedCenter(){
        Vector2 perceivedCenter = Vector2.zero;

        foreach(GameObject go in Fishes){
            
            perceivedCenter += (Vector2)go.transform.position;
            
        }

        return perceivedCenter / (NumberOfFishes-1);
    }
}
