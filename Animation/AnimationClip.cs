
using OpenTK.Mathematics;

namespace GameEngine.Animation
{
    public class AnimationClip
    {
        public float duration;
        public KeyFrame[] keyFrames;

        public bool loop = false;

        public AnimationClip(float duration, KeyFrame[] keyFrames)
        {
            this.duration = duration;
            this.keyFrames = keyFrames;
        }
    }

    public class KeyFrame
    {
        public float timeStamp;
        public Dictionary<string, Vector3> positions = new();
        public Dictionary<string, Quaternion> rotations = new();
        public Dictionary<string, Vector3> scales = new();

        public KeyFrame(float timeStamp, World.Armature armature)
        {
            this.timeStamp = timeStamp;
            var arr = armature.bones.ToArray();
            for (int i = 0; i < armature.bones.Count; i++)
            {
                positions.Add(arr[i].Key, arr[i].Value.position);
                rotations.Add(arr[i].Key, arr[i].Value.rotation);
                scales.Add(arr[i].Key, arr[i].Value.scale);
            }
        }

        protected float GetTimeStamp()
        {
            return timeStamp;
        }
    }
}