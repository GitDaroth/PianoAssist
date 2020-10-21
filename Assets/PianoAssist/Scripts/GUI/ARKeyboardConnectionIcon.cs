using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARKeyboardConnectionIcon : MonoBehaviour
{
    public Material m_connectedMaterial;
    public Material m_disconnectedMaterial;

    void Start()
    {
        gameObject.GetComponent<MeshRenderer>().material = m_disconnectedMaterial;
    }

    void Update()
    {
        
    }

    public void SetIsConnected(bool isConnected)
    {
        if(isConnected)
            gameObject.GetComponent<MeshRenderer>().material = m_connectedMaterial;
        else
            gameObject.GetComponent<MeshRenderer>().material = m_disconnectedMaterial;
    }
}
