﻿using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Schema;
using QSwagGenerator.Models;
using QSwagSchema;

namespace QSwagGenerator.Generators
{
    internal class ParameterGenerator
    {
        private static Location GetBinding(ParameterInfo parameter, string httpPath, JSchema schema)
        {
            var attributes = parameter.GetCustomAttributes().ToDictionary(a => a.GetType().Name);
            if(attributes.ContainsKey(nameof(FromQueryAttribute)))
                return Location.Query;
            if (attributes.ContainsKey(nameof(FromBodyAttribute)))
                return Location.Body;
            if (attributes.ContainsKey(nameof(FromFormAttribute)))
                return Location.FormData;
            if (attributes.ContainsKey(nameof(FromHeaderAttribute)))
                return Location.Header;
            var paramRegex = new Regex(@"\{" + parameter.Name + @"[:\?\}]");
            if(attributes.ContainsKey(nameof(FromRouteAttribute)) || paramRegex.IsMatch(httpPath))
                return Location.Path;

            return SchemaGenerator.IsComplex(schema) ? Location.Body : Location.Query;
        }
        internal static Parameter CreateParameter(ParameterInfo parameter, string httpPath, SchemaGenerator schemaGenerator, XmlDoc doc)
        {
            var name = parameter.Name;
            var param = new Parameter { Name = name };
            var jSchema = schemaGenerator.GetSchema(parameter.ParameterType);
            param.In = GetBinding(parameter, httpPath, jSchema);
            param.Description = doc!=null && doc.Parameters.ContainsKey(name)
                ? doc.Parameters[name]
                : string.Empty;
            param.Required = SchemaGenerator.IsParameterRequired(parameter);
            if (param.In == Location.Body)
                param.Schema = schemaGenerator.MapToSchema(jSchema);
            else
            {
                var item = schemaGenerator.MapToItem(jSchema);
                param.Map(item);
            }
            return param;
        }
    }
}
