//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;

namespace Landis.Extension.Output.WildlifeHabitat
{
    public static class SiteVars
    {
        private static ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts;
        private static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> ageCohorts;

        private static ISiteVar<string> prescriptionName;
        private static ISiteVar<byte> fireSeverity;

        private static ISiteVar<int> yearOfFire;
        private static ISiteVar<int> yearOfHarvest;
        private static ISiteVar<int[]> dominantAge;
        //---------------------------------------------------------------------

        public static void Initialize()
        {
            biomassCohorts = PlugIn.ModelCore.GetSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>("Succession.BiomassCohorts");
            ageCohorts = PlugIn.ModelCore.GetSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>("Succession.AgeCohorts");
            prescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            fireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");

            yearOfFire = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            yearOfHarvest = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            dominantAge = PlugIn.ModelCore.Landscape.NewSiteVar<int[]>();

            if (biomassCohorts == null && ageCohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> BiomassCohorts
        {
            get
            {
                return biomassCohorts;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> AgeCohorts
        {
            get
            {
                return ageCohorts;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<string> PrescriptionName
        {
            get
            {
                return prescriptionName;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> FireSeverity
        {
            get
            {
                return fireSeverity;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> YearOfFire
        {
            get
            {
                return yearOfFire;
            }
            set
            {
                yearOfFire = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> YearOfHarvest
        {
            get
            {
                return yearOfHarvest;
            }
            set
            {
                yearOfHarvest = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int[]> DominantAge
        {
            get
            {
                return dominantAge;
            }
            set
            {
                dominantAge = value;
            }
        }
        //---------------------------------------------------------------------
    }
}
