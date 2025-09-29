using SharpGLTF.Schema2;
using GameEngine.Graphics;
using OpenTK.Mathematics;
using static OpenTK.Graphics.OpenGL4.GL;
namespace GameEngine.World
{
    public class Armature
    {
        Skin skin = null!;
        public Dictionary<string, Bone> bones = new();
        public Armature(string filePath)
        {
            skin = LoadSkin(filePath);

            List<Matrix4> inverseBindMatrices = new();
            foreach (var mat in skin.InverseBindMatrices)
            {
                inverseBindMatrices.Add(new(
                    mat.M11, mat.M12, mat.M13, mat.M14,
                    mat.M21, mat.M22, mat.M23, mat.M24,
                    mat.M31, mat.M32, mat.M33, mat.M34,
                    mat.M41, mat.M42, mat.M43, mat.M44
                ));
            }

            // Pass 1: create bones without hierarchy
            for (int i = 0; i < skin.JointsCount; i++)
            {
                var joint = skin.Joints[i];
                Matrix4 worldMatrix = new(
                    joint.WorldMatrix.M11, joint.WorldMatrix.M12, joint.WorldMatrix.M13, joint.WorldMatrix.M14,
                    joint.WorldMatrix.M21, joint.WorldMatrix.M22, joint.WorldMatrix.M23, joint.WorldMatrix.M24,
                    joint.WorldMatrix.M31, joint.WorldMatrix.M32, joint.WorldMatrix.M33, joint.WorldMatrix.M34,
                    joint.WorldMatrix.M41, joint.WorldMatrix.M42, joint.WorldMatrix.M43, joint.WorldMatrix.M44
                );

                var bone = new Bone(
                    joint.LogicalIndex,
                    joint.Name,
                    worldMatrix,
                    inverseBindMatrices[i],
                    joint.LocalTransform,
                    null!,
                    new List<Bone>()
                );

                bones.Add(bone.name, bone);
            }

            for (int i = 0; i < skin.JointsCount; i++)
            {
                var joint = skin.Joints[i];
                if (joint.VisualParent != null && bones.ContainsKey(joint.VisualParent.Name))
                {
                    bones[joint.Name].parent = bones[joint.VisualParent.Name];
                }

                foreach (var child in joint.VisualChildren)
                {
                    if (bones.ContainsKey(child.Name))
                        bones[joint.Name].children.Add(bones[child.Name]);
                }
            }
        }

        private Skin LoadSkin(string filePath)
        {
            try
            {
                var scene = ModelRoot.Load("Models/" + filePath);

                if (scene.LogicalScenes.Count <= 0)
                    throw new ArgumentException("No Armature Found!");

                return scene.LogicalSkins[0];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null!;
            }
        }

        public void Update(ShaderProgram shader)
        {
            Matrix4[] finalMatrices = new Matrix4[bones.Count];

            foreach (var bone in bones.Values)
            {
                finalMatrices[bone.ID] = GetFinalMatrix(bone);
            }

            UseProgram(shader.ID);
            
            float[] f = new float[finalMatrices.Length * 16];
            for (int i = 0; i < finalMatrices.Length; i++)
            {
                float[] fA = Matrix4ToArray(finalMatrices[i]);
                int offset = i * 16;
                for (int j = 0; j < 16; j++)
                {
                    f[offset + j] = fA[j];
                }
            }
            int loc = GetUniformLocation(shader.ID, "finalBonesMatrices");
            UniformMatrix4(loc, finalMatrices.Length, false, f);
        }

        private float[] Matrix4ToArray(Matrix4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }
        
        private Matrix4 GetFinalMatrix(Bone bone)
        {
            Matrix4 global = GetGlobalMatrix(bone);

            bone.finalMatrix = bone.offset * global;

            return bone.finalMatrix;
        }
        
        private Matrix4 GetGlobalMatrix(Bone bone)
        {
            if (bone.parent != null)
                return bone.GetLocalMatrix() * GetGlobalMatrix(bone.parent) ;
            else
                return bone.GetLocalMatrix();
        }
        
        public class Bone
        {
            public int ID;
            public string name;

            public Matrix4 finalMatrix;
            public Matrix4 worldMatrix;
            public Matrix4 offset;

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;


            public Bone parent;
            public List<Bone> children = new();

            public Bone(int ID, string name, Matrix4 worldMatrix, Matrix4 offset, SharpGLTF.Transforms.AffineTransform localTransform, Bone parent, List<Bone> children)
            {
                this.ID = ID;
                this.name = name;
                this.worldMatrix = worldMatrix;
                this.offset = offset;

                this.position = new(localTransform.Translation.X, localTransform.Translation.Y, localTransform.Translation.Z);
                this.rotation = new(localTransform.Rotation.X, localTransform.Rotation.Y, localTransform.Rotation.Z, localTransform.Rotation.W);
                this.scale = new(localTransform.Scale.X, localTransform.Scale.Y, localTransform.Scale.Z);

                this.parent = parent;
                this.children = children;
            }

            public Matrix4 GetLocalMatrix()
            {
                return
                    Matrix4.CreateScale(scale) *
                    Matrix4.CreateFromQuaternion(rotation) *
                    Matrix4.CreateTranslation(position);
            }
        };
    }
}