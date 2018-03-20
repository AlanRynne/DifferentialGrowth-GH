using System;
using System.Collections.Generic;
using System.Diagnostics;

using Grasshopper;
using Grasshopper.Documentation;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DifferentialGrowth
{
    public class DifferentialGrowthComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DifferentialGrowthComponent()
          : base("Differential-Growth", "DiffGrth",
            "Differential Growth 2D Algorithm",
            "Alan", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //Add input parameters
            pManager.AddIntegerParameter("Iterations", 
                                         "i", 
                                         "Number of iterations to run", 
                                         GH_ParamAccess.item, 
                                         0);
            pManager.AddNumberParameter("Max Force", 
                                        "Fmax", 
                                        "Maximum allowed force", 
                                        GH_ParamAccess.item, 
                                        0.15);
            pManager.AddNumberParameter("Max Speed", 
                                        "Smax", 
                                        "Maximum allowed speed", 
                                        GH_ParamAccess.item, 
                                        2);
            pManager.AddNumberParameter("Desired Separation", 
                                        "D", 
                                        "Desired separation", 
                                        GH_ParamAccess.item, 
                                        10);
            pManager.AddNumberParameter("Separation/Cohesion Ratio",
                                        "SDR", 
                                        "SDR", 
                                        GH_ParamAccess.item, 
                                        11);
            pManager.AddNumberParameter("Max Edge Length", 
                                        "Lmax", 
                                        "Maximum Edge Length", 
                                        GH_ParamAccess.item, 
                                        5);
            pManager.AddBooleanParameter("RESET",
                                         "reset",
                                         "If true, previous results will be erased",
                                         GH_ParamAccess.item,
                                         false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //Add Output parameters
            pManager.AddCurveParameter("Results",
                                       "R",
                                       "Resulting curves of differential growth process",
                                       GH_ParamAccess.tree);
            pManager.AddPointParameter("Final Points",
                                       "P",
                                       "List of points corresponding to the last iteration",
                                       GH_ParamAccess.list);
            pManager.AddPointParameter("Final Lines",
                                       "L",
                                       "List of lines corresponding to the last iteration",
                                       GH_ParamAccess.list);
        }

        // START - CUSTOM Component Fields

        DifferentialLine _diff_line = null; //DiffLine lives outside solveinstance to avoid re-instantiation
        int actualRuns = 0; // Store the ammount of actual runs of the diffline
        bool resetComponent; // When true, erase previous results and reset _diff_line
        DataTree<Line> runResults = new DataTree<Line>();

        // FIELDS TO BE IMPLEMENTED YET INSIDE THE CODE!!!!!! (Brief description of what they should do)

        bool runComponent; // When true, start timer to add +1 to actualRuns. It will allow for infinite runs, one at a time. "Kangaroo solver style"

        bool diffLineHasFinishedRunning; // TRUE if current _diffLine.Run() call has ended. FALSE on start.

        double minimumRunDuration = 1; // IN SECONDS!! Don't know if it is the correct value or it should be changed to an actual instance of gh_time.

        GH_Time time = new GH_Time(); // Timer module or connection to GH timer. Should +1 ACTUALRUNS when time per run has been reached AND solution has ended.

        // Custom message on component when running like K2. See tutorials and GH Forum, solution is there.


        //END - CUSTOM Component Fields


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ----------------- SET FIELD VALUES ---------------

            int runIterations = 0;
            if (!DA.GetData(0, ref runIterations)) return;

            double _maxForce = 0.15; // Maximum steering force
            if (!DA.GetData(1, ref _maxForce)) return;

            double _maxSpeed = 2; // Maximum speed
            if (!DA.GetData(2, ref _maxSpeed)) return;

            double _desiredSeparation = 10; 
            if (!DA.GetData(3, ref _desiredSeparation)) return;

            double _separationCohesionRatio = 11;
            if (!DA.GetData(4, ref _separationCohesionRatio)) return;

            double _maxEdgeLength = 10;
            if (!DA.GetData(5, ref _maxEdgeLength)) return;


            if (!DA.GetData(6, ref resetComponent)) return;

            // ---------------- VALUE CHECKING ----------------

            // Check for reset
            if (resetComponent) 
            { //Erase previous results and null _diff_line
                runResults = new DataTree<Line>();
                _diff_line = null;
                actualRuns = 0;
            }

            // Check if iterations has increased more than actual runs.

            if (runIterations >= actualRuns)
            {
                runIterations -= actualRuns;
            }
            else
            {
                runIterations = 0;
            }
            // Check if DifferentialLine has been instantiated
            if (_diff_line == null)
            {
                //Setup diff_line with starting points
                _diff_line =
                    new DifferentialLine(_maxForce,
                                        _maxSpeed,
                                        _desiredSeparation,
                                        _separationCohesionRatio,
                                        _maxEdgeLength);
                double nodeStart = 20;
                double angInc = 2 * Math.PI / nodeStart;
                double rayStart = 10;

                for (double a = 0; a < 2 * Math.PI; a += angInc)
               { // Create new Nodes
                    double tempX = 0 + Math.Cos(a) * rayStart;
                    double tempY = 0 + Math.Sin(a) * rayStart;

                    _diff_line.AddNode(new Node(tempX,
                                                tempY,
                                                _diff_line.maxForce,
                                                _diff_line.maxSpeed,
                                                _diff_line));
                }
            }






            // ----------------------------- RUN DIFFERENTIAL LINE ------------------------------

            // Check if DifferentialLine hasn't run
            if (actualRuns == 0)
            { //OUTPUT Initial Curve
                runResults.AddRange(_diff_line.RenderLine(), new Grasshopper.Kernel.Data.GH_Path(0));
            }
            // Run differential growth loop
            for (int i = 0; i < runIterations; i++)
            {
                _diff_line.Run(); // Run diff line once
                actualRuns++; //Add +1 to actualRuns, component class value holder.

                //Add iteration result to component class tree
                runResults.AddRange(_diff_line.RenderLine(), new Grasshopper.Kernel.Data.GH_Path(actualRuns));
            }



            // ----------------------------- SET OUTPUT DATA ------------------------------------


            DA.SetDataTree(0,runResults);

        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e1d967e7-45b1-40b1-a7a2-dc374db78a10"); }
        }

    }

    public class DifferentialLine
    {
        // Public Fields
        public List<Node> nodes;
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
            nodes = new List<Node>();
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

        public void AddNode(Node n)
        {
            //Debug.WriteLine("DiffLine: AddNode() Method Called");
            nodes.Add(n);
        }

        public void AddNodeAt(Node n, int index)
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
                Node n1 = nodes[i];
                Node n2 = nodes[i + 1];
                double d = n1.position.DistanceTo(n2.position); //Get distance

                //Basic growth rule
                if (d > maxEdgeLength)
                {
                    int index = nodes.IndexOf(n2);
                    Point3d middleNodePosition = (n1.position + n2.position) / 2;
                    nodes.Insert(index, 
                                     new Node(middleNodePosition.X,
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

            Node nodei;
            Node nodej;

            for (int w = 0; w < n; w++)
            {
                separateForces.Add(new Vector3d(0, 0, 0));
                nearNodes.Add(0);
            }

            for (int i = 0; i < n; i++)
            {
                nodei = nodes[i];

                //Old internal loop
                for (int j = i+1; j < n; j++)
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

        public Vector3d GetSeparationForce(Node n1, Node n2)
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

    // </Custom additional code> 
}


