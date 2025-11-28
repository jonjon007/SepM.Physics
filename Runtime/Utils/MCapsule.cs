using UnityEngine;
using Unity.Mathematics.FixedPoint;

namespace SepM.Physics
{
    public class MCapsule : MonoBehaviour
    {
        [Header("Set in Inspector")]
        [SerializeField]
        private Transform topSphere;
        [SerializeField]
        private Transform cylinder;
        [SerializeField]
        private Transform bottomSphere;

        /// <summary>
        /// Creates a procedurally generated MCapsule GameObject with proper hierarchy.
        /// This replaces the need for a prefab in the Resources folder.
        /// </summary>
        /// <returns>A GameObject with MCapsule component and child primitives</returns>
        public static GameObject CreateMCapsule()
        {
            // Create root GameObject with MCapsule component
            GameObject root = new GameObject("MCapsule");
            MCapsule mCapsule = root.AddComponent<MCapsule>();

            // Create top sphere
            GameObject topSphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topSphereObj.name = "TopSphere";
            topSphereObj.transform.SetParent(root.transform);
            topSphereObj.transform.localPosition = Vector3.zero;
            topSphereObj.transform.localRotation = Quaternion.identity;
            topSphereObj.transform.localScale = Vector3.one;
            
            // Remove Unity's default collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(topSphereObj.GetComponent<UnityEngine.SphereCollider>());
            else
                GameObject.Destroy(topSphereObj.GetComponent<UnityEngine.SphereCollider>());

            // Create cylinder
            GameObject cylinderObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinderObj.name = "Cylinder";
            cylinderObj.transform.SetParent(root.transform);
            cylinderObj.transform.localPosition = Vector3.zero;
            cylinderObj.transform.localRotation = Quaternion.identity;
            cylinderObj.transform.localScale = Vector3.one;
            
            // Remove Unity's default collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(cylinderObj.GetComponent<UnityEngine.CapsuleCollider>());
            else
                GameObject.Destroy(cylinderObj.GetComponent<UnityEngine.CapsuleCollider>());

            // Create bottom sphere
            GameObject bottomSphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bottomSphereObj.name = "BottomSphere";
            bottomSphereObj.transform.SetParent(root.transform);
            bottomSphereObj.transform.localPosition = Vector3.zero;
            bottomSphereObj.transform.localRotation = Quaternion.identity;
            bottomSphereObj.transform.localScale = Vector3.one;
            
            // Remove Unity's default collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(bottomSphereObj.GetComponent<UnityEngine.SphereCollider>());
            else
                GameObject.Destroy(bottomSphereObj.GetComponent<UnityEngine.SphereCollider>());

            // Assign references to MCapsule component
            mCapsule.topSphere = topSphereObj.transform;
            mCapsule.cylinder = cylinderObj.transform;
            mCapsule.bottomSphere = bottomSphereObj.transform;

            return root;
        }

        public void SetDimensions(fp height, fp radius)
        {
            // Convert fp to float for Unity transforms
            float h = (float)height;
            float r = (float)radius;
            
            // The cylinder spans the full height with spheres centered at top and bottom
            // Sphere centers are at +/- height/2, coinciding with cylinder ends
            
            // Position top sphere at the top (center at height/2)
            topSphere.localPosition = new Vector3(0f, h / 2f, 0f);
            topSphere.localScale = new Vector3(r * 2f, r * 2f, r * 2f); // Unity sphere primitive has diameter 1
            
            // Position cylinder at the center, spanning full height
            cylinder.localPosition = Vector3.zero;
            // Unity cylinder primitive: diameter=1, height=2 (from -1 to +1)
            // Scale: x and z for radius, y for half-height (cylinder extends from -y to +y)
            cylinder.localScale = new Vector3(r * 2f, h / 2f, r * 2f);
            
            // Position bottom sphere at the bottom (center at -height/2)
            bottomSphere.localPosition = new Vector3(0f, -h / 2f, 0f);
            bottomSphere.localScale = new Vector3(r * 2f, r * 2f, r * 2f);
        }
    }
}