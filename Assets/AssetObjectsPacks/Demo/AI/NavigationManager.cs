using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;

namespace Game.AI {
    public class NavigationManager : MonoBehaviour
    {
        static float totalNavArea;
        static int[] navTriangles;
        static Vector3[] navVertices;
        static HashSet<LineSegment> navBorders;
        void Awake()
        {
            var nav = NavMesh.CalculateTriangulation();
            navVertices = nav.vertices;
            navTriangles = nav.indices;
            totalNavArea = 0.0f;
            for (int i = 0; i < navTriangles.Length / 3; i++) totalNavArea += GetTriangleArea(navVertices, navTriangles, i);
            navBorders = FindNavMeshBorders();
        }
        void OnDrawGizmos () {
            OffMeshLink[] allManualLinks = GameObject.FindObjectsOfType<OffMeshLink>();
            int l = allManualLinks.Length;
            Color32 oColor = new Color32 (255, 125, 0, 255);
            for (int i = 0; i < l; i++) {
                Vector3 a = allManualLinks[i].startTransform.position;
                Vector3 b = allManualLinks[i].endTransform.position;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(a, .25f);
                Gizmos.color = oColor;
                Gizmos.DrawLine(a, b);
                Gizmos.DrawWireSphere(b, .25f);
            }
        }


        //Get a random triangle on the NavMesh
        public static Vector3 GetRandomPointOnNavMesh() {
            int triangle = GetRandomTriangleOnNavMesh(navVertices, navTriangles, totalNavArea);
            return GetRandomPointOnTriangle(navVertices, navTriangles, triangle);
        }
        //Get a random triangle on the NavMesh that has connectivity to a starting point
        public static Vector3 GetConnectedPointOnNavMesh(Vector3 startingPoint) {
            int triangle = GetRandomConnectedTriangleOnNavMesh(navVertices, navTriangles, startingPoint);
            return GetRandomPointOnTriangle(navVertices, navTriangles, triangle);
        }

        //Grabs a random triangle in the mesh, weighted by size so random point distribution is even  
        static int GetRandomTriangleOnNavMesh(Vector3[] vertices, int[] triangles, float totalArea) {
            float rnd = UnityEngine.Random.Range(0, totalArea);
            int nTriangles = triangles.Length / 3;
            for (int i = 0; i < nTriangles; i++) {
                rnd -= GetTriangleArea(vertices, triangles, i);
                if (rnd <= 0) return i;
            }
            return 0;
        }

        /*
            Grabs a random triangle in the mesh (connected to p), 
            weighted by size so random point distribution is even
        */
        static int GetRandomConnectedTriangleOnNavMesh(Vector3[] vertices, int[] triangles, Vector3 p)
        {
            // Check for triangle connectivity and calculate total area of all *connected* triangles
            int nTriangles = triangles.Length / 3;
            float tArea = 0.0f;
            List<int> connectedTriangles = new List<int>();
            NavMeshPath path = new NavMeshPath();
            for (int i = 0; i < nTriangles; i++) {
                path.ClearCorners();
                if (NavMesh.CalculatePath(p, vertices[triangles[3 * i + 0]], NavMesh.AllAreas, path)) {
                    if (path.status == NavMeshPathStatus.PathComplete) {
                        tArea += GetTriangleArea(vertices, triangles, i);
                        connectedTriangles.Add(i);
                    }
                }
            }

            float rnd = UnityEngine.Random.Range(0, tArea);
            foreach (int i in connectedTriangles) {
                rnd -= GetTriangleArea(vertices, triangles, i);
                if (rnd <= 0) return i;
            }
            return 0;
        }


        //Gets a random point on a triangle.
        static Vector3 GetRandomPointOnTriangle(Vector3[] vertices, int[] triangles, int idx)
        {
            Vector3[] v = new Vector3[3];

            for (int i = 0; i < 3; i++) 
                v[i] = vertices[triangles[3 * idx + i]];
            
            Vector3 a = v[1] - v[0];
            Vector3 b = v[2] - v[1];
            Vector3 c = v[2] - v[0];

            // Generate a random point in the trapezoid
            Vector3 result = v[0] + UnityEngine.Random.Range(0f, 1f) * a + UnityEngine.Random.Range(0f, 1f) * b;

            // Barycentric coordinates on triangles
            float alpha = ((v[1].z - v[2].z) * (result.x - v[2].x) + (v[2].x - v[1].x) * (result.z - v[2].z)) / ((v[1].z - v[2].z) * (v[0].x - v[2].x) + (v[2].x - v[1].x) * (v[0].z - v[2].z));
            float beta = ((v[2].z - v[0].z) * (result.x - v[2].x) + (v[0].x - v[2].x) * (result.z - v[2].z)) / ((v[1].z - v[2].z) * (v[0].x - v[2].x) + (v[2].x - v[1].x) * (v[0].z - v[2].z));
            float gamma = 1.0f - alpha - beta;

            // The selected point is outside of the triangle (wrong side of the trapezoid), project it inside through the center.
            if (alpha < 0 || beta < 0 || gamma < 0) {
                Vector3 center = v[0] + c / 2;
                center = center - result;
                result += 2 * center;
            }
            return result;
        }

