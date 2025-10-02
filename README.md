# 🤖 AIAdaptiveVerification

## Overview
**AIAdaptiveVerification** is a **.NET-based** solution designed to validate user information through **adaptive AI-powered prompts**.  
This project leverages cutting-edge AI technologies to provide secure, intelligent, and efficient verification processes.

---

## ✨ Features
- **Adaptive Verification** – Dynamically adjusts verification processes based on user input and risk assessment.  
- **AI-Powered Analysis** – Utilizes machine learning models for intelligent user information validation.  
- **.NET SDK Integration** – Built on the robust .NET framework for enterprise-grade performance.  
- **Multi-Source Data Processing** – Handles diverse data inputs and formats seamlessly.  
- **Real-Time Validation** – Provides immediate verification results with detailed feedback.

---

## 🛠️ Technologies Used
- **.NET SDK** – Core framework  
- **Azure AI Foundry** – AI model deployment and management  
- **OpenAI** – Advanced language model capabilities  
- **Entity Framework** – Data access and management  
- **ASP.NET Core** – Web API framework  

---

## 📋 Prerequisites
Before setting up, ensure you have:
- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or later  
- Azure AI Foundry account and credentials  
- OpenAI API access  
- SQL Server or compatible database  

---

## ⚙️ Configuration

### 1️⃣ Set up your Azure AI Foundry project
Add your Azure AI Foundry configuration in **`appsettings.json`**:

```json
{
  "AzureAI": {
    "Endpoint": "your-project-endpoint",
    "Credential": "DefaultAzureCredential"
  }
}
