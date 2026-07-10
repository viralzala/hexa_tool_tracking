using Swashbuckle.Swagger;
using System.Web.Http.Description;

namespace HexaERP.MVC.Utility
{
    public class AddAuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters != null)
            {
                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    description = "access token",
                    required = true,
                    type = "string",
                    @default = "bearer "
                    //schema = new 
                    //{
                    //    type = "String",
                    //    Default = new OpenApiString("")
                    //}
                });
            }
        }
    }
}