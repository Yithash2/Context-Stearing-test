using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField]
    Transform _target;

    [SerializeField]
    LayerMask _fiendLayer;

    Vector3 _offset = new Vector3(0,0,-10);

    [SerializeField, Range(0.1f, 1)]
    float _smoothF;

    void Start(){
        StartCoroutine(FindTarget());
    }

    void Update(){
        
    }
    void FixedUpdate()
    {
        if(_target == null || Input.GetKeyDown(KeyCode.Space))
        _target = GetRandomActiveGameObject().transform;

        transform.position = Vector3.Lerp(transform.position, _target.position + _offset, _smoothF);
    }

    public GameObject GetRandomActiveGameObject()
    {
        Collider2D[] fiends = Physics2D.OverlapCircleAll(transform.position, 1000000f, _fiendLayer);
        List<IEnnemy> fiend = new List<IEnnemy>();

        foreach (Collider2D co in fiends)
        {
            if(co.gameObject.TryGetComponent<IEnnemy>(out IEnnemy friend)){
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
