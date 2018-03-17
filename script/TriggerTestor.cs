using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTestor : MonoBehaviour {
    public GameObject sd;

    void OnTriggerEnter()
    {
        Debug.Log("dsfa");
        sd.SetActive(true);
        this.enabled = false;
    }

    public void ToggleTrigger(bool isOn)
    {
        if (isOn)
        {
            Debug.Log(name);
        }
    }
}
