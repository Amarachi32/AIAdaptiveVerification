using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

/*namespace AdaptiveVerification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KCYController : ControllerBase
    {
    }
}*/

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace AdaptiveVerification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KCYController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Kernel _kernel;

        public KCYController(IConfiguration configuration)
        {
            _configuration = configuration;
            _kernel = InitializeKernel();
        }

        private Kernel InitializeKernel()
        {
            string modelId = _configuration["AzureOpenAI:ModelId"]!;
            string endpoint = _configuration["AzureOpenAI:Endpoint"]!;
            string apiKey = _configuration["AzureOpenAI:ApiKey"]!;

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            return builder.Build();
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCustomer([FromBody] CustomerData customerData)
        {
            var riskProfile = await AnalyzeCustomerData(customerData);
            var verificationRequirements = GetVerificationRequirements(riskProfile);
            var verificationResult = await PerformVerification(customerData, verificationRequirements);
            await StoreVerificationResult(customerData, verificationResult);
            return Ok(verificationResult);
        }

        private async Task<string> PerformSmallWebSearch(string name, string email)
        {
            using var httpClient = new HttpClient();
            string query = Uri.EscapeDataString($"{name} {email}");
            string url = $"https://s.jina.ai/?q={query}&size=1";
            var response = await httpClient.GetStringAsync(url);
            return response;
        }

        private async Task<string> PerformFullWebSearch(string name, string email)
        {
            using var httpClient = new HttpClient();
            string query = Uri.EscapeDataString($"{name} {email}");
            string url = $"https://s.jina.ai/?q={query}&size=5";
            var response = await httpClient.GetStringAsync(url);
            return response;
        }

        private async Task<string> AnalyzeCustomerData(CustomerData customerData)
        {
            try
            {
                var smallSearchResult = await PerformSmallWebSearch(customerData.Name, customerData.Email);
                var fullSearchResult = await PerformFullWebSearch(customerData.Name, customerData.Email);

                var systemMessage = @"
                You are a risk analyzer expert. Based on these public records and BVN records, propose a risk level.
                For example:
                <user>Has unpaid loans
                Suspected multiple identities</user>
                <assistant>{ ""risk"": ""High"" }</assistant>
                <user>No unpaid loans
                Single identity
                Good financial standing</user>
                <assistant>{ ""risk"": ""Low"" }</assistant>
                Now analyze the following:";

                                string userMessage = $@"
                <user>
                Full Name: {customerData.Name}
                Email: {customerData.Email}
                Web Summary (brief): {smallSearchResult}
                Web Details (full): {fullSearchResult}
                </user>";

                var prompt = $"{systemMessage}\n{userMessage}";
                var result = await _kernel.InvokePromptAsync(prompt);
                var response = result.GetValue<string>();
                Console.WriteLine($"AI Evaluation Response:\n{response}");

                return ExtractRiskLevel(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AnalyzeCustomerData: {ex.Message}");
                return "MEDIUM";
            }
        }

        private string ExtractRiskLevel(string aiResponse)
        {
            var lines = aiResponse.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("RISK_LEVEL:", StringComparison.OrdinalIgnoreCase))
                {
                    var riskLevel = line.Substring("RISK_LEVEL:".Length).Trim();
                    return riskLevel.ToUpper();
                }
            }

            var responseUpper = aiResponse.ToUpper();
            if (responseUpper.Contains("CRITICAL")) return "CRITICAL";
            if (responseUpper.Contains("HIGH")) return "HIGH";
            if (responseUpper.Contains("LOW")) return "LOW";

            return "MEDIUM";
        }

        private List<string> GetVerificationRequirements(string riskProfile)
        {
            return riskProfile.ToUpper() switch
            {
                "LOW" => new List<string>
                {
                    "Basic ID verification",
                    "Email confirmation"
                },
                "MEDIUM" => new List<string>
                {
                    "Government ID verification",
                    "Address confirmation",
                    "Phone verification",
                    "Employment verification"
                },
                "HIGH" => new List<string>
                {
                    "Enhanced ID verification",
                    "Utility bill confirmation",
                    "Income verification",
                    "Reference checks",
                    "Credit report review"
                },
                "CRITICAL" => new List<string>
                {
                    "Manual review required",
                    "Enhanced due diligence",
                    "Source of funds verification",
                    "Background check",
                    "Compliance officer approval"
                },
                _ => new List<string>
                {
                    "Standard verification package"
                }
            };
        }

        private async Task<string> AnalyzeCustomerDatas(CustomerData customerData)
        {
            // Placeholder for actual implementation
            return "medium";
        }

        private async Task<VerificationResult> PerformVerification(CustomerData customerData, List<string> requirements)
        {
            await Task.Delay(100); // Simulated async operation

            return new VerificationResult
            {
                CustomerEmail = customerData.Email,
                Status = "Pending",
                Requirements = requirements,
                Timestamp = DateTime.UtcNow,
                RiskProfile = await AnalyzeCustomerData(customerData)
            };
        }

        private async Task<string> PerformVerification(CustomerData customerData, string requirements)
        {
            // Placeholder
            return "Verification successful";
        }

        private async Task StoreVerificationResult(CustomerData customerData, string result)
        {
            // Placeholder for actual Azure Storage implementation
        }

        private async Task StoreVerificationResult(CustomerData customerData, VerificationResult result)
        {
            await Task.Delay(50); // Simulated async operation
            Console.WriteLine($"Stored verification result for customer {customerData.Email}");
        }
    }
}
