using System.IO;

namespace SepM.Physics
{
    public interface Serial
    {
        public void Serialize(BinaryWriter bw);
        public void Deserialize(BinaryReader br);
    }
}