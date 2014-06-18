using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{

    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class SuitabilityFileParametersParser
        : TextParser<ISuitabilityParameters>
    {
        public static ISpeciesDataset SpeciesDataset = null;


        //---------------------------------------------------------------------

        public SuitabilityFileParametersParser()
        {
        }
        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get
            {
                return "HabitatSuitabilityTable";
            }
        }
        //---------------------------------------------------------------------

        protected override ISuitabilityParameters Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != "HabitatSuitabilityTable")
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", "HabitatSuitabilityTable");

            SuitabilityParameters suitabilityParameters = new SuitabilityParameters(PlugIn.ModelCore.Species.Count);

            InputVar<string> wildlifeName = new InputVar<string>("WildlifeName");
            ReadVar(wildlifeName);
            suitabilityParameters.WildlifeName = wildlifeName.Value;

            InputVar<string> suitabilityType = new InputVar<string>("SuitabilityType");
            ReadVar(suitabilityType);
            if ((suitabilityType.Value.Actual != "AgeClass_ForestType") & (suitabilityType.Value.Actual != "AgeClass_TimeSinceFire") & (suitabilityType.Value.Actual != "ForestType_TimeSinceFire"))
                throw new InputValueException(suitabilityType.Value.String, "The value is not \"{0}\"", "AgeClass_ForestType or AgeClass_TimeSinceFire or ForestType_TimeSinceFire");
            suitabilityParameters.SuitabilityType = suitabilityType.Value;

            List<string> keywordList = new List<string>();
            if (suitabilityType.Value == "AgeClass_ForestType")
            {
                keywordList.Add("ReclassCoefficients");
                keywordList.Add("ForestTypeTable");
                keywordList.Add("SuitabilityTable");
            }
            else if (suitabilityType.Value == "AgeClass_TimeSinceFire")
            {
                keywordList.Add("FireSeverityTable");
                keywordList.Add("SuitabilityTable");
            }
            else if (suitabilityType.Value == "ForestType_TimeSinceFire")
            {
                keywordList.Add("ReclassCoefficients");
                keywordList.Add("ForestTypeTable");
                keywordList.Add("FireSeverityTable");
                keywordList.Add("SuitabilityTable");
            }

            InputVar<string> speciesName = new InputVar<string>("Species");

            int keywordIndex = 0;
            foreach (string keyword in keywordList)
            {
                if (keyword == "ReclassCoefficients")
                {
                    // Table of reclass coefficients
                    ReadName("ReclassCoefficients");
                    InputVar<double> reclassCoeff = new InputVar<double>("Reclass Coefficient");

                    Dictionary<string, int> lineNumbers = new Dictionary<string, int>();


                    while (!AtEndOfInput && CurrentName != keywordList[keywordIndex + 1])
                    {
                        StringReader currentLine = new StringReader(CurrentLine);

                        ReadValue(speciesName, currentLine);
                        ISpecies species = GetSpecies(speciesName.Value);
                        CheckForRepeatedName(speciesName.Value, "species", lineNumbers);

                        ReadValue(reclassCoeff, currentLine);
                        suitabilityParameters.ReclassCoefficients[species.Index] = reclassCoeff.Value;

                        CheckNoDataAfter(string.Format("the {0} column", reclassCoeff.Name),
                                         currentLine);
                        GetNextLine();
                    }
                }
                if (keyword == "ForestTypeTable")
                {
                    // Table of forest types
                    ReadName("ForestTypeTable");
                    InputVar<string> forestType = new InputVar<string>("Forest Type");

                    Dictionary<string, int> forestTypeLineNumbers = new Dictionary<string, int>();
                    
                    IMapDefinition mapDefn = null;
                    mapDefn = new MapDefinition();
                    mapDefn.Name = "ForestTypes";
                    suitabilityParameters.ForestTypes.Add(mapDefn);

                    while (!AtEndOfInput && CurrentName != keywordList[keywordIndex + 1])
                    {
                        StringReader currentLine = new StringReader(CurrentLine);

                        ReadValue(forestType, currentLine);
                        CheckForRepeatedName(forestType.Value, "forest type",
                                             forestTypeLineNumbers);

                        IForestType currentForestType = new ForestType(PlugIn.ModelCore.Species.Count);
                        currentForestType.Name = forestType.Value;
                        mapDefn.ForestTypes.Add(currentForestType);

                        //  Read species for forest types

                        List<string> speciesNames = new List<string>();

                        TextReader.SkipWhitespace(currentLine);
                        while (currentLine.Peek() != -1)
                        {
                            ReadValue(speciesName, currentLine);
                            string name = speciesName.Value.Actual;
                            bool negativeMultiplier = name.StartsWith("-");
                            if (negativeMultiplier)
                            {
                                name = name.Substring(1);
                                if (name.Length == 0)
                                    throw new InputValueException(speciesName.Value.String,
                                        "No species name after \"-\"");
                            }
                            ISpecies species = GetSpecies(new InputValue<string>(name, speciesName.Value.String));
                            if (speciesNames.Contains(species.Name))
                                throw NewParseException("The species {0} appears more than once.", species.Name);
                            speciesNames.Add(species.Name);

                            currentForestType[species.Index] = negativeMultiplier ? -1 : 1;

                            TextReader.SkipWhitespace(currentLine);
                        }
                        if (speciesNames.Count == 0)
                            throw NewParseException("At least one species is required.");

                        GetNextLine();
                    }
                }
                if (keyword == "FireSeverityTable")
                {
                    // Table of Fire Severities
                    ReadName("FireSeverityTable");

                    InputVar<int> severityClass = new InputVar<int>("Fire Severity Class");
                    InputVar<double> fireSuitability = new InputVar<double>("Fire Class Suitability");
                    Dictionary<int, double> fireSeverityTable = new Dictionary<int, double>();
                    while (!AtEndOfInput && CurrentName != keywordList[keywordIndex + 1])
                    {
                        StringReader currentLine = new StringReader(CurrentLine);
                        TextReader.SkipWhitespace(currentLine);
                        ReadValue(severityClass, currentLine);
                        ReadValue(fireSuitability, currentLine);

                        fireSeverityTable.Add(severityClass.Value, fireSuitability.Value);
                    }
                    
                    suitabilityParameters.FireSeverities = fireSeverityTable;
                }
                if (keyword == "SuitabilityTable")
                {
                    // Table of suitabilities
                    ReadName("SuitabilityTable");
                    Dictionary<string,Dictionary<int, double>> suitabilityTable = new Dictionary<string,Dictionary<int, double>>();

                    InputVar<int> ageCutoff = new InputVar<int>("Age Cutoff");
                    InputVar<string> suitabilityClass = new InputVar<string>("Suitability Class");
                    InputVar<double> suitabilityValue = new InputVar<double>("Suitability Value");

                    List<int> ageList = new List<int>();

                    StringReader currentLine = new StringReader(CurrentLine);
                    while (currentLine.Peek() != -1)
                    {
                        ReadValue(ageCutoff, currentLine);
                        ageList.Add(ageCutoff.Value);
                    }
                    GetNextLine();
                    while (!AtEndOfInput && CurrentName != keywordList[keywordIndex + 1])
                    {
                        TextReader.SkipWhitespace(currentLine);
                        ReadValue(suitabilityClass, currentLine);
                        Dictionary<int, double> suitabilityRow = new Dictionary<int, double>();
                        foreach (int age in ageList)
                        {
                            ReadValue(suitabilityValue, currentLine);
                            suitabilityRow.Add(age, suitabilityValue.Value);
                        }
                        suitabilityTable.Add(suitabilityClass.Value, suitabilityRow);
                        
                    }
                    suitabilityParameters.Suitabilities = suitabilityTable;
                }

                keywordIndex++;
            }
  

            return suitabilityParameters;
        }
        //---------------------------------------------------------------------

        protected ISpecies GetSpecies(InputValue<string> name)
        {
            ISpecies species = PlugIn.ModelCore.Species[name.Actual];
            if (species == null)
                throw new InputValueException(name.String,
                                              "{0} is not a species name.",
                                              name.String);
            return species;
        }

        //---------------------------------------------------------------------

        private void CheckForRepeatedName(InputValue<string> name,
                                          string description,
                                          Dictionary<string, int> lineNumbers)
        {
            int lineNumber;
            if (lineNumbers.TryGetValue(name.Actual, out lineNumber))
                throw new InputValueException(name.String,
                                              "The {0} {1} was previously used on line {2}",
                                              description, name.String, lineNumber);
            lineNumbers[name.Actual] = LineNumber;
        }
    }
}
