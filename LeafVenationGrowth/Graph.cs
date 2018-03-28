using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace LeafVenationGrowth
{
    public class Graph
    {
        // Private variables
        private PointCloud _data;
        private List<Vertex> _vertices;
        private List<Line> _edges;

        // Public properties
        public PointCloud Data
        {
            get { return _data; }
        }
        public List<Vertex> Vertices
        {
            get { return _vertices; }
        }
        public List<Line> Edges 
        {
            get { return _edges; }
        }

        // Constructor
        public Graph()
        {
            _data = new PointCloud();
            _vertices = new List<Vertex>();
            _edges = new List<Line>();
        }

        // Methods
        public void AddNewVertexToClosest(Point3d data)
        {
            Vertex parent = FindClosestVertexFrom(data);
            Vertex newVert = new Vertex(data, parent);

            _data.Add(newVert.position);
            _vertices.Add(newVert);
            //_edges.Add(new Line(parent.position,newVert.position));
        }

        private Vertex FindClosestVertexFrom(Point3d position)
        {
            if (_vertices.Count > 0) {
                int i = _data.ClosestPoint(position);
                return _vertices[i];
            }
            else {
                return null;
            }

        }

        public List<double> CalculateVertexWeigth()
        {
            // Create a list to hold the vertex weight values
            List<double> VertexWeights = new List<double>(_vertices.Count);

            //Call first node weight, which will call every other child to calculate it's own weight.
            double firstWeight = _vertices[0].Weight;

            //Once weights have been calculated, iterate all vertices and add it's weight to the weight list.
            foreach (Vertex vert in _vertices) VertexWeights.Add(vert.Weight);

            //Return the list containing the weight values
            return VertexWeights;
        }
    }

    public class Vertex
    {
        private readonly double MINIMUM_WEIGHT = 1.0;

        private double _weight;
        private bool weightWasCalculated;

        public Vertex parent;
        public List<Vertex> children;
        public Point3d position;

        public double Weight 
        {
            get {
                //if (!weightWasCalculated){
                //    CalculateWeight();
                //}
                return _weight;
            }
        }

        // Constructor
        public Vertex(Point3d pos, Vertex par)
        {
            position = pos;
            _weight = 0.0;
            if (par != null) {
                parent = par;
                parent.children.Add(this);
            }
            children = new List<Vertex>();
            weightWasCalculated = false;
        }

        private void CalculateWeight()
        {
            // If weight hasn't been calculated and vertex has children
            if (!weightWasCalculated && children.Count > 0) 
            {
                foreach (Vertex v in children) 
                {
                    double tWeight = v.Weight;
                    if (tWeight > 0) { _weight += v.Weight; } // If child weight is not 0, add to vertex weight
                    else { _weight += MINIMUM_WEIGHT; } // Else -> Add minimum weight value
                }
                Console.WriteLine("Vertex weight was updated with {0} children weights",children.Count);
            } 

            // If weight hasn't been calculated and vertex has NO children
            else if (!weightWasCalculated && children.Count == 0)
            {
                _weight = 0.0; // Return minimum weight
                Console.WriteLine("Vertex weight is MINIMIMAL");
            }

            // If vertex weight had already been calculated or was unchanged after update
            else if (weightWasCalculated) 
            {
                Console.WriteLine("Vertex weight was unchanged");
            }

            weightWasCalculated = true; //Set bool property to true after calculations;
        }

    }

}
