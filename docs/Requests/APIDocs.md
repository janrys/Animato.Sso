# Animato SSO

## Configuration


```js
GET {{baseUrl}}/.well-known/openid-configuration
```

```json
{
  "issuer": "http://animato-sso-dev-api.azurewebsites.net:80/",
  "authorization_endpoint": "https://animato-sso-dev-api.azurewebsites.net/authorize",
  "token_endpoint": "https://animato-sso-dev-api.azurewebsites.net/token",
  "revocation_endpoint": "https://animato-sso-dev-api.azurewebsites.net/revoke",
  "response_types_supported": [
    "code",
    "token"
  ]
}
```

## Authorization


```js
GET {{baseUrl}}/authorize?response_type=token&client_id=animato:crm&redirect_uri=https://crm.animato.cz
```

```json
{
  "issuer": "http://animato-sso-dev-api.azurewebsites.net:80/",
  "authorization_endpoint": "https://animato-sso-dev-api.azurewebsites.net/authorize",
  "token_endpoint": "https://animato-sso-dev-api.azurewebsites.net/token",
  "revocation_endpoint": "https://animato-sso-dev-api.azurewebsites.net/revoke",
  "response_types_supported": [
    "code",
    "token"
  ]
}
```