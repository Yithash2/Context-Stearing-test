using UnityEngine;
using System.Collections;

public class Egg : MonoBehaviour, IKillable{
    public int Health {get {return _heath;} set {_heath = value;}}

    [SerializeField]
    int _heath;

    [SerializeField]
    private float _timeUntilHatch;

    [SerializeField]
    GameObject _fish;

    public int Fertility;

    [ContextMenu("Hatch Egg")]
    public void StartHatching(){
        StartCoroutine(HatchInSec());
    }

    IEnumerator HatchInSec(){
        yield return new WaitForSeconds(_timeUntilHatch);
        GameObject go = Instantiate(_fish);
        GameManager.Instance.RemoveFish(this.gameObject);
        GameManager.Instance.AddFish(go);
        Destroy(this.gameObject);
        go.TryGetComponent<Fish>(out Fish _f);
        if(_f)_f.MaxNumberOfChild = Fertility;
    }
}