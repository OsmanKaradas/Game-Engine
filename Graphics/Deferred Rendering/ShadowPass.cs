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
        public ShadowCubeMapFBO cubeMapFBO = null!;
        public Matrix4 lightSpaceMatrix;

        public float nearPlane = 1f, farPlane = 75f;

        public ShadowPass(string vertShader, string fragShader, string geomShader)
        {
            shader = new(vertShader, fragShader);
            fbo = new();
            cubeMapFBO = new();
        }

        public void RenderDirLightShadows()
        {
            Viewport(0, 0, fbo.width, fbo.height);

            fbo.Bind();

            Clear(ClearBufferMask.DepthBufferBit);
            Enable(EnableCap.CullFace);
            CullFace(TriangleFace.Front);

            Matrix4 lightProjection = Matrix4.CreateOrthographic(35f, 35f, nearPlane, farPlane);
            Matrix4 lightView = Matrix4.LookAt(35f * -Light.directionalLight.direction, Vector3.Zero, new(0f, 1f, 0f));
            lightSpaceMatrix = lightView * lightProjection;

            UseProgram(shader.ID);
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

            GameObject.Render(shader);

            fbo.Unbind();
            CullFace(TriangleFace.Back);
            Disable(EnableCap.CullFace);
        }

        public void RenderPointLightShadows(Vector3 lightPos)
        {
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1f, nearPlane, farPlane);
            Matrix4[] transforms = {
                projection * Matrix4.LookAt(lightPos, lightPos + Vector3.UnitX, Vector3.UnitY),
                projection * Matrix4.LookAt(lightPos, lightPos - Vector3.UnitX, Vector3.UnitY),
                projection * Matrix4.LookAt(lightPos, lightPos + Vector3.UnitY, Vector3.UnitY),
                projection * Matrix4.LookAt(lightPos, lightPos - Vector3.UnitY, Vector3.UnitY),
                projection * Matrix4.LookAt(lightPos, lightPos + Vector3.UnitZ, Vector3.UnitY),
                projection * Matrix4.LookAt(lightPos, lightPos - Vector3.UnitZ, Vector3.UnitY),
            };

            Viewport(0, 0, fbo.width, fbo.height);
            cubeMapFBO.Bind();
            Clear(ClearBufferMask.DepthBufferBit);
            UseProgram(shader.ID);
            for (int i = 0; i < 6; i++)
            {
                shader.SetMatrix4($"shadowMatrices[{i}]", transforms[i]);
            }

            shader.SetFloat("farPlane", farPlane);
            shader.SetVector3("lightPos", lightPos);
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

            GameObject.Render(shader);
            cubeMapFBO.Unbind();
        }
    }
}

