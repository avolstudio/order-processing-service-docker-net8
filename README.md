# Order Processing Service (.NET 8, Docker, RabbitMQ, MassTransit, Prometheus)

Minimal async order processing microservice built with .NET 8, PostgreSQL, EF Core, and RabbitMQ via MassTransit.


## Run
Make sure you have Docker installed and after:

docker-compose up --build


### How to
POST create order (idempotent)

Requires header Idempotency-Key (example: generated per client request).

curl -v -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: your-unique-key-123" \
  -d '{
    "customerId": 1212,
    "items": ["item1","item2"]
  }'

### Endpoints
- **POST:** http://localhost:8080/api/orders
- **Swagger UI:** http://localhost:8080/swagger/index.html 
- **RabbitMQ UI:** http://localhost:15672 (guest / guest)
- **Prometheus** http://localhost:8080/metrics

### Notes
- Orders are saved to Postgres.
- Processing happens asynchronously via RabbitMQ consumer (MassTransit).
- Database setup up on init with no migrations ( for simplicity )
- Swagger available
- Prometheus metrics available http://localhost:8080/metrics
- Basic request valiadation
- Idempotency-Key header
- Direct publish instead of Outbox patter impl
- Service apply discount policy and total amount calculations

### Assumptions

Test environment has Docker + Docker Compose installed.
RabbitMQ and PostgreSQL run as separate services (provided in docker-compose.yml).
Clients generate an idempotency key per logical request attempt.
Prometheus scraping or CI/CD are outside the scope of this test; /metrics available for scraping.
For minimal reproducible example I chose simplicity over production concerns (use of EnsureCreated() from my perspective can be acceptable for test task).


### Design choises
TotalAmout of order calculated on a server to mock anti-fraud behavior. It production environment anti-fraud policies should be perfomed on a dedicated service

MassTransit + RabbitMQ gives a full-featured message processing platform (retry policies, dead-letter, integration with consumers) with minimal code. MassTransit also supports EF Outbox, delayed redelivery, and integrates nicely with ASP.NET DI. Outbox pattern can be added for production version

Idempotency-Key header
Simple to implement and mirrors real-world approaches. Ensures duplicate POSTs (double-clicks / retries on a client) do not create duplicate orders.

Direct publish.Publish() after SaveChanges() was chosen for a minimal working example. Optionally Outbox or MassTransit EF Outbox can be enabled for stronger guarantees.

EnsureCreated is simplest to guarantee DB/tables exist on start. For more realistic flows, use EF migrations and db.Database.Migrate().

Provided both -console logs and Prometheus metrics:
orders_processed_total - simple counter
orders_total_amount - gauge accumulated by consumer (in-memory increment)