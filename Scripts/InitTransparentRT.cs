using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitTransparentRT : MonoBehaviour
{
    public RenderTexture rt;

    void Start()
    {
        if (rt != null)
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, new Color(0, 0, 0, 0)); // Alpha = 0
            RenderTexture.active = null;
        }
    }
}
