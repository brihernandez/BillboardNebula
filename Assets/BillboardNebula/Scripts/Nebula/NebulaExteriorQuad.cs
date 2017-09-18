using UnityEngine;

public class NebulaExteriorQuad : MonoBehaviour
{
    [HideInInspector]
    public Transform coreTransform;
    [HideInInspector]
    public int renderOrder = 0;

    [HideInInspector]
    public float nebRadius = 0f;

    public float radius = 0f;

    public Texture[] textureOptions;

    Material mat;
    
    public Color startColor;
    Color lastFrameColor;

    new Transform transform;
    Transform refCamTransform;

    void Awake()
    {
        mat = GetComponentInChildren<MeshRenderer>().material;
        refCamTransform = Camera.main.transform;
        transform = GetComponent<Transform>();
    }

    void Start()
    {
        mat.SetTexture(0, textureOptions[Random.Range(0, textureOptions.Length)]);

        // Initialize the color.
        mat.SetColor("_TintColor", startColor);
        lastFrameColor = startColor;

        // Each exterior quad gameobject is expected to have the Quad on a child gameobject.
        // This allows the quad to be easily given a random rotation to start with.
        Transform tForm = transform.GetChild(0);
        tForm.localEulerAngles = new Vector3(0f, 0f, Random.Range(-90f, 90f));

        // Specify an order in the queue that way there's no Z-fighting.
        if (renderOrder != 0)
            mat.renderQueue = renderOrder;
    }

    // Update is called once per frame
    void Update()
    {
        float camDistToQuad = Vector3.Distance(refCamTransform.position, transform.position);
        float camDistToCore = Vector3.Distance(refCamTransform.position, coreTransform.position);

        // Hide the quad if it's far behind the center of the nebula. This prevents billboards
        // directly behind the nebula from being rendered and looking weird because the draw
        // order for these things is both locked down and pretty random.
        Color col = startColor;
        if (camDistToQuad > camDistToCore + (nebRadius / 3.5f))
            col.a = 0f;
        else
        {
            float dist = (camDistToCore + (nebRadius / 3.5f)) - camDistToQuad;
            col.a = Mathf.Clamp(dist / radius, 0f, startColor.a);
        }

        // Hide the quad when the camera gets too close to it. This prevents ugly
        // rotation of the billboards that is really noticeable. Fades to zero at
        // half the radius size.
        if (camDistToQuad < radius)
        {
            col.a *= 1f - ((radius - camDistToQuad) / (radius / 2f));
        }

        // Only update the material if something changed. Updating the material is expensive.
        if (NebulaUtils.isColorDifferent(col, lastFrameColor))
            mat.SetColor("_TintColor", col);

        lastFrameColor = col;        
        
        // Only the exterior billboards rotate to face the camera. The fills just sit there.
        Vector3 vec2Cam = refCamTransform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(vec2Cam, transform.up);
    }
}