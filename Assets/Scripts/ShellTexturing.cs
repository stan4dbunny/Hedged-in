using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellTexturing : MonoBehaviour
{
    Texture2D texture;
    public int resolution;
    public int layers;
    public float height;
    public float thickness;
    public float minimumHairLength;
   
    //public Texture2D blackAndWhiteTexture;

    public Material material;


    Renderer renderer;


    void Start()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.name = "TEST";
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float randVal = minimumHairLength + Random.value;
                print(randVal);
                texture.SetPixel(x, y, new Color(randVal, randVal, randVal, randVal));
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        material.SetTexture("_MainTex", texture);

        //give the regular texture to the shader/GPU
        renderer = GetComponent<Renderer>();
        // Access the mainTexture of the material
        Texture mainTexture = renderer.material.mainTexture;
        material.SetTexture("_AlbedoTex", mainTexture);  
    }

    private void Update()
    {
        RenderParams renderParams = new RenderParams(material);

        renderParams.matProps = new MaterialPropertyBlock();
        renderParams.matProps.SetTexture("_MainTex", texture);
        renderParams.matProps.SetInt("_Resolution", resolution);
        renderParams.matProps.SetFloat("_Thickness", thickness);
        renderParams.matProps.SetInt("_Layers", layers);
        renderParams.matProps.SetFloat("_Height", height);
        //shadows
        renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderParams.worldBounds = renderer.bounds;
        
        int prevHeight = 0;
        for (int i = 0; i < layers; i++)
        {
            Matrix4x4 localToWorld = transform.localToWorldMatrix;
            renderParams.matProps.SetFloat("_PrevHeight", prevHeight / (float)layers);
            renderParams.matProps.SetFloat("_CurrHeight", i);
            Graphics.RenderMesh(renderParams, GetComponent<MeshFilter>().mesh, 0, localToWorld);

            prevHeight = i;
        }
    }
}