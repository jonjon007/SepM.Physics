using System.IO;

namespace SepM.Serialization
{
    public interface Serial
    {
        public void Serialize(BinaryWriter bw);
        // Transforms the Serial in place
        // Returns Serial type so that structs can be reassigned to the result
        public Serial Deserialize(BinaryReader br);
    }
}