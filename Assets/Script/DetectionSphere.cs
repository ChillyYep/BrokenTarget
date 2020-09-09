using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public class DetectionSphere : chenyi.DetectionSphere
    {
        public DetectionSphere(Vector3 center, float radius = 1f, chenyi.SegmentVerticalType segmentVerticalType = chenyi.SegmentVerticalType.Ten,
    chenyi.SegmentCircleType segmentCircleType = chenyi.SegmentCircleType.Eighteen) : base(center, radius, segmentVerticalType, segmentCircleType)
        {
        }
        public override void Traversal(chenyi.IBreakable breakable)
        {

        }
    }
}
