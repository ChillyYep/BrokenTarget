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
        private List<int> GetInsertionTriangleFromSphere()
        {
            List<int> triangle = new List<int>();

            return triangle;
        }
        /// <summary>
        /// 保持静态的物体
        /// </summary>
        /// <param name="target"></param>
        public virtual void Traversal(IBreakable breakable)
        {

            Vector3[] sphereVertices = sphereMesh.vertices;
            int[] sphereTriangle = sphereMesh.triangles;
            TriangleFace[] triangleFace = new TriangleFace[]{
                new TriangleFace(),
                new TriangleFace(),
                new TriangleFace()
            };
            List<ModelInfo> newModelInfoList = new List<ModelInfo>();
            ModelInfo addModelInfo = new ModelInfo();
            bool beginGenNewTriangle = false;//对每一个三角形而言，开始生成新的三角形
            CreateNewModelInfo(ref addModelInfo);
            foreach (var target in breakable.ModelInfos)
            {
                var newModelInfo = new ModelInfo();
                CreateNewModelInfo(ref newModelInfo);
                for (int i = 0; i < target.triangles.Count; i += 3)
                {
                    beginGenNewTriangle = false;
                    Reset();
                    Vector3[] targetPoint = new Vector3[]
                    {
                        target.vertices[target.triangles[i]],
                        target.vertices[target.triangles[i + 1]],
                        target.vertices[target.triangles[i + 2]]
                    };
                    int[] indices = new int[]
                    {
                        strategy.GetTriangleIndex(targetPoint[0], center),
                        strategy.GetTriangleIndex(targetPoint[1], center),
                        strategy.GetTriangleIndex(targetPoint[2], center)
                    };
                    for (int j = 0; j < 3; ++j)
                    {
                        int triangleIndex = indices[j] * 3;
                        triangleFace[j].Compute(sphereVertices[sphereTriangle[triangleIndex]] + center, sphereVertices[sphereTriangle[triangleIndex + 1]] + center, sphereVertices[sphereTriangle[triangleIndex + 2]] + center);
                    }
                    int count = 0;
                    for (int j = 0; j < 3; ++j)
                    {
                        Vector3 crossPoint;
                        if (!triangleFace[j].ComputeCrossPoint(new Line() { p1 = center, p2 = targetPoint[j] }, out crossPoint))
                        {
                            continue;
                        }
                        float magnitude1 = (targetPoint[j] - center).magnitude;
                        float magnitude2 = (crossPoint - center).magnitude;
                        if (magnitude1 > magnitude2 || Mathf.Approximately(magnitude1, magnitude2))
                        {
                            count++;
                            triangleVertexInSphere[j] = false;
                        }
                    }
                    switch (count)
                    {
                        case 0:
                            break;
                        case 1:
                            break;
                        case 2:
                            ////两个点在球外
                            //TriangleFace face = new TriangleFace();
                            //if(!triangleVertexInSphere[0])
                            //{
                            //    addModelInfo.vertices.Add(targetPoint[2]);
                            //    addModelInfo.vertices.Add(targetPoint[1]);
                            //}
                            //else if(!triangleVertexInSphere[1])
                            //{
                            //    addModelInfo.vertices.Add(targetPoint[0]);
                            //    addModelInfo.vertices.Add(targetPoint[2]);
                            //}
                            //else
                            //{
                            //    addModelInfo.vertices.Add(targetPoint[1]);
                            //    addModelInfo.vertices.Add(targetPoint[0]);
                            //}
                            //for (int j = 0; j < 3; ++j)
                            //{
                            //    if (!triangleVertexInSphere[j])
                            //    {
                            //        addModelInfo.vertices.Add(targetPoint[j]);
                            //    }
                            //}
                            //Line tempLine = new Line();
                            //face.Compute(targetPoint[0], targetPoint[1], targetPoint[2]);
                            //for (int j = 0; j < sphereMesh.triangles.Length; j += 3)
                            //{
                            //    Vector3 crossPoint;
                            //    for (int x = 0; x < 3; ++x)
                            //    {
                            //        tempLine.p1 = sphereMesh.vertices[sphereMesh.triangles[j + x]] + center;
                            //        tempLine.p2 = sphereMesh.vertices[sphereMesh.triangles[j + (x + 1) % 3]] + center;
                            //        if (face.ComputeCrossPoint(tempLine, out crossPoint, true))
                            //        {
                            //            addModelInfo.vertices.Add(crossPoint);
                            //            if (!beginGenNewTriangle)
                            //            {
                            //                addModelInfo.triangles.Add(0);
                            //                addModelInfo.triangles.Add(addModelInfo.vertices.Count - 1);
                            //                addModelInfo.triangles.Add(1);
                            //                beginGenNewTriangle = true;
                            //            }
                            //            else
                            //            {
                            //                addModelInfo.triangles.Add(addModelInfo.vertices.Count - 1);
                            //                addModelInfo.triangles.Add(1);
                            //                addModelInfo.triangles.Add(addModelInfo.vertices.Count - 2);
                            //            }
                            //        }
                            //    }
                            //}
                            break;
                        case 3:
                            //newModelInfo.triangles.Add(target.triangles[i]);
                            //newModelInfo.triangles.Add(target.triangles[i + 1]);
                            //newModelInfo.triangles.Add(target.triangles[i + 2]);
                            break;
                    }
                    TriangleFace face = new TriangleFace();
                    face.Compute(targetPoint[0], targetPoint[1], targetPoint[2]);
                    if(face.IsSphereOutTriangle(center,radius))
                    {
                        newModelInfo.triangles.Add(target.triangles[i]);
                        newModelInfo.triangles.Add(target.triangles[i + 1]);
                        newModelInfo.triangles.Add(target.triangles[i + 2]);
                    }
                }
                newModelInfo.vertices.AddRange(target.vertices);
                newModelInfo.normals.AddRange(target.normals);
                newModelInfo.uvs.AddRange(target.uvs);
                newModelInfoList.Add(newModelInfo);
            }
            //newModelInfoList.Add(addModelInfo);
            breakable.ModelInfos.Clear();
            foreach (var modelInfo in newModelInfoList)
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
                triangleVertexInSphere[i] = true;
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
