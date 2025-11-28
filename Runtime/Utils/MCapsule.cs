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
            // Total height = cylinder height + 2 * radius (for the two hemisphere caps)
            // Therefore: cylinder height = total height - 2 * radius
            fp cylinderHeight = height - radius * 2;
            
            cylinder.localScale = new(cylinder.localScale.x * (float)radius*2f, (float)height/2, cylinder.localScale.z * (float)radius*2f);
            topSphere.localScale = new((float)radius*2f, (float)radius*2f, (float)radius*2f);
            bottomSphere.localScale = new((float)radius*2f, (float)radius*2f, (float)radius*2f);

            // Position spheres at the ends of the cylinder
            fp halfCylinderHeight = cylinderHeight / 2;
            topSphere.localPosition = new(0, (float)halfCylinderHeight, 0);
            bottomSphere.localPosition = new(0, -(float)halfCylinderHeight, 0);
        }
    }
}