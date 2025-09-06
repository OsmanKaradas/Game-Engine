using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;

namespace GameEngine.Graphics
{
    public class ShadowPass
    {
        public ShaderProgram shader;
        public ShadowFBO fbo;
        public ShadowFBO cubeMapFBO;
        public Matrix4 lightSpaceMatrix;
        public ShadowPass(string shaderVert, string shaderFrag)
        {
            shader = new(shaderVert, shaderFrag);
            fbo = new();
            cubeMapFBO = new();
        }

        public void Render(int width, int height)
        {
            Viewport(0, 0, fbo.width, fbo.height);
            UseProgram(shader.ID);

            fbo.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            Enable(EnableCap.CullFace);
            CullFace(TriangleFace.Front);

            Matrix4 lightProjection = Matrix4.CreateOrthographic(35f, 35f, 1f, 75f);
            Matrix4 lightProjPerspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1f, 0.1f, 75f);
            Matrix4 lightView = Matrix4.LookAt(35f * -Light.directionalLight.direction, Vector3.Zero, new Vector3(0f, 1f, 0f));
            lightSpaceMatrix = lightView * lightProjection;
            UniformMatrix4(GetUniformLocation(shader.ID, "lightSpaceMatrix"), false, ref lightSpaceMatrix);

            GameObject.Render(shader);

            fbo.Unbind();
            CullFace(TriangleFace.Back);
            Disable(EnableCap.CullFace);
            
            // --- CUBE MAP ---
            Viewport(0, 0, width, height);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cubeMapFBO.Bind();
            float near = 1f;
            float far = 25f;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1f, near, far);
            Matrix4[] transform = {
                projection * Matrix4.LookAt()
            };

            GameObject.Render(shader);
        }
    }
}

