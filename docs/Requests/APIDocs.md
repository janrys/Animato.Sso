# Animato SSO

baseUrl = 

DEV: https://animato-sso-dev-api.azurewebsites.net/

TEST: ...

PROD: ...

Swagger documentation 

```js
GET {{baseUrl}}/swagger/index.html
```

## Test environment

Test applications: 

| client_id | secret | redirect_uri |
| ----------- | ----------- | ----------- |
| animato:crm | secret1, secret2 | https://crm.animato.cz, https://crm.animato.com |
| test:app1 | secret1, secret2 | https://testapp1.animato.cz, https://testapp1.animato.com |
| animato:norights | secret1, secret2 | https://norights.animato.cz, https://norights.animato.com |

Test users: 

| Login | Password |
| ----------- | ----------- |
| tester@animato.cz | testpass |
| admin@animato.cz | adminpass |


Data in test environment are refreshed after each restart.

## Configuration

Get basic information about SSO service settings, endpoints, certificates
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

## Authorization - implicit flow


```js
GET {{baseUrl}}/authorize?response_type=token&client_id=animato:crm&redirect_uri=https://crm.animato.cz
```

User is not logged in - response is HTML form for user interactive log in:

```json
HTTP/1.1 200 OK

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Animato autorizační služba</title>
</head>
<body>
  ...
```

... after succesful login:

```json
HTTP/1.1 302 OK
Location: https://crm.animato.cz#access_token=eyJhbGciOiJIUzI1NiIsIn....
```

## Authorization - code grant flow

```js
GET {{baseUrl}}/authorize?response_type=code&client_id=animato:crm&redirect_uri=https://crm.animato.cz
```

User is not logged in - response is HTML form for user interactive log in:

```json
HTTP/1.1 200 OK

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Animato autorizační služba</title>
</head>
<body>
  ...
```

... after succesful login:

```json
HTTP/1.1 302 REDIRECT
Location: Location: https://crm.animato.cz?code=8L3YJh2ZbAsjsPCo1HVcXR9IkGtLp1
```

Authorization code expires in 10 minutes and can be used just once.
```js
POST {{baseUrl}}/token

{
  "grant_type": "authorization_code",
  "code": "8L3YJh2ZbAsjsPCo1HVcXR9IkGtLp1",
  "client_id": "animato:crm",
  "redirect_uri": "https://crm.animato.cz",
  "client_secret": "secret1"
}
```

```json
{
  "token_type": "Bearer",
  "access_token": "eyJhbGciOiJIUzI1NiIsI...",
  "expires_in": 3599,
  "refresh_token": "pWWgqMjV7DWVJF8V51UBP...",
  "refresh_token_expires_in": 86399,
  "id_token": "eyJhbGciOiJIUzIdnF54...",
}
```

Access token and Id token are JWT tokens and can be decoded to get additional information.
Access token contains security related data for authorized user.
Id token contains personal data for authorized user.
Refresh token is long term token used to get new access token after it's expiration without need to user login.

## Token status
Check and validate token. Receive decoded information.

```js
POST {{baseUrl}}/token-info

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5...."
}
```

Active valid token
```json
{
  "active": true,
  "client_id": "animato:crm",
  "username": "tester@animato.cz",
  "aud": "animato:crm",
  "iss": "http://animato-sso-dev-api.azurewebsites.net:80/",
  "exp": 1655670302,
  "iat": 1655666702
}
```

Invalid token
```json
{
  "active": false
}
```

## Refresh token
Use long life refresh token to get new access token after it's expiration without need to new user login.

```js
POST {{baseUrl}}/token-info

{
  "grant_type": "refresh_token",
  "refresh_token": "pKfEqdEPQKw3TeWUUhMLnDx5bsymn...",
  "client_id": "animato:crm",
  "client_secret": "secret1"
}
```

```json
{
  "token_type": "Bearer",
  "access_token": "eyJhbGciOiJIUzI1NiI...",
  "expires_in": 3599
}
```

## Revoke token
Invalidate token. Invalidated token cannot be used for future authorization.

```js
POST {{baseUrl}}/revoke

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5...."
}
```

```js
HTTP/1.1 200 OK
```

## Logout current user
Logout current user, revoke all active tokens for current user.

```js
POST {{baseUrl}}/logout
Cookie: ....
```

```js
HTTP/1.1 200 OK
```

## User information

Current user information, onboarding QR code for TOTP, authorization method, status flags, etc.
```js
GET {{baseUrl}}/login/me
```

# Administration

List of registered applications

```js
GET {{baseUrl}}/api/application
```

Get specific application

```js
GET {{baseUrl}}/api/application/{id}
```
{id} - application id

Each application can have multiple application roles

```js
GET {{baseUrl}}/api/application/{id}/role
```

POST/PUT/DELETE - create, update, delete applications and roles


List of registered users

```js
GET {{baseUrl}}/api/user
```

List of registered users

```js
GET {{baseUrl}}/api/user/{id}
```
{id} - user id

POST/PUT/DELETE - create, update, delete user

List of roles for specific user
```js
GET {{baseUrl}}/api/user/{id}/role
```
{id} - user id

Assign user specific application role
```js
POST {{baseUrl}}/api/user/{id}/role/{roleId}
```

Remove user specific application role
```js
DELETE {{baseUrl}}/api/user/{id}/role/{roleId}
```