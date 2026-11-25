# Documentação das APIs — Locadora de Veículos

Backend em ASP.NET Core + Entity Framework Core + SQL Server Express.
Base URL local: `http://localhost:5084` (a porta pode variar conforme o `launchSettings.json`).
Interface de testes: `/swagger`.

---

## 1. Fabricantes — `/api/fabricantes`

| Método | Rota | Parâmetros | Corpo (body) | Códigos de resposta |
|---|---|---|---|---|
| GET | `/api/fabricantes` | — | — | `200 OK` — lista de fabricantes |
| GET | `/api/fabricantes/{id}` | `id` (rota, int) | — | `200 OK` com o fabricante · `404 Not Found` se não existir |
| POST | `/api/fabricantes` | — | `{ "nome": string, "paisOrigem": string, "ativo": bool }` | `201 Created` · `400 Bad Request` (validação) · `409 Conflict` (nome duplicado) |
| PUT | `/api/fabricantes/{id}` | `id` (rota, int) | mesmo formato do POST | `204 No Content` · `400 Bad Request` · `404 Not Found` · `409 Conflict` |
| DELETE | `/api/fabricantes/{id}` | `id` (rota, int) | — | `204 No Content` · `404 Not Found` · `400 Bad Request` (se houver veículos vinculados) |

**Regras de validação:** `nome` obrigatório (2 a 80 caracteres, único); `paisOrigem` até 60 caracteres.

---

## 2. Veículos — `/api/veiculos`

| Método | Rota | Parâmetros | Corpo (body) | Códigos de resposta |
|---|---|---|---|---|
| GET | `/api/veiculos` | — | — | `200 OK` — lista com nome do fabricante |
| GET | `/api/veiculos/{id}` | `id` (rota, int) | — | `200 OK` · `404 Not Found` |
| POST | `/api/veiculos` | — | `{ "placa", "modelo", "anoFabricacao", "quilometragem", "cor", "chassi", "status", "fabricanteId" }` | `201 Created` · `400 Bad Request` (validação ou fabricante inexistente) · `409 Conflict` (placa duplicada) |
| PUT | `/api/veiculos/{id}` | `id` (rota, int) | mesmo formato do POST | `204 No Content` · `400 Bad Request` · `404 Not Found` · `409 Conflict` |
| DELETE | `/api/veiculos/{id}` | `id` (rota, int) | — | `204 No Content` · `404 Not Found` · `400 Bad Request` (se houver aluguéis vinculados) |

**Regras de validação:** `placa` obrigatória (7–8 caracteres, única); `modelo` obrigatório; `anoFabricacao` entre 1950 e 2100; `quilometragem` ≥ 0; `fabricanteId` deve existir.

**Status possíveis:** `1` Disponível · `2` Alugado · `3` Manutenção · `4` Inativo.

---

## 3. Clientes — `/api/clientes`

| Método | Rota | Parâmetros | Corpo (body) | Códigos de resposta |
|---|---|---|---|---|
| GET | `/api/clientes` | — | — | `200 OK` — lista de clientes |
| GET | `/api/clientes/{id}` | `id` (rota, int) | — | `200 OK` · `404 Not Found` |
| POST | `/api/clientes` | — | `{ "nome", "cpf", "email", "telefone", "numeroCnh", "dataNascimento" }` | `201 Created` · `400 Bad Request` · `409 Conflict` (CPF ou e-mail duplicado) |
| PUT | `/api/clientes/{id}` | `id` (rota, int) | mesmo formato do POST | `204 No Content` · `400 Bad Request` · `404 Not Found` · `409 Conflict` |
| DELETE | `/api/clientes/{id}` | `id` (rota, int) | — | `204 No Content` · `404 Not Found` · `400 Bad Request` (se houver aluguéis vinculados) |

**Regras de validação:** `nome` obrigatório; `cpf` obrigatório, exatamente 11 dígitos numéricos, único; `email` obrigatório, formato válido, único.

---

## 4. Aluguéis — `/api/alugueis`

| Método | Rota | Parâmetros | Corpo (body) | Códigos de resposta |
|---|---|---|---|---|
| GET | `/api/alugueis` | — | — | `200 OK` — lista com nome do cliente e dados do veículo |
| GET | `/api/alugueis/{id}` | `id` (rota, int) | — | `200 OK` · `404 Not Found` |
| POST | `/api/alugueis` | — | `{ "clienteId", "veiculoId", "dataRetirada", "dataPrevistaDevolucao", "quilometragemInicial", "valorDiaria", "observacoes" }` | `201 Created` (calcula `valorTotal`) · `400 Bad Request` (cliente/veículo inexistente, veículo já alugado, datas inválidas) |
| PUT | `/api/alugueis/{id}` | `id` (rota, int) | mesmo formato do POST | `204 No Content` · `400 Bad Request` · `404 Not Found` |
| PATCH | `/api/alugueis/{id}/devolucao` | `id` (rota, int) | `{ "quilometragemFinal", "dataDevolucao" }` | `200 OK` com o aluguel finalizado · `400 Bad Request` · `404 Not Found` |
| DELETE | `/api/alugueis/{id}` | `id` (rota, int) | — | `204 No Content` (cancela o aluguel) · `400 Bad Request` (se já finalizado) · `404 Not Found` |

**Regras de negócio:** não permite alugar veículo que já tem aluguel em aberto; `valorTotal` é calculado automaticamente (diárias × valor da diária, mínimo 1 diária); ao registrar devolução, o veículo volta para status "Disponível" e sua quilometragem é atualizada.

---

## 5. Filtros com JOIN — `/api/filtros`

Rotas que atendem ao item 2.5 do enunciado (5 filtros, dois tipos de JOIN diferentes).

| # | Método | Rota | Parâmetros (query) | Tipo de JOIN | Códigos de resposta |
|---|---|---|---|---|---|
| 1 | GET | `/api/filtros/veiculos-por-fabricante` | `nome` (string, obrigatório) | INNER JOIN (Veículo × Fabricante) | `200 OK` · `400 Bad Request` (parâmetro ausente) |
| 2 | GET | `/api/filtros/alugueis-por-periodo` | `inicio`, `fim` (datas, obrigatórios) | INNER JOIN (Aluguel × Cliente × Veículo) | `200 OK` · `400 Bad Request` (período inválido) |
| 3 | GET | `/api/filtros/clientes-com-aluguel-ativo` | — | INNER JOIN (Cliente × Aluguel) | `200 OK` |
| 4 | GET | `/api/filtros/veiculos-sem-aluguel` | — | LEFT JOIN (Veículo × Aluguel) | `200 OK` |
| 5 | GET | `/api/filtros/clientes-com-total-alugueis` | — | LEFT JOIN (Cliente × Aluguel) com agregação | `200 OK` |

---

## Tratamento de erros (global)

Qualquer exceção não prevista nos controllers é capturada pelo `ExceptionMiddleware` e devolvida como JSON padronizado:

```json
{
  "sucesso": false,
  "status": 400,
  "mensagem": "descrição do erro"
}
```

| Situação | Status HTTP |
|---|---|
| Violação de restrição única (placa, CPF, e-mail) ou chave estrangeira inválida | `400 Bad Request` |
| Conflito de concorrência no banco | `409 Conflict` |
| Recurso não encontrado | `404 Not Found` |
| Erro inesperado não tratado | `500 Internal Server Error` |
