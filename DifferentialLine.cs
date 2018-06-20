using System;
using System.Collections.Generic;
using System.Diagnostics;

using Rhino.Geometry;

namespace DifferentialGrowth
{
    public class DifferentialLine
    {
        // Public Fields
        public List<DifferentialNode> nodes;
        public double maxForce;
        public double maxSpeed;
        public double desiredSeparation;
        public double sq_desiredSeparation;
        public double separationCohesionRatio;
        public double maxEdgeLength;
        public Random mainRandom;

        // Constructor
        public DifferentialLine(double mF,
                                double mS,
                                double dS,
                                double sCr,
                                double eL)
        {
            Debug.WriteLine("DiffLine: Constructor Called");
            nodes = new List<DifferentialNode>();
            maxForce = mF;
            maxSpeed = mS;
            desiredSeparation = dS;
            sq_desiredSeparation = Math.Pow(desiredSeparation, 2);
            separationCohesionRatio = sCr;
            maxEdgeLength = eL;
            mainRandom = new Random();
        }

        // Methods

        public void Run()
        {
            Debug.Print("DiffLine: Run() Method Called");
            this.Differentiate();
            this.Growth();
        }

        public void AddNode(DifferentialNode n)
        {
            //Debug.WriteLine("DiffLine: AddNode() Method Called");
            nodes.Add(n);
        }

        public void AddNodeAt(DifferentialNode n, int index)
        {
            //Debug.WriteLine("DiffLine: AddNodeAt({0}) Method Called", index);
            nodes.Insert(index, n);
        }

        public void Growth()
        {
            Debug.WriteLine("DiffLine: Growth() Method Called");

            //List<Node> tempNodes = new List<Node>(nodes);

            for (int i = 0; i < nodes.Count - 1; i++) //Iterate all points
            {
                DifferentialNode n1 = nodes[i];
                DifferentialNode n2 = nodes[i + 1];
                double d = n1.position.DistanceTo(n2.position); //Get distance

                //Basic growth rule
                if (d > maxEdgeLength)
                {
                    int index = nodes.IndexOf(n2);
                    Point3d middleNodePosition = (n1.position + n2.position) / 2;
                    nodes.Insert(index,
                                     new DifferentialNode(middleNodePosition.X,
                                              middleNodePosition.Y,
                                              maxForce,
                                              maxSpeed,
                                              this)
                                    );
                }

            }
            //nodes = tempNodes;
        }

        public void Differentiate()
        {
            Debug.WriteLine("DiffLine: Differentiate() Method Called");
            List<Vector3d> separationForces = GetSeparationForces();
            List<Vector3d> cohesionForces = GetEdgeCohesionForces();

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3d separation = separationForces[i];
                Vector3d cohesion = cohesionForces[i];

                separation *= separationCohesionRatio;

                nodes[i].ApplyForce(separation);
                nodes[i].ApplyForce(cohesion);
                nodes[i].Update();
            }
        }

        public List<Vector3d> GetSeparationForces()
        {
            Debug.WriteLine("DiffLine: GetSeparationForces() Method Called");
            int n = nodes.Count;
            List<Vector3d> separateForces = new List<Vector3d>(n);
            List<int> nearNodes = new List<int>(n);

            DifferentialNode nodei;
            DifferentialNode nodej;

            for (int w = 0; w < n; w++)
            {
                separateForces.Add(new Vector3d(0, 0, 0));
                nearNodes.Add(0);
            }

            for (int i = 0; i < n; i++)
            {
                nodei = nodes[i];

                //Old internal loop
                for (int j = i + 1; j < n; j++)
                {
                    nodej = nodes[j];

                    Vector3d forceij = GetSeparationForce(nodei, nodej);

                    if (forceij.Length > 0)
                    {
                        separateForces[i] += forceij;
                        separateForces[j] -= forceij;
                        nearNodes[i]++;
                        nearNodes[j]++;
                    }
                }
                // End of old internal loop
                if (nearNodes[i] > 0)
                {
                    separateForces[i] /= nearNodes[i];
                }


                if (separateForces[i].Length > 0)
                {
                    //Set vector length to maxSpeed
                    separateForces[i].Unitize();
                    separateForces[i] *= maxSpeed;
                    //Substract velocity
                    separateForces[i] -= nodes[i].velocity;
                    //Limit size of vector to maxForce
                    if (separateForces[i].Length > maxForce)
                    {
                        separateForces[i].Unitize();
                        separateForces[i] *= maxForce;
                    }
                }

            }

            return separateForces;
        }

        public Vector3d GetSeparationForce(DifferentialNode n1, DifferentialNode n2)
        {
            //Debug.WriteLine("DiffLine: GetSeparationForce() Method Called");

            Vector3d steer = new Vector3d(0, 0, 0);
            double sq_d = Math.Pow(n1.position.X - n2.position.X, 2)
                              + Math.Pow(n1.position.Y - n2.position.Y, 2);

            if (sq_d > 0 && sq_d < sq_desiredSeparation)
            {

                Vector3d diff = new Vector3d(n1.position - n2.position);
                diff.Unitize();
                diff /= Math.Sqrt(sq_d);
                steer += diff;
            }
            return steer;
        }

        public List<Vector3d> GetEdgeCohesionForces()
        {
            Debug.WriteLine("DiffLine: GetEdgeCohesionForces() Method Called");

            int n = nodes.Count;
            List<Vector3d> cohesionForces = new List<Vector3d>(n);

            for (int w = 0; w < n; w++)
            {
                cohesionForces.Insert(w, new Vector3d(0, 0, 0));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3d sum = new Vector3d(0, 0, 0);
                Point3d n1;
                Point3d n2;

                if (i != 0 && i != nodes.Count - 1)
                { // Middle items
                    n1 = nodes[i - 1].position;
                    n2 = nodes[i + 1].position;
                    sum += new Vector3d(n1);
                    sum += new Vector3d(n2);
                }
                else if (i == 0)
                { // First item in list
                    n1 = nodes[nodes.Count - 1].position;
                    n2 = nodes[i + 1].position;
                    sum += new Vector3d(n1);
                    sum += new Vector3d(n2);
                }
                else if (i == nodes.Count - 1)
                { // Last item in list
                    n1 = nodes[i - 1].position;
                    n2 = nodes[0].position;
                    sum += new Vector3d(n1);
                    sum += new Vector3d(n2);
                }
                sum /= 2;
                cohesionForces[i] = nodes[i].Seek(sum);
            }
            return cohesionForces;
        }

        //Not shure if i have to implement this ones here or re do them in the main script.

        //public void RenderShape(){} //This method is just to color the inside of the curve

        public List<Line> RenderLine()
        {
            Debug.WriteLine("DiffLine: RenderLine() Method Called");
            List<Line> tempLines = new List<Line>();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                tempLines.Add(new Line(nodes[i].position, nodes[i + 1].position));
                if (i == nodes.Count - 2)
                { // LAST ITERATION - Add
                    tempLines.Add(new Line(nodes[i + 1].position, nodes[0].position));
                }
            }
            return tempLines;
        }

        // ------------------ END OF DIFFERENTIAL LINE CLASS ----------------------------
    }
}
