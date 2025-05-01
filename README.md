
# 📘 Logging Broker – Centralized Logging Service

**LoggingBroker** is a centralized logging API for microservice architectures, allowing services to send structured logs to Loki.

- Logs are pushed via HTTP POST and enriched with labels before being forwarded to Loki.
- Logs can be queried using HTTP GET with advanced filtering options.
- The service is built with .NET 8 using modern C# features and uses Serilog for log forwarding.

---

## 🛠️ Getting Started

1. Clone the repository:
```bash
git clone https://github.com/emreinan/loggingservice.git
```

2. Start the service using Docker Compose:
```bash
cd loggingservice/src/logging-service/logging-broker
docker-compose up -d
```

3. To test using Postman:
- Import the file `docs/LoggingBrokerApi.postman_collection.json` into Postman.

---

## 🔗 API Overview (Brief)

### POST `/api/v1/logs`

Example body:

```json
{
  "Source": "auth-service",
  "LogLevel": "warning",
  "Message": "User entered incorrect password 3 times.",
  "Parameters": {
    "userId": "4567"
  },
  "EventUnixTimeMs": 1742498360601
}
```

- The `Message` is stored as the log entry.
- Other fields are converted to labels.
- `Parameters` are prefixed with `param_`.

### GET `/api/v1/logs`

Query logs with filters. Full usage is documented in `docs/LoggingBrokerAPI – Request Documentation.md`.

---

## 🧪 Testing

- **Unit Tests**:  
  Service logic is tested with FluentAssertions. Core components like services and utilities are covered.

- **Integration Tests**:  
  A temporary Loki container is created. Real HTTP requests are sent, then the container is removed after validation.

---

## 📂 Documentation

Available in the `docs/` directory:

- `LoggingBrokerApi.postman_collection.json` → Postman requests
- `LoggingBrokerAPI – Request Documentation.md` → Full API reference

---

## 📌 Contribution

- Development takes place on the `dev` branch.
- If you'd like to contribute, fork the repository and open pull requests to the `dev` branch.

---

## 🧩 Flexibility & Extensibility

- Can switch from Loki to other logging backends like Elasticsearch or Graylog.
- Supports custom metadata, label extensions, and multi-destination sinks.
- Log formatting and routing can be fully customized.
