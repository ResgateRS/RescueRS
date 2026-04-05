# ResgateRS

API REST para coordenação de operações de resgate em situações de emergência.

## Stack

- .NET 8.0
- Oracle Database
- Entity Framework Core
- JWT Authentication
- Docker

## Configuração

1. Configurar `appsettings.json` com conexão Oracle e segredo JWT:

```json
{
  "ConnectionStrings": {
    "ResgateRSDbContext": "sua-connection-string"
  },
  "AuthenticationSettings": {
    "TokenSecret": "seu-secret",
    "ExpiresInMinutes": 60
  }
}
```

2. Aplicar migrations:

```bash
dotnet ef database update --project RescueRS
```

## Executar

**Local:**
```bash
dotnet run --project RescueRS
```

**Docker:**
```bash
docker-compose up
```

API disponível em `http://localhost:8094`

## Endpoints Principais

### Autenticação
- `POST /api/v1/Login` - Login de usuário

### Resgates
- `POST /api/v1/Rescue/Request` - Solicitar resgate
- `GET /api/v1/Rescue/ListPendingRescues` - Listar pendentes
- `GET /api/v1/Rescue/ListPendingRescuesByProximity` - Listar por proximidade
- `GET /api/v1/Rescue/Details?rescueId={id}` - Detalhes
- `POST /api/v1/Rescue/Confirm` - Confirmar atendimento
- `POST /api/v1/Rescue/Start` - Iniciar operação
- `POST /api/v1/Rescue/Cancel` - Cancelar
- `GET /api/v1/Rescue/ListMyRescues` - Meus resgates
- `GET /api/v1/Rescue/ListInProgressRescues` - Em andamento
- `GET /api/v1/Rescue/ListCompletedRescues` - Completados

Documentação Swagger: `/swagger`

## Estrutura de Resgate

```json
{
  "latitude": 0.0,
  "longitude": 0.0,
  "adultsNumber": 0,
  "childrenNumber": 0,
  "elderlyNumber": 0,
  "disabledNumber": 0,
  "animalsNumber": 0,
  "contactPhone": "string",
  "description": "string"
}
```

## Status

- `0` - Pending
- `1` - InProgress
- `2` - Completed
