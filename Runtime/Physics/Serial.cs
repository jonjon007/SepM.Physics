using System.IO;

namespace SepM.Serialization
{
    public interface Serial
    {
        public void Serialize(BinaryWriter bw);
        // Transforms the Serial in place
        // Returns Serial type so that structs can be reassigned to the result
        public Serial Deserialize<T>(BinaryReader br, T context);
        
        /// <summary>
        /// Gets the deterministic checksum for this serializable state.
        /// This checksum is calculated using Fletcher32 algorithm on the
        /// serialized binary representation of the state.
        /// The checksum should be cached and only recalculated when the state changes.
        /// </summary>
        /// <remarks>
        /// This checksum is used for network desync detection in GGPO.
        /// Unlike GetHashCode(), this checksum is guaranteed to be:
        /// - Deterministic: Same state always produces same checksum
        /// - Persistent: Checksum remains stable across serialization/deserialization
        /// - Network-safe: Independent instances with identical state produce identical checksums
        /// </remarks>
        public int Checksum { get; }
    }
}