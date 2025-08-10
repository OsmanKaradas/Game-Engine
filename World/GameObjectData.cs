using OpenTK.Mathematics;

namespace GameEngine.World
{
    public enum Type
    {
        Plane,
        Cube,
        Sphere,
        Pyramid
    }

    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<Vector2> UV;
        public List<uint> Indices;

        public MeshData(List<Vector3> Vertices, List<uint> Indices, List<Vector2> UV)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
            this.UV = UV;
        }
    }
}