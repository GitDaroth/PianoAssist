using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ARHolographicEffect : MonoBehaviour {

    public Shader holoShader;
    private Material material;

    void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture) 
    {
        if(holoShader != null)
        {
            if (material == null) {
                material = new Material (holoShader);
            }

            Graphics.Blit(sourceTexture, destTexture, material);
            //RenderBuffer buffer = destTexture.colorBuffer;
            //IntPtr ptr = buffer.GetNativeRenderBufferPtr();

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
