using OpenTK.Mathematics;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Graphics
{
    public class Quad
    {
        private List<float> vertices = new List<float>
        {
            // positions   // texcoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        };

        VAO vao;
        VBO vbo;

        public Quad()
        {
            vao = new VAO();
            vbo = new VBO(vertices);
            VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            EnableVertexAttribArray(0);
            VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            EnableVertexAttribArray(1);
        }

        public void Render()
        {
            Disable(EnableCap.DepthTest);
            vao.Bind();

            DrawArrays(PrimitiveType.Triangles, 0, vertices.Count);

            vao.Unbind();
            Enable(EnableCap.DepthTest);
        }

        public void Delete()
        {
            vao.Delete();
            vbo.Delete();
        }
    }
}