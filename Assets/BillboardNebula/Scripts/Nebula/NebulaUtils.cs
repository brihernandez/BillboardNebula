using UnityEngine;

public static class NebulaUtils
{
    // This is faster than comparing color the direct way. Alpha is the most likely thing
    // to change, so it checks for that first.
    public static bool isColorDifferent(Color c1, Color c2)
    {
        if (c1.a != c2.a)
            return true;
        else if (c1.r != c2.r)
            return true;
        else if (c1.g != c2.g)
            return true;
        else if (c1.b != c2.b)
            return true;

        return false;
    }
    
    /// <summary>
    /// Returns the distance from a point to the surface of a given nebula.
    /// </summary>
    /// <param name="from">Point to measure distance to surface.</param>
    /// <param name="neb">Nebula whose surface is being checked.</param>
    /// <param name="rayDistance">Maximum distance for this raycast.</param>
    /// <param name="nebulaMask">Mask with only nebula objects on it.</param>
    /// <returns></returns>
    public static float DistanceToNebulaSurface(Vector3 from, Nebula neb, float rayDistance, LayerMask nebulaMask)
    {
        float retVal = 0.0f;

        Ray cast = new Ray(from, neb.transform.position - from);
        RaycastHit hitInfo;

        bool rayHit = Physics.Raycast(cast, out hitInfo, rayDistance, nebulaMask);
        if (rayHit)
            return Vector3.Distance(hitInfo.point, from);

        return retVal;
    }
}


