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
            // Get configuration values
            string modelId = _configuration["AzureOpenAI:ModelId"]!;
            string endpoint = _configuration["AzureOpenAI:Endpoint"]!;
            string apiKey = _configuration["AzureOpenAI:ApiKey"]!;

            // Create a kernel builder with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            // Build and return the kernel
            return builder.Build();
        }
        [HttpPost("verify")]

        public async Task<IActionResult> VerifyCustomer([FromBody] CustomerData customerData)
        {
            // Step 1: Analyze customer data using Azure OpenAI
            var riskProfile = await AnalyzeCustomerData(customerData);
            // Step 2: Determine verification requirements based on risk profile
            var verificationRequirements = GetVerificationRequirements(riskProfile);
            // Step 3: Perform verification (this could involve calling another service)
            var verificationResult = await PerformVerification(customerData, verificationRequirements);
            // Step 4: Store results in Azure Storage
            await StoreVerificationResult(customerData, verificationResult);
            return Ok(verificationResult);
        }
        private async Task<string> AnalyzeCustomerData(CustomerData customerData)
        {
            try
            {
                // Serialize customer data to JSON for analysis
                var customerDataJson = JsonSerializer.Serialize(customerData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Create a comprehensive prompt for risk analysis

                string prompt = $@"
                You are a KYC assistant. Based on UTC time and user submission info (name, email), assign a KYC verification method.
                Risk window mapping:
                - 00:00–01:00 → low
                - 02:00–05:00 → medium
                - 06:00–10:00 → high

                For low: PIN verification.
                For medium: PIN + OTP.
                For high: Facial Recognition.

                Current UTC time: {DateTime.UtcNow}
                Name: {customerData.Name}
                Email: {customerData.Email}
                """;

                // Call Azure OpenAI using Semantic Kernel
                var result = await _kernel.InvokePromptAsync(prompt);

                // Parse the response to extract risk level
                var response = result.GetValue<string>();
                var riskLevel = ExtractRiskLevel(response);

                // Log the full response for audit purposes
                Console.WriteLine($"AI Analysis Response: {response}");

                return riskLevel;
            }
            catch (Exception ex)
            {
                // Log error and return default risk level
                Console.WriteLine($"Error in AnalyzeCustomerData: {ex.Message}");
                return "MEDIUM"; // Default to medium risk if analysis fails
            }
        }

        private string ExtractRiskLevel(string aiResponse)
        {
            // Extract risk level from AI response
            var lines = aiResponse.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("RISK_LEVEL:", StringComparison.OrdinalIgnoreCase))
                {
                    var riskLevel = line.Substring("RISK_LEVEL:".Length).Trim();
                    return riskLevel.ToUpper();
                }
            }

            // If no explicit risk level found, try to infer from response
            var responseUpper = aiResponse.ToUpper();
            if (responseUpper.Contains("CRITICAL")) return "CRITICAL";
            if (responseUpper.Contains("HIGH")) return "HIGH";
            if (responseUpper.Contains("LOW")) return "LOW";

            return "MEDIUM"; // Default fallback
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
            // Call Azure OpenAI to analyze data and return risk profile
            // This is a placeholder for actual implementation
            return "medium"; // Example risk profile
        }

        /*        private string GetVerificationRequirements(string riskProfile)
                {
                    // Define verification requirements based on risk profile
                    return riskProfile switch
                    {
                        "low" => "Basic verification",
                        "medium" => "Standard verification",
                        "high" => "Enhanced verification",
                        _ => "Unknown"
                    };
                }*/

        private async Task<VerificationResult> PerformVerification(CustomerData customerData, List<string> requirements)
        {
            // Placeholder implementation - replace with actual verification logic
            await Task.Delay(100); // Simulate async operation

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
            // Implement verification logic based on requirements
            // This is a placeholder for actual implementation
            return "Verification successful";
        }

        private async Task StoreVerificationResult(CustomerData customerData, string result)
        {
            // Store the verification result in Azure Storage
            // This is a placeholder for actual implementation
        }

        private async Task StoreVerificationResult(CustomerData customerData, VerificationResult result)
        {
            // Placeholder implementation - replace with actual Azure Storage logic
            await Task.Delay(50); // Simulate async storage operation
            Console.WriteLine($"Stored verification result for customer {customerData.Email}");
        }
    }

    public class CustomerData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        // Add other relevant fields
    }
    public class VerificationResult
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Requirements { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string RiskProfile { get; set; } = string.Empty;
        public Dictionary<string, object>? VerificationDetails { get; set; }
    }
}
