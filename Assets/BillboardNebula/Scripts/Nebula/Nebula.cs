using UnityEngine;

public class Nebula : MonoBehaviour
{
    public Color ambientLight = Color.white;
    public Color nebulaColor = Color.white;

    public float maxViewDistance = 2000f;
    public float fogStart = 0f;
    public float fogEnd = 2000f;

    public Vector3 dimensions = new Vector3(45000.0f, 20000.0f, 25000.0f);

    public float bitRadius = 140000.0f;
    public float bitRadiusDeviation = 0.2f;
    public NebulaFillQuad circleSprite;
    public NebulaExteriorQuad exteriorSprite;

    public int exteriorSlices = 25;

    // Use this for initialization
    void Start()
    {
        // Central component of the nebula that gets rendered behind the exterior surrounding quads.
        CreateFillQuads();

        // Done in three layers: Central ring, high ring, and low ring.
        CreateExteriorQuadLayer(1, 1f, 0f);
        CreateExteriorQuadLayer(2, 0.5f, dimensions.y / 6f);
        CreateExteriorQuadLayer(2, 0.5f, -dimensions.y / 6f);

        // Scale the collider to the appropriate size.
        Transform collider = transform.Find("Collider");
        collider.localScale = dimensions * 0.7f;
    }

    private void CreateExteriorQuadLayer(int sliceDivisor, float horizontalScale, float heightOffset)
    {
        Transform obj;
        NebulaExteriorQuad neb;
        
        float angle = 0f;
        float radDeviation = 0f;

        for (int i = 1; i <= exteriorSlices / sliceDivisor; i++)
        {
            angle = 360f / exteriorSlices * sliceDivisor * i * Mathf.Deg2Rad;
            radDeviation = bitRadius * Random.Range(1f - bitRadiusDeviation, 1f + bitRadiusDeviation);

            obj = Instantiate(exteriorSprite).transform;
            obj.parent = transform;
            neb = obj.GetComponent<NebulaExteriorQuad>();

            neb.radius = radDeviation * 0.8f;
            neb.coreTransform = transform;
            neb.renderOrder = 3005 + i;
            neb.nebRadius = Mathf.Min(dimensions.x, dimensions.z, dimensions.y);
            neb.startColor = nebulaColor;

            obj.localEulerAngles = new Vector3(0f, -angle * Mathf.Rad2Deg + 270f, 0f);

            //float deviation = Random.Range(-radDeviation, radDeviation) / 50f;
            obj.localPosition = new Vector3(Mathf.Cos(angle) * dimensions.x / 2.5f * horizontalScale,
                                            heightOffset,
                                            Mathf.Sin(angle) * dimensions.z / 2.5f * horizontalScale);

            obj.localScale = Vector3.one * radDeviation * 0.8f;
        }
    }

    private void CreateFillQuads()
    {
        // This is the head on quad.
        // Because these two quads share the exact same point, their draw order
        // depends on who gets created first. Prefer to draw the head on first.
        NebulaFillQuad quad = Instantiate(circleSprite).GetComponent<NebulaFillQuad>();
        quad.transform.parent = transform;
        quad.transform.localPosition = Vector3.zero;
        quad.radius = Mathf.Min(dimensions.x, dimensions.z, dimensions.y);
        quad.SetHeadOn(dimensions, transform.forward, bitRadius, nebulaColor, this);

        // This is the longitudinal quad.
        quad = Instantiate(circleSprite).GetComponent<NebulaFillQuad>();
        quad.transform.parent = transform;
        quad.transform.localPosition = Vector3.zero;
        quad.radius = Mathf.Min(dimensions.x, dimensions.z, dimensions.y);
        quad.SetLongitudinal(transform.forward, dimensions, nebulaColor, this);
    }
}