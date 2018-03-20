using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace DifferentialGrowth
{
    public class DifferentialGrowthInfo : GH_AssemblyInfo
  {
    public override string Name
    {
        get
        {
            return "DifferentialGrowth Info";
        }
    }
    public override Bitmap Icon
    {
        get
        {
            //Return a 24x24 pixel bitmap to represent this GHA library.
            return null;
        }
    }
    public override string Description
    {
        get
        {
            //Return a short string describing the purpose of this GHA library.
            return "";
        }
    }
    public override Guid Id
    {
        get
        {
            return new Guid("ed885860-f3fe-407a-a4f0-1e77b5e38c5d");
        }
    }

    public override string AuthorName
    {
        get
        {
            //Return a string identifying you or your company.
            return "";
        }
    }
    public override string AuthorContact
    {
        get
        {
            //Return a string representing your preferred contact details.
            return "";
        }
    }
}
}
