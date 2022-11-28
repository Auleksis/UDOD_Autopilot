using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targets : MonoBehaviour
{
    public bool got = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!got)
        {
            AirplaneController airplane = GameObject.Find("Aircraft").GetComponent<AirplaneController>();
            airplane.current++;
            got = true;
        }
    }
}
