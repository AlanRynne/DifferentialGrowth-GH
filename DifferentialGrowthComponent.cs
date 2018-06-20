using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.IO;
using System.Resources;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using DifferentialGrowth;

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
            "Alan", "Growth")
        {
            // Initialize any Class level variable here.
            HasFinishedRunning = false;

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
                                         "Reset",
                                         "If true, previous results will be erased",
                                         GH_ParamAccess.item,
                                         false);
            
            pManager.AddBooleanParameter("Run", 
                                         "Run Component", 
                                         "Set to TRUE to start running", 
                                         GH_ParamAccess.item, 
                                         false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //Add Output parameters
            pManager.AddNumberParameter("Iterations",
                                       "i",
                                       "Current Iterations",
                                       GH_ParamAccess.tree);
            pManager.AddPointParameter("Final Points",
                                       "P",
                                       "List of points corresponding to the last iteration",
                                       GH_ParamAccess.list);
            pManager.AddLineParameter("Final Lines",
                                       "L",
                                       "List of lines corresponding to the last iteration",
                                       GH_ParamAccess.list);
        }


        // START - CUSTOM Component Fields

        DifferentialLine _diff_line = null; //DiffLine lives outside solveinstance to avoid re-instantiation
        int actualRuns = 0; // Store the ammount of actual runs of the diffline
        bool resetComponent; // When true, erase previous results and reset _diff_line
        DataTree<Line> runResults = new DataTree<Line>();
        bool runComponent; // When true, start timer to add +1 to actualRuns. It will allow for infinite runs, one at a time. "Kangaroo solver style"
        bool diffLineHasFinishedRunning; // TRUE if current _diffLine.Run() call has ended. FALSE on start.

        // Public properties

        /// TODO:
        /// This was copied directly from the GH Component guides! must be changed to switch between
        /// 3 strings "Initial", "Running" and "Converged". Aditionally, it would be wise to add one more
        /// states to the Message: "Paused" when runComponent is false.

        public bool HasFinishedRunning
        {
            get { return diffLineHasFinishedRunning; }
            set
            {
                diffLineHasFinishedRunning = value;
                if ((diffLineHasFinishedRunning))
                {
                    Message = "Stopped";
                }
                else
                {
                    Message = "Running";
                }
            }
        }


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

            if (!DA.GetData(7, ref runComponent)) return;


            // ---------------- VALUE CHECKING ----------------

            if (!runComponent)
            {
                HasFinishedRunning = true;
            } else {
                HasFinishedRunning = false;
            }

            // Check for reset
            if (resetComponent) 
            { //Erase previous results and null _diff_line
                runResults = new DataTree<Line>();
                _diff_line = null;
                actualRuns = 0;
            }

            //// Check if iterations has increased more than actual runs.
            //if (runIterations >= actualRuns)
            //{
            //    runIterations -= actualRuns;
            //}
            //else
            //{
            //    runIterations = 0;
            //}

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

                    _diff_line.AddNode(new DifferentialNode(tempX,
                                                tempY,
                                                _diff_line.maxForce,
                                                _diff_line.maxSpeed,
                                                _diff_line));
                }
            }


            // ----------------------------- RUN DIFFERENTIAL LINE ------------------------------

            if (runComponent)
            {
                // Check if DifferentialLine hasn't run
                if (actualRuns == 0)
                { //OUTPUT Initial Curve
                    runResults.AddRange(_diff_line.RenderLine(), new Grasshopper.Kernel.Data.GH_Path(0));
                }

                if (actualRuns < runIterations)
                {
                    _diff_line.Run(); // Run diff line once
                    actualRuns++; //Add +1 to actualRuns, component class value holder.
                } else {
                    HasFinishedRunning = true;
                }


                //Add iteration result to component class tree

                // ----------------------------- SET OUTPUT DATA ------------------------------------


                DA.SetData(0, actualRuns);
                DA.SetDataList(2, _diff_line.RenderLine());
            }

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
                //Resources.IconForThisComponent;
                return Properties.Resources.DifferentialLineICON;
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
}


