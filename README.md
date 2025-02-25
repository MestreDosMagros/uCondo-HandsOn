
# Projeto desafio técnico uCondo

Desafio técnico para a empresa uCondo, consistindo em uma API REST com um CRUD de contas seguindos os padrões de boas práticas REST.

## Tecnologias usadas

- .net9
- Docker
- MySQL 8.4
- OpenApi

## Usabilidade 

1. Antes de cadastrar uma conta, devemos cadastrar um tipo de conta, no endpoint POST: /account-type
2. Consultar o próximo código sugerido em GET accounts/next-code
3. Cadastrar a conta no endpoint POST: /accounts

### Endpoints

POST: /accounts
GET: /accounts
GET: /accounts/{id}
PUT: /accounts
DELETE: /accounts/{id}
GET: /accounts/next-code?accountId={id}

POST: /account-types
GET: /account-types
GET: /account-types/{id}
PUT: /account-types
DELETE: /account-types/{id}

## Buildando a API

1. Na pasta src/Api rodar o comando `dotnet build`

## Testes

1. Na pasta src/Api rodar o comando `dotnet test`

## Rodando a API com Docker

1. Rodar o comando `docker run --name ucondo-db  -e MYSQL_ROOT_PASSWORD=root  -e MYSQL_DATABASE=ucondo  -e MYSQL_USER=admin  -e MYSQL_PASSWORD=admin  -p 3306:3306  -d mysql:8.4` para criar o container da base MySql versão 8; 
2. Buildar a imagem do Docker usar o comando `docker build -t ucondo ../ -f .\Api\Dockerfile` dentro da pasta /src do projeto;
3. Rodar o container usar o comando `docker run --link mysql8 -p 8080:8080 --name ucondo ucondo`

## Rodando a API com Docker compose (Recomendado)

1. Rodar o comando `docker compose up` dentro da pasta raiz do projeto;
2. Pode acontecer 

## Especificação da API

Todos os endpoints são especificados num arquivo OpenApi 3.0, que depois de ser executada a API, estará disponível na rota `openapi/v1.json`, podendo ser importada para qualquer programa que consiga ler essas documentações eg: Postman, Insomnia, Redoc