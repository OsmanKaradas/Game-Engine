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

        public Armature(Dictionary<string, Bone> bones)
        {
            this.bones = bones;
        }
        public Armature(SharpGLTF.Schema2.Skin skin)
        {
            // Pass 1: create bones without hierarchy
            for (int i = 0; i < skin.JointsCount; i++)
            {
                var joint = skin.Joints[i];

                var m = skin.InverseBindMatrices[i];
                Matrix4 inverseBindMatrix = new(
                    m.M11, m.M12, m.M13, m.M14,
                    m.M21, m.M22, m.M23, m.M24,
                    m.M31, m.M32, m.M33, m.M34,
                    m.M41, m.M42, m.M43, m.M44                
                );

                System.Numerics.Matrix4x4 mat = joint.LocalMatrix;
                Matrix4 localMatrix = new(
                    mat.M11, mat.M12, mat.M13, mat.M14,
                    mat.M21, mat.M22, mat.M23, mat.M24,
                    mat.M31, mat.M32, mat.M33, mat.M34,
                    mat.M41, mat.M42, mat.M43, mat.M44
                );

                var bone = new Bone(
                    bones.Count,
                    joint.Name,
                    inverseBindMatrix,
                    localMatrix,
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
            
            float[] f = new float[finalMatrices.Length * 16];
            for (int i = 0; i < finalMatrices.Length; i++)
            {
                var m = finalMatrices[i];
                float[] fA = new[]
                { 
                    m.M11, m.M12, m.M13, m.M14,
                    m.M21, m.M22, m.M23, m.M24,
                    m.M31, m.M32, m.M33, m.M34,
                    m.M41, m.M42, m.M43, m.M44
                };
                int offset = i * 16;
                for (int j = 0; j < 16; j++)
                {
                    f[offset + j] = fA[j];
                }
            }

            UseProgram(shader.ID);
            int loc = GetUniformLocation(shader.ID, "finalBonesMatrices");
            UniformMatrix4(loc, finalMatrices.Length, false, f);
        }
        
        private Matrix4 GetFinalMatrix(Bone bone)
        {
            Matrix4 global = GetGlobalMatrix(bone);

            bone.finalMatrix = bone.offset * global;

            return bone.finalMatrix;
        }
        
        private Matrix4 GetGlobalMatrix(Bone bone)
        {
            Matrix4 local = bone.GetLocalMatrix();

            if (bone.parent != null)
                return local * GetGlobalMatrix(bone.parent);
            else
                return local;
        }

        public List<Vector3> GetBoneDebugLines()
        {
            var lines = new List<Vector3>();
            foreach (var b in bones)
            {
                Bone bone = b.Value;
                Vector3 bonePos = GetGlobalMatrix(bone).ExtractTranslation();

                if (bone.parent != null)
                {
                    Vector3 parentPos = GetGlobalMatrix(bone.parent).ExtractTranslation();
                    lines.Add(parentPos);
                    lines.Add(bonePos);
                }
            }
            return lines;
        }

        public void SetBindPose()
        {
            Bone bone;

            foreach (var b in bones)
            {
                bone = b.Value;

                Matrix4 bindMatrix = bone.offset.Inverted();

                // If the bone has a parent, compute local relative to parent
                if (bone.parent != null)
                {
                    Matrix4 parentBind = bone.parent.offset.Inverted();
                    Matrix4 localBind = bindMatrix * parentBind.Inverted();

                    bone.position = localBind.ExtractTranslation();
                    var rot = localBind.ExtractRotation();
                    bone.rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                    bone.scale = localBind.ExtractScale();
                }
                else
                {
                    bone.position = bindMatrix.ExtractTranslation();
                    var rot = bindMatrix.ExtractRotation();
                    bone.rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                    bone.scale = bindMatrix.ExtractScale();
                }
            }
        }
        public class Bone
        {
            public int ID;
            public string name;

            public Matrix4 finalMatrix;
            public Matrix4 offset;

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public Bone parent = null!;
            public List<Bone> children;

            public Bone(int ID, string name, Matrix4 offset, Matrix4 localTransform, List<Bone> children, Bone? parent = null)
            {
                this.ID = ID;
                this.name = name;
                this.offset = offset;

                this.position = new(localTransform.ExtractTranslation());
                var rot = localTransform.ExtractRotation();
                this.rotation = new(rot.X, rot.Y, rot.Z, rot.W);
                this.scale = new(localTransform.ExtractScale());

                this.children = children;
                if (parent != null)
                    this.parent = parent;
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