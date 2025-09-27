# BlazorSPAEntra

Aplicación SPA desarrollada con Blazor WebAssembly que implementa autenticación segura mediante Azure Entra ID. Incluye validación de usuario, protección de rutas y llamadas autenticadas a APIs protegidas.

## 🚀 Características principales

- 🔐 Autenticación con Azure Entra ID (antes Azure AD)
- 🧭 Navegación SPA con protección de rutas
- 📡 Llamadas HTTP seguras con validación de tokens JWT
- 🧪 Validación manual de claims y control de acceso
- 🧱 Arquitectura desacoplada y extensible

## 🛠️ Tecnologías utilizadas

- Blazor WebAssembly (.NET 8+)
- Azure Entra ID (OpenID Connect)
- ASP.NET Core API protegida
- MSAL.js / MSAL.NET para autenticación
- Azure Portal para configuración de manifiestos y permisos

## 📦 Requisitos previos

- Cuenta de Azure con acceso a Azure Entra ID
- Registro de aplicación en Azure Entra ID
- API protegida con validación de tokens
- SDK .NET 8 o superior

## ⚙️ Configuración rápida

1. Clona el repositorio:
   ```bash
   git clone https://github.com/jorgefdezsa/BlazorSPA.git

2. Configura los valores en wwwroot/appsettings.json:
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/{tenantId}",
    "ClientId": "{clientId}",
    "ValidateAuthority": true
  },
  "ApiBaseUrl": "https://{your-api-endpoint}"
}

dotnet run

Validación de tokensLa validación de tokens se realiza manualmente en la API, extrayendo claims relevantes y controlando el acceso por roles o scopes. Se recomienda usar Microsoft.IdentityModel.Tokens para validar la firma y los tiempos de expiración.


  
