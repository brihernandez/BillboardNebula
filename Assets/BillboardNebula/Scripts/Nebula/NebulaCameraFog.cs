using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NebulaCameraFog : MonoBehaviour
{
    Camera cam;
    [Tooltip("Point this to an image that covers an entire screen space canvas. This is required for nebula enter/exit fading effects.")]
    public Image fadeQuad;

    [Range(0, 1f)]
    public float density = 0f;

    bool initialFogEnabled;

    Color initialFog;
    float initialFogStart;
    float initialFogEnd;

    Color nebulaColor = Color.white;
    float nebulaFogStart = 0f;
    float nebulaFogEnd = 500f;
    float nebulaMaxViewDist = 500f;

    UnityEngine.Rendering.AmbientMode initialAmbientMode;
    Color initialAmbientLight;
    Color nebulaAmbientLight = Color.white;

    // TODO: Remove the arrays and replace them with Lists.

    Light[] sunLights;
    Color sunColor = Color.black;

    Nebula[] nebulas;

    public Nebula insideNebula;
    public Nebula fadingNebula;

    [Tooltip("Put all Nebula objects on a Nebula layer and set this mask to hit only that Nebula.")]
    public LayerMask nebulaMask;

    float initialFarClip;

    // Hacky workaround. See TODO above call to DistanceToNebulaSurface.
    const float MAX_NEBULA_DIST = 1000000.0f;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Get a list of all the nebula.
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Nebula");
        nebulas = new Nebula[obj.Length];
        for (int i = 0; i < nebulas.Length; i++)
            nebulas[i] = obj[i].GetComponent<Nebula>();

        // Get a list of all the "suns" in the sky.

        // TODO: Change how this works, because right now this is hyper dependent on
        // scene hierarchy. This implemention is a hack to get it working in a random
        // scene separate from the original project.
        obj = GameObject.FindGameObjectsWithTag("Sun");
        if (obj != null)
        {
            sunLights = new Light[obj.Length];

            for (int i = 0; i < obj.Length; i++)
                sunLights[i] = obj[i].GetComponent<Light>();

            if (sunLights.Length > 0)
                sunColor = sunLights[0].color;
        }
    }

    private void Start()
    {
        initialFogEnabled = RenderSettings.fog;

        initialFog = RenderSettings.fogColor;
        initialFogStart = RenderSettings.fogStartDistance;
        initialFogEnd = RenderSettings.fogEndDistance;

        initialAmbientMode = RenderSettings.ambientMode;
        initialAmbientLight = RenderSettings.ambientLight;

        initialFarClip = cam.farClipPlane;

        if (fadeQuad != null)
        {
            fadeQuad.material.renderQueue = 3500;
        }
        else
        {
            Debug.LogError(name + ": NebulaCameraFog missing assigned Fade Quad! Nebula fog fading will not work correctly!");
        }
    }

    private void OnPreRender()
    {
        if (fadeQuad != null)
        {
            // TODO: Consider re-working this UI canvas based solution. As a safety, the camera
            // is going to assign this stuff on its own that used to be set manually in the scene.
            fadeQuad.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            fadeQuad.canvas.worldCamera = cam;
        }
    }

    void Update()
    {
        if (nebulas.Length > 0)
        {
            // Find the closest nebula.
            Nebula closestNeb = nebulas[0];
            float closestDistance = float.MaxValue;
            foreach (Nebula neb in nebulas)
            {
                float dist = Vector3.Distance(transform.position, neb.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestNeb = neb;
                }
            }

            // Update the nebula stats.
            nebulaAmbientLight = closestNeb.ambientLight;
            nebulaColor = closestNeb.nebulaColor;
            nebulaFogStart = closestNeb.fogStart;
            nebulaFogEnd = closestNeb.fogEnd;
            nebulaMaxViewDist = closestNeb.maxViewDistance;


            // For now, just do it binary.
            // TODO: Figure out what to do when the raycast misses from a nebula being too far away.
            // Right now, it'll return 0 and then it'll look like you're inside the nebula.
            float distToNebSurface = NebulaUtils.DistanceToNebulaSurface(Camera.main.transform.position, closestNeb, MAX_NEBULA_DIST, nebulaMask);
            if (distToNebSurface > closestNeb.bitRadius / 3f)
            {
                insideNebula = null;
                fadingNebula = null;
                density = 0f;

                if (fadeQuad != null)
                    fadeQuad.color = Color.clear;
            }
            else if (distToNebSurface > 0f)
            {
                insideNebula = null;
                fadingNebula = closestNeb;
                density = 1f - (distToNebSurface / (closestNeb.bitRadius / 3f));

                if (fadeQuad != null)
                {
                    Color fadeIn = nebulaColor;
                    Color fadeOut = nebulaColor;
                    fadeOut.a = 0f;
                    fadeIn.a = 1f;

                    fadeQuad.color = Color.Lerp(fadeOut, fadeIn, density);
                }
            }
            else
            {
                fadingNebula = closestNeb;
                insideNebula = closestNeb;                
                density = 1f;

                if (fadeQuad != null)
                    fadeQuad.color = Color.clear;
            }

            // Update all the effects.
            ApplyNebulaEffects();
        }
    }

    private void ApplyNebulaEffects()
    {
        RenderSettings.ambientLight = Color.Lerp(initialAmbientLight, nebulaAmbientLight, density);

        if (density == 1f)
        {
            cam.farClipPlane = nebulaMaxViewDist;
            cam.backgroundColor = nebulaColor;
            cam.clearFlags = CameraClearFlags.Color;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = nebulaAmbientLight;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = nebulaColor;
            RenderSettings.fogStartDistance = nebulaFogStart;
            RenderSettings.fogEndDistance = nebulaFogEnd;
        }
        else if (density > 0f)
        {
            cam.farClipPlane = initialFarClip;
            cam.clearFlags = CameraClearFlags.Skybox;

            RenderSettings.fog = initialFogEnabled;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = initialFog;
            RenderSettings.fogStartDistance = initialFogStart;
            RenderSettings.fogEndDistance = initialFogEnd;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = Color.Lerp(initialAmbientLight, nebulaAmbientLight, density);
        }
        else
        {
            RenderSettings.ambientMode = initialAmbientMode;
            RenderSettings.ambientSkyColor = initialAmbientLight;
        }

        // Fade the light because the nebula is blocking most of it.
        foreach (Light sun in sunLights)
        {
            sun.color = Color.Lerp(sunColor, nebulaColor, density * 0.5f);
            sun.shadowStrength = 1 - density * 0.8f;
        }
    }
}