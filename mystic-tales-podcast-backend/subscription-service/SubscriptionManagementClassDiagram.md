# Subscription Management Class Diagram

## Subscribe to a Subscription Sequence Diagram
## Update an ACTIVE Subscription Sequence Diagram

```classDiagram
    class PodcastSubscriptionController {
        -podcastSubscriptionService: PodcastSubscriptionService
        -kafkaProducerService: KafkaProducerService
        -messagingService: IMessagingService
        +SubscribeToPodcastSubscriptionById(subscriptionId, request): Task
        +UpdatePodcastSubscriptionById(subscriptionId, request): Task
    }

    class PodcastSubscriptionService {
        -accountCachingService: AccountCachingService
        -kafkaProducerService: KafkaProducerService
        -messagingService: IMessagingService
        +CreateAccountPodcastSubscriptionRegistrationAsync(parameter, command): Task
        +UpdatePodcastSubscriptionAsync(parameter, command): Task
        +GetPodcastSubscriptionByIdAsync(isPodcaster, subscriptionId): Task
    }

    class SubscriptionManagementDomainMessageHandler {
        -podcastSubscriptionService: PodcastSubscriptionService
        -kafkaProducerService: KafkaProducerService
        -messagingService: IMessagingService
        +HandleCreateAccountPodcastSubscriptionRegistrationCommandAsync(key, messageJson): Task
        +HandleUpdatePodcastSubscriptionCommandAsync(key, messageJson): Task
    }

    class BaseSagaCommandMessageHandler {
        -messagingService: IMessagingService
        -kafkaProducerService: KafkaProducerService
        +ExecuteSagaCommandMessageAsync(messageJson, handler, responseTopic, failedEmitMessage): Task
    }

    class IMessagingService {
        +SendSagaMessageAsync(message): Task~bool~
    }

    class KafkaProducerService {
        +PrepareStartSagaTriggerMessage(topic, requestData, messageName): SagaCommandMessage
        +PrepareSagaEventMessage(topic, requestData, responseData, messageName): SagaEventMessage
    }

    class SagaCommandMessage {
        +SagaInstanceId: Guid
        +MessageName: string
        +RequestData: JObject
    }

    class SagaEventMessage {
        +SagaInstanceId: Guid
        +MessageName: string
        +ResponseData: JObject
    }

    class AccountStatusCache {
        +Id: int
        +Email: string
        +FullName: string
        +HasVerifiedPodcasterProfile: bool
    }

    class AccountCachingService {
        +GetAccountStatusCacheById(accountId): Task~AccountStatusCache~
    }

    %% Dependencies
    PodcastSubscriptionController --> PodcastSubscriptionService
    PodcastSubscriptionController --> KafkaProducerService
    PodcastSubscriptionController --> IMessagingService
    
    PodcastSubscriptionService --> AccountCachingService
    PodcastSubscriptionService --> KafkaProducerService
    PodcastSubscriptionService --> IMessagingService
    
    KafkaProducerService --> SagaCommandMessage
    KafkaProducerService --> SagaEventMessage
    
    SubscriptionManagementDomainMessageHandler --> BaseSagaCommandMessageHandler
    SubscriptionManagementDomainMessageHandler --> PodcastSubscriptionService
    SubscriptionManagementDomainMessageHandler --> KafkaProducerService
    SubscriptionManagementDomainMessageHandler --> IMessagingService
    
    BaseSagaCommandMessageHandler --> IMessagingService
    BaseSagaCommandMessageHandler --> KafkaProducerService
```

## Flow Description

### Subscribe to a Subscription (podcast-subscription-registration-flow)
1. **Controller** → Sends `SagaCommandMessage` via Kafka
2. **MessageHandler** → Receives and routes to `PodcastSubscriptionService`
3. **Service** → Validates account, creates registration, sends emails via Kafka
4. **Service** → Emits `SagaEventMessage` back (success/failed)

### Update an ACTIVE Subscription (podcast-subscription-update-flow)
1. **Controller** → Sends `SagaCommandMessage` via Kafka
2. **MessageHandler** → Receives and routes to `PodcastSubscriptionService`
3. **Service** → Validates ownership, compares changes, creates new version
4. **Service** → Sends emails to existing subscribers about new version
5. **Service** → Emits `SagaEventMessage` back (success/failed)
