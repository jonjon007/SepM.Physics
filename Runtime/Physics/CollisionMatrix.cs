using System.Linq;

namespace SepM.Physics{
public class CollisionMatrix
    {
        public bool[][] matrix;

        // Every layer is defaulted to collide to every layer
        public CollisionMatrix(){
            int enumLen = 7; // TODO: Remove hardcoded number
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
    }
}