using GoMap;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GlobalMap
{
    public static class MapHelpers
    {
        private const string WaterName = "water";
        private const string BuildingName = "building";

        public static bool IsWater(Vector3 position)
        {
            return CheckForExceptionObjectsOnPosition(position, WaterName);
        }

        public static bool IsBuilding(Vector3 position)
        {
            return CheckForExceptionObjectsOnPosition(position, BuildingName);
        }
    
        public static bool IsLoot(Vector3 position)
        {
            return CheckForExceptionObjectsOnPosition<MapResource>(position);
        }

        public static Vector3 GetRandomWorldPointFromGoTile3D(GOTile goTile)
        {
            var tileMeshFilter = goTile.GetComponent<MeshFilter>();

            if (tileMeshFilter == null)
            {
                Debug.LogError("Go tile not contains meshFilter!");
                return Vector3.zero;
            }

            var vertices = tileMeshFilter.mesh.vertices;
            var localToWorld = goTile.transform.localToWorldMatrix;

            var randomVertice = vertices[Random.Range(0, vertices.Length)];
            var worldPos = localToWorld.MultiplyPoint3x4(randomVertice);

            return worldPos;
        }

        public static Vector3 GetRandomWorldPointFromGoTile2D(GOTile goTile)
        {
            var tileMeshFilter = goTile?.GetComponent<MeshFilter>();

            if (tileMeshFilter == null)
            {
                Debug.LogError("Go tile not contains meshFilter!");
                return Vector3.zero;
            }

            var boundPairMin = tileMeshFilter.mesh.bounds.min;
            var boundPairMax = tileMeshFilter.mesh.bounds.max;

            var randomX = Random.Range(boundPairMin.x, boundPairMax.x);
            var randomZ = Random.Range(boundPairMin.z, boundPairMax.z);

            var randomVector = new Vector3(randomX, 0, randomZ);

            var localToWorld = goTile.transform.localToWorldMatrix;
            var worldPos = localToWorld.MultiplyPoint3x4(randomVector);

            return worldPos;
        }

        private static bool CheckForExceptionObjectsOnPosition(Vector3 position, string objectName)
        {
            var radius = 10f;
            var results = new Collider[300];
            var collisionsCount = Physics.OverlapSphereNonAlloc(position, radius, results);

            for (var i = 0; i < collisionsCount; i++)
            {
                var collider = results[i];

                if (collider.transform.name.Equals(objectName) || collider.transform.parent.name.Equals(objectName))
                    return true;
            }

            return false;
        }

        private static bool CheckForExceptionObjectsOnPosition<T>(Vector3 position)
        {
            var radius = 15f;
            var results = new Collider[300];
            var collisionsCount = Physics.OverlapSphereNonAlloc(position, radius, results);

            for (var i = 0; i < collisionsCount; i++)
            {
                var collider = results[i];
                var component = collider.transform.gameObject.GetComponent<T>();

                if (component != null)
                    return true;
            }

            return false;
        }
    }
}