using System;
using System.Linq;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {

        public int GetIntConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con int
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nel tipo e ritorno
            int returnIntValue = 0;

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    if (!Int32.TryParse(configurationValue.VarValue, out returnIntValue))
                    {
                        throw new Exception("Data Layer Exception - Impossible convert the DB value to interger");
                    }
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
            else
            {
                throw new Exception("Data Layer Exception - Variable Not Found");
            }

            return returnIntValue;
        }

        public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con decimal
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nle tipo e ritorno
            decimal returnDecimalValue = 0;

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    if (!Decimal.TryParse(configurationValue.VarValue, out returnDecimalValue))
                    {
                        throw new Exception("Data Layer Exception - Impossible convert the DB value to decimal");
                    }
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
            else
            {
                throw new Exception("Data Layer Exception - Variable Not Found");
            }

            return returnDecimalValue;
        }

        public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con string
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nle tipo e ritorno
            string returnStringValue = "";

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                {
                    returnStringValue = configurationValue.VarValue;
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
            else
            {
                throw new Exception("Data Layer Exception - Variable Not Found");
            }

            return returnStringValue;
        }

        public int GetIntRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con int
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nle tipo e ritorno
            throw new NotImplementedException();
        }

        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con decimal
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nle tipo e ritorno
            throw new NotImplementedException();
        }

        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Cercare la var nel DB
            // Confrontare il tipo nel DB con string
            // se diverso throw an exception creare l'eccezione Data layer Exception con Invalid Data Type
            // se uguale, converto il valore nle tipo e ritorno
            throw new NotImplementedException();
        }

        public void SetIntConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null) // Insert a new record
            {
                //Create new Variable in the DB
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.integerType;
                newConfigurationValue.VarValue = value.ToString();

                //Add new Employee to database
                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else // Update the existing record
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    configurationValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
        }

        public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null) // Insert a new record
            {
                //Create new Variable in the DB
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.decimalType;
                newConfigurationValue.VarValue = value.ToString();

                //Add new Employee to database
                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else // Update the existing record
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    configurationValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
        }

        public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null) // Insert a new record
            {
                //Create new Variable in the DB
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.stringType;
                newConfigurationValue.VarValue = value;

                //Add new Employee to database
                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else // Update the existing record
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                {
                    configurationValue.VarValue = value;
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new Exception("Data Layer Exception - Invalid Data Type");
                }
            }
        }

        public void SetIntRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            throw new NotImplementedException();
        }

        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            throw new NotImplementedException();
        }

        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            // Ricevo un valore in input
            // cerco se esite nel DB la var
            // se esiste controllo il tipo del record
            // se è diverso da int, throw exception "invalid data type"
            // se esiste lo aggiorno
            // se non esiste lo creo
            throw new NotImplementedException();
        }
    }
}
