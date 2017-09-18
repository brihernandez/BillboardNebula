using UnityEngine;
using System.Collections.Generic;

public class SpaceParticleQuad : MonoBehaviour
{
    public bool particleActive = false;

    [Tooltip("When false, particles align with view vector rather than point at the camera.")]
    public bool pointAtCamera = true;

    Material mat;
    MeshRenderer mesh;
    Transform meshTransform;
    public Texture[] textureOptions;

    int colorPropID = 0;
    Color startColor;
    Color startColorNoA;

    new Transform transform;
    Camera refCam;
    Transform refCamTransform;

    Vector3 vel;
    float rotVel;
    float radius;
    float drawDistance;

    float nearFadeDistance;
    float farFadeDistance;

    float densityFade;


    void Awake()
    {
        transform = GetComponent<Transform>();
        refCam = Camera.main;
        refCamTransform = refCam.transform;

        mesh = GetComponentInChildren<MeshRenderer>();
        meshTransform = mesh.GetComponent<Transform>();
        mat = mesh.material;
    }

    void Start()
    {
        // Set a random texture based on the choices given.
        mat.SetTexture(0, textureOptions[Random.Range(0, textureOptions.Length)]);
        mat.renderQueue = 4000;
        startColor = mat.GetColor("_TintColor");
        
        startColorNoA = startColor;
        startColorNoA.a = 0f;

        colorPropID = Shader.PropertyToID("_TintColor");

        // Each space particle gameobject is expected to have the Quad on a child gameobject.
        // This allows the quad to be easily given a random rotation to start with.
        meshTransform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-90f, 90f));
    }

    void Update()
    {
        if (particleActive)
        {
            // First find if the particle is outside of the cameras bounds.
            bool visible = CheckVisibility();
            
            // Normal case when it's on camera.
            if (visible)
            {
                // Move the quad around based on the initial velocity.
                transform.position += vel * Time.deltaTime;

                // Rotate quad to face the camera
                if (pointAtCamera)
                {
                    Vector3 vec2Cam = refCamTransform.position - transform.position;
                    transform.rotation = Quaternion.LookRotation(vec2Cam, transform.up);
                }

                // Alternatively, align the quad with the camera's look vector.
                else
                {
                    transform.rotation = Quaternion.LookRotation(-refCamTransform.forward, transform.up);
                }

                // Rotate the (actual) quad to give it more interest.
                if (rotVel != 0f)
                    meshTransform.Rotate(0f, 0f, rotVel * Time.deltaTime);
            }

            else
            {
                particleActive = false;
                mesh.enabled = false;
            }
        }

        else
        {
            mesh.enabled = false;
        }
    }

    private bool CheckVisibility()
    {
        bool visible = false;

        // Check FrustumCode.txt for the old frustum based code.

        // If they are in the frustum, then check for distance. They are allowed to persist
        // at half the max distance when out of view. This way particles immediately behind you
        // and just off screen don't disappear just because you looked away for an instant.
        float distToCam = Vector3.Distance(refCamTransform.position, transform.position);

        // Distance method     
        if (distToCam < drawDistance)
        {
            visible = true;

            // Fade the particle if it's getting too far.
            float endFadeOutStart = drawDistance - radius;
            if (distToCam > endFadeOutStart)
            {
                Color col = Color.Lerp(startColor, startColorNoA, (distToCam - endFadeOutStart) / (radius));
                col.a *= densityFade;
                mat.SetColor(colorPropID, col);
            }

            // Fade the particle out if it's getting too close.
            if (distToCam < farFadeDistance && distToCam > nearFadeDistance)
            {
                Color col = Color.Lerp(startColorNoA, startColor, (distToCam - nearFadeDistance) / (farFadeDistance - nearFadeDistance));
                col.a *= densityFade;
                mat.SetColor(colorPropID, col);

                visible = true;
            }

            else if (distToCam < nearFadeDistance)
            {
                mat.SetColor(colorPropID, startColorNoA);
            }
        }        

        return visible;
    }

    public bool Initialize(Vector3 startVel, float startRot, float startRadius, float maxDistance, float nearFade, float farFade, Color newCol, float nebDensity)
    {
        mesh.enabled = true;
        particleActive = true;

        vel = startVel;
        rotVel = startRot;

        radius = startRadius;
        transform.localScale = Vector3.one * radius * 2f;

        drawDistance = maxDistance;
        nearFadeDistance = nearFade;
        farFadeDistance = farFade;

        startColor = newCol;
        startColorNoA = startColor;
        startColorNoA.a = 0f;
        mat.SetColor("_TintColor", startColor);

        densityFade = nebDensity;

        return true;
    }
}

public class SpaceParticlePool
{
    List<SpaceParticleQuad> pool;
    SpaceParticleQuad spacePar;

    public SpaceParticlePool(int poolSize, SpaceParticleQuad newPar)
    {
        pool = new List<SpaceParticleQuad>(poolSize);
        spacePar = newPar;

        for (int i = 0; i < pool.Capacity; i++)
        {
            SpaceParticleQuad par = (SpaceParticleQuad)GameObject.Instantiate(spacePar);
            pool.Add(par);
        }
    }

    public bool ActivateParticle(Vector3 pos, Vector3 startVel, float startRot, float startRadius, float maxDistance, float nearFade, float farFade, Color color, float nebDensity)
    {
        // Find the first inactive particle.
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].particleActive)
            {
                pool[i].transform.position = pos;
                pool[i].Initialize(startVel, startRot, startRadius, maxDistance, nearFade, farFade, color, nebDensity);
                return true;
            }
        }

        // Return false if there weren't any available particles.
        return false;
    }

    public bool CheckAvailable()
    {
        foreach (SpaceParticleQuad quad in pool)
        {
            if (!quad.particleActive)
                return true;
        }

        return false;
    }

    //public void PrepForDeleteParticles()
    //{
    //    for (int i = 0; i < pool.Count; i++)
    //    {
    //        if (pool[i] != null)
    //            GameObject.Destroy(pool[i].gameObject);
    //    }
    //}
}
