
# Projeto desafio técnico uCondo

Desafio técnico para a empresa uCondo, consistindo em um CRUD de contas.

## Tecnologias usadas

- .net9
- Docker
- MySQL 8.4
- OpenApi

## Rodando a API com Docker

1. Rodar o comando `docker run --name ucondo-db  -e MYSQL_ROOT_PASSWORD=root  -e MYSQL_DATABASE=ucondo  -e MYSQL_USER=admin  -e MYSQL_PASSWORD=admin  -p 3306:3306  -d mysql:8.4` para criar o container da base MySql versão 8; 
2. Buildar a imagem do Docker usar o comando `docker build -t ucondo ../ -f .\Api\Dockerfile` dentro da pasta /src do projeto;
3. Rodar o container usar o comando `docker run --link mysql8 -p 8080:8080 --name ucondo ucondo`

## Rodando a API com Docker compose

1. Rodar o comando docker compose up dentro da pasta /src do projeto;
2. Pode acontecer 

## Especificação da API

Todos os endpoints são especificados num arquivo OpenApi 3.0, que depois de ser executada a API, estará disponível na rota `openapi/v1.json`, podendo ser importada para qualquer programa que consiga ler essas documentações eg: Postman, Insomnia, Redoc