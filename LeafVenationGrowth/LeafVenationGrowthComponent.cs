using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace LeafVenationGrowth
{
    public class LeafVenationGrowthComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LeafVenationGrowthComponent()
          : base("LeafVenationGrowth", "LeafVen",
            "LeafVenationGrowth Algorithm Implementation",
            "Alan", "Growth")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Mandatory parameters
            pManager.AddCurveParameter("Boundary","B","Initial leaf boundary edge curve",GH_ParamAccess.item);
            pManager.AddNumberParameter("Start Point","P","Inivital leaf venation node",GH_ParamAccess.item);

            // Optional parameters
            pManager.AddNumberParameter("Marginal Growth", "mG", "Marginal growth factor", GH_ParamAccess.item, 1.00);
            pManager.AddNumberParameter("Uniform Growth", "uG", "Uniform growth factor",GH_ParamAccess.item, 1.00);
            pManager.AddNumberParameter("Auxin Birth Distance", "dA", "Auxin/Auxin Birth Distance", GH_ParamAccess.item,1.00);
            pManager.AddNumberParameter("Vein Birth Distance", "dV", "New vein node creation distance", GH_ParamAccess.item,1.00);

            // Control Paramteres
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[1].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("LeafVeins","lV","Resulting Leaf Veins",GH_ParamAccess.list);
            pManager.AddPointParameter("LeafNodes","lN","Resulting Leaf Nodes", GH_ParamAccess.list);

        }
        // Class level variables

        private Graph veinGraph;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            veinGraph = new Graph();

            veinGraph.AddNewVertexToClosest(new Point3d(0,0,0));
            Point3d x = new Point3d(1, 0, 0);

            veinGraph.AddNewVertexToClosest(x);
            Point3d y = new Point3d(2, 0, 0);
            veinGraph.AddNewVertexToClosest(y);
            Point3d z = new Point3d(0.1, 0, 0);
            veinGraph.AddNewVertexToClosest(z);

            Console.WriteLine("Testing LIVE");

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
            get { return new Guid("43c6f1d1-6633-4d81-8f13-530096cc2d80"); }
        }
    }
}
