using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScreen : MonoBehaviour
{
    float _currentOpacity;
    bool _isOnFire;
    [SerializeField] float fireDuration;
    float _currentfireDuration;
    private void Start()
    {
        FullScreenManager.Instance.FireVignette.SetFloat("_Opacity", 0f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _currentfireDuration = fireDuration;
            _isOnFire = true;
            _currentOpacity = 1f;
            FullScreenManager.Instance.FireVignette.SetFloat("_Opacity", _currentOpacity);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isOnFire = false;

           StartCoroutine(TurnOfffire(fireDuration));
        }
    }
  
    private IEnumerator TurnOfffire (float time)
    {
        print("hola");
        _currentfireDuration -= Time.deltaTime;
        _currentOpacity = _currentOpacity - Time.deltaTime/ fireDuration;
        FullScreenManager.Instance.FireVignette.SetFloat("_Opacity", _currentOpacity);
        if (_isOnFire == false && _currentOpacity > 0f)
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(TurnOfffire(fireDuration));
        }
        yield return null;


    }
}
