

namespace GameEngine.Animation
{
    public class Animator
    {
        public AnimationClip[] animations;

        public Animator()
        {

        }

        public void AddAnimation(AnimationClip animation)
        {
            animations.Append(animation);
        }
    }
}