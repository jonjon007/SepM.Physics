using System;
using System.IO;
using System.Linq;
using SepM.Serialization;

namespace SepM.Physics{
public class CollisionMatrix : Serial
    {
        public int Checksum => GetHashCode();
        public bool[][] matrix;

        // Every layer is defaulted to collide to every layer
        public CollisionMatrix(){
            int enumLen = Enum.GetNames(typeof(Constants.coll_layers)).Length;
            matrix = new bool[enumLen][]; //Constants.coll_layers length
            for(int i = 0; i < enumLen; i++)
                matrix[i] = Enumerable.Repeat(true, enumLen).ToArray();
        }

        public void SetLayerCollisions(Constants.coll_layers a, Constants.coll_layers b, bool isColl){
            int a_index = (int)a;
            int b_index = (int)b;
            // Set the matrix values
            matrix[a_index][b_index] = isColl;
            matrix[b_index][a_index] = isColl;
        }

        public bool CanLayersCollide(Constants.coll_layers a, Constants.coll_layers b){
            int a_index = (int)a;
            int b_index = (int)b;
            return matrix[a_index][b_index];
        }

        public void Serialize(BinaryWriter bw)
        {
        //matrix
            bw.Write(matrix.Length);
            foreach (bool[] arr in matrix)
            {
                bw.Write(arr.Length);
                foreach(bool b in arr)
                {
                    bw.Write(b);
                }
            }
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //matrix
            int matrix_len = br.ReadInt32();
            // Create a new list if the counts aren't the same
            if (matrix_len != matrix.Length)
            {
                matrix = new bool[matrix_len][];
            }
            // Read down the data for each object
            for (int i = 0; i < matrix_len; i++)
            {
                int arr_len = br.ReadInt32();
                matrix[i] = new bool[arr_len];
                for (int j = 0; j < matrix_len; j++)
                {
                    matrix[i][j] = br.ReadBoolean();
                }
            }

            return this;
        }

        public override int GetHashCode()
        {
            int hashCode = -1214587014;
        //matrix
            foreach (var arr in matrix)
                foreach(var b in arr)
                    hashCode = hashCode * -1521134295 + b.GetHashCode();
            return hashCode;
        }
    }
}