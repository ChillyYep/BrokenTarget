using System.Collections;
using System.Collections.Generic;
using UnityEngine
    ;
namespace chenyi2
{
    public interface IVistor
    {
        void Traversal(IBreakable target);
    }
}
