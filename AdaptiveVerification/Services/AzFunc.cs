namespace AdaptiveVerification.Services
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.IO;

    public class AzFunc
    {
    }
    public static class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public static void Run(
            [BlobTrigger("verificationresults/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,string name,ILogger log)
        {
            log.LogInformation($"Blob trigger function processed blob\n Name: {name} \n Size: {myBlob.Length} Bytes");

            using (var reader = new StreamReader(myBlob))
            {
                var jsonData = reader.ReadToEnd();
                var verificationData = JsonConvert.DeserializeObject<VerificationData>(jsonData);

                // Process the verification data as needed
                log.LogInformation($"Customer Email: {verificationData.CustomerData.Email}");
                log.LogInformation($"Verification Result: {verificationData.VerificationResult}");
            }
        }

        public class VerificationData
        {
            public CustomerData CustomerData { get; set; }
            public VerificationResult VerificationResult { get; set; }
        }

        public class CustomerData
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public int AccountAge { get; set; }
            public string Geolocation { get; set; }
        }

        public class VerificationResult
        {
            public string RiskLevel { get; set; }
            public string VerificationMethod { get; set; }
        }
    }

}