        /*
            calculate the area of a triangle.
            Used as weights when selecting a random triangle so bigger triangles have a higher chance 
            (yielding an even distribution of points on the entire mesh)
        */
        static float GetTriangleArea(Vector3[] vertices, int[] triangles, int idx) {
            Vector3[] v = new Vector3[3];
            for (int i = 0; i < 3; i++) v[i] = vertices[triangles[3 * idx + i]];
            Vector3 a = v[1] - v[0];
            Vector3 b = v[2] - v[1];
            Vector3 c = v[2] - v[0];
            float ma = a.magnitude;
            float mb = b.magnitude;
            float mc = c.magnitude;
            float area = 0f;
            float S = (ma + mb + mc) / 2;
            area = Mathf.Sqrt(S * (S - ma) * (S - mb) * (S - mc));
            return area;
        }


        
        public static HashSet<LineSegment> FindNavMeshBorders( )
        {
            var mesh = NavMesh.CalculateTriangulation();
            
            Vector3[] verts = null;
            int[] triangles = null;
            weldVertices( mesh, 0.01f, 2f, out verts, out triangles );
            
            var map = new Dictionary<uint, int>();
            var reversed = new Dictionary<uint, bool>();
            Action<ushort, ushort> processEdge = ( a, b ) => {
                var swap = ( a > b );
                if ( swap ){
                    var temp = b;
                    b = a;
                    a = temp;
                }
                uint key = ( (uint)a << 16 ) | (uint)b;
                if( swap ) reversed[ key ] = true;
                if( !map.ContainsKey( key ) )
                    map[ key ] = 1;
                else
                    map[ key ] += 1;
            };
            for( int i = 0; i < triangles.Length; i += 3 ) {
                var a = (ushort)triangles[ i + 0 ];
                var b = (ushort)triangles[ i + 1 ];
                var c = (ushort)triangles[ i + 2 ];
                processEdge( a, b );
                processEdge( b, c );
                processEdge( c, a );
            }

            var borderEdges = new HashSet<LineSegment>();
            foreach( var key in map.Keys ) {
                var count = map[ key ];
                if( count != 1 ) continue;
                var a = ( key >> 16 );
                var b = ( key & 0xFFFF );
                LineSegment line = LineSegment.empty;
                var isReversed = false;
                if( reversed.TryGetValue( key, out isReversed ) && isReversed )
                    line = new LineSegment( verts[ b ], verts[ a ]);//, maxSize );
                else
                    line = new LineSegment( verts[ a ], verts[ b ]);//, maxSize );
                borderEdges.Add( line );
            }
            return borderEdges;
        }
    
        static void weldVertices( NavMeshTriangulation mesh, float threshold, float bucketStep, out Vector3[] vertices, out int[] indices )
        {

            // This code was adapted from http://answers.unity3d.com/questions/228841/dynamically-combine-verticies-that-share-the-same.html

            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[ oldVertices.Length ];
            int[] old2new = new int[ oldVertices.Length ];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            Vector3 max = new Vector3( float.MinValue, float.MinValue, float.MinValue );
            for( int i = 0; i < oldVertices.Length; i++ )
            {
                if( oldVertices[ i ].x < min.x ) min.x = oldVertices[ i ].x;
                if( oldVertices[ i ].y < min.y ) min.y = oldVertices[ i ].y;
                if( oldVertices[ i ].z < min.z ) min.z = oldVertices[ i ].z;
                if( oldVertices[ i ].x > max.x ) max.x = oldVertices[ i ].x;
                if( oldVertices[ i ].y > max.y ) max.y = oldVertices[ i ].y;
                if( oldVertices[ i ].z > max.z ) max.z = oldVertices[ i ].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt( ( max.x - min.x ) / bucketStep ) + 1;
            int bucketSizeY = Mathf.FloorToInt( ( max.y - min.y ) / bucketStep ) + 1;
            int bucketSizeZ = Mathf.FloorToInt( ( max.z - min.z ) / bucketStep ) + 1;
            List<int>[ , , ] buckets = new List<int>[ bucketSizeX, bucketSizeY, bucketSizeZ ];

            // Make new vertices
            for( int i = 0; i < oldVertices.Length; i++ )
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt( ( oldVertices[ i ].x - min.x ) / bucketStep );
                int y = Mathf.FloorToInt( ( oldVertices[ i ].y - min.y ) / bucketStep );
                int z = Mathf.FloorToInt( ( oldVertices[ i ].z - min.z ) / bucketStep );

                // Check to see if it's already been added
                if( buckets[ x, y, z ] == null ) buckets[ x, y, z ] = new List<int>(); // Make buckets lazily

                for( int j = 0; j < buckets[ x, y, z ].Count; j++ ) {
                    Vector3 to = newVertices[ buckets[ x, y, z ][ j ] ] - oldVertices[ i ];
                    if( Vector3.SqrMagnitude( to ) < threshold ) {
                        old2new[ i ] = buckets[ x, y, z ][ j ];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[ newSize ] = oldVertices[ i ];
                buckets[ x, y, z ].Add( newSize );
                old2new[ i ] = newSize;
                newSize++;

                skip:
                    ;
            }
            // Make new triangles
            int[] oldTris = mesh.indices;
            indices = new int[ oldTris.Length ];
            for( int i = 0; i < oldTris.Length; i++ ) indices[ i ] = old2new[ oldTris[ i ] ];
            vertices = new Vector3[ newSize ];
            for( int i = 0; i < newSize; i++ ) vertices[ i ] = newVertices[ i ];
        }        
        
    }

}
