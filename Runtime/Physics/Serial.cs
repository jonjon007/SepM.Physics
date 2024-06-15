using System.IO;

namespace SepM.Serialization
{
    public interface Serial
    {
        public void Serialize(BinaryWriter bw);
        public void Deserialize(BinaryReader br);
    }
}