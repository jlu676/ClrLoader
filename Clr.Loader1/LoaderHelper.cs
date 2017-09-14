using Clr.Shared;
using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Clr.Loader
{
    public class LoaderHelper
    {
        private SqlHelper _sqlHelper;
        private Assembly _clrAssembly;
        private permissionSetType _permissionSet;
        private string _assemblyName;



        public LoaderHelper()
        {

        }

        public LoaderHelper(string ConnectionString) : this(new SqlConnection(ConnectionString)) { }

        public LoaderHelper(SqlConnection Connection)
        {
            _sqlHelper = new SqlHelper(Connection);
        }



        #region Installer

        public void InstallClrAssemblyDirectory(string clrAssemblyDirectory, permissionSetType permissionSet)
        {
            foreach (var file in Directory.GetFiles(clrAssemblyDirectory, "*.dll"))
            {
                InstallClrAssembly(file, permissionSet);
            }
        }

        public void InstallClrAssembly(string clrAssemblyPath, permissionSetType permissionSet)
        {
            var clrAssembly = Assembly.LoadFile(clrAssemblyPath);
            InstallClrAssembly(clrAssembly, permissionSet);
            Console.WriteLine($"Assembly {_assemblyName} installed!");
        }

        public void InstallClrAssembly(Assembly clrAssembly, permissionSetType permissionSet)
        {
            _clrAssembly = clrAssembly;
            _assemblyName = _clrAssembly.GetName().Name.Replace(".", "");
            _permissionSet = permissionSet;

            DropAssemblyFunctions();
            DropAssembly();
            CreateAssembly();
            CreateAssemblyFunctions();
        }

        public void UninstallClrAssembly(string clrAssemblyPath)
        {
            var clrAssembly = Assembly.LoadFile(clrAssemblyPath);
            UninstallClrAssembly(clrAssembly);
        }


        public void UninstallClrAssembly(Assembly clrAssembly)
        {
            _clrAssembly = clrAssembly;
            _assemblyName = _clrAssembly.GetName().Name.Replace(".", "");

            DropAssemblyFunctions();
            DropAssembly();
        }

        public void ViewClrAssemblyFunctions(string clrAssemblyPath)
        {
            var clrAssembly = Assembly.LoadFile(clrAssemblyPath);
            ViewClrAssemblyFunctions(clrAssembly);
        }

        public void ViewClrAssemblyFunctions(Assembly clrAssembly)
        {
            _clrAssembly = clrAssembly;
            _assemblyName = _clrAssembly.GetName().Name.Replace(".", "");

            foreach (Type classInfo in _clrAssembly.GetTypes())
            {
                foreach (MethodInfo methodInfo in GetAssemblySQLFunction(classInfo))
                {
                    var sql = BuildSQLFunction(methodInfo);
                    Console.WriteLine(sql);
                }
            }
        }

        #endregion

        #region private methods

        private string ConvertClrTypeToSql(Type clrType)
        {
            switch (clrType.Name)
            {
                case "SqlString":
                    return "NVARCHAR(MAX)";
                case "SqlDateTime":
                    return "DATETIME";
                case "SqlInt16":
                    return "SMALLINT";
                case "SqlInt32":
                    return "INTEGER";
                case "SqlInt64":
                    return "BIGINT";
                case "SqlBoolean":
                    return "BIT";
                case "SqlMoney":
                    return "MONEY";
                case "SqlSingle":
                    return "REAL";
                case "SqlDouble":
                    return "DOUBLE";
                case "SqlDecimal":
                    return "DECIMAL(18,0)";
                case "SqlBinary":
                    return "VARBINARY(MAX)";
                case "SqlXml":
                    return "XML";
                default:
                    throw new ArgumentOutOfRangeException(clrType.Name + " is not a valid sql type.");
            }
        }

        private void DropAssemblyFunctions()
        {
            var sql = $@"DECLARE @sql NVARCHAR(MAX)
            SET @sql = 'DROP FUNCTION ' + STUFF(
            (
                SELECT
                    ', ' + assembly_method
                FROM
                    sys.assembly_modules
                WHERE
                assembly_id IN(SELECT assembly_id FROM sys.assemblies WHERE name = '{_assemblyName}')
                FOR XML PATH('')
            ), 1, 1, '')
            IF @sql IS NOT NULL EXEC sp_executesql @sql";
            _sqlHelper.ExecuteNonQuery(sql);
        }

        private void DropAssembly()
        {
            var sql = $@" IF EXISTS(SELECT 1 FROM sys.assemblies WHERE name = '{_assemblyName}')
                DROP ASSEMBLY {_assemblyName};";
            _sqlHelper.ExecuteNonQuery(sql);
        }

        private void CreateAssembly()
        {
            var bytes = new StringBuilder();
            using (FileStream dll = File.OpenRead(_clrAssembly.Location))
            {
                int @byte;
                while ((@byte = dll.ReadByte()) >= 0)
                    bytes.AppendFormat("{0:X2}", @byte);
            }

            var sql = $@"-- Create assembly '{0}' from dll
                CREATE ASSEMBLY [{_assemblyName}] 
                AUTHORIZATION [dbo]
                FROM 0x{bytes}
                WITH PERMISSION_SET = {_permissionSet};";
            _sqlHelper.ExecuteNonQuery(sql);
        }

        private void CreateAssemblyFunctions()
        {
            foreach (Type classInfo in _clrAssembly.GetTypes())
            {
                foreach (MethodInfo methodInfo in GetAssemblySQLFunction(classInfo))
                {
                    var sql = BuildSQLFunction(methodInfo);
                    _sqlHelper.ExecuteNonQuery(sql);
                    Console.WriteLine($"Function {methodInfo.Name} installed!");
                }
            }
        }

        private List<MethodInfo> GetAssemblySQLFunction(Type ClassInfo)
        {
            return ClassInfo.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(x => Attribute.IsDefined(x, typeof(SqlFunctionAttribute))).ToList();
        }

        private string BuildSQLFunction(MethodInfo methodInfo)
        {
            var methodParameters = new StringBuilder();
            var firstParameter = true;


            foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
            {
                if (firstParameter)
                    firstParameter = false;
                else
                    methodParameters.Append(", ");
                methodParameters.Append($@"@{paramInfo.Name} {ConvertClrTypeToSql(paramInfo.ParameterType)}");
            }
            var returnType = ConvertClrTypeToSql(methodInfo.ReturnParameter.ParameterType);
            var methodName = methodInfo.Name;
            var className = (methodInfo.DeclaringType.Namespace == null ? "" : methodInfo.DeclaringType.Namespace + ".") + methodInfo.DeclaringType.Name;
            var externalName = $@"{_assemblyName}.[{className}].{methodName}";
            var sql = $@"CREATE FUNCTION {methodName}({methodParameters}) RETURNS {returnType} AS EXTERNAL NAME {externalName};";
            return sql;
        }

        #endregion  
    }
}
