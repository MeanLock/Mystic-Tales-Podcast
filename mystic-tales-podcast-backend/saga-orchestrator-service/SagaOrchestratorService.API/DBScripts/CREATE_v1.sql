-- =====================================================
-- SAGA ORCHESTRATOR SERVICE DATABASE
-- =====================================================

-- SagaInstance table
CREATE TABLE SagaInstance (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    flowName NVARCHAR(250) NOT NULL,
    currentStepName NVARCHAR(250) NULL,
    initialData NVARCHAR(MAX) NULL,
    resultData NVARCHAR(MAX) NULL,
    flowStatus NVARCHAR(50) NOT NULL,
    errorStepName NVARCHAR(250) NULL,
    errorMessage NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    completedAt DATETIME NULL
);

-- SagaStepExecution table
CREATE TABLE SagaStepExecution (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    sagaInstanceId UNIQUEIDENTIFIER NOT NULL,
    stepName NVARCHAR(250) NOT NULL,
    topicName NVARCHAR(250) NOT NULL,
    stepStatus NVARCHAR(50) NOT NULL,
    requestData NVARCHAR(MAX) NULL,
    responseData NVARCHAR(MAX) NULL,
    errorMessage NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (sagaInstanceId) REFERENCES SagaInstance(id)
);