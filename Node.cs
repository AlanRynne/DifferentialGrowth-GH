
using Rhino.Geometry;

namespace DifferentialGrowth
{

    public class Node
    {
        //Properties
        public Point3d position;
        public Vector3d velocity;
        public Vector3d acceleration;
        public double maxForce;
        public double maxSpeed;

        public DifferentialLine owner; // Just to pass the main random instance into the node


        //Constructor
        public Node(double x, double y, double mF, double mS, DifferentialLine diff)
        {
            //Debug.WriteLine("Node: Constructor Called");
            acceleration = new Vector3d(0, 0, 0);
            owner = diff; // Assign owner BEFORE calling Random2D vector since it uses owner.
            velocity = Random2DVector();
            position = new Point3d(x, y, 0);
            maxForce = mF;
            maxSpeed = mS;

        }

        //Methods
        public void ApplyForce(Vector3d force)
        {
            acceleration += force;
        }

        public void Update()
        {
            velocity += acceleration;
            if (velocity.Length > maxSpeed)
            { //Limit size of vector to maxSpeed
                velocity.Unitize();
                velocity *= maxSpeed;
            }
            position += velocity;
            acceleration *= 0;
        }

        public Vector3d Seek(Vector3d target)
        {

            Vector3d desired = target - new Vector3d(position);
            desired.Unitize();
            desired *= maxSpeed;
            Vector3d steer = desired - velocity;
            if (steer.Length > maxForce)
            { //Limit size of vector to maxForce
                steer.Unitize();
                steer *= maxForce;
            }
            return steer;

        }
        //Utility
        public Vector3d Random2DVector() //Takes and returns a Vector3d with Z coordinate ALWAYS 0.
        {
            double lowerBound = -1.0;
            double upperBound = 1.0;
            double x = owner.mainRandom.NextDouble() * (upperBound - lowerBound) + (lowerBound);
            double y = owner.mainRandom.NextDouble() * (upperBound - lowerBound) + (lowerBound);
            Vector3d tempV = new Vector3d(x, y, 0);
            tempV.Unitize();
            return tempV;
        }


    }
}
