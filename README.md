# From 0 to Senior AI — Capítulo 1: Instalación y Backend

Serie educativa para aprender desarrollo de software construyendo proyectos con
Codex, Claude Code o Gemini mediante Google Antigravity, sin convertir al agente
en un sustituto del aprendizaje.
Este repositorio contiene el primer capítulo de la serie:

1. **Instalación y Backend** — este repositorio.
2. **Frontend y pruebas automatizadas** — próximo capítulo.
3. **Harness** — capítulo final.

La aplicación de referencia administra categorías, productos, pedidos y sus
elementos con ASP.NET Core 8, Entity Framework Core y PostgreSQL.

## Entorno guiado por el agente

El prompt le pide al agente comprobar y, con autorización, guiar la instalación
de estas herramientas oficiales:

- [Docker Desktop](https://docs.docker.com/desktop/) en Windows/macOS, o
  [Docker Engine con Compose](https://docs.docker.com/engine/install/) en Linux;
- [Git](https://git-scm.com/downloads);
- [GitHub CLI](https://github.com/cli/cli#installation).

No es necesario instalar .NET ni PostgreSQL en la máquina anfitriona. Docker
descarga el SDK para compilar y probar, el runtime para ejecutar la API y la
imagen oficial de PostgreSQL.

La persona completa personalmente el registro o login de GitHub en el navegador.
Después de confirmar nombre, propietario y visibilidad, el agente puede preparar
el primer commit y crear el repositorio o fork personal mediante GitHub CLI.

## Ejecutar

```bash
docker compose up --build
```

Cuando ambos contenedores estén listos, abre:

- Swagger: <http://localhost:5080/swagger>
- API: <http://localhost:5080/api/products>

La API aplica la migración de EF Core al iniciar en `Development`. Es práctico
para este laboratorio local; en producción las migraciones deberían ejecutarse
como un paso controlado del despliegue.

Para detener los contenedores:

```bash
docker compose down
```

Para detenerlos y borrar también la base local:

```bash
docker compose down --volumes
```

El último comando elimina los datos almacenados en el volumen del laboratorio.

## Ejecutar las pruebas en Docker

```bash
docker build --target test -t from-0-to-senior-ai-tests .
```

La construcción falla si falla alguna prueba.

## Aprender con un agente

1. Instala en VS Code la extensión oficial de Codex, Claude Code o Google
   Antigravity.
2. Abre la carpeta completa del repositorio.
3. Inicia una conversación nueva.
4. Pide al agente leer [`docs/PROMPT.md`](docs/PROMPT.md) en español o
   [`docs/PROMPT.en.md`](docs/PROMPT.en.md) en inglés. `AGENTS.md`, `CLAUDE.md` y
   `GEMINI.md` hacen que cada agente cargue el contrato correspondiente.
5. Responde las preguntas del agente y avanza una etapa a la vez.

El prompt referencia este repositorio y le ordena al agente explicar antes de
editar. Los archivos de instrucciones de cada herramienta apuntan al mismo
contrato para mantener una conducta consistente.

### Modelo recomendado

Como mínimo recomendado para seguir el taller:

- **Codex:** GPT-5.6 Sol con esfuerzo `medium`.
- **Claude Code:** Claude Opus 5.5 con esfuerzo `medium`.
- **Google Antigravity:** Gemini 3.1 Pro con esfuerzo `high`.

Modelos posteriores o niveles superiores son opcionales. Esta API no requiere
el modelo más costoso: importa más mantener pasos pequeños, revisar los cambios
y ejecutar las pruebas.

## Flujo de una petición

```text
HTTP request
→ routing
→ controller
→ service
→ AppDbContext / Entity Framework Core
→ PostgreSQL
→ service result
→ controller
→ HTTP response
```

## Estructura

```text
src/FromZeroToSeniorAI.Api/
├── Controllers/    recibe HTTP y decide el código de respuesta
├── Services/       contiene las reglas de la aplicación
├── Data/           configura EF Core y las migraciones
├── Entities/       representa los datos persistidos
├── DTOs/           define los datos que entran y salen de la API
└── Program.cs      configura e inicia la aplicación

tests/              pruebas de las reglas principales
docs/               prompt y material de aprendizaje
```

No hay CQRS, MediatR, repositorios genéricos, AutoMapper ni interfaces sin una
necesidad concreta. El mapeo es explícito para que pueda leerse y depurarse.

## Endpoints

| Método | Ruta | Propósito |
|---|---|---|
| GET | `/api/categories` | Listar categorías |
| POST | `/api/categories` | Crear una categoría |
| GET | `/api/products` | Listar productos |
| GET | `/api/products/{id}` | Consultar un producto |
| POST | `/api/products` | Crear un producto |
| PUT | `/api/products/{id}` | Reemplazar un producto |
| DELETE | `/api/products/{id}` | Eliminar un producto |
| GET | `/api/orders` | Listar pedidos con sus elementos |
| POST | `/api/orders` | Crear un pedido |
| PATCH | `/api/orders/{id}/status` | Cambiar el estado de un pedido |
| DELETE | `/api/orders/{id}` | Eliminar un pedido |

El archivo
[`FromZeroToSeniorAI.Api.http`](src/FromZeroToSeniorAI.Api/FromZeroToSeniorAI.Api.http)
incluye peticiones que pueden ejecutarse desde VS Code.

## Licencia

[MIT](LICENSE)
