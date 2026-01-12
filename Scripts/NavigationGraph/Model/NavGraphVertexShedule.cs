using Godot;
using System;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraphVertexShedule
    {
        public float timeOpen;
        public float timeClosed;
        public float offset;

        public NavGraphVertexShedule(float timeOpen, float timeClosed, float offset)
        {
            if (timeOpen <= 0 || !float.IsNormal(timeOpen))
                throw new ArgumentOutOfRangeException(nameof(timeOpen));

            if (timeClosed <= 0 || !float.IsNormal(timeClosed))
                throw new ArgumentOutOfRangeException(nameof(timeClosed));

            if (offset < 0 || float.IsNaN(offset) || float.IsInfinity(offset))
                throw new ArgumentOutOfRangeException(nameof(offset));

            this.timeOpen = timeOpen;
            this.timeClosed = timeClosed;
            this.offset = offset;
        }

        public bool IsOpen(float currentTime)
        {
            currentTime = (currentTime + offset) % (timeOpen + timeClosed);
            if (currentTime <= timeOpen)
                return true;
            return false;
        }
    }
}
