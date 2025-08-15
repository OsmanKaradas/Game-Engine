using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
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

            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            EnableVertexAttribArray(0);
            EnableVertexAttribArray(1);
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

