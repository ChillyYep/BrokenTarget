﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace chenyi
{
    public interface IVistor
    {
        void Traversal(IBreakable target);
    }
}
