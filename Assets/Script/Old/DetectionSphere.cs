using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    public class DetectionSphere : IVistor
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
        public SegmentVerticalType segmentVerticalType { get; set; }
        public SegmentCircleType segmentCircleType { get; set; }
        private Mesh sphereMesh;
        private GenDetectionObjectStrategy strategy;
        public Mesh SphereMesh
        {
            get
            {
                if (sphereMesh == null)
                {
                    DrawSphere();
                }
                return sphereMesh;
            }
        }

        public DetectionSphere(Vector3 center, float radius = 1f, SegmentVerticalType segmentVerticalType = SegmentVerticalType.Ten,
            SegmentCircleType segmentCircleType = SegmentCircleType.Eighteen)
        {
            this.segmentVerticalType = segmentVerticalType;
            this.segmentCircleType = segmentCircleType;
            this.center = center;
            this.radius = radius;
        }
        public bool IsInSphere(Vector3 targetPoint)
        {
            return Vector3.Distance(targetPoint, center) <= radius;
        }
        public void DrawSphere()
        {
            if (strategy == null)
            {
                strategy = new DefaultGenSphereStrategy(segmentVerticalType, segmentCircleType, radius);
            }
            sphereMesh = new Mesh();
            strategy.GenSphereData();
            strategy.GenMesh(ref sphereMesh);
        }
        /// <summary>
        /// 保持静态的物体
        /// </summary>
        /// <param name="target"></param>
        public virtual void Traversal(IBreakable breakable)
        {

            Vector3[] sphereVertices = sphereMesh.vertices;
            TriangleFace triangleFace = new TriangleFace();
            List<ModelInfo> newModelInfoList=new List<ModelInfo>();
            ModelInfo addModelInfo = new ModelInfo();
            foreach (var target in breakable.ModelInfos)
            {
                var newModelInfo = new ModelInfo();
                newModelInfoList.Add(newModelInfo);
                CreateNewModelInfo(ref newModelInfo);
                for (int i = 0; i < target.triangles.Count; i += 3)
                {
                    Reset();
                    Vector3[] targetPoint = new Vector3[]
                    {
                        target.vertices[target.triangles[i]] - center,
                        target.vertices[target.triangles[i + 1]] - center,
                        target.vertices[target.triangles[i + 2]] - center
                    };
                    int[] indices = new int[]
                    {
                        strategy.GetTriangleIndex(targetPoint[0], center),
                        strategy.GetTriangleIndex(targetPoint[1], center),
                        strategy.GetTriangleIndex(targetPoint[2], center)
                    };
                    triangleFace.Compute(sphereVertices[indices[0]], sphereVertices[indices[1]], sphereVertices[indices[2]]);
                    int count = 0;
                    for (int j = 0; j < 3; ++j)
                    {
                        Vector3 crossPoint;
                        triangleFace.ComputeCrossPoint(new Line()
                        {
                            p1 = center,
                            p2 = targetPoint[j]
                        }, out crossPoint);
                        float magnitude1 = (targetPoint[j] - center).magnitude;
                        float magnitude2 = (crossPoint - center).magnitude;
                        if (magnitude1 >= magnitude2 || Mathf.Approximately(magnitude1, magnitude2))
                        {
                            count++;
                        }
                    }
                    if (count == 3)
                    {
                        newModelInfo.triangles.Add(target.triangles[i]);
                        newModelInfo.triangles.Add(target.triangles[i + 1]);
                        newModelInfo.triangles.Add(target.triangles[i + 2]);
                    }
                    else
                    {
                        //三角面切割

                    }
                }
                newModelInfo.vertices.AddRange(target.vertices);
                newModelInfo.normals.AddRange(target.normals);
                newModelInfo.uvs.AddRange(target.uvs);
            }
            breakable.ModelInfos.Clear();
            foreach(var modelInfo in newModelInfoList)
            {
                breakable.ModelInfos.Add(modelInfo);
            }
            breakable.CreateNew();

        }
        private bool[] triangleVertexInSphere = new bool[3];

        private void Reset()
        {
            for (int i = 0; i < triangleVertexInSphere.Length; ++i)
            {
                triangleVertexInSphere[i] = false;
            }
        }
        private void CreateNewModelInfo(ref ModelInfo modelInfo)
        {
            modelInfo.vertices = new List<Vector3>();
            modelInfo.triangles = new List<int>();
            modelInfo.uvs = new List<Vector2>();
            modelInfo.normals = new List<Vector3>();
        }
    }
}
