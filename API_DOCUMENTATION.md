# TelcoNet API Documentation
*Base URL: `http://localhost:5096/api` (or your deployed server URL)*

## Authentication 🔐

### 1. Login
- **Endpoint:** `POST /auth/login`
- **Description:** Authenticates a user and returns a JWT token.
- **Request Body:**
  ```json
  {
    "email": "admin@noc.com",
    "password": "Admin@123"
  }
  ```
- **Response:**
  ```json
  {
    "token": "eyJhbG...",
    "fullName": "Admin User",
    "email": "admin@noc.com",
    "role": "Admin",
    "expiresAt": "2026-04-28T13:44:44Z"
  }
  ```

---
*⚠️ **Note:** All endpoints below require the JWT token in the header: `Authorization: Bearer <token>`*
---

## Network & Infrastructure 🗼

### 2. Network Health Summary
- **Endpoint:** `GET /network/health`
- **Description:** Gets high-level stats for the top-nav or dashboard summary (Healthy/Degraded/Critical counts).
- **Response:**
  ```json
  {
    "overallStatus": "Critical",
    "totalNodes": 15,
    "healthyNodes": 9,
    "degradedNodes": 3,
    "downNodes": 3,
    "activeOutages": 3
  }
  ```

### 3. Get Network Nodes (For Map View)
- **Endpoint:** `GET /network/nodes?region={optionalRegion}`
- **Description:** Returns all towers/hubs with their GPS coordinates and status colors for the Map component.
- **Response:**
  ```json
  {
    "region": "All",
    "nodes": [
      {
        "nodeId": "LW-001",
        "name": "Lagos West Hub",
        "region": "Lagos West",
        "lat": 6.4541,
        "lng": 3.3947,
        "status": "degraded",
        "nodeType": "Hub"
      }
    ]
  }
  ```

### 4. Incident Timeline
- **Endpoint:** `GET /network/timeline?region={optionalRegion}`
- **Description:** Returns the chronological list of events/alerts for the right-hand Timeline sidebar.
- **Response:**
  ```json
  {
    "incidents": [
      {
        "time": "13:28",
        "title": "Secondary backhaul activated",
        "description": "40% of traffic rerouted",
        "severity": "resolved"
      }
    ]
  }
  ```

### 5. Active Outages
- **Endpoint:** `GET /network/outages?region={optionalRegion}`
- **Description:** Returns a list of current network outages and affected users.

### 6. Get Alerts
- **Endpoint:** `GET /network/alerts?severity={optionalSeverity}`
- **Description:** Returns the alerts feed (Critical, Warning, Info, etc.).

---

## Analytics & Dashboard 📊

### 7. Dashboard KPIs
- **Endpoint:** `GET /dashboard/kpis`
- **Description:** Returns the data for the 4 main KPI cards (Latency, Packet Loss, Throughput, Active Users).
- **Response:**
  ```json
  {
    "avgLatency": { "value": 101, "unit": "ms", "changePercent": 0, "scope": "Lagos West" },
    "packetLoss": { "value": 2, "unit": "%", "changePercent": 0, "scope": "Network-wide" }
    // ... throughput and activeUsers
  }
  ```

### 8. Latency Chart Data
- **Endpoint:** `GET /dashboard/charts/latency?timeRange=24h`
- **Description:** Returns formatted multi-line chart data for Latency across regions.

### 9. Throughput Chart Data
- **Endpoint:** `GET /dashboard/charts/throughput?timeRange=24h`
- **Description:** Returns area chart data for network throughput.

---

## AI Copilot 🤖

### 10. Chat with AI
- **Endpoint:** `POST /copilot/chat`
- **Description:** Send a message to the AI. It will use plugins to fetch real network data and respond. *(Currently awaiting Deployment Name to activate)*
- **Request Body:**
  ```json
  {
    "sessionId": null, // Send null for new chat, or string for existing chat
    "message": "Are there any outages in Lagos?"
  }
  ```
- **Response:**
  ```json
  {
    "sessionId": "abc-123",
    "response": "Yes, there is an active fiber cut in Lagos West affecting 8,500 users...",
    "pluginsUsed": ["OutageDetection"],
    "timestamp": "2026-04-27T14:00:00Z"
  }
  ```

### 11. Get Chat Sessions (Sidebar)
- **Endpoint:** `GET /copilot/sessions`
- **Description:** Gets the user's "Recent Queries" and "Saved Investigations" for the chat sidebar.

---

## Admin & User Management 👥 (Requires "Admin" Role)

### 12. Get All Users
- **Endpoint:** `GET /users`
- **Description:** Populates the User Management table.
- **Response:** Array of users containing `id, fullName, email, role, status, lastActive`

### 13. Update User Role
- **Endpoint:** `PUT /users/{id}/role`
- **Request Body:** `{ "role": "Operator" }` (Valid roles: `Admin`, `Operator`, `Viewer`)

### 14. Audit Logs
- **Endpoint:** `GET /audit-logs?page=1&pageSize=50`
- **Description:** Returns the security audit trail of all API requests made by users.
