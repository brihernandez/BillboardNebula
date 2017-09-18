using UnityEngine;

[RequireComponent(typeof(Nebula))]
public class NebulaPuffs : MonoBehaviour
{
    Nebula parentNebula;

    SpaceParticlePool pool;
    public SpaceParticleQuad particle;
    public bool emitPuffs = false;

    public Color puffColor = Color.white;

    public int count = 20;
    public float radius = 150f;
    public float sizeVariation = 0.2f;
    public float maxDistance = 400f;
    public float drift = 1f;
    public float driftRotation = 5f;
    public float nearFadeDistance = 125f;
    public float farFadeDistance = 150f;

    Camera refCam;
    NebulaCameraFog refCamNebula;
    Transform refCamTransform;

    void Awake()
    {
        refCam = Camera.main;
        refCamNebula = refCam.GetComponent<NebulaCameraFog>();
        refCamTransform = refCam.transform;

        parentNebula = GetComponent<Nebula>();
    }

    void Start()
    {
        pool = new SpaceParticlePool(count, particle);
    }

    void Update()
    {
        if (refCamNebula.fadingNebula == parentNebula)
            emitPuffs = true;
        else
            emitPuffs = false;

        if (emitPuffs)
        {
            Vector3 pos = Vector3.zero;
            Vector3 startVel = Vector3.zero;
            float startRadius = radius;
            float startRot = 0f;

            // Find a random point in front of the camera and instantiate a particle there.
            while (pool.CheckAvailable())
            {
                // Sphere method.

                // Generate the sphere with respect to the camera's velocity. If sitting still,
                // then the sphere is centered. If the camera moves, then the sphere is biased in
                // that direction.
                pos = Random.onUnitSphere * maxDistance * 0.999f;

                startVel = Random.onUnitSphere * Random.Range(-drift, drift);
                startRot = Random.Range(-driftRotation, driftRotation);

                startRadius = radius * Random.Range(1f - sizeVariation, 1f + sizeVariation);

                // Convert all these local coordinates into world coordinates.
                pos = refCamTransform.TransformPoint(pos);

                // DEBUG 
                //Debug.DrawLine(refCamTransform.position, pos, Color.red);

                // Fade the particles in based on how dense the nebula is at this point.
                float densityFade = refCamNebula.density;

                // Create the particle.
                pool.ActivateParticle(pos, startVel, startRot, startRadius, maxDistance, nearFadeDistance, farFadeDistance, puffColor, densityFade);
            }
        }
    }

    //public void OnDestroy()
    //{
    //    pool.PrepForDeleteParticles();
    //}    
}