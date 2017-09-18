using UnityEngine;

public class NebulaFillQuad : MonoBehaviour
{
    Nebula parentNebula;

    [HideInInspector]
    public int renderOrder = 0;

    [HideInInspector]
    public float radius = 0f;

    bool longitudinal = false;
    
    Vector3 nebForwardVec;
    Vector3 nebUpVec;

    const float headOnFaceFadeStartAngle = 30f;
    const float headOnFaceFadeEndAngle = 50f;
    const float headOnVertFadeStartAngle = 50f;
    const float headOnVertFadeEndAngle = 30f;
    
    const float longFadeStartAngle = 50f;
    const float longFadeEndAngle = 70f;
    
    public float angleToCamera = 0f;
    public float angleToFront = 0f;

    Material mat;
    public Color startColor;
    Color lastFrameColor;

    public float temp = 0f;

    new Transform transform;
    Transform refCamTransform;
    NebulaCameraFog camNebFog;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        transform = GetComponent<Transform>();
        refCamTransform = Camera.main.transform;
        camNebFog = Camera.main.GetComponent<NebulaCameraFog>();
    }

    void Start()
    {
        //startColor = mat.GetColor("_TintColor");
        lastFrameColor = startColor;

        mat.renderQueue = 3001;
    }

    public void SetLongitudinal(Vector3 forwardVec, Vector3 dimensions, Color col, Nebula parent)
    {
        // Set the initial rotation such that this quad will fill the dimensions of the ellipsoid.
        nebForwardVec = forwardVec;

        transform.rotation = Quaternion.LookRotation(nebForwardVec);

        // Set the size of the quad.
        transform.localScale = dimensions;
        longitudinal = true;

        parentNebula = parent;

        // Initialize the color.
        startColor = col;
        mat.SetColor("_TintColor", startColor);
    }

    public void SetHeadOn(Vector3 dimensions, Vector3 upVec, float bitRadius, Color col, Nebula parent)
    {
        // When spawned, point the quad at the camera. This quad is used to display something when
        // we are looking down the thin end of the nebula.
        transform.rotation = Quaternion.LookRotation(nebForwardVec);

        nebUpVec = upVec;
        
        // Set the size to be what would be seen from head on.
        float minSize = Mathf.Min(dimensions.x, dimensions.z);
        transform.localScale = new Vector3(minSize, dimensions.y, minSize);

        longitudinal = false;
        
        parentNebula = parent;

        // Initialize the color.
        startColor = col;
        mat.SetColor("_TintColor", startColor);
    }

    // Update is called once per frame
    void Update()
    {        
        Color col = startColor;

        // The fill clouds keep track of the camera angle because it causes them to fade.
        Vector3 vec2Cam = refCamTransform.position - transform.position;

        // Rotate the longitudinal quad. This one rotates along its axis.
        if (longitudinal)
        {
            // Calculate the angle to the camera. Used to fade when at near perpendicular angles.
            angleToCamera = Vector3.Angle(transform.forward, vec2Cam);

            // Remove the X component in local space. This way the original direction is preserved
            // and the billboard rotates along the nebula's longitudinal axis.
            Vector3 localVec2Cam = transform.InverseTransformDirection(vec2Cam);
            localVec2Cam.x = 0f;
            vec2Cam = transform.TransformDirection(localVec2Cam);

            transform.rotation = Quaternion.LookRotation(vec2Cam, transform.up);

            // Fade the cloud when the angle gets too big.
            // When over 30 degrees, fade to nothing when it hits 45 degrees.
            if (angleToCamera > longFadeStartAngle && angleToCamera < longFadeEndAngle)
            {
                col.a *= (1 - ((angleToCamera - longFadeStartAngle) / (longFadeEndAngle - longFadeStartAngle)));
            }
            else if (angleToCamera > longFadeEndAngle)
            {
                col.a = 0f;
            }
        }

        // Rotate the head on quad so that it's always facing the camera.
        else
        {
            // Remove the X component in local space. This way the original direction is preserved
            // and the billboard rotates along the nebula's longitudinal axis.
            Vector3 localVec2Pole = transform.InverseTransformDirection(vec2Cam);
            localVec2Pole.y = 0f;
            Vector3 vec2CamAlt = transform.TransformDirection(localVec2Pole);

            // Calculate the angle from the north/south pole to the camera. This is used to
            // fade the billboard so that you don't see when it flips around.
            angleToFront = Vector3.Angle(nebUpVec, vec2CamAlt);
            angleToCamera = Vector3.Angle(transform.forward, vec2Cam);

            // Remove the X component in local space. This way the original direction is preserved
            // and the billboard rotates along the nebula's longitudinal axis.
            localVec2Pole = transform.InverseTransformDirection(vec2Cam);
            localVec2Pole.y = 0f;
            vec2Cam = transform.TransformDirection(localVec2Pole);

            transform.rotation = Quaternion.LookRotation(vec2Cam, Vector3.up);

            // Count the reverse direction
            if (angleToFront > 90f)
                angleToFront = (180f - angleToFront) * 1f;

            // Fade the cloud when the angle gets too small.
            if (angleToFront < headOnVertFadeStartAngle && angleToFront > headOnVertFadeEndAngle)
            {
                col.a *= (angleToFront - headOnVertFadeEndAngle) / (headOnVertFadeStartAngle - headOnVertFadeEndAngle);
            }
            else if (angleToFront < headOnVertFadeEndAngle)
            {
                col.a = 0f;
            }

            // Fade the cloud when the angle gets too big.
            // When over 30 degrees, fade to nothing when it hits 45 degrees.
            if (angleToCamera > headOnFaceFadeStartAngle && angleToCamera < headOnFaceFadeEndAngle)
            {
                col.a *= (1 - ((angleToCamera - headOnFaceFadeStartAngle) / (headOnFaceFadeEndAngle - headOnFaceFadeStartAngle)));
            }
            else if (angleToCamera > headOnFaceFadeEndAngle)
            {
                col.a = 0f;
            }           
        }

        // Hide the quad when the camera is inside this nebula.
        if (camNebFog != null && camNebFog.insideNebula == parentNebula)
            col.a = 0f;

        // Only update the material if something changed. Updating the material is expensive.
        if (NebulaUtils.isColorDifferent(col, lastFrameColor))
            mat.SetColor("_TintColor", col);

        lastFrameColor = col;
    }
}