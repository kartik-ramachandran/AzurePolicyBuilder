using System.Text;
using System.Xml.Linq;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class ArmTemplateService
{
    public Task<ArmTemplate> GenerateArmTemplateAsync(string policyXml, string scope)
    {
        var template = new ArmTemplate();

        // Add parameters
        template.Parameters.Add("apimServiceName", new
        {
            type = "string",
            metadata = new { description = "Name of the API Management service instance" }
        });

        if (scope == "api")
        {
            template.Parameters.Add("apiName", new
            {
                type = "string",
                metadata = new { description = "Name of the API" }
            });
        }

        // Add variables
        template.Variables.Add("apiManagementId", "[resourceId('Microsoft.ApiManagement/service', parameters('apimServiceName'))]");

        // Create policy resource
        var resource = CreatePolicyResource(policyXml, scope);
        template.Resources.Add(resource);

        return Task.FromResult(template);
    }

    public Task<string> GenerateBicepAsync(string policyXml, string scope)
    {
        var sb = new StringBuilder();

        // Parameters
        sb.AppendLine("@description('Name of the API Management service instance')");
        sb.AppendLine("param apimServiceName string");
        sb.AppendLine();

        if (scope == "api")
        {
            sb.AppendLine("@description('Name of the API')");
            sb.AppendLine("param apiName string");
            sb.AppendLine();
        }

        // Get existing APIM service
        sb.AppendLine("resource apimService 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {");
        sb.AppendLine("  name: apimServiceName");
        sb.AppendLine("}");
        sb.AppendLine();

        // API resource (if scope is api)
        if (scope == "api")
        {
            sb.AppendLine("resource api 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' existing = {");
            sb.AppendLine("  parent: apimService");
            sb.AppendLine("  name: apiName");
            sb.AppendLine("}");
            sb.AppendLine();

            // Policy
            sb.AppendLine("resource policy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {");
            sb.AppendLine("  parent: api");
            sb.AppendLine("  name: 'policy'");
            sb.AppendLine("  properties: {");
            sb.AppendLine("    format: 'xml'");
            
            // Escape the policy XML for Bicep
            var escapedXml = EscapeXmlForBicep(policyXml);
            sb.AppendLine($"    value: '''{escapedXml}'''");
            
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }
        else if (scope == "global")
        {
            sb.AppendLine("resource policy 'Microsoft.ApiManagement/service/policies@2023-05-01-preview' = {");
            sb.AppendLine("  parent: apimService");
            sb.AppendLine("  name: 'policy'");
            sb.AppendLine("  properties: {");
            sb.AppendLine("    format: 'xml'");
            
            var escapedXml = EscapeXmlForBicep(policyXml);
            sb.AppendLine($"    value: '''{escapedXml}'''");
            
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }

        return Task.FromResult(sb.ToString());
    }

    private object CreatePolicyResource(string policyXml, string scope)
    {
        if (scope == "api")
        {
            return new
            {
                type = "Microsoft.ApiManagement/service/apis/policies",
                apiVersion = "2023-05-01-preview",
                name = "[concat(parameters('apimServiceName'), '/', parameters('apiName'), '/policy')]",
                properties = new
                {
                    format = "xml",
                    value = policyXml
                },
                dependsOn = new[] { "[variables('apiManagementId')]" }
            };
        }
        else if (scope == "operation")
        {
            return new
            {
                type = "Microsoft.ApiManagement/service/apis/operations/policies",
                apiVersion = "2023-05-01-preview",
                name = "[concat(parameters('apimServiceName'), '/', parameters('apiName'), '/', parameters('operationName'), '/policy')]",
                properties = new
                {
                    format = "xml",
                    value = policyXml
                },
                dependsOn = new[] { "[variables('apiManagementId')]" }
            };
        }
        else // global
        {
            return new
            {
                type = "Microsoft.ApiManagement/service/policies",
                apiVersion = "2023-05-01-preview",
                name = "[concat(parameters('apimServiceName'), '/policy')]",
                properties = new
                {
                    format = "xml",
                    value = policyXml
                },
                dependsOn = new[] { "[variables('apiManagementId')]" }
            };
        }
    }

    private string EscapeXmlForBicep(string xml)
    {
        // Format and escape XML for Bicep multi-line string
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString(SaveOptions.DisableFormatting);
        }
        catch
        {
            return xml;
        }
    }
}
