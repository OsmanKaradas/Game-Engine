using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
{
    public class VBO
    {
        public int ID;
        public VBO(List<float> data)
        {
            ID = GenBuffer();
            Bind();
            BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(), BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            BindBuffer(BufferTarget.ArrayBuffer, ID);
        }
        public void Unbind()
        {
            BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            DeleteBuffer(ID);
        }
    }
}

