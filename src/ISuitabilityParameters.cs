//  Authors:  Brian R. Miranda, Donald J. Brown

using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    /// <summary>
    /// The parameters for the plug-in.
    /// </summary>
    public interface ISuitabilityParameters
    {

        /// <summary>
        /// Reclass coefficients for species
        /// </summary>
        double[] ReclassCoefficients
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Forest Type Table
        /// </summary>
        List<IMapDefinition> ForestTypes
        {
            get;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Fire Severity Table
        /// </summary>
        Dictionary<int,double> FireSeverities
        {
            get;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Harvest Prescription Table
        /// </summary>
        Dictionary<string, double> HarvestPrescriptions
        {
            get;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability Table
        /// </summary>
        Dictionary<string, Dictionary<int, double>> Suitabilities
        {
            get;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability Type
        /// </summary>
       string SuitabilityType
        {
            get;

        }

        //---------------------------------------------------------------------
       /// <summary>
       /// Wildlife Name
       /// </summary>
       string WildlifeName
       {
           get;

       }
       //---------------------------------------------------------------------
       /// <summary>
       /// DisturbanceType
       /// </summary>
       string DisturbanceType
       {
           get;

       }
        //---------------------------------------------------------------------
    }
}
