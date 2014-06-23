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

        public static readonly ExtensionType Type = new ExtensionType("output");
        public static readonly string ExtensionName =  "Wildlife Habitat Output";

        private string mapNameTemplate;
        private List<string> suitabilityFiles;

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
            // foreach (Site site in modelCore.Landscape.AllSites)
            //{
            // calculate dominant age as site variable (DomAge), store for retreival
            //  Note: DomAge site variable is an array of values giving dominant age for this year and last year

            // calculate forest type as site variable (ForestType), store for retreival
            //  Note: ForestType site variable is an array of values giving forest type for this year and last year
            
            //}

            foreach (string suitabilityFile in suitabilityFiles)
            {
                // get suitability parameters

                // foreach (Site site in modelCore.Landscape.AllSites)
                //{
                // depending on suitability type, calculate final suitability value

                // if suitabilityType == AgeClass_ForestType:
                //   calculate forest type (CalcForestTypeBiomass or CalcForestTypeAge)
                //   calculate dominant age among species in forest type
                //   look up suitabilty in suitabilityTable for combination of forest type and age
                //  write sitevar for suitability value
                //  if output timestep then write to map

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
                // if output timestep then write to map
                //}

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

            }

        }
        //---------------------------------------------------------------------
        // Copied from biomass-reclass
        // Added reclass coefficients
        private string CalcForestTypeBiomass(List<IForestType> forestTypes,
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
            string forTypeName = forestTypes[finalForestType].Name;
            return forTypeName;
        }
        //---------------------------------------------------------------------
        // Copied from output-reclass
        private string CalcForestTypeAge(Site site, List<IForestType> forestTypes, double [] reclassCoefs)
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
            return forTypeName;
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
    }
}
