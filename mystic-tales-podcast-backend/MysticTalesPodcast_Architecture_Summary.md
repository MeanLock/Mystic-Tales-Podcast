# MYSTIC TALES PODCAST - MICROSERVICES ARCHITECTURE SUMMARY

> **Document Purpose**: Complete system architecture reference for AI prompts
> **Last Updated**: 2025-10-27
> **Project**: Mystic Tales Podcast Platform

---

## 📋 TABLE OF CONTENTS
1. [Infrastructure Components](#infrastructure-components)
2. [Service Architecture](#service-architecture)
3. [Communication Patterns](#communication-patterns)
4. [Load Balancing Strategy](#load-balancing-strategy)
5. [Kafka Configuration](#kafka-configuration)
6. [Database Schema](#database-schema)
7. [External Integrations](#external-integrations)
8. [High Availability Features](#high-availability-features)

---

## 🏗️ INFRASTRUCTURE COMPONENTS

### **1. Message Broker - Apache Kafka (KRaft Mode)**
- **Cluster ID**: kraft-cluster-1
- **Topology**: 3-node cluster (broker + controller combined)
- **Ports**:
  - kafka-broker-1: `9092` (container: kafka-broker-dev-001)
  - kafka-broker-2: `9093` (container: kafka-broker-dev-002)
  - kafka-broker-3: `9094` (container: kafka-broker-dev-003)
- **Configuration**:
  - Replication Factor: 3
  - Min ISR: 2
  - Auto Create Topics: false
  - Auto Leader Rebalance: true (check every 30s, trigger at 10% imbalance)
  - Transaction State Log Min ISR: 2
  - Transaction State Log Replication Factor: 3

### **2. Service Discovery & Configuration**
- **Consul**: 
  - Port: `8500`
  - Function: Service registry, health monitoring
  - Health Check Interval: 30s
  - Registered Services: 9 services (8 business services + 1 orchestrator)

### **3. Caching Layer**
- **Redis**:
  - Port: `6379`
  - Use Cases:
    - YARP routes/clusters cache (with TTL)
    - General application cache
    - Distributed cache

### **4. Database**
- **SQL Server**:
  - Port: `1433`
  - Version: Latest
  - Connection: Server=sql-server,1433;User Id=sa;Password=Banana100;TrustServerCertificate=true
  - Pattern: **Database per Service**
  
**Database List** (8 databases):
1. `MTP_UserDb_dev`
2. `MTP_SystemConfigurationDb_dev`
3. `MTP_BookingManagementDb_dev`
4. `MTP_PodcastDb_dev`
5. `MTP_SubscriptionDb_dev`
6. `MTP_ModerationDb_dev`
7. `MTP_TransactionDb_dev`
8. `MTP_SagaOrchestratorDb_dev`

### **5. Load Balancer & API Gateway**
- **Nginx**:
  - Ports: `80`, `443`
  - Algorithm: **least_conn** (least connections)
  - Function: L7 load balancer, SSL termination
  
- **API Gateway (YARP Reverse Proxy)**:
  - Instances: 3
  - Ports: `8041`, `8042`, `8043`
  - Containers: api-gateway-dev-001, api-gateway-dev-002, api-gateway-dev-003
  - Features:
    - Dynamic route building from Consul
    - Health check integration
    - Load balancing: **RoundRobin**
    - Route caching in Redis (with TTL)
    - Periodic service discovery refresh

---

## 🎯 SERVICE ARCHITECTURE

### **Microservices Overview** (8 services, each with 2 instances)

| Service Name | Instance 1 | Instance 2 | Database | Kafka Consumer Group | Entities |
|--------------|------------|------------|----------|---------------------|----------|
| **UserService** | 8046 | 8047 | MTP_UserDb_dev | user-service-consumer-group | 10 |
| **SystemConfigurationService** | 8051 | 8052 | MTP_SystemConfigurationDb_dev | system-configuration-service-consumer-group | 7 |
| **BookingManagementService** | 8056 | 8057 | MTP_BookingManagementDb_dev | booking-management-service-consumer-group | 12 |
| **PodcastService** | 8061 | 8062 | MTP_PodcastDb_dev | podcast-service-consumer-group | 30 |
| **SubscriptionService** | 8066 | 8067 | MTP_SubscriptionDb_dev | subscription-service-consumer-group | 12 |
| **ModerationService** | 8071 | 8072 | MTP_ModerationDb_dev | moderation-service-consumer-group | 18 |
| **TransactionService** | 8076 | 8077 | MTP_TransactionDb_dev | transaction-service-consumer-group | 7 |
| **SagaOrchestratorService** | 8081 | 8082 | MTP_SagaOrchestratorDb_dev | saga-orchestrator-service-consumer-group | 2 |

### **Service Details**

#### **1. UserService** [Ports: 8046, 8047]
**Entities (10)**:
- Role, Account, AccountPodcastListenHistory, PasswordResetToken
- PodcasterProfile, PodcastBuddyReview
- AccountFollowedPodcaster, AccountFavoritedPodcastChannel
- AccountFollowedPodcastShow, AccountSavedPodcastEpisode
- AccountNotification, NotificationType

#### **2. SystemConfigurationService** [Ports: 8051, 8052]
**Entities (7)**:
- SystemConfigProfile, PodcastSubscriptionConfig, PodcastSuggestionConfig
- BookingConfig, AccountConfig, AccountViolationLevelConfig, ReviewSessionConfig

#### **3. BookingManagementService** [Ports: 8056, 8057]
**Entities (12)**:
- BookingRequirementAttachFile, Booking, BookingNegotiation, BookingStatusTracking
- BookingStatus, BookingOptionalManualCancelReason
- BookingPodcastTrack, BookingProducingRequest, BookingProducingRequestPodcastTrackToEdit
- BookingChatRoom, BookingChatMessages, BookingChatMember

#### **4. PodcastService** [Ports: 8061, 8062]
**Entities (30)**:
- PodcastChannel, PodcastShow, PodcastEpisode, PodcastEpisodeListenSession
- PodcastEpisodeLicense, PodcastEpisodeLicenseType
- PodcastEpisodeIllegalContentTypeMarking, PodcastEpisodePublishDuplicateDetection
- PodcastEpisodePublishReviewSession, PodcastEpisodePublishReviewSessionStatusTracking
- PodcastEpisodePublishReviewSessionStatus, PodcastIllegalContentType, PodcastRestrictedTerm
- PodcastChannelStatus, PodcastShowStatus, PodcastEpisodeStatus
- PodcastChannelStatusTracking, PodcastShowStatusTracking, PodcastEpisodeStatusTracking
- PodcastCategory, PodcastSubCategory
- PodcastEpisodeSubscriptionType, PodcastShowsSubscriptionType
- PodcastShowReview, Hashtag
- PodcastChannelHashtag, PodcastShowHashtag, PodcastEpisodeHashtag
- PodcastBackgroundSoundTrack

#### **5. SubscriptionService** [Ports: 8066, 8067]
**Entities (12)**:
- SubscriptionCycleType, PodcastSubscriptionBenefit, MemberSubscriptionBenefit
- PodcastSubscription, PodcastSubscriptionCycleTypePrice, PodcastSubscriptionBenefitMapping
- PodcastSubscriptionRegistration, PodcastSubscriptionRegistrationBenefit
- MemberSubscription, MemberSubscriptionCycleTypePrice, MemberSubscriptionBenefitMapping
- MemberSubscriptionRegistration

#### **6. ModerationService** [Ports: 8071, 8072]
**Entities (18)**:
- PodcastBuddyReport, PodcastShowReport, PodcastEpisodeReport
- PodcastBuddyReportType, PodcastShowReportType, PodcastEpisodeReportType
- PodcastBuddyReportReviewSession, PodcastShowReportReviewSession, PodcastEpisodeReportReviewSession
- DMCAAccusation, CounterNotice, DMCANotice, LawsuitProof
- DMCAAccusationStatus, DMCAAccusationStatusTracking
- CounterNoticeAttachFile, DMCANoticeAttachFile, LawsuitProofAttachFile

#### **7. TransactionService** [Ports: 8076, 8077]
**Entities (7)**:
- TransactionType, TransactionStatus
- AccountBalanceTransaction, PodcastSubscriptionTransaction, MemberSubscriptionTransaction
- BookingTransaction, BookingStorageTransaction, AccountBalanceWithdrawalRequest

#### **8. SagaOrchestratorService** [Ports: 8081, 8082]
**Entities (2)**:
- SagaInstance (tracks: id, flowName, currentStepName, initialData, resultData, flowStatus, errorStepName, errorMessage, timestamps)
- SagaStepExecution (tracks: id, sagaInstanceId, stepName, topicName, stepStatus, requestData, responseData, errorMessage, createdAt)

---

## 🔄 COMMUNICATION PATTERNS

### **1. Synchronous Communication (HTTP/REST)**
```
Client 
  ↓
Nginx (least_conn) [80, 443]
  ↓
API Gateway (RoundRobin) [8041/8042/8043]
  ↓
YARP Proxy (fetches from Consul)
  ↓
Consul Service Discovery [8500]
  ↓
Service Instance (RoundRobin) [Instance 1 or 2]
```

**Cross-Service Query API**:
- Each service has generic query service
- Services query each other via HTTP through API Gateway
- Pattern: Service A → API Gateway → Service B

### **2. Asynchronous Communication (Event-Driven via Kafka)**
```
Service 
  ↓ (produces message)
Kafka Topic (domain-specific)
  ↓ (consumed by)
SagaOrchestratorService OR Target Service
  ↓ (orchestrates flow)
Kafka Topic (step-specific)
  ↓ (consumed by)
Service Handlers
  ↓ (emit result)
Kafka Topic
  ↓
SagaOrchestrator (determines next steps/flows)
```

**Event-Driven DAG Orchestrator Pattern**:
- Load YAML configuration at startup (no hot-reload)
- Flow execution based on event emissions
- **nextSteps**: Maintains same saga ID, sends parallel messages to Kafka
- **nextFlows**: Creates new saga ID, sends flow message to Kafka

**Message Handling**:
- Step handlers only read message body data
- No need to understand which flow the message belongs to
- Rollback messages handled by business logic in data body
- No guaranteed rollback (depends on business setup)

---

## ⚖️ LOAD BALANCING STRATEGY

### **Layer 1: Nginx (least_conn)**
- **Algorithm**: Least Connections
- **Reason**: 
  - Suitable for long-lived connections (WebSocket, streaming)
  - Better load distribution when connection processing times vary
  - Prevents overloading specific API Gateway instances
- **Targets**: 3 API Gateway instances

### **Layer 2: YARP Proxy (RoundRobin)**
- **Algorithm**: Round-Robin
- **Reason**:
  - Simple and fair distribution
  - Works well with stateless services
  - Equal load distribution across service instances
- **Targets**: 2 instances per service (16 total service instances)

### **Load Balancing Flow**:
```
[Client Request]
      ↓
[Nginx: least_conn]  ← Chooses API Gateway with fewest active connections
      ↓
[API Gateway 8041/8042/8043]
      ↓
[YARP: RoundRobin]  ← Alternates between service instances
      ↓
[Service Instance 1 or 2]  ← Based on health check status
```

---

## 📬 KAFKA CONFIGURATION

### **Domain Topics** (12 topics, all with same configuration)
**Configuration**:
- Partitions: 6
- Replication Factor: 3
- Min ISR: 2
- Retention: 189 hours (7.875 days)

**Topic List**:
1. `user-management-domain`
2. `booking-management-domain`
3. `chat-communication-domain`
4. `content-management-domain`
5. `content-moderation-domain`
6. `subscription-management-domain`
7. `payment-processing-domain`
8. `dmca-management-domain`
9. `public-review-management-domain`
10. `report-management-domain`
11. `system-configuration-domain`
12. `notification-domain`

### **Consumer Groups** (8 groups, one per service)
1. `user-service-consumer-group`
2. `system-configuration-service-consumer-group`
3. `booking-management-service-consumer-group`
4. `podcast-service-consumer-group`
5. `subscription-service-consumer-group`
6. `moderation-service-consumer-group`
7. `transaction-service-consumer-group`
8. `saga-orchestrator-service-consumer-group`

### **Producer/Consumer Pattern**
- Each service acts as both producer and consumer
- Services produce to domain topics
- SagaOrchestrator consumes flow messages and produces step messages
- Services consume step messages and produce result messages

---

## 🗄️ DATABASE SCHEMA

### **Database-per-Service Pattern**
Each microservice has its own isolated database to ensure:
- Data ownership and autonomy
- Independent scaling
- Technology diversity (if needed)
- Fault isolation

### **Total Entities**: 98 tables across 8 databases

**Entity Distribution**:
- UserService: 10 tables
- SystemConfigurationService: 7 tables
- BookingManagementService: 12 tables
- PodcastService: 30 tables (largest)
- SubscriptionService: 12 tables
- ModerationService: 18 tables
- TransactionService: 7 tables
- SagaOrchestratorService: 2 tables (smallest)

---

## 🌐 EXTERNAL INTEGRATIONS

### **1. Cloud Storage - AWS S3**
- **Region**: ap-southeast-1 (Singapore)
- **Bucket**: mystic-tales-podcast-bucket
- **Use Cases**: 
  - Audio file storage
  - Image uploads
  - Document storage

### **2. AI Service - Audio Transcription**
- **Technology**: FastAPI
- **Port**: 8001
- **Function**: Audio-to-text transcription

### **3. Payment Gateway - PayOS**
- **Function**: Payment processing
- **Integration**: Via TransactionService

### **4. Media Processing**
- **FFmpeg**: 
  - Audio/video processing
  - Format conversion
- **HLS Streaming**: 
  - Audio streaming protocol
  - Encryption support
- **Audio Tuning**: 7 mood presets
  - Mysterious, Eerie, Dark, Haunting, Balance, Warm, Cool

### **5. Google Services**
- **OAuth2**: Social login authentication
- **SMTP**: Email notifications
  - Port: 587
  - Use Cases: Password reset, notifications, alerts

---

## ⚡ HIGH AVAILABILITY FEATURES

### **Service Redundancy**
- ✅ Each microservice: 2 instances (16 total service instances)
- ✅ API Gateway: 3 instances
- ✅ Kafka brokers: 3 nodes
- ✅ Total: 22 containerized services

### **Health Monitoring**
- ✅ Health check endpoint: `/health`
- ✅ Check interval: 30 seconds
- ✅ Timeout: 10 seconds
- ✅ Retries: 3 attempts
- ✅ Consul integration for service health tracking

### **Fault Tolerance**
- ✅ Kafka replication factor: 3 (can lose 2 brokers)
- ✅ Min ISR: 2 (ensures data durability)
- ✅ Auto-restart policy: `unless-stopped`
- ✅ Transaction guarantees: Kafka transactional messaging

### **Data Durability**
- ✅ Kafka log retention: 189 hours
- ✅ Database persistent volumes
- ✅ Redis data persistence (optional)

### **Load Distribution**
- ✅ Nginx least_conn: Prevents gateway overload
- ✅ YARP RoundRobin: Equal distribution across instances
- ✅ Kafka partitions: 6 per topic (parallel processing)

### **Network Isolation**
- ✅ Docker network: gateway-network (bridge mode)
- ✅ Subnet: 172.20.0.0/16
- ✅ Gateway: 172.20.0.1

---

## 🎭 SAGA ORCHESTRATOR MECHANISM

### **Architecture Pattern**: Event-Driven DAG Orchestrator

### **Flow Execution Model**:
```
API Request 
  → Nginx 
  → API Gateway 
  → YARP 
  → Consul 
  → Service 
  → Service sends flow name message to start saga flow
  → Saga orchestrator handles flow message and sends first step message
  → Service handles step message
  → Service emits message (Kafka)
  → Orchestrator (flowName lookup)
  → Orchestrator sends nextSteps and nextFlows
  → nextSteps (Kafka) 
  → Step handlers 
  → Emit 
  → Orchestrator 
  → ... (loop continues)
```

### **Key Concepts**:

**1. Flow Initialization**:
- Service sends flow message with flow name
- SagaOrchestrator creates SagaInstance (new saga ID)
- Loads flow definition from YAML
- Sends first step message(s)

**2. Step Execution**:
- Steps are NOT sequential by default
- Execution depends on `nextSteps` in emit messages
- Steps can run in parallel (sent simultaneously to Kafka)
- Each step is handled by the service that owns that domain

**3. Flow Control**:

**nextSteps** (Same Saga):
```yaml
onSuccess:
  emit: "step-name.success"
  topic: "domain-topic"
  nextSteps:
    - name: "next-step-1"
      topic: "domain-topic-1"
    - name: "next-step-2"
      topic: "domain-topic-2"
```
- Maintains same saga ID
- Sends multiple messages in parallel to Kafka
- Continues current flow

**nextFlows** (New Saga):
```yaml
onSuccess:
  emit: "step-name.success"
  topic: "domain-topic"
  nextFlows: 
    - name: "new-flow-name"
      topic: "new-flow-topic"
```
- Creates new saga ID
- Sends flow message to Kafka with current data body
- Orchestrator listens and initializes new flow

**4. Message Handling**:
- Step handlers only look at message data body
- No need to understand which flow the message belongs to
- Focus on business logic within step scope
- Idempotent processing recommended

**5. Rollback Mechanism**:
```yaml
onFailure:
  emit: "step-name.failed"
  topic: "domain-topic"
  nextSteps:
    - name: "previous-step.rollback"
      topic: "domain-topic"
```
- NOT absolute/guaranteed rollback
- Depends on business logic setup in data body
- Rollback steps are regular steps that perform compensating actions
- Example: payment.rollback reads transaction ID from body and refunds

**6. Configuration**:
- Flow definitions: YAML files
- Loaded at startup (no hot-reload)
- Located in: `AppConfigurations/Saga/Flows/`

**7. Saga State Tracking**:

**SagaInstance Table**:
- `id`: Saga unique identifier
- `flowName`: Current flow name
- `currentStepName`: Active step (null when completed)
- `initialData`: Starting flow data (JSON)
- `resultData`: Final response data (JSON)
- `flowStatus`: RUNNING | SUCCESS | FAILED
- `errorStepName`: Step that failed (if any)
- `errorMessage`: Error details
- `completedAt`: Flow completion timestamp

**SagaStepExecution Table**:
- `sagaInstanceId`: Links to SagaInstance
- `stepName`: Name of executed step
- `topicName`: Kafka topic used
- `stepStatus`: RUNNING | SUCCESS | FAILED
- `requestData`: Input to step (JSON)
- `responseData`: Output from step (JSON)
- `errorMessage`: Runtime/business errors

**8. Saga Completion**:
- `onSuccess` with empty `nextSteps` → Flow marked SUCCESS
- `onFailure` → Flow marked FAILED
- `currentStepName` set to NULL on completion
- Final `responseData` stored in SagaInstance

**9. Simplified Assumptions** (Project-Specific):
- Focus on happy cases
- No complex failure scenarios
- No timeouts
- No retry logic
- Rollback is best-effort based on business data

---

## 📊 DEPLOYMENT TOPOLOGY

```
                          [Client Devices]
                                 |
                                 | HTTPS
                                 ↓
                    [Nginx Load Balancer]
                    [80, 443 - least_conn]
                                 |
                    ┌────────────┼────────────┐
                    ↓            ↓            ↓
              [API GW 8041] [API GW 8042] [API GW 8043]
                    |            |            |
                    └────────────┴────────────┘
                                 |
                    [YARP Proxy - RoundRobin]
                                 |
                    ┌────────────┴────────────┐
                    ↓                         ↓
              [Consul 8500]            [Redis 6379]
            (Service Registry)      (Route Cache)
                    |
        ┌───────────┼───────────────────────────┐
        |           |           |               |
        ↓           ↓           ↓               ↓
   [UserSvc]  [PodcastSvc] [BookingSvc]  [SagaSvc]
   8046|8047  8061|8062     8056|8057     8081|8082
        |           |           |               |
        └───────────┴───────────┴───────────────┘
                          |
                          ↓
               [Kafka Cluster - KRaft]
            [9092, 9093, 9094 - 3 brokers]
                          |
                          ↓
                  [SQL Server 1433]
              (8 isolated databases)
```

---

## 🏷️ SERVICE METADATA

```yaml
project:
  name: "Mystic Tales Podcast"
  architecture: "Microservices"
  deployment: "Docker Compose"
  
infrastructure:
  message_broker: "Apache Kafka (KRaft)"
  service_discovery: "HashiCorp Consul"
  cache: "Redis"
  database: "Microsoft SQL Server"
  load_balancer: "Nginx + YARP"
  api_gateway: "YARP Reverse Proxy"
  orchestration: "Event-Driven Saga Pattern"
  
services_count:
  microservices: 8
  total_instances: 16
  api_gateway_instances: 3
  total_containers: 22
  
high_availability:
  service_replication: 2x
  kafka_replication: 3x
  api_gateway_replication: 3x
  
network:
  subnet: "172.20.0.0/16"
  driver: "bridge"
  gateway: "172.20.0.1"
```

---

## 📝 NOTES FOR AI PROMPTS

**When using this document with AI**:

1. **Architecture Pattern**: Microservices with event-driven saga orchestration
2. **Communication**: Hybrid sync (HTTP) and async (Kafka) 
3. **Database**: Strict database-per-service isolation
4. **Scaling**: Horizontal scaling via multiple instances
5. **Service Discovery**: Dynamic via Consul
6. **Load Balancing**: Two-tier (Nginx + YARP)
7. **Message Broker**: Kafka with high availability (RF=3)
8. **Health Monitoring**: Integrated health checks + Consul

**Key Design Decisions**:
- ✅ Each service has 2 instances for HA
- ✅ Database per service pattern for isolation
- ✅ Event-driven for async workflows
- ✅ Saga pattern for distributed transactions
- ✅ YARP for dynamic routing based on Consul
- ✅ Redis for caching YARP routes
- ✅ Kafka KRaft mode (no Zookeeper)

---

**End of Architecture Summary**

*This document contains all necessary information to prompt AI systems about the Mystic Tales Podcast microservices architecture.*
