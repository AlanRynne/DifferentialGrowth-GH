using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace LeafVenationGrowth
{
    public class LeafVenationGrowthInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "LeafVenationGrowth Info";
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
                return new Guid("88ce8297-55b6-4d80-b2aa-eb592441d960");
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
