# qswag

Fast & Light Swagger generator for .NET Core

For more information, checkout the [documentation](https://swimlane.gitbooks.io/qswag/content/).

The documentation is not complete. So here is a quick sample of a controller that will document your other controllers.

```
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QSwagGenerator;
using QSwagSchema;

namespace API.Controllers
{
    /// <summary>
    /// Swagger spec controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class SwaggerController : Controller
    {
        private readonly List<Type> _types;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerController"/> class.
        /// </summary>
        public SwaggerController()
        {
            _types = new List<Type>
            {
                typeof(ApplicationController),
                typeof(ApplicationHistoryController),
                typeof(ApplicationValidateController),
                typeof(AssetController),
                typeof(CredentialsController),
                typeof(DashboardController),
                typeof(DataImportController),
                typeof(GroupsController),
                typeof(RecordController),
                typeof(ReportsController),
                typeof(RolesController),
                typeof(SearchController),
                typeof(SettingsController),
                typeof(TaskController),
                typeof(ValuesListController),
                typeof(WorkflowController),
                typeof(WorkspaceController)

            };
        }
        /// <summary>
        /// Gets the swagger.
        /// </summary>
        /// <returns>Swagger specification Json
        /// </returns>
        [AllowAnonymous]
        [HttpGet("/swagger")]
        public ActionResult GetSwagger()
        {
            var httpRequest = HttpContext?.Request;
            var generatorSettings = new GeneratorSettings(httpRequest)
            {
                DefaultUrlTemplate = "/[controller]/{id?}",
                IgnoreObsolete = true,
                Info = new Info() { Title = "Swimlane API", Version = "3.0" },
                XmlDocPath = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, "xml"),
                SecurityDefinitions = new Dictionary<string, SecurityDefinition>()
                {
                    {
                        "jwt_token",
                        new SecurityDefinition("Authorization", SecuritySchemeType.ApiKey) {In = Location.Header}
                    }
                },
                JsonSchemaLicense = Core.Licenses.JsonSchema
            };
            generatorSettings.Security.Add(new SecurityRequirement("jwt_token"));
            var generateForControllers = WebApiToSwagger.GenerateForControllers(_types, generatorSettings, nameof(GetSwagger));
            return new FileContentResult(Encoding.UTF8.GetBytes(generateForControllers), "application/json");
        }



        #region Access: Private

        private Type GetTypeFromString(string type)
        {
            var typeFromString = Type.GetType(type);
            if (typeFromString != null)
                return typeFromString;
            if (!type.Contains("."))
                return GetTypeFromString(string.Join(".", GetType().Namespace, type));
            if (!type.EndsWith("controller", StringComparison.CurrentCultureIgnoreCase))
                return GetTypeFromString(string.Concat(type, "Controller"));
            return null;
        }

        #endregion
    }
}
```
