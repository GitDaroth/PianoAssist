using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARDarkerScreen : MonoBehaviour {

    public Shader darkerShader;
    public Color color;
    private Material material;

    void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture) 
    {
        if(darkerShader != null)
        {
            if (material == null) {
                material = new Material (darkerShader);
            }

            material.SetColor("_TintColor", color);

            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
            material = null;
        }
    }

    void OnDisable ()
    {
        if(material)
        {
            DestroyImmediate(material);
        }
    }
}
