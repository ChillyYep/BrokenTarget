using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    public class DetectionSphere : IVistor, IConvertable
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
        private Transform _transfrom;
        public Transform transform {
            get
            {
                return _transfrom;
            }
            set
            {
                _transfrom = value;
                World2Object = transform.worldToLocalMatrix;
                Object2World = transform.localToWorldMatrix;
            }
        }
        public Matrix4x4 World2Object { get; private set; }
        public Matrix4x4 Object2World { get; private set; }
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
            TriangleFace breakableFace = new TriangleFace();
            List<ModelInfo> newModelInfoList = new List<ModelInfo>();
            ModelInfo addModelInfo = new ModelInfo();
            CreateNewModelInfo(ref addModelInfo);
            foreach (var target in breakable.ModelInfos)
            {
                var newModelInfo = new ModelInfo();
                CreateNewModelInfo(ref newModelInfo);
                for (int i = 0; i < target.triangles.Count; i += 3)
                {
                    Vector3[] targetPoint = new Vector3[]
                    {
                        breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i]]),
                        breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i + 1]]),
                        breakable.Object2World.MultiplyPoint(target.vertices[target.triangles[i + 2]])
                    };
                    breakableFace.Compute(targetPoint[0], targetPoint[1], targetPoint[2]);
                    //用球半径粗略计算哪些三角形要加入三角形切割，其中可能会有不被切割的三角形
                    TriangleOutInfo triangleOutInfo;
                    if (breakableFace.IsSphereOutTriangle(center, radius, out triangleOutInfo, new DefaultCutTriangleStrategy(segmentCircleType, breakable.transform)))
                    {
                        newModelInfo.triangles.Add(target.triangles[i]);
                        newModelInfo.triangles.Add(target.triangles[i + 1]);
                        newModelInfo.triangles.Add(target.triangles[i + 2]);
                    }
                    else
                    {
                        int tempCount = addModelInfo.vertices.Count;
                        for (int x = 0; x < triangleOutInfo.triangles.Count; ++x)
                        {
                            addModelInfo.triangles.Add(triangleOutInfo.triangles[x] + tempCount);
                        }
                        addModelInfo.vertices.AddRange(triangleOutInfo.vertexes);
                        addModelInfo.normals.AddRange(triangleOutInfo.normals);
                        addModelInfo.uvs.AddRange(triangleOutInfo.uvs);
                    }
                }
                newModelInfo.vertices.AddRange(target.vertices);
                newModelInfo.normals.AddRange(target.normals);
                newModelInfo.uvs.AddRange(target.uvs);
                newModelInfoList.Add(newModelInfo);
            }
            newModelInfoList.Add(addModelInfo);
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
