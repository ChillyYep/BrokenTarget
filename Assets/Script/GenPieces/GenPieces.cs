using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BrokenSys
{
    public enum ShapeType
    {
        Cube,
        Sphere
    }
    public enum CutToPiecesPerTime
    {
        CutToTwo = 0x01,
        CutToThree = 0x02,
        CutToFour = 0x04,
        TwoAndThree = CutToTwo | CutToThree,
        TwoAndFour = CutToTwo | CutToFour,
        ThreeAndFour = CutToThree | CutToFour,
        All = CutToTwo | CutToThree | CutToFour
    }
    /// <summary>
    /// 生成棱台式的碎块
    /// </summary>
    public class GenFrustumPieces : IGenPieces
    {
        private float depth;
        //偏向
        private float v2cMaxFactor = 0.5f;
        public GenFrustumPieces(float depth)
        {
            this.depth = depth;
        }
        public GenPiecesType GetEnumType()
        {
            return GenPiecesType.GenFrustumPieces;
        }
        public List<ModelData> GenModelData(TriangleFace triangleFace)
        {
            List<ModelData> modelDatas = new List<ModelData>();
            ModelData modelInfo = new ModelData();
            modelDatas.Add(modelInfo);
            modelInfo.AllocateStorage();
            Vector3 center = ((triangleFace.pointA.vertex + triangleFace.pointB.vertex) / 2f + triangleFace.pointC.vertex) / 2f;
            Vector3 modelCenter = center - triangleFace.normal * depth / 2f;
            Vector3 a2c = center - triangleFace.pointA.vertex;
            Vector3 b2c = center - triangleFace.pointB.vertex;
            Vector3 c2c = center - triangleFace.pointC.vertex;
            Vector3 a1 = triangleFace.pointA.vertex + a2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
            Vector3 b1 = triangleFace.pointB.vertex + b2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
            Vector3 c1 = triangleFace.pointC.vertex + c2c * v2cMaxFactor * Random.value - triangleFace.normal * depth;
            modelInfo.vertices.Add(triangleFace.pointA.vertex);
            modelInfo.normals.Add((triangleFace.pointA.vertex - modelCenter).normalized);
            modelInfo.vertices.Add(triangleFace.pointB.vertex);
            modelInfo.normals.Add((triangleFace.pointB.vertex - modelCenter).normalized);
            modelInfo.vertices.Add(triangleFace.pointC.vertex);
            modelInfo.normals.Add((triangleFace.pointC.vertex - modelCenter).normalized);
            modelInfo.vertices.Add(a1);
            modelInfo.normals.Add((a1 - modelCenter).normalized);
            modelInfo.vertices.Add(b1);
            modelInfo.normals.Add((b1 - modelCenter).normalized);
            modelInfo.vertices.Add(c1);
            modelInfo.normals.Add((c1 - modelCenter).normalized);
            modelInfo.triangles.Add(0);
            modelInfo.triangles.Add(1);
            modelInfo.triangles.Add(2);
            for (int i = 0; i < 3; ++i)
            {
                modelInfo.triangles.Add(i);
                modelInfo.triangles.Add(i + 3);
                modelInfo.triangles.Add((i + 1) % 3);
                modelInfo.triangles.Add(i + 3);
                modelInfo.triangles.Add((i + 1) % 3 + 3);
                modelInfo.triangles.Add((i + 1) % 3);
            }
            modelInfo.triangles.Add(4);
            modelInfo.triangles.Add(3);
            modelInfo.triangles.Add(5);
            return modelDatas;
        }
    }
    /// <summary>
    /// 生成棱锥式的碎块
    /// </summary>
    public class GenPyramidPieces : IGenPieces
    {
        private float depth;
        private Vector2 beginUV;
        private Vector2 endUV;
        private ShapeType shapeType;
        private bool isNewVertexRandom;
        public GenPyramidPieces(float depth, Vector2 beginUV, Vector2 endUV, bool isNewVertexRandom, ShapeType shapeType = ShapeType.Cube)
        {
            this.depth = depth;
            this.shapeType = shapeType;
            this.beginUV = beginUV;
            this.endUV = endUV;
            this.isNewVertexRandom = isNewVertexRandom;
        }
        public GenPiecesType GetEnumType()
        {
            return GenPiecesType.GenPyramidPieces;
        }
        private Vector2 MapUVCube(Vector3 vertex, ref TriangleFace triangleFace)
        {
            Vector3 mapVertex = triangleFace.Map2ThisFace(vertex);
            return triangleFace.Interlopation(mapVertex, false, true).uv;
        }
        private Vector2 MapUV(Vector3 vertex, ref TriangleFace triangleFace)
        {
            Vector2 sourceUV;
            Vector2 uvDelta = endUV - beginUV;
            switch (shapeType)
            {
                case ShapeType.Cube:
                    sourceUV = MapUVCube(vertex, ref triangleFace);
                    break;
                default:
                    sourceUV = Vector2.zero;
                    break;
            }
            var destUv = new Vector2(sourceUV.x * uvDelta.x, sourceUV.y * uvDelta.y);
            destUv += beginUV;
            return destUv;
        }
        public List<ModelData> GenModelData(TriangleFace triangleFace)
        {
            List<ModelData> modelDatas = new List<ModelData>();
            ModelData modelInfo = new ModelData();
            modelDatas.Add(modelInfo);
            modelInfo.AllocateStorage();
            Vector3 newPoint;
            if (isNewVertexRandom)
            {
                float r1 = Random.value;
                float r2 = Random.value;
                newPoint = r2 * (r1 * triangleFace.pointA.vertex + (1 - r1) * triangleFace.pointB.vertex) + (1 - r2) * triangleFace.pointC.vertex - triangleFace.normal * depth;
            }
            else
            {
                newPoint = 0.5f * (0.5f * (triangleFace.pointA.vertex + triangleFace.pointB.vertex) + triangleFace.pointC.vertex) - triangleFace.normal * depth;
            }
            Vector2 newUV = MapUV(newPoint, ref triangleFace);
            Vector2 newUVA = MapUV(triangleFace.pointA.vertex, ref triangleFace);
            Vector2 newUVB = MapUV(triangleFace.pointB.vertex, ref triangleFace);
            Vector2 newUVC = MapUV(triangleFace.pointC.vertex, ref triangleFace);
            modelInfo.vertices.Add(triangleFace.pointA.vertex);
            modelInfo.uvs.Add(triangleFace.pointA.uv);
            modelInfo.vertices.Add(triangleFace.pointB.vertex);
            modelInfo.uvs.Add(triangleFace.pointB.uv);
            modelInfo.vertices.Add(triangleFace.pointC.vertex);
            modelInfo.uvs.Add(triangleFace.pointC.uv);
            modelInfo.vertices.Add(triangleFace.pointB.vertex);
            modelInfo.uvs.Add(newUVB);
            modelInfo.vertices.Add(triangleFace.pointA.vertex);
            modelInfo.uvs.Add(newUVA);
            modelInfo.vertices.Add(newPoint);
            modelInfo.uvs.Add(newUV);
            modelInfo.vertices.Add(triangleFace.pointC.vertex);
            modelInfo.uvs.Add(newUVC);
            modelInfo.vertices.Add(triangleFace.pointB.vertex);
            modelInfo.uvs.Add(newUVB);
            modelInfo.vertices.Add(newPoint);
            modelInfo.uvs.Add(newUV);
            modelInfo.vertices.Add(triangleFace.pointA.vertex);
            modelInfo.uvs.Add(newUVA);
            modelInfo.vertices.Add(triangleFace.pointC.vertex);
            modelInfo.uvs.Add(newUVC);
            modelInfo.vertices.Add(newPoint);
            modelInfo.uvs.Add(newUV);
            Vector3[] normals = new Vector3[]
            {
                Vector3.Cross(triangleFace.pointB.vertex - triangleFace.pointA.vertex, triangleFace.pointC.vertex - triangleFace.pointA.vertex).normalized,
                Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointA.vertex,triangleFace.pointB.vertex - triangleFace.pointA.vertex).normalized,
                Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointB.vertex,triangleFace.pointC.vertex - triangleFace.pointB.vertex).normalized,
                Vector3.Cross(modelInfo.vertices[5] - triangleFace.pointC.vertex,triangleFace.pointA.vertex - triangleFace.pointC.vertex).normalized
            };
            for (int i = 0; i < 12; ++i)
            {
                modelInfo.triangles.Add(i);
            }
            for (int i = 0; i < 4; ++i)
            {
                modelInfo.normals.Add(normals[i]);
                modelInfo.normals.Add(normals[i]);
                modelInfo.normals.Add(normals[i]);
            }
            return modelDatas;
        }
    }
    //public struct GenSmallerPiecesJob:IJobParallelFor
    //{
    //    public NativeArray<NativeArray<float>> triangleNormal;
    //    public NativeArray<NativeArray<float>> vertexAs;
    //    public NativeArray<NativeArray<float>> uvAs;
    //    public NativeArray<NativeArray<float>> normalAs;
    //    public NativeArray<NativeArray<float>> vertexBs;
    //    public NativeArray<NativeArray<float>> uvBs;
    //    public NativeArray<NativeArray<float>> normalBs;
    //    public NativeArray<NativeArray<float>> vertexCs;
    //    public NativeArray<NativeArray<float>> uvCs;
    //    public NativeArray<NativeArray<float>> normalCs;
    //    float areaUnit;
    //    float distance2CrossPointPer;
    //    bool infectedByCrossPoint;
    //    bool limitAreaUnit;
    //    CutToPiecesPerTime cutToPiecesPerTime;
    //    float maxAreaUnit;
    //    float minAreaUnit;
    //    public NativeArray<float> crossPointNative;
    //    public int newIndex;
    //    public NativeArray<NativeArray<float>> triangleNormalResult;
    //    public NativeArray<NativeArray<float>> vertexAsResult;
    //    public NativeArray<NativeArray<float>> uvAsResult;
    //    public NativeArray<NativeArray<float>> normalAsResult;
    //    public NativeArray<NativeArray<float>> vertexBsResult;
    //    public NativeArray<NativeArray<float>> uvBsResult;
    //    public NativeArray<NativeArray<float>> normalBsResult;
    //    public NativeArray<NativeArray<float>> vertexCsResult;
    //    public NativeArray<NativeArray<float>> uvCsResult;
    //    public NativeArray<NativeArray<float>> normalCsResult;
    //    public void ChangeToTriangleFace(int index,out TriangleFace triangleFace)
    //    {
    //        Vector3 vertexA = new Vector3(vertexAs[index][0], vertexAs[index][1], vertexAs[index][2]);
    //        Vector3 vertexB = new Vector3(vertexBs[index][0], vertexBs[index][1], vertexBs[index][2]);
    //        Vector3 vertexC = new Vector3(vertexCs[index][0], vertexCs[index][1], vertexCs[index][2]);
    //        Vector3 normalA = new Vector3(normalAs[index][0], normalAs[index][1], normalAs[index][2]);
    //        Vector3 normalB = new Vector3(normalBs[index][0], normalBs[index][1], normalBs[index][2]);
    //        Vector3 normalC = new Vector3(normalCs[index][0], normalCs[index][1], normalCs[index][2]);
    //        Vector2 uvA = new Vector2(uvAs[index][0], uvAs[index][1]);
    //        Vector2 uvB = new Vector2(uvBs[index][0], uvBs[index][1]);
    //        Vector2 uvC = new Vector2(uvCs[index][0], uvCs[index][1]);
    //        triangleFace = new TriangleFace(new VertexData(vertexA, uvA, normalA), new VertexData(vertexB, uvB, normalB), new VertexData(vertexC, uvC, normalC));
    //    }
    //    public void ChangeToNativeArray(ref TriangleFace triangleFace,int index)
    //    {
    //        var tempNormal = new NativeArray<float>(3, Allocator.TempJob);
    //        tempNormal[0] = triangleFace.normal.x;
    //        tempNormal[1] = triangleFace.normal.y;
    //        tempNormal[2] = triangleFace.normal.z;
    //        var tempVertexA = new NativeArray<float>(3, Allocator.TempJob);
    //        tempVertexA[0] = triangleFace.pointA.vertex.x;
    //        tempVertexA[1] = triangleFace.pointA.vertex.y;
    //        tempVertexA[2] = triangleFace.pointA.vertex.z;
    //        var tempVertexB = new NativeArray<float>(3, Allocator.TempJob);
    //        tempVertexB[0] = triangleFace.pointB.vertex.x;
    //        tempVertexB[1] = triangleFace.pointB.vertex.y;
    //        tempVertexB[2] = triangleFace.pointB.vertex.z;
    //        var tempVertexC = new NativeArray<float>(3, Allocator.TempJob);
    //        tempVertexC[0] = triangleFace.pointC.vertex.x;
    //        tempVertexC[1] = triangleFace.pointC.vertex.y;
    //        tempVertexC[2] = triangleFace.pointC.vertex.z;
    //        var tempUVA = new NativeArray<float>(2, Allocator.TempJob);
    //        tempUVA[0] = triangleFace.pointA.uv.x;
    //        tempUVA[1] = triangleFace.pointA.uv.y;
    //        var tempUVB = new NativeArray<float>(2, Allocator.TempJob);
    //        tempUVB[0] = triangleFace.pointB.uv.x;
    //        tempUVB[1] = triangleFace.pointB.uv.y;
    //        var tempUVC = new NativeArray<float>(2, Allocator.TempJob);
    //        tempUVC[0] = triangleFace.pointC.uv.x;
    //        tempUVC[1] = triangleFace.pointC.uv.y;
    //        var tempNormalA = new NativeArray<float>(3, Allocator.TempJob);
    //        tempNormalA[0] = triangleFace.pointA.normal.x;
    //        tempNormalA[1] = triangleFace.pointA.normal.y;
    //        tempNormalA[2] = triangleFace.pointA.normal.z;
    //        var tempNormalB = new NativeArray<float>(3, Allocator.TempJob);
    //        tempNormalB[0] = triangleFace.pointB.normal.x;
    //        tempNormalB[1] = triangleFace.pointB.normal.y;
    //        tempNormalB[2] = triangleFace.pointB.normal.z;
    //        var tempNormalC = new NativeArray<float>(3, Allocator.TempJob);
    //        tempNormalC[0] = triangleFace.pointC.normal.x;
    //        tempNormalC[1] = triangleFace.pointC.normal.y;
    //        tempNormalC[2] = triangleFace.pointC.normal.z;
    //        triangleNormalResult[index] = tempNormal;
    //        vertexAsResult[index] = tempVertexA;
    //        vertexBsResult[index] = tempVertexB;
    //        vertexBsResult[index] = tempVertexC;
    //        uvAs[index] = tempUVA;
    //        uvBs[index] = tempUVB;
    //        uvCs[index] = tempUVC;
    //        normalAsResult[index] = tempNormalA;
    //        normalBsResult[index] = tempNormalB;
    //        normalCsResult[index] = tempNormalC;
    //    }
    //    private float ComputeArea(ref TriangleFace triangleFace)
    //    {
    //        Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
    //        Vector3 ac = triangleFace.pointC.vertex - triangleFace.pointA.vertex;
    //        float height = Mathf.Sqrt(Vector3.Dot(ab, ab) - Mathf.Pow(Vector3.Dot(ab, ac.normalized), 2f));
    //        return height * ac.magnitude / 2f;
    //    }
    //    private void SplitTwoPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    //    {
    //        Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
    //        Vector3 ca = triangleFace.pointA.vertex - triangleFace.pointC.vertex;
    //        Vector3 bc = triangleFace.pointC.vertex - triangleFace.pointB.vertex;
    //        Vector3 maxLenVec;
    //        VertexData startPointData;
    //        VertexData endPointData;
    //        VertexData leftPointData;
    //        if (ab.magnitude > ca.magnitude)
    //        {
    //            maxLenVec = ab;
    //            startPointData = triangleFace.pointA;
    //            endPointData = triangleFace.pointB;
    //            leftPointData = triangleFace.pointC;
    //        }
    //        else
    //        {
    //            maxLenVec = ca;
    //            startPointData = triangleFace.pointC;
    //            endPointData = triangleFace.pointA;
    //            leftPointData = triangleFace.pointB;
    //        }
    //        if (maxLenVec.magnitude < bc.magnitude)
    //        {
    //            maxLenVec = bc;
    //            startPointData = triangleFace.pointB;
    //            endPointData = triangleFace.pointC;
    //            leftPointData = triangleFace.pointA;
    //        }
    //        Vector3 newPoint = startPointData.vertex + maxLenVec * 0.5f;
    //        VertexData newPointData = triangleFace.Interlopation(newPoint, true, true);
    //        faceStack.Push(new TriangleFace(leftPointData, startPointData, newPointData));
    //        faceStack.Push(new TriangleFace(leftPointData, newPointData, endPointData));
    //    }
    //    private void SplitThreePart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    //    {
    //        Vector3 barycentric;
    //        triangleFace.ComputeBarycentric(out barycentric);
    //        VertexData newPointData = triangleFace.Interlopation(barycentric, true, true);
    //        faceStack.Push(new TriangleFace(triangleFace.pointA, triangleFace.pointB, newPointData));
    //        faceStack.Push(new TriangleFace(triangleFace.pointB, triangleFace.pointC, newPointData));
    //        faceStack.Push(new TriangleFace(triangleFace.pointC, triangleFace.pointA, newPointData));
    //    }
    //    private void SplitFourPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
    //    {
    //        Vector3 midAB = (triangleFace.pointA.vertex + triangleFace.pointB.vertex) * 0.5f;
    //        Vector3 midAC = (triangleFace.pointA.vertex + triangleFace.pointC.vertex) * 0.5f;
    //        Vector3 midBC = (triangleFace.pointC.vertex + triangleFace.pointB.vertex) * 0.5f;
    //        VertexData midABData = triangleFace.Interlopation(midAB, true, true);
    //        VertexData midACData = triangleFace.Interlopation(midAC, true, true);
    //        VertexData midBCData = triangleFace.Interlopation(midBC, true, true);
    //        faceStack.Push(new TriangleFace(triangleFace.pointA, midABData, midACData));
    //        faceStack.Push(new TriangleFace(midABData, triangleFace.pointB, midBCData));
    //        faceStack.Push(new TriangleFace(midACData, midABData, midBCData));
    //        faceStack.Push(new TriangleFace(midACData, midBCData, triangleFace.pointC));
    //    }
    //    public void Execute(int index)
    //    {
    //        Vector3 crossPoint = new Vector3(crossPointNative[0], crossPointNative[1], crossPointNative[2]);
    //        ChangeToTriangleFace(index, out TriangleFace triangleFace);
    //        Stack<TriangleFace> faceStack = new Stack<TriangleFace>();
    //        faceStack.Push(triangleFace);
    //        float areaUnitLocal = areaUnit;
    //        if (infectedByCrossPoint)
    //        {
    //            Vector3 barycentric;
    //            triangleFace.ComputeBarycentric(out barycentric);
    //            areaUnitLocal = areaUnit * (crossPoint - barycentric).magnitude / distance2CrossPointPer;
    //            if (limitAreaUnit)
    //            {
    //                if (maxAreaUnit < minAreaUnit)
    //                {
    //                    maxAreaUnit = minAreaUnit;
    //                }
    //                if (areaUnitLocal > maxAreaUnit)
    //                {
    //                    areaUnitLocal = maxAreaUnit;
    //                }
    //                else if (areaUnitLocal < minAreaUnit)
    //                {
    //                    areaUnitLocal = minAreaUnit;
    //                }
    //            }
    //        }
    //        while (faceStack.Count > 0)
    //        {
    //            var curFace = faceStack.Pop();
    //            float triangleArea = ComputeArea(ref curFace);
    //            int count = (int)(triangleArea / areaUnitLocal) + 1;
    //            if (count <= 1)
    //            {
    //                ChangeToNativeArray(ref curFace, newIndex);
    //                newIndex = newIndex + 1;
    //                continue;
    //            }

    //            //能整除的优先
    //            if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToFour) == CutToPiecesPerTime.CutToFour && count % 4 == 0)
    //            {
    //                SplitFourPart(ref curFace, ref faceStack);
    //            }
    //            else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToThree) == CutToPiecesPerTime.CutToThree && count % 3 == 0)
    //            {
    //                SplitThreePart(ref curFace, ref faceStack);
    //            }
    //            else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToTwo) == CutToPiecesPerTime.CutToTwo && count % 2 == 0)
    //            {
    //                SplitTwoPart(ref curFace, ref faceStack);
    //            }
    //            else
    //            {
    //                //优先级2》3》4
    //                if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToTwo) == CutToPiecesPerTime.CutToTwo && count / 2 > 0)
    //                {
    //                    SplitTwoPart(ref curFace, ref faceStack);
    //                }
    //                else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToThree) == CutToPiecesPerTime.CutToThree && count / 3 > 0)
    //                {
    //                    SplitThreePart(ref curFace, ref faceStack);
    //                }
    //                else
    //                {
    //                    SplitFourPart(ref curFace, ref faceStack);
    //                }
    //            }
    //        }
    //    }
    //}
    /// <summary>
    /// 嵌套一个IGenPieces对象,三角面细化切割后再用该IGenPieces对象生成碎片
    /// </summary>
    public class GenSmallerPieces : IGenPieces
    {
        private List<TriangleFace> splitTriangleFace = new List<TriangleFace>();
        private List<float> triangleAreaList = new List<float>();
        private IGenPieces sonGenPieces;
        private float areaUnit;
        private Vector3 crossPoint;
        private bool infectedByCrossPoint;
        private float distance2CrossPointPer;
        private CutToPiecesPerTime cutToPiecesPerTime;
        private float maxAreaUnit;
        private float minAreaUnit;
        private bool limitAreaUnit;
        public GenSmallerPieces(IGenPieces sonGenPieces, float areaUnit, CutToPiecesPerTime cutToPiecesPerTime)
        {
            this.sonGenPieces = sonGenPieces;
            this.areaUnit = areaUnit;
            this.cutToPiecesPerTime = cutToPiecesPerTime;
        }
        public GenPiecesType GetEnumType()
        {
            return GenPiecesType.GenSmallerPieces;
        }
        private float ComputeArea(ref TriangleFace triangleFace)
        {
            Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
            Vector3 ac = triangleFace.pointC.vertex - triangleFace.pointA.vertex;
            float height = Mathf.Sqrt(Vector3.Dot(ab, ab) - Mathf.Pow(Vector3.Dot(ab, ac.normalized), 2f));
            return height * ac.magnitude / 2f;
        }
        private void SplitTwoPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
        {
            Vector3 ab = triangleFace.pointB.vertex - triangleFace.pointA.vertex;
            Vector3 ca = triangleFace.pointA.vertex - triangleFace.pointC.vertex;
            Vector3 bc = triangleFace.pointC.vertex - triangleFace.pointB.vertex;
            Vector3 maxLenVec;
            VertexData startPointData;
            VertexData endPointData;
            VertexData leftPointData;
            if (ab.magnitude > ca.magnitude)
            {
                maxLenVec = ab;
                startPointData = triangleFace.pointA;
                endPointData = triangleFace.pointB;
                leftPointData = triangleFace.pointC;
            }
            else
            {
                maxLenVec = ca;
                startPointData = triangleFace.pointC;
                endPointData = triangleFace.pointA;
                leftPointData = triangleFace.pointB;
            }
            if (maxLenVec.magnitude < bc.magnitude)
            {
                maxLenVec = bc;
                startPointData = triangleFace.pointB;
                endPointData = triangleFace.pointC;
                leftPointData = triangleFace.pointA;
            }
            Vector3 newPoint = startPointData.vertex + maxLenVec * 0.5f;
            VertexData newPointData = triangleFace.Interlopation(newPoint, true, true);
            faceStack.Push(new TriangleFace(leftPointData, startPointData, newPointData));
            faceStack.Push(new TriangleFace(leftPointData, newPointData, endPointData));
        }
        private void SplitThreePart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
        {
            Vector3 barycentric;
            triangleFace.ComputeBarycentric(out barycentric);
            VertexData newPointData = triangleFace.Interlopation(barycentric, true, true);
            faceStack.Push(new TriangleFace(triangleFace.pointA, triangleFace.pointB, newPointData));
            faceStack.Push(new TriangleFace(triangleFace.pointB, triangleFace.pointC, newPointData));
            faceStack.Push(new TriangleFace(triangleFace.pointC, triangleFace.pointA, newPointData));
        }
        private void SplitFourPart(ref TriangleFace triangleFace, ref Stack<TriangleFace> faceStack)
        {
            Vector3 midAB = (triangleFace.pointA.vertex + triangleFace.pointB.vertex) * 0.5f;
            Vector3 midAC = (triangleFace.pointA.vertex + triangleFace.pointC.vertex) * 0.5f;
            Vector3 midBC = (triangleFace.pointC.vertex + triangleFace.pointB.vertex) * 0.5f;
            VertexData midABData = triangleFace.Interlopation(midAB, true, true);
            VertexData midACData = triangleFace.Interlopation(midAC, true, true);
            VertexData midBCData = triangleFace.Interlopation(midBC, true, true);
            faceStack.Push(new TriangleFace(triangleFace.pointA, midABData, midACData));
            faceStack.Push(new TriangleFace(midABData, triangleFace.pointB, midBCData));
            faceStack.Push(new TriangleFace(midACData, midABData, midBCData));
            faceStack.Push(new TriangleFace(midACData, midBCData, triangleFace.pointC));
        }
        public void SetCrossPoint(Vector3 crossPoint, bool infectedByCrossPoint = false, float distance2CrossPointPer = 0f, bool limitAreaUnit = false, float maxAreaUnit = 0f, float minAreaUnit = 0f)
        {
            this.crossPoint = crossPoint;
            this.infectedByCrossPoint = infectedByCrossPoint;
            this.distance2CrossPointPer = distance2CrossPointPer;
            this.limitAreaUnit = limitAreaUnit;
            this.maxAreaUnit = maxAreaUnit;
            this.minAreaUnit = minAreaUnit;
        }
        /// <summary>
        /// 切割后的三角形面积会小于areaUnit
        /// </summary>
        /// <param name="areaUnit"></param>
        /// <returns></returns>
        public void SplitByArea(ref TriangleFace triangleFace)
        {
            Stack<TriangleFace> faceStack = new Stack<TriangleFace>();
            faceStack.Push(triangleFace);
            float areaUnitLocal = areaUnit;
            if (infectedByCrossPoint)
            {
                Vector3 barycentric;
                triangleFace.ComputeBarycentric(out barycentric);
                areaUnitLocal = areaUnit * (crossPoint - barycentric).magnitude / distance2CrossPointPer;
                if (limitAreaUnit)
                {
                    if (maxAreaUnit < minAreaUnit)
                    {
                        maxAreaUnit = minAreaUnit;
                    }
                    if (areaUnitLocal > maxAreaUnit)
                    {
                        areaUnitLocal = maxAreaUnit;
                    }
                    else if (areaUnitLocal < minAreaUnit)
                    {
                        areaUnitLocal = minAreaUnit;
                    }
                }
            }
            while (faceStack.Count > 0)
            {
                var curFace = faceStack.Pop();
                float triangleArea = ComputeArea(ref curFace);
                int count = (int)(triangleArea / areaUnitLocal) + 1;
                if (count <= 1)
                {
                    splitTriangleFace.Add(curFace);
                    triangleAreaList.Add(triangleArea);
                    continue;
                }

                //能整除的优先
                if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToFour) == CutToPiecesPerTime.CutToFour && count % 4 == 0)
                {
                    SplitFourPart(ref curFace, ref faceStack);
                }
                else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToThree) == CutToPiecesPerTime.CutToThree && count % 3 == 0)
                {
                    SplitThreePart(ref curFace, ref faceStack);
                }
                else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToTwo) == CutToPiecesPerTime.CutToTwo && count % 2 == 0)
                {
                    SplitTwoPart(ref curFace, ref faceStack);
                }
                else
                {
                    //优先级2》3》4
                    if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToTwo) == CutToPiecesPerTime.CutToTwo && count / 2 > 0)
                    {
                        SplitTwoPart(ref curFace, ref faceStack);
                    }
                    else if ((cutToPiecesPerTime & CutToPiecesPerTime.CutToThree) == CutToPiecesPerTime.CutToThree && count / 3 > 0)
                    {
                        SplitThreePart(ref curFace, ref faceStack);
                    }
                    else
                    {
                        SplitFourPart(ref curFace, ref faceStack);
                    }
                }
            }
        }
        private void PreProcess(ref TriangleFace triangleFace)
        {
            splitTriangleFace.Clear();
            triangleAreaList.Clear();
            SplitByArea(ref triangleFace);
        }
        public List<ModelData> GenModelData(TriangleFace triangleFace)
        {
            List<ModelData> modelDatas = new List<ModelData>();
            PreProcess(ref triangleFace);
            for (int i = 0; i < splitTriangleFace.Count; ++i)
            {
                modelDatas.AddRange(sonGenPieces.GenModelData(splitTriangleFace[i]));
            }
            return modelDatas;
        }
    }
}