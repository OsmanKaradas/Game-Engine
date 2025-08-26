using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class VAO
    {
        public int ID;

        public VAO()
        {
            ID = GenVertexArray();
            BindVertexArray(ID);
        }

        public void LinkToVAO(VBO vbo)
        {
            int stride = 8 * sizeof(float);
            Bind();
            vbo.Bind();

            // POS
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            EnableVertexAttribArray(0);

            // NORMAL
            VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            EnableVertexAttribArray(1);

            // UV
            VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            EnableVertexAttribArray(2);
    
            Unbind();
        }

        public void Bind()
        {
            BindVertexArray(ID);
        }
        public void Unbind()
        {
            BindVertexArray(0);
        }
        public void Delete()
        {
            DeleteVertexArray(ID);
        }
    }
}

