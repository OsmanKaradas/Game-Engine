using SharpGLTF.Animations;
using SharpGLTF.Schema2;
using OpenTK.Mathematics;

namespace GameEngine.Animation
{
    public class AnimationClip
    {
        private float duration;
        private float currentTime;
        private KeyFrame[] keyFrames;
        private bool finished = false;

        public AnimationClip(float duration, KeyFrame[] keyFrames)
        {
            this.duration = duration;
            this.keyFrames = keyFrames;
        }

        public void UpdateAnimation()
        {
            if (finished)
                return;

            if (currentTime >= duration)
            {
                Console.WriteLine("Animation Finished!");
                finished = true;
                return;
            }

            currentTime += Time.deltaTime;
        }
    }

    public class KeyFrame
    {
        private float timeStamp;
        private Dictionary<string, Matrix4> pose;
        
        public KeyFrame(float timeStamp, Dictionary<string, Matrix4> pose)
        {
            this.timeStamp = timeStamp;
            this.pose = pose;
        }

        protected float GetTimeStamp()
        {
            return timeStamp;
        }

        protected Dictionary<string, Matrix4> GetMap()
        {
            return pose;
        }
    }
}