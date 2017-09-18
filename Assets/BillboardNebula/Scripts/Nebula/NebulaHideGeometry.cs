//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class NebulaHideGeometry : MonoBehaviour
//{
//    Nebula[] nebulas;
//    public Nebula insideNebula;

//    Camera mainCam;
//    NebulaCameraFog mainFog;

//    MeshRenderer[] mesh;

//    const float overrideHide = 3000f;

//    void Awake()
//    {
//        mesh = GetComponentsInChildren<MeshRenderer>();
//        mainCam = Camera.main;
//        mainFog = mainCam.GetComponent<NebulaCameraFog>();
//    }

//    void Start()
//    {
//        // Get a list of all the nebula.
//        GameObject[] obj = GameObject.FindGameObjectsWithTag("Nebula");
//        nebulas = new Nebula[obj.Length];
//        for (int i = 0; i < nebulas.Length; i++)
//            nebulas[i] = obj[i].GetComponent<Nebula>();
//    }

//    void Update()
//    {
//        if (mesh != null && mainFog != null)
//        {
//            // First check distance to the camera to avoid things flickering right in front
//            // of the player.
//            float distToCam = Vector3.Distance(transform.position, mainCam.transform.position);
            
//            // If they're far away, check to see if they should be obscured or not by a nebula.
//            if (distToCam < overrideHide)
//                ShowAllMeshes(true);
//            else
//                CalculateInsideNebula();
//        }
//    }

//    void CalculateInsideNebula()
//    {
//        if (nebulas.Length > 0)
//        {
//            // Find the closest nebula.
//            Nebula closestNeb = nebulas[0];
//            float closestDistance = float.MaxValue;
//            foreach (Nebula neb in nebulas)
//            {
//                float dist = Vector3.Distance(transform.position, neb.transform.position);
//                if (dist < closestDistance)
//                {
//                    closestDistance = dist;
//                    closestNeb = neb;
//                }
//            }

//            float minRadius = Mathf.Min(closestNeb.dimensions.x, closestNeb.dimensions.y, closestNeb.dimensions.z) / 2f;
//            float maxRadius = Mathf.Max(closestNeb.dimensions.x, closestNeb.dimensions.y, closestNeb.dimensions.z) / 2f;

//            // Only do the raycast if we can't be 100% positive that we are inside or outside the closest nebula.
//            if (closestDistance > minRadius && closestDistance < maxRadius)
//            {
//                float distToNebSurface = NebulaUtils.DistanceToNebulaSurface(transform.position, closestNeb, mainCam.farClipPlane);
//                if (distToNebSurface == 0f)
//                    insideNebula = closestNeb;
//                else
//                    insideNebula = null;
//            }

//            // If it's known that this is inside the minimum radius, then it must be inside the nebula.
//            else if (closestDistance < minRadius)
//            {
//                insideNebula = closestNeb;
//            }
//            else
//                insideNebula = null;

//            // If the camera and mesh are not in the same nebula then don't render it.
//            if (mainFog.insideNebula != insideNebula)
//                ShowAllMeshes(false);
//            else
//                ShowAllMeshes(true);
//        }
//    }

//    void ShowAllMeshes(bool hide)
//    {
//        foreach (MeshRenderer m in mesh)
//            m.enabled = hide;
//    }
//}