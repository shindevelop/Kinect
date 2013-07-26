using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Kinect.Toolkit
{
    public class BarycenterHelper
    {
        readonly List<Vector3> positions = new List<Vector3>();
        readonly int windowSize;

        public bool IsStable { get; private set; }

        public BarycenterHelper(int windowSize)
        {
            this.windowSize = windowSize;
        }

        public void Add(Vector3 position)
        {
            positions.Add(position);

            if (positions.Count > windowSize)
                positions.RemoveAt(0);

            if (positions.Count == windowSize)
            {
                CheckStability();
            }
        }

        void CheckStability()
        {
            float avg = positions.Average(p => p.Length());

            IsStable = Math.Abs(avg - positions[positions.Count - 1].Length()) < 0.05f;

            Debug.WriteLine(Math.Abs(avg - positions[positions.Count - 1].Length()));
        }
    }
}
