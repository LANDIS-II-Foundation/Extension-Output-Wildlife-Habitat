//  Authors:  Brian R. Miranda, Donald J. Brown

using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System;

namespace Landis.Extension.Output.WildlifeHabitat
{
    public class PlugIn
        : ExtensionMain
    {

        public static readonly  new ExtensionType Type = new ExtensionType("output");
        public static readonly string ExtensionName =  "Wildlife Habitat Output";

        private string mapNameTemplate;
        private List<string> suitabilityFiles;
        private List<ISuitabilityParameters> suitabilityParameters;

        private static IInputParameters parameters;
        private static ICore modelCore;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, Type)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParametersParser.SpeciesDataset = modelCore.Species;
            InputParametersParser parser = new InputParametersParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the component with a data file.
        /// </summary>
        public override void Initialize()
        {

            Timestep = parameters.Timestep;

            this.mapNameTemplate = parameters.MapFileNames;
            this.suitabilityFiles = parameters.SuitabilityFiles;
            this.suitabilityParameters = parameters.SuitabilityParameters;
            SiteVars.Initialize(this.suitabilityParameters);
            

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Runs the component for a particular timestep.
        /// </summary>
        /// <param name="currentTime">
        /// The current model timestep.
        /// </param>
        public override void Run()
        {
            // The index variable increments with each suitability file - an index of multiple wildlife input files
            int index = 0;
            foreach (string suitabilityFile in suitabilityFiles)
            {
                // get suitability parameters
                ISuitabilityParameters mySuitabilityParameters = suitabilityParameters[index];

                foreach (Site site in modelCore.Landscape.ActiveSites)
                {
                    double suitabilityValue = 0;
                    // calculate dominant age as site variable (DomAge), store for retrieval
                    //  Note: DomAge site variable is an array of values giving dominant age for this year and last year
                    UpdateDominantAge(site);

                    // calculate forest type as site variable (ForestType), store for retreival
                    //  Note: ForestType site variable is an array of values giving the index of forest type for this year and last year
                    UpdateForestType(index, mySuitabilityParameters, site);

                    // depending on suitability type, calculate final suitability value

                    if (mySuitabilityParameters.SuitabilityType == "AgeClass_ForestType")
                    {
                        //   get forest type (calculated above with UpdateForestType)
                        int currentForestType = SiteVars.ForestType[site][index][0];

                        if (currentForestType > 0)
                        {
                            IForestType forestType = mySuitabilityParameters.ForestTypes[0].ForestTypes[currentForestType - 1];
                            //   calculate dominant age among species in forest type
                            int domFTAge = CalculateDomAgeForestType(site, forestType);
                            //   look up suitability in suitabilityTable for combination of forest type and age
                            Dictionary<int, double> suitabilityRow = mySuitabilityParameters.Suitabilities[forestType.Name];

                            foreach (KeyValuePair<int, double> item in suitabilityRow)
                            {
                                int ageLimit = item.Key;
                                if (domFTAge <= ageLimit)
                                {
                                    suitabilityValue = item.Value;
                                    break;
                                }
                            }
                        }
                        else
                            suitabilityValue = 0;
                        //  write sitevar for suitability value
                        SiteVars.SuitabilityValue[site][index] = suitabilityValue;
                    }
                    // if suitabilityType == AgeClass_TimeSinceDisturbance:
                    else if (mySuitabilityParameters.SuitabilityType == "AgeClass_TimeSinceDisturbance")
                    {
                        int ageAtDisturbanceYear = 0;
                        int timeSinceDisturbance = 0;
                        double suitabilityWeight = 0.0;
                        // if disturbanceType == "Fire" then:
                        if (mySuitabilityParameters.DisturbanceType == "Fire")
                        {
                            // Check if fire severity output exists
                            if (SiteVars.FireSeverity == null)
                            {
                                string mesg = string.Format("The DisturbanceType is Fire, but FireSeverity SiteVariable is not defined.  Please double-check that a fire extension is running.");
                                throw new System.ApplicationException(mesg);
                            }
                            else
                            {
                                //   Check this year fire severity
                                int currentFireSeverity = (int)SiteVars.FireSeverity[site];
                                int timeofLastFire = SiteVars.TimeOfLastFire[site];
                                //   if > 0 then update sites with new values
                                if (currentFireSeverity > 0 && timeofLastFire == ModelCore.CurrentTime)
                                {
                                    //      translate to suitability weight
                                    suitabilityWeight = mySuitabilityParameters.FireSeverities[currentFireSeverity];

                                    SiteVars.SuitabilityWeight[site][index] = suitabilityWeight;

                                    //      if suitability weight > 0 then
                                    if (suitabilityWeight > 0)
                                    {
                                        //        store sitevar YearOfFire by index
                                        SiteVars.YearOfFire[site][index] = ModelCore.CurrentTime;
                                        //        read previous year dominant age
                                        int prevYearDomAge = SiteVars.DominantAge[site][1];
                                        //        store sitevar AgeAtFireYear by index
                                        SiteVars.AgeAtFireYear[site][index] = prevYearDomAge;
                                    }
                                }
                                //  read sitevar AgeAtDisturbanceYear for age value
                                ageAtDisturbanceYear = SiteVars.AgeAtFireYear[site][index];
                                //  read sitevar YearOfFire
                                int yearOfFire = SiteVars.YearOfFire[site][index];
                                //  calculate timeSinceDisturbance = currentYear - YearOfFire
                                timeSinceDisturbance = ModelCore.CurrentTime - yearOfFire;
                            }
                        }
                        // if disturbanceType == "Harvest" then:
                        else if (mySuitabilityParameters.DisturbanceType == "Harvest")
                        {
                            // Check if harvest prescription output exists
                            if (SiteVars.PrescriptionName == null)
                            {
                                string mesg = string.Format("The DisturbanceType is Harvest, but prescriptionName SiteVariable is not defined.  Please double-check that a Harvest extension is running.");
                                throw new System.ApplicationException(mesg);
                            }
                            else
                            {
                                //  Check this year harvest prescription names
                                string currentprescriptionName = SiteVars.PrescriptionName[site];
                                int timeofLastHarvest =SiteVars.TimeOfLastHarvest[site];
                                //   if != null then 
                                if (currentprescriptionName != null && timeofLastHarvest == ModelCore.CurrentTime)
                                {
                                    if (mySuitabilityParameters.HarvestPrescriptions.TryGetValue(currentprescriptionName, out suitabilityWeight))
                                    {
                                        //      translate to suitability weight
                                        suitabilityWeight = mySuitabilityParameters.HarvestPrescriptions[currentprescriptionName];
                                        //      if suitability weight > 0 then
                                    }
                                    if (suitabilityWeight > 0)
                                    {
                                        //        store sitevar YearOfHarvest by index
                                        SiteVars.YearOfHarvest[site][index] = ModelCore.CurrentTime;
                                        //        read previous year dominant age
                                        int prevYearDomAge = SiteVars.DominantAge[site][1];
                                        //        store sitevar AgeAtHarvestYear by index
                                        SiteVars.AgeAtHarvestYear[site][index] = prevYearDomAge;
                                        //        store sitevar SuitabilityWeight by index
                                        SiteVars.SuitabilityWeight[site][index] = suitabilityWeight;
                                    }
                                }
                                //  read sitevar AgeAtDisturbanceYear for age value
                                ageAtDisturbanceYear = SiteVars.AgeAtHarvestYear[site][index];
                                //  read sitevar YearOfHarvest
                                int yearOfHarvest = SiteVars.YearOfHarvest[site][index];
                                //  calculate timeSinceDisturbance = currentYear - YearOfHarvest
                                timeSinceDisturbance = ModelCore.CurrentTime - yearOfHarvest;
                            }
                        }
                        //  look up suitabilty in suitabilityTable for combination of AgeAtDisturbanceYear and timeSinceDisturbance
                        foreach (KeyValuePair<string, Dictionary<int, double>> suitabilityRow in mySuitabilityParameters.Suitabilities)
                        {
                            int maxTimeSinceDist = int.Parse(suitabilityRow.Key);
                            if (timeSinceDisturbance <= maxTimeSinceDist)
                            {
                                foreach (KeyValuePair<int, double> item in suitabilityRow.Value)
                                {
                                    int ageLimit = item.Key;
                                    if (ageAtDisturbanceYear <= ageLimit)
                                    {
                                        suitabilityValue = (item.Value * SiteVars.SuitabilityWeight[site][index]);
                                        break;
                                    }
                                }
                                break;
                            }
                        }


                        // write sitevar for suitability value
                        SiteVars.SuitabilityValue[site][index] = suitabilityValue;
                    }

                  // if suitabilityType == ForestType_TimeSinceDisturbance:
                    else if (mySuitabilityParameters.SuitabilityType == "ForestType_TimeSinceDisturbance")
                    {
                        int forestTypeAtDisturbanceYear = 0;
                        int timeSinceDisturbance = 0;
                        double suitabilityWeight = 0.0;
                        // if disturbanceType == "Fire" then:
                        if (mySuitabilityParameters.DisturbanceType == "Fire")
                        {
                            // Check if fire output exists
                            if (SiteVars.FireSeverity == null)
                            {
                                string mesg = string.Format("The DisturbanceType is Fire, but FireSeverity SiteVariable is not defined.  Please double-check that a Fire extension is running.");
                                throw new System.ApplicationException(mesg);
                            }
                            else
                            {
                                //   Check this year fire severity
                                int currentFireSeverity = SiteVars.FireSeverity[site];
                                int timeofLastFire = SiteVars.TimeOfLastFire[site];
                                //   if > 0 then 
                                if (currentFireSeverity > 0 && timeofLastFire == ModelCore.CurrentTime)
                                {
                                    //      translate to suitability weight
                                    suitabilityWeight = mySuitabilityParameters.FireSeverities[currentFireSeverity];

                                    //      if suitability weight > 0 then
                                    if (suitabilityWeight > 0)
                                    {
                                        //        store sitevar YearOfFire by index
                                        SiteVars.YearOfFire[site][index] = ModelCore.CurrentTime;
                                        //        read previous year forest type
                                        int prevYearForType = SiteVars.ForestType[site][index][1];
                                        //        store sitevar forestTypeAtFireYear by index
                                        SiteVars.ForestTypeAtFireYear[site][index] = prevYearForType;
                                        //        store sitevar SuitabilityWeight by index
                                        SiteVars.SuitabilityWeight[site][index] = suitabilityWeight;
                                    }
                                }
                            }
                            //  read sitevar ForestTypeFireYear for age value
                            forestTypeAtDisturbanceYear = SiteVars.ForestTypeAtFireYear[site][index];
                            //  read sitevar YearOfFire
                            int yearOfFire = SiteVars.YearOfFire[site][index];
                            //  calculate timeSinceDisturbance = currentYear - YearOfFire
                            timeSinceDisturbance = ModelCore.CurrentTime - yearOfFire;
                        }
                        // if disturbanceType == "Harvest" then:
                        else if (mySuitabilityParameters.DisturbanceType == "Harvest")
                        {
                            // Check if harvest prescription output exists
                            if (SiteVars.PrescriptionName == null)
                            {
                                string mesg = string.Format("The DisturbanceType is Harvest, but prescriptionName SiteVariable is not defined.  Please double-check that a Harvest extension is running.");
                                throw new System.ApplicationException(mesg);
                            }
                            else
                            {
                                //  Check this year harvest prescription names
                                string currentprescriptionName = SiteVars.PrescriptionName[site];
                                int timeofLastHarvest = SiteVars.TimeOfLastHarvest[site];
                                //   if != null then 
                                if (currentprescriptionName != null && timeofLastHarvest == ModelCore.CurrentTime)
                                {
                                    if (mySuitabilityParameters.HarvestPrescriptions.TryGetValue(currentprescriptionName, out suitabilityWeight))
                                    {
                                        //      translate to suitability weight
                                        suitabilityWeight = mySuitabilityParameters.HarvestPrescriptions[currentprescriptionName];
                                        //      if suitability weight > 0 then
                                    }
                                    if (suitabilityWeight > 0)
                                    {
                                        //        store sitevar YearOfHarvest by index
                                        SiteVars.YearOfHarvest[site][index] = ModelCore.CurrentTime;
                                        //        read previous year forest type
                                        int prevYearForType = SiteVars.ForestType[site][index][1];
                                        //        store sitevar forestTypeAtHarvestYear by index
                                        SiteVars.ForestTypeAtHarvestYear[site][index] = prevYearForType;
                                        //        store sitevar SuitabilityWeight by index
                                        SiteVars.SuitabilityWeight[site][index] = suitabilityWeight;
                                    }
                                }
                                //  read sitevar AgeAtHarvestYear for age value
                                forestTypeAtDisturbanceYear = SiteVars.ForestTypeAtHarvestYear[site][index];
                                //  read sitevar YearOfHarvest
                                int yearOfHarvest = SiteVars.YearOfHarvest[site][index];
                                //  calculate timeSinceDisturbance = currentYear - YearOfHarvest
                                timeSinceDisturbance = ModelCore.CurrentTime - yearOfHarvest;
                            }
                        }
                        if (forestTypeAtDisturbanceYear > 0)
                        {
                            IForestType forestType = mySuitabilityParameters.ForestTypes[0].ForestTypes[forestTypeAtDisturbanceYear - 1];

                            // look up suitability in suitabilityTable for combination of forest type and timeSinceDisturbance
                            Dictionary<int, double> suitabilityRow = mySuitabilityParameters.Suitabilities[forestType.Name];

                            foreach (KeyValuePair<int, double> item in suitabilityRow)
                            {
                                int maxTimeSinceDist = item.Key;
                                if (timeSinceDisturbance <= maxTimeSinceDist)
                                {
                                    suitabilityValue = (item.Value * SiteVars.SuitabilityWeight[site][index]);
                                    break;
                                }

                            }
                        }
                        else
                            suitabilityValue = 0;

                        // write sitevar for suitability value
                        SiteVars.SuitabilityValue[site][index] = suitabilityValue;
                    }
                }
                // if output timestep then write all maps
                if ((ModelCore.CurrentTime % parameters.OutputTimestep) == 0)
                {
                    string mapName = mySuitabilityParameters.WildlifeName;
                    string path = MapFileNames.ReplaceTemplateVars(mapNameTemplate, mapName, modelCore.CurrentTime);
                    ModelCore.UI.WriteLine("   Writing Wildlife Habitat Output map to {0} ...", path);
                    using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
                    {
                        IntPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in modelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                                pixel.MapCode.Value = (int)(SiteVars.SuitabilityValue[site][index] * 100);
                            else
                                pixel.MapCode.Value = 0;

                            outputRaster.WriteBufferPixel();
                        }
                    }
                }


                /*Copied from biomass-reclass
                 * foreach (IMapDefinition map in mapDefs)
                {
                    List<IForestType> forestTypes = map.ForestTypes;

                    string path = MapFileNames.ReplaceTemplateVars(mapNameTemplate, map.Name, modelCore.CurrentTime);
                    modelCore.Log.WriteLine("   Writing Biomass Reclass map to {0} ...", path);
                    using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, modelCore.Landscape.Dimensions))
                    {
                        BytePixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in modelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                                pixel.MapCode.Value = CalcForestType(forestTypes, site);
                            else
                                pixel.MapCode.Value = 0;
                        
                            outputRaster.WriteBufferPixel();
                        }
                    }

                }
                 * */
                index++;
            }


        }
        //---------------------------------------------------------------------
        // Copied from biomass-reclass
        // Added reclass coefficients
        private static int CalcForestTypeBiomass(List<IForestType> forestTypes,
                                    Site site, double [] reclassCoeffs)
        {
            int forTypeCnt = 0;

            double[] forTypValue = new double[forestTypes.Count];

            foreach(ISpecies species in modelCore.Species)
            {
                double sppValue = 0.0;

                if (SiteVars.Cohorts[site] == null)
                    break;

                sppValue = Util.ComputeBiomass(SiteVars.Cohorts[site][species]);

                forTypeCnt = 0;
                foreach(IForestType ftype in forestTypes)
                {
                    if(ftype[species.Index] != 0)
                    {
                        if(ftype[species.Index] == -1)
                            forTypValue[forTypeCnt] -= sppValue * reclassCoeffs[species.Index];
                        if(ftype[species.Index] == 1)
                            forTypValue[forTypeCnt] += sppValue * reclassCoeffs[species.Index];
                    }
                    forTypeCnt++;
                }
            }

            int finalForestType = 0;
            double maxValue = 0.0;
            forTypeCnt = 0;
            foreach(IForestType ftype in forestTypes)
            {
                if(forTypValue[forTypeCnt]>maxValue)
                {
                    maxValue = forTypValue[forTypeCnt];
                    finalForestType = forTypeCnt+1;
                }
                forTypeCnt++;
            }
            if (maxValue == 0)
            {
                finalForestType = 0;
            }
            //string forTypeName = forestTypes[finalForestType].Name;
            return finalForestType;
        }
        //---------------------------------------------------------------------
        // Copied from output-reclass
        private static byte CalcForestTypeAge( List<IForestType> forestTypes,Site site, double [] reclassCoefs)
        {
            int forTypeCnt = 0;

            double[] forTypValue = new double[forestTypes.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                if (SiteVars.Cohorts[site] != null)
                {
                    ushort maxSpeciesAge = 0;
                    double sppValue = 0.0;
                    maxSpeciesAge = GetSppMaxAge(site, species);


                    if (maxSpeciesAge > 0)
                    {
                        sppValue = (double)maxSpeciesAge /
                            (double)species.Longevity *
                            (double)reclassCoefs[species.Index];

                        forTypeCnt = 0;
                        foreach (IForestType ftype in forestTypes)
                        {
                            if (ftype[species.Index] != 0)
                            {
                                if (ftype[species.Index] == -1)
                                    forTypValue[forTypeCnt] -= sppValue;
                                if (ftype[species.Index] == 1)
                                    forTypValue[forTypeCnt] += sppValue;
                            }
                            forTypeCnt++;
                        }
                    }
                }
            }

            int finalForestType = 0;
            double maxValue = 0.0;
            forTypeCnt = 0;
            foreach (IForestType ftype in forestTypes)
            {
                //System.Console.WriteLine("ForestTypeNum={0}, Value={1}.",forTypeCnt,forTypValue[forTypeCnt]);
                if (forTypValue[forTypeCnt] > maxValue)
                {
                    maxValue = forTypValue[forTypeCnt];
                    finalForestType = forTypeCnt + 1;
                }
                ModelCore.UI.WriteLine("ftype={0}, value={1}.", ftype.Name, forTypValue[forTypeCnt]);
                forTypeCnt++;
            }
            string forTypeName = forestTypes[finalForestType].Name;
            return (byte) finalForestType;
        }
        //---------------------------------------------------------------------
        // Copied from output-reclass
        public static ushort GetSppMaxAge(Site site, ISpecies spp)
        {
            if (!site.IsActive)
                return 0;
            ushort max = 0;
            if (SiteVars.Cohorts[site] == null)
            {
                if (SiteVars.Cohorts[site] == null)
                {
                    PlugIn.ModelCore.UI.WriteLine("Cohort are null.");
                    return 0;
                }
                else
                {
                    max = 0;
                    foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
                    {
                        if (sppCohorts.Species == spp)
                        {
                            //ModelCore.UI.WriteLine("cohort spp = {0}, compare species = {1}.", sppCohorts.Species.Name, spp.Name);
                            foreach (ICohort cohort in sppCohorts)
                                if (cohort.Data.Age > max)
                                    max = cohort.Data.Age;
                        }
                    }
                }
            }
            else
            {
                max = 0;

                foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
                {
                    if (sppCohorts.Species == spp)
                    {
                        //ModelCore.UI.WriteLine("cohort spp = {0}, compare species = {1}.", sppCohorts.Species.Name, spp.Name);
                        foreach (ICohort cohort in sppCohorts)
                            if (cohort.Data.Age > max)
                                max = cohort.Data.Age;
                    }
                }
            }
            return max;
        }
        //---------------------------------------------------------------------
        // Calculate dominant age class
        // For age-only succession dominant has most cohorts
        // For biomass succession dominat has most biomass
        public static void UpdateDominantAge( Site site)
        {
            int domAge = 0;
            if (SiteVars.Cohorts[site] == null)
            {
                domAge = CalculateDomAgeAgeOnly(site);
            }
            else
            {
                domAge = CalculateDomAgeBiomass(site);
            }
            SiteVars.DominantAge[site][1] = SiteVars.DominantAge[site][0];
            SiteVars.DominantAge[site][0] = domAge;
        }
        //---------------------------------------------------------------------
        public static void UpdateForestType(int index, ISuitabilityParameters suitabilityParameters, Site site)
        {
            if ((suitabilityParameters.SuitabilityType == "AgeClass_ForestType") || (suitabilityParameters.SuitabilityType == "ForestType_TimeSinceDisturbance"))
            {
                double[] reclassCoeffs = suitabilityParameters.ReclassCoefficients;
                int forTypeIndex = 0;
                foreach (IMapDefinition map in suitabilityParameters.ForestTypes)
                {
                    List<IForestType> forestTypes = map.ForestTypes;
                    if (SiteVars.Cohorts[site] == null)
                    {
                        forTypeIndex = CalcForestTypeAge(forestTypes, site, reclassCoeffs);
                    }
                    else
                    {
                        forTypeIndex = CalcForestTypeBiomass(forestTypes, site, reclassCoeffs);
                    }
                }
                SiteVars.ForestType[site][index][1] = SiteVars.ForestType[site][index][0];
                SiteVars.ForestType[site][index][0] = forTypeIndex;
            }
        }
        //---------------------------------------------------------------------
        public static int CalculateDomAgeForestType(Site site, IForestType forestType)
        {
            
            List<ISpecies> speciesList = new List<ISpecies>();
            int speciesIndex = 0;
            foreach (ISpecies species in modelCore.Species)
            {
                if (forestType[speciesIndex] == 1)
                    speciesList.Add(species);
                speciesIndex++;
            }
            Dictionary<int, int> ageDictionary = new Dictionary<int, int>();
            foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
            {
                if (speciesList.Contains(sppCohorts.Species))
                {
                    foreach (ICohort cohort in sppCohorts)
                    {
                        int age = cohort.Data.Age;
                        int biomass = cohort.Data.Biomass;
                        if (ageDictionary.ContainsKey(age))
                        {
                            ageDictionary[age] = ageDictionary[age] + biomass;
                        }
                        else
                        {
                            ageDictionary[age] = biomass;
                        }
                    }
                }
            }
            int domAge = 0;
            int maxValue = 0;
            foreach (var i in ageDictionary)
            {
                if (i.Value > maxValue)
                {
                    domAge = i.Key;
                    maxValue = i.Value;
                }
            }
            return domAge;
        }
        //---------------------------------------------------------------------
        public static int CalculateDomAgeBiomass(Site site)
        {

            Dictionary<int, int> ageDictionary = new Dictionary<int, int>();
            foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in sppCohorts)
                {
                    int age = cohort.Data.Age;
                    int biomass = cohort.Data.Biomass;
                    if (ageDictionary.ContainsKey(age))
                    {
                        ageDictionary[age] = ageDictionary[age] + biomass;
                    }
                    else
                    {
                        ageDictionary[age] = biomass;
                    }
                }
            }
            int domAge = 0;
            int maxValue = 0;
            foreach (var i in ageDictionary)
            {
                if (i.Value > maxValue)
                {
                    domAge = i.Key;
                    maxValue = i.Value;
                }
            }
            return domAge;
        }
        //---------------------------------------------------------------------
        public static int CalculateDomAgeAgeOnly(Site site)
        {
            Dictionary<int, int> ageDictionary = new Dictionary<int, int>();
            foreach (ISpeciesCohorts sppCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in sppCohorts)
                {
                    int age = cohort.Data.Age;
                    if (ageDictionary.ContainsKey(age))
                    {
                        ageDictionary[age] = ageDictionary[age] + 1;
                    }
                    else
                    {
                        ageDictionary[age] = 1;
                    }
                }
            }
            int domAge = 0;
            int maxValue = 0;
            foreach (var i in ageDictionary)
            {
                if (i.Value > maxValue)
                {
                    domAge = i.Key;
                    maxValue = i.Value;
                }
            }
            return domAge;
        }

        public override void AddCohortData()
        {
            // ADD CUSTOM DYNAMIC COHORT PARAMETERS HERE
            return;
        }
    }
    
}
