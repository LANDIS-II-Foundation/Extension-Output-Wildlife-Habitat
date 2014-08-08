//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Core;
using Landis.Library.BiomassCohorts;
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
            SiteVars.Initialize();
            this.mapNameTemplate = parameters.MapFileNames;
            this.suitabilityFiles = parameters.SuitabilityFiles;
            this.suitabilityParameters = parameters.SuitabilityParameters;
            

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
            int index = 0;
            foreach (string suitabilityFile in suitabilityFiles)
            {
                // get suitability parameters
                ISuitabilityParameters mySuitabilityParameters = suitabilityParameters[index];

                foreach (Site site in modelCore.Landscape.ActiveSites)
                {
                  // calculate dominant age as site variable (DomAge), store for retreival
                  //  Note: DomAge site variable is an array of values giving dominant age for this year and last year
                  UpdateDominantAge(index, site);

                // calculate forest type as site variable (ForestType), store for retreival
                //  Note: ForestType site variable is an array of values giving the index of forest type for this year and last year
                  UpdateForestType(index,mySuitabilityParameters, site);

                // depending on suitability type, calculate final suitability value

                  if (mySuitabilityParameters.SuitabilityType == "AgeClass_ForestType")
                  {
                      //   get forest type (calculated above with UpdateForestType)
                      int currentForestType = SiteVars.ForestType[site][index][0];
                      double suitabilityValue = 0;
                      if (currentForestType > 0)
                      {
                          IForestType forestType = mySuitabilityParameters.ForestTypes[0].ForestTypes[currentForestType - 1];
                          //   calculate dominant age among species in forest type
                          int domFTAge = CalculateDomAgeForestType(site, forestType);
                          //   look up suitabilty in suitabilityTable for combination of forest type and age
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
                // if disturbanceType == "Fire" then:
                //   Check this year fire severity
                //   if > 0 then 
                //      translate to suitability weight
                //      if suitability weight > 0 then
                //        store sitevar YearOfFire by wildlifeName
                //        read previous year dominant age
                //        store sitevar AgeAtFireYear by wildlifeName
                //        store sitevar SuitabilityWeight by wildlifeName
                //  read sitevar AgeAtFireYear for age value
                //  read sitevar YearOfFire
                //  calculate timeSinceDisturbance = currentYear - YearOfFire
                // if disturbaceType == "Harvest" then:
                //  Check this year harvest prescription names
                //   if != null then 
                //      translate to suitability weight
                //      if suitability weight > 0 then
                //        store sitevar YearOfHarvest by wildlifeName
                //        read previous year dominant age
                //        store sitevar AgeAtHarvestYear by wildlifeName
                //        store sitevar SuitabilityWeight by wildlifeName
                //  read sitevar AgeAtHarvestYear for age value
                //  read sitevar YearOfHarvest
                //  calculate timeSinceDisturbance = currentYear - YearOfHarvest
                //  look up suitabilty in suitabilityTable for combination of age and timeSinceDisturbance
                // write sitevar for suitability value
                // if output timestep then write to map

                // if suitabilityType == ForestType_TimeSinceDisturbance:
                //   calculate forest type (CalcForestTypeBiomass or CalcForestTypeAge)
                // if disturbanceType == "Fire" then:
                //   Check this year fire severity
                //   if > 0 then 
                //      translate to suitability weight
                //      if suitability weight > 0 then
                //        store sitevar YearOfFire by wildlifeName
                //        read previous year forest type
                //        store sitevar AgeAtFireYear by wildlifeName
                //        store sitevar SuitabilityWeight by wildlifeName
                //  read sitevar AgeAtFireYear for age value
                //  read sitevar YearOfFire
                //  calculate timeSinceDisturbance = currentYear - YearOfFire
                // if disturbaceType == "Harvest" then:
                //  Check this year harvest prescription names
                //   if != null then 
                //      translate to suitability weight
                //      if suitability weight > 0 then
                //        store sitevar YearOfHarvest by wildlifeName
                //        read previous year forest type
                //        store sitevar AgeAtHarvestYear by wildlifeName
                //        store sitevar SuitabilityWeight by wildlifeName
                //  read sitevar AgeAtHarvestYear for age value
                //  read sitevar YearOfHarvest
                //  calculate timeSinceDisturbance = currentYear - YearOfHarvest
                // look up suitabilty in suitabilityTable for combination of forest type and timeSinceDisturbance
                // write sitevar for suitability value
                }
                // if output timestep then write all maps
                  if (ModelCore.CurrentTime == parameters.OutputTimestep)
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
                                  pixel.MapCode.Value = (int) (SiteVars.SuitabilityValue[site][index] * 100);
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
                index ++;
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

                if (SiteVars.BiomassCohorts[site] == null)
                    break;

                sppValue = Util.ComputeBiomass(SiteVars.BiomassCohorts[site][species]);

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
                if (SiteVars.AgeCohorts[site] != null)
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
            if (SiteVars.BiomassCohorts[site] == null)
            {
                if (SiteVars.AgeCohorts[site] == null)
                {
                    PlugIn.ModelCore.UI.WriteLine("Cohort are null.");
                    return 0;
                }
                else
                {
                    max = 0;
                    foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts sppCohorts in SiteVars.AgeCohorts[site])
                    {
                        if (sppCohorts.Species == spp)
                        {
                            //ModelCore.UI.WriteLine("cohort spp = {0}, compare species = {1}.", sppCohorts.Species.Name, spp.Name);
                            foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in sppCohorts)
                                if (cohort.Age > max)
                                    max = cohort.Age;
                        }
                    }
                }
            }
            else
            {
                max = 0;

                foreach (ISpeciesCohorts sppCohorts in SiteVars.AgeCohorts[site])
                {
                    if (sppCohorts.Species == spp)
                    {
                        //ModelCore.UI.WriteLine("cohort spp = {0}, compare species = {1}.", sppCohorts.Species.Name, spp.Name);
                        foreach (ICohort cohort in sppCohorts)
                            if (cohort.Age > max)
                                max = cohort.Age;
                    }
                }
            }
            return max;
        }
        //---------------------------------------------------------------------
        // Calculate dominant age class
        // For age-only succession dominant has most cohorts
        // For biomass succession dominat has most biomass
        public static void UpdateDominantAge(int index, Site site)
        {
            int domAge = 0;
            if (SiteVars.BiomassCohorts[site] == null)
            {
                domAge = CalculateDomAgeAgeOnly(site);
            }
            else
            {
                domAge = CalculateDomAgeBiomass(site);
            }
            SiteVars.DominantAge[site][index][1] = SiteVars.DominantAge[site][index][0];
            SiteVars.DominantAge[site][index][0] = domAge;
        }
        //---------------------------------------------------------------------
        public static void UpdateForestType(int index, ISuitabilityParameters suitabilityParameters, Site site)
        {
            double[] reclassCoeffs = suitabilityParameters.ReclassCoefficients;
            int forTypeIndex  = 0;
            foreach (IMapDefinition map in suitabilityParameters.ForestTypes)
            {
                List<IForestType> forestTypes = map.ForestTypes;
                if (SiteVars.BiomassCohorts[site] == null)
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
            foreach (ISpeciesCohorts sppCohorts in SiteVars.BiomassCohorts[site])
            {
                if (speciesList.Contains(sppCohorts.Species))
                {
                    foreach (ICohort cohort in sppCohorts)
                    {
                        int age = cohort.Age;
                        int biomass = cohort.Biomass;
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
            foreach (ISpeciesCohorts sppCohorts in SiteVars.BiomassCohorts[site])
            {
                foreach (ICohort cohort in sppCohorts)
                {
                    int age = cohort.Age;
                    int biomass = cohort.Biomass;
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
            foreach (ISpeciesCohorts sppCohorts in SiteVars.AgeCohorts[site])
            {
                foreach (ICohort cohort in sppCohorts)
                {
                    int age = cohort.Age;
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

    }
    
}
