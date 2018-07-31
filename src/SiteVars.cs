//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    public static class SiteVars
    {
        private static ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts;
        private static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> ageCohorts;

        private static ISiteVar<string> prescriptionName;
        private static ISiteVar<byte> fireSeverity;

        private static ISiteVar<Dictionary<int, int>> yearOfFire;
        private static ISiteVar<Dictionary<int, int>> yearOfHarvest;
        private static ISiteVar<int[]> dominantAge;
        private static ISiteVar<Dictionary<int, int[]>> forestType;
        private static ISiteVar<Dictionary<int, double>> suitabilityValue;
        private static ISiteVar<Dictionary<int, int>> ageAtFireYear;
        private static ISiteVar<Dictionary<int, int>> ageAtHarvestYear;
        private static ISiteVar<Dictionary<int, double>> suitabilityWeight;
        private static ISiteVar<Dictionary<int, int>> forestTypeAtFireYear;
        private static ISiteVar<Dictionary<int, int>> forestTypeAtHarvestYear;
        private static ISiteVar<int> timeOfLastHarvest;
        private static ISiteVar<int> timeOfLastFire;



        

        //---------------------------------------------------------------------

        public static void Initialize(List<ISuitabilityParameters> suitabilityParameters)
        {
            biomassCohorts = PlugIn.ModelCore.GetSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>("Succession.BiomassCohorts");
            ageCohorts = PlugIn.ModelCore.GetSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>("Succession.AgeCohorts");
            prescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            timeOfLastHarvest = PlugIn.ModelCore.GetSiteVar<int>("Harvest.TimeOfLastEvent");
            fireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
            timeOfLastFire = PlugIn.ModelCore.GetSiteVar<int>("Fire.TimeOfLastEvent");

            yearOfFire = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int,int>>();
            ageAtFireYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            yearOfHarvest = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            ageAtHarvestYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            dominantAge = PlugIn.ModelCore.Landscape.NewSiteVar<int[]>();
            forestType = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int[]>>();
            forestTypeAtFireYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            forestTypeAtHarvestYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            suitabilityValue = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, double>>();
            suitabilityWeight = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, double>>();


            if (biomassCohorts == null && ageCohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }

            foreach (Site site in PlugIn.ModelCore.Landscape.ActiveSites)
            {
                Dictionary<int, int> yofDict = new Dictionary<int, int>();
                Dictionary<int, int> ageFireDict = new Dictionary<int, int>();
                Dictionary<int, int> yohDict = new Dictionary<int, int>();
                Dictionary<int, int> ageHarvestDict = new Dictionary<int, int>();
                

                int[] domAgeArray = new int[2];
                SiteVars.DominantAge[site] = domAgeArray;

                Dictionary<int, int> forestTypeFireDict = new Dictionary<int, int>();
                Dictionary<int, int> forestTypeHarvestDict = new Dictionary<int, int>();
                Dictionary<int, double> suitValDict = new Dictionary<int, double>();
                Dictionary<int, double> suitWtDict = new Dictionary<int, double>();
                Dictionary<int, int[]> forTypeDict = new Dictionary<int, int[]>();

                for (int index = 0; index < suitabilityParameters.Count; index++)
                {
                    suitValDict.Add(index, 0.0);
                    suitWtDict.Add(index, 0.0);

                    int[] forTypeArray = new int[2];

                    if (suitabilityParameters[index].SuitabilityType == "AgeClass_ForestType")
                    {
                        forTypeDict.Add(index, forTypeArray);
                        SiteVars.ForestType[site] = forTypeDict;
                    }
                    else if (suitabilityParameters[index].SuitabilityType == "AgeClass_TimeSinceDisturbance")
                    {
                        if(suitabilityParameters[index].DisturbanceType == "Fire")
                        {
                            yofDict.Add(index, -99999);
                            SiteVars.YearOfFire[site] = yofDict;
                            ageFireDict.Add(index, -99999);
                            SiteVars.AgeAtFireYear[site] = ageFireDict;
                        }
                        else if (suitabilityParameters[index].DisturbanceType == "Harvest")
                        {
                            yohDict.Add(index, -99999);
                            SiteVars.YearOfHarvest[site] = yohDict;
                            ageHarvestDict.Add(index, -99999);
                            SiteVars.AgeAtHarvestYear[site] = ageHarvestDict;
                        }
                    }
                    else if (suitabilityParameters[index].SuitabilityType == "ForestType_TimeSinceDisturbance")
                    {
                        if (suitabilityParameters[index].DisturbanceType == "Fire")
                        {
                            SiteVars.ForestType[site] = forTypeDict;
                            yofDict.Add(index, -99999);
                            SiteVars.YearOfFire[site] = yofDict;
                            forestTypeFireDict.Add(index, -99999);
                            SiteVars.ForestTypeAtHarvestYear[site] = forestTypeHarvestDict;
                        }
                        else if (suitabilityParameters[index].DisturbanceType == "Harvest")
                        {
                            SiteVars.ForestType[site] = forTypeDict;
                            yohDict.Add(index, -99999);
                            SiteVars.YearOfHarvest[site] = yohDict;
                            forestTypeHarvestDict.Add(index, -99999);
                            SiteVars.ForestTypeAtFireYear[site] = forestTypeFireDict;
                        }
                    }
                                        
                }

                SiteVars.SuitabilityWeight[site] = suitWtDict;
                SiteVars.SuitabilityValue[site] = suitValDict;
                //SiteVars.ForestTypeAtHarvestYear[site] = forestTypeHarvestDict;
                //SiteVars.ForestTypeAtFireYear[site] = forestTypeFireDict;
                //SiteVars.AgeAtHarvestYear[site] = ageHarvestDict;
                //SiteVars.YearOfHarvest[site] = yohDict;
                //SiteVars.AgeAtFireYear[site] = ageFireDict;
                //SiteVars.YearOfFire[site] = yofDict;
                //SiteVars.ForestType[site] = forTypeDict;
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
        public static ISiteVar<int> TimeOfLastHarvest
        {
            get
            {
                return timeOfLastHarvest;
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
        public static ISiteVar<int> TimeOfLastFire
        {
            get
            {
                return timeOfLastFire;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, int>> YearOfFire
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
        public static ISiteVar<Dictionary<int, int>> AgeAtFireYear
        {
            get
            {
                return ageAtFireYear;
            }
            set
            {
                ageAtFireYear = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, int>> YearOfHarvest
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
        public static ISiteVar<Dictionary<int, int>> AgeAtHarvestYear
        {
            get
            {
                return ageAtHarvestYear;
            }
            set
            {
                ageAtHarvestYear = value;
            }
        }
        //---------------------------------------------------------------------
        // Dictionary with key equal the index of the suitability file and 
        // value equal to an array of this year's and last year's dominant age class
        // [0] is this year, [1] is last year
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
        // Dictionary with key equal the index of the suitability file and 
        // value equal to an array of this year's and last year's dominant forest type
        // [0] is this year, [1] is last year
        public static ISiteVar<Dictionary<int, int[]>> ForestType
        {
            get
            {
                return forestType;
            }
            set
            {
                forestType = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, int>> ForestTypeAtFireYear
        {
            get
            {
                return forestTypeAtFireYear;
            }
            set
            {
                forestTypeAtFireYear = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, int>> ForestTypeAtHarvestYear
        {
            get
            {
                return forestTypeAtHarvestYear;
            }
            set
            {
                forestTypeAtHarvestYear = value;
            }
        }
        //---------------------------------------------------------------------
        // Dictionary with key equal the index of the suitability file and 
        // value equal to the calculated suitability
        public static ISiteVar<Dictionary<int, double>> SuitabilityValue
        {
            get
            {
                return suitabilityValue;
            }
            set
            {
                suitabilityValue = value;
            }
        }
        //---------------------------------------------------------------------
        // Dictionary with key equal the index of the suitability file and 
        // value equal to the suitability weight
        public static ISiteVar<Dictionary<int, double>> SuitabilityWeight
        {
            get
            {
                return suitabilityWeight;
            }
            set
            {
                suitabilityWeight = value;
            }
        }
        //---------------------------------------------------------------------
    }
}
