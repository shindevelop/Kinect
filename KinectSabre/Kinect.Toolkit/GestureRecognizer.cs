using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Kinect.Toolkit
{
    public enum SupportedGesture
    {
        LeftToRight,
        RightToLeft,
        BackToFront,
        FrontToBack
    }    

    public class GestureRecognizer
    {
        struct Entry
        {
            public DateTime Time;
            public Vector3 Position;
        }

        readonly List<Entry> entries = new List<Entry>();

        public event Action<SupportedGesture> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize;

        public GestureRecognizer(int windowSize = 20)
        {
            this.windowSize = windowSize;
        }

        public void Add(Vector3 position)
        {
            entries.Add(new Entry{Position = position, Time = DateTime.Now});

            if (entries.Count > windowSize)
                entries.RemoveAt(0);

            if (entries.Count < windowSize)
                return;

            LookForGesture();
        }

        void RaiseGestureDetected(SupportedGesture gesture)
        {
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > 500)
            {
                if (OnGestureDetected != null)
                    OnGestureDetected(gesture);

                lastGestureDate = DateTime.Now;
            }

            entries.Clear();
        }

        bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction, Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < entries.Count - 1; index++)
            {
                if (!heightFunction(entries[0].Position, entries[index].Position) || !directionFunction(entries[index].Position, entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(entries[index].Position, entries[start].Position))
                {
                    double totalMilliseconds = (entries[index].Time - entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void LookForGesture()
        {
            // Left to Right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < 0.20f, (p1, p2) => p2.X - p1.X > - 0.01f , (p1, p2) => Math.Abs(p2.X - p1.X) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected(SupportedGesture.LeftToRight);
                return;
            }

            // Right to Left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < 0.20f, (p1, p2) => p2.X - p1.X < 0.01f, (p1, p2) => Math.Abs(p2.X - p1.X) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected(SupportedGesture.RightToLeft);
                return;
            }

            // Back to Front
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < 0.15f, (p1, p2) => p2.Z - p1.Z < 0.01f, (p1, p2) => Math.Abs(p2.Z - p1.Z) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected(SupportedGesture.BackToFront);
                return;
            }

            // Front to Back
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < 0.15f, (p1, p2) => p2.Z - p1.Z > -0.01f, (p1, p2) => Math.Abs(p2.Z - p1.Z) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected(SupportedGesture.FrontToBack);
                return;
            }
        }

    }
}