using GameEngine.World;
using OpenTK.Mathematics;

namespace GameEngine.Animation
{
    public class Animator
    {
        public Dictionary<string, AnimationClip> animations = new();
        public AnimationClip currentClip = null!;
        private AnimationClip? nextClip = null;
        private float currentTime = 0f;
        private float nextClipTime = 0f;

        // Crossfade state
        private float blendTime = 0f;
        private float blendDuration = 0f;

        public Armature armature = null!;

        public Animator(Armature? armature = null)
        {
            if (armature != null)
                this.armature = armature;
        }

        public void Update()
        {
            if (armature == null || currentClip == null)
                return;

            currentTime += Time.deltaTime;

            // Handle looping or stopping
            if (currentClip.loop)
                currentTime %= currentClip.duration;
            else if (currentTime >= currentClip.duration)
            {
                Stop();
                return;
            }

            bool blending = nextClip != null;

            if (blending)
            {
                if (nextClip!.loop)
                    nextClipTime %= nextClip.duration;
                else if (nextClipTime >= nextClip.duration)
                    nextClipTime = nextClip.duration;
                blendTime += Time.deltaTime;
                float blendFactor = Math.Clamp(blendTime / blendDuration, 0f, 1f);
                blendFactor = blendFactor * blendFactor * (3f - 2f * blendFactor);
                nextClipTime += Time.deltaTime;

                SampleClip(currentClip, currentTime, out var pos1, out var rot1, out var scale1);
                SampleClip(nextClip!, nextClipTime, out var pos2, out var rot2, out var scale2);

                foreach (var bone in armature.bones)
                {
                    string name = bone.Key;
                    bone.Value.position = Vector3.Lerp(pos1[name], pos2[name], blendFactor);
                    bone.Value.rotation = Quaternion.Slerp(rot1[name], rot2[name], blendFactor);
                    bone.Value.scale = Vector3.Lerp(scale1[name], scale2[name], blendFactor);
                }

                if (blendFactor >= 1f)
                {
                    currentClip = nextClip!;
                    nextClip = null;
                    currentTime = nextClipTime;
                    blendTime = 0f;
                }
            }
            else
            {
                SampleClip(currentClip, currentTime, out var pos, out var rot, out var scale);
                foreach (var bone in armature.bones)
                {
                    bone.Value.position = pos[bone.Key];
                    bone.Value.rotation = rot[bone.Key];
                    bone.Value.scale = scale[bone.Key];
                }
            }
        }

        public void Play(AnimationClip animation)
        {
            currentClip = animation;
            currentTime = 0f;
        }

        public void Stop()
        {
            currentClip = null!;
            currentTime = 0f;
            blendTime = 0f;
            nextClip = null;

            armature?.SetBindPose();
        }

        public void AddAnimation(string name, AnimationClip animation)
        {
            animations.Add(name, animation);
        }

        public void AddAnimation(SharpGLTF.Schema2.Animation animation)
        {
            List<KeyFrame> keyframes = new();
            float step = 1f / 30f;

            for (float t = 0; t < animation.Duration; t += step)
            {
                var kf = new KeyFrame(t, armature);
                foreach (var channel in animation.Channels)
                {
                    var node = channel.TargetNode;
                    string boneName = node.Name;
                    var xform = node.GetLocalTransform(animation, t);

                    var pos = new Vector3(xform.Translation.X, xform.Translation.Y, xform.Translation.Z);
                    var rot = new Quaternion(xform.Rotation.X, xform.Rotation.Y, xform.Rotation.Z, xform.Rotation.W);
                    var scl = new Vector3(xform.Scale.X, xform.Scale.Y, xform.Scale.Z);

                    kf.positions[boneName] = pos;
                    kf.rotations[boneName] = rot;
                    kf.scales[boneName] = scl;
                }
                keyframes.Add(kf);
            }

            animations[animation.Name] = new AnimationClip(animation.Duration, keyframes.ToArray());
        }

        private void SampleClip(AnimationClip clip, float time,
            out Dictionary<string, Vector3> positions,
            out Dictionary<string, Quaternion> rotations,
            out Dictionary<string, Vector3> scales)
        {
            positions = new();
            rotations = new();
            scales = new();

            KeyFrame prev = clip.keyFrames.First();
            KeyFrame next = clip.keyFrames.Last();

            foreach (var kf in clip.keyFrames)
            {
                if (kf.timeStamp <= time) prev = kf;
                if (kf.timeStamp > time) { next = kf; break; }
            }

            float t = (float)((time - prev.timeStamp) / (next.timeStamp - prev.timeStamp));
            if (float.IsNaN(t)) t = 0f;

            foreach (var boneName in prev.positions.Keys)
            {
                positions[boneName] = Vector3.Lerp(prev.positions[boneName], next.positions[boneName], t);
                rotations[boneName] = Quaternion.Slerp(prev.rotations[boneName], next.rotations[boneName], t);
                scales[boneName] = Vector3.Lerp(prev.scales[boneName], next.scales[boneName], t);
            }
        }

        public void CrossFade(AnimationClip clip, float duration)
        {
            if (currentClip == clip) return;
            nextClip = clip;
            blendTime = 0f;
            nextClipTime = 0f;
            blendDuration = duration;
        }
    }
}
