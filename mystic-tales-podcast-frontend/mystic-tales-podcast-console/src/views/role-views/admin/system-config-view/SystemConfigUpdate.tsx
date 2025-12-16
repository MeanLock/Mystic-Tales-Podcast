import type React from "react"
import { useContext, useEffect, useState, useRef } from "react"
import { CButton, CCol, CForm, CFormInput, CFormLabel, CRow, CModal, CModalHeader, CModalTitle, CModalBody, CModalFooter } from "@coreui/react"
import { formatDate } from "../../../../core/utils/date.util"
import { SystemConfigViewContext } from "."
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, type ColDef, ModuleRegistry } from "ag-grid-community"
import { Plus, Trash } from "phosphor-react"

ModuleRegistry.registerModules([AllCommunityModule])

export const mockSystemConfig = {
  SystemConfig: {
    Id: 2,
    Name: "Configuration B",
    IsActive: false,
    DeletedAt: null,
    CreatedAt: "2025-01-01T08:00:00.000Z",
    UpdatedAt: "2025-10-07T04:32:38.343Z",

    PodcastSubscriptionConfigList: [
      {
        SubscriptionCycleType: {
          Id: 1,
          Name: "Weekly",
        },
        ProfitRate: 0.85,
        IncomeTakenDelayDays: 7,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-10T04:32:38.343Z",
      },
      {
        SubscriptionCycleType: {
          Id: 2,
          Name: "Annually",
        },
        ProfitRate: 0.9,
        IncomeTakenDelayDays: 14,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-10T04:32:38.343Z",
      },
    ],

    PodcastSuggestionConfig: {
      BehaviorLookbackDayCount: 30,
      MinChannelQuery: 5,
      MinShowQuery: 10,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-09-20T04:32:38.343Z",
    },

    BookingConfig: {
      ProfitRate: 0.8,
      DepositRate: 0.2,
      PodcastTrackPreviewListenSlot: 3,
      PreviewResponseAllowedDays: 5,
      ProducingRequestResponseAllowedDays: 7,
      ChatRoomExpiredHours: 72,
      ChatRoomFileMessageExpiredHours: 48,
      FreeInitalBookingStorageSize: 500,
      SingleStorageUnitPurchasePrice: 10000,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-09-15T04:32:38.343Z",
    },

    AccountConfig: {
      ViolationPointDecayHours: 720,
      PodcastListenSlotThreshold: 10,
      PodcastListenSlotRecoverySeconds: 60,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-09-01T04:32:38.343Z",
    },

    AccountViolationLevelConfigList: [
      {
        ViolationLevel: 1,
        ViolationPointThreshold: 5,
        PunishmentDays: 3,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-01T04:32:38.343Z",
      },
      {
        ViolationLevel: 2,
        ViolationPointThreshold: 10,
        PunishmentDays: 7,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-01T04:32:38.343Z",
      },
      {
        ViolationLevel: 3,
        ViolationPointThreshold: 20,
        PunishmentDays: 30,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-01T04:32:38.343Z",
      },
      {
        ViolationLevel: 4,
        ViolationPointThreshold: 40,
        PunishmentDays: 40,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-09-01T04:32:38.343Z",
      },
    ],

    ReviewSessionConfig: {
      PodcastBuddyUnResolvedReportStreak: 3,
      PodcastShowUnResolvedReportStreak: 5,
      PodcastEpisodeUnResolvedReportStreak: 10,
      PodcastEpisodePublishEditRequirementExpiredHours: 72,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-09-30T04:32:38.343Z",
    },
  },
}

interface SystemConfigUpdateProps {
  systemConfigId: number | null
  onClose: () => void
}

interface ViolationLevelFormProps {
  isOpen: boolean
  onClose: () => void
  onSave: (data: any) => void
  editingData?: any
}



const ViolationLevelForm: React.FC<ViolationLevelFormProps> = ({ isOpen, onClose, onSave, editingData }) => {
  const [formData, setFormData] = useState({
    ViolationPointThreshold: editingData?.ViolationPointThreshold || '0',
    PunishmentDays: editingData?.PunishmentDays || '0'
  })

  // Reset form when modal opens for adding new level
  useEffect(() => {
    if (isOpen) {
      setFormData({
        ViolationPointThreshold: editingData?.ViolationPointThreshold || '0',
        PunishmentDays: editingData?.PunishmentDays || '0'
      })
    }
  }, [isOpen, editingData])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSave({
      ...formData,
      ViolationPointThreshold: parseInt(formData.ViolationPointThreshold),
      PunishmentDays: parseInt(formData.PunishmentDays),
      CreatedAt: editingData?.CreatedAt || new Date().toISOString(),
      UpdatedAt: new Date().toISOString()
    })
    onClose()
  }

  return (
    <CModal visible={isOpen} onClose={onClose}>
      <CModalHeader>
        <CModalTitle>{editingData ? 'Edit' : 'Add'} Violation Level</CModalTitle>
      </CModalHeader>
      <CForm onSubmit={handleSubmit}>
        <CModalBody>
          <CRow className="g-3">
            <CCol md={12}>
              <CFormLabel>Point Threshold</CFormLabel>
              <CFormInput
                type="number"
                value={formData.ViolationPointThreshold}
                onChange={(e) => setFormData({ ...formData, ViolationPointThreshold: e.target.value })}
                required
              />
            </CCol>
            <CCol md={12}>
              <CFormLabel>Punishment Days</CFormLabel>
              <CFormInput
                type="number"
                value={formData.PunishmentDays}
                onChange={(e) => setFormData({ ...formData, PunishmentDays: e.target.value })}
                required
              />
            </CCol>
          </CRow>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={onClose}>Cancel</CButton>
          <CButton color="primary" type="submit">Save</CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}

const SystemConfigForm: React.FC<SystemConfigUpdateProps> = ({ systemConfigId, onClose }) => {
  const context = useContext(SystemConfigViewContext)
  const [config, setConfig] = useState<any>(null)
  const [isLoading, setIsLoading] = useState<boolean>(true)
  const [violationModalOpen, setViolationModalOpen] = useState(false)
  const [editingViolation, setEditingViolation] = useState<any>(null)
  const isAddMode = systemConfigId === null

  const name = useRef<HTMLInputElement>(null)

  // Form states for all configurations
  const [formData, setFormData] = useState({
    name: '',
    // Podcast Subscription Config
    weeklyProfitRate: '',
    weeklyIncomeDelayDays: '',
    annuallyProfitRate: '',
    annuallyIncomeDelayDays: '',
    // Podcast Suggestion Config
    behaviorLookbackDays: '',
    minChannelQuery: '',
    minShowQuery: '',
    // Account Config
    violationPointDecayHours: '',
    listenSlotThreshold: '',
    slotRecoverySeconds: '',
    // Booking Config
    profitRate: '',
    depositRate: '',
    previewListenSlot: '',
    previewResponseDays: '',
    producingResponseDays: '',
    chatRoomExpiredHours: '',
    fileMessageExpiredHours: '',
    freeStorageSize: '',
    storageUnitPrice: '',
    // Review Session Config
    buddyUnresolvedStreak: '',
    showUnresolvedStreak: '',
    episodeUnresolvedStreak: '',
    publishEditExpiredHours: ''
  })
  useEffect(() => {
    console.log("Component mounted or updated", formData)
  }, [formData])


  useEffect(() => {
    // Fetch config data if systemConfigId is provided
    if (systemConfigId !== null) {
      const configData = mockSystemConfig.SystemConfig
      setConfig(configData)
      console.log("Fetched config data:", configData)

      // Get subscription configs
      const weeklyConfig = configData.PodcastSubscriptionConfigList?.find((c: any) => c.SubscriptionCycleType.Id === 1)
      const annuallyConfig = configData.PodcastSubscriptionConfigList?.find((c: any) => c.SubscriptionCycleType.Id === 2)

      // Populate form data
      setFormData({
        name: configData.Name || '',
        // Subscription configs
        weeklyProfitRate: weeklyConfig?.ProfitRate?.toString() || '',
        weeklyIncomeDelayDays: weeklyConfig?.IncomeTakenDelayDays?.toString() || '',
        annuallyProfitRate: annuallyConfig?.ProfitRate?.toString() || '',
        annuallyIncomeDelayDays: annuallyConfig?.IncomeTakenDelayDays?.toString() || '',
        // Other configs
        behaviorLookbackDays: configData.PodcastSuggestionConfig?.BehaviorLookbackDayCount?.toString() || '',
        minChannelQuery: configData.PodcastSuggestionConfig?.MinChannelQuery?.toString() || '',
        minShowQuery: configData.PodcastSuggestionConfig?.MinShowQuery?.toString() || '',
        violationPointDecayHours: configData.AccountConfig?.ViolationPointDecayHours?.toString() || '',
        listenSlotThreshold: configData.AccountConfig?.PodcastListenSlotThreshold?.toString() || '',
        slotRecoverySeconds: configData.AccountConfig?.PodcastListenSlotRecoverySeconds?.toString() || '',
        profitRate: configData.BookingConfig?.ProfitRate?.toString() || '',
        depositRate: configData.BookingConfig?.DepositRate?.toString() || '',
        previewListenSlot: configData.BookingConfig?.PodcastTrackPreviewListenSlot?.toString() || '',
        previewResponseDays: configData.BookingConfig?.PreviewResponseAllowedDays?.toString() || '',
        producingResponseDays: configData.BookingConfig?.ProducingRequestResponseAllowedDays?.toString() || '',
        chatRoomExpiredHours: configData.BookingConfig?.ChatRoomExpiredHours?.toString() || '',
        fileMessageExpiredHours: configData.BookingConfig?.ChatRoomFileMessageExpiredHours?.toString() || '',
        freeStorageSize: configData.BookingConfig?.FreeInitalBookingStorageSize?.toString() || '',
        storageUnitPrice: configData.BookingConfig?.SingleStorageUnitPurchasePrice?.toString() || '',
        buddyUnresolvedStreak: configData.ReviewSessionConfig?.PodcastBuddyUnResolvedReportStreak?.toString() || '',
        showUnresolvedStreak: configData.ReviewSessionConfig?.PodcastShowUnResolvedReportStreak?.toString() || '',
        episodeUnresolvedStreak: configData.ReviewSessionConfig?.PodcastEpisodeUnResolvedReportStreak?.toString() || '',
        publishEditExpiredHours: configData.ReviewSessionConfig?.PodcastEpisodePublishEditRequirementExpiredHours?.toString() || ''
      })
      setIsLoading(false)
    } else {
      // Initialize empty config for add mode
      setConfig({
        Id: null,
        Name: '',
        IsActive: false,
        AccountViolationLevelConfigList: [], // Initialize empty array
        PodcastSubscriptionConfigList: [],
        PodcastSuggestionConfig: null,
        BookingConfig: null,
        AccountConfig: null,
        ReviewSessionConfig: null,
        CreatedAt: new Date().toISOString(),
        UpdatedAt: new Date().toISOString()
      })
      setIsLoading(false)
    }
  }, [systemConfigId])

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const validateFormData = () => {
    const errors: string[] = []

    if (!formData.name.trim()) {
      errors.push("Configuration name is required")
    }

    // Validate subscription configs
    if (formData.weeklyProfitRate && (parseFloat(formData.weeklyProfitRate) < 0 || parseFloat(formData.weeklyProfitRate) > 1)) {
      errors.push("Weekly profit rate must be between 0 and 1")
    }

    if (formData.annuallyProfitRate && (parseFloat(formData.annuallyProfitRate) < 0 || parseFloat(formData.annuallyProfitRate) > 1)) {
      errors.push("Annually profit rate must be between 0 and 1")
    }

    // Validate booking config rates
    if (formData.profitRate && (parseFloat(formData.profitRate) < 0 || parseFloat(formData.profitRate) > 1)) {
      errors.push("Booking profit rate must be between 0 and 1")
    }

    if (formData.depositRate && (parseFloat(formData.depositRate) < 0 || parseFloat(formData.depositRate) > 1)) {
      errors.push("Deposit rate must be between 0 and 1")
    }

    return errors
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()

    // Validate form data
    const validationErrors = validateFormData()
    if (validationErrors.length > 0) {
      console.error("Validation errors:", validationErrors)
      alert("Please fix the following errors:\n" + validationErrors.join("\n"))
      return
    }

    // Format data according to API structure
    const apiPayload = {
      SystemConfigInfo: {
        Name: formData.name,
        PodcastSubsriptionConfigInfoList: [
          {
            SubscriptionCycleTypeId: 1, // Weekly
            ProfitRate: parseFloat(formData.weeklyProfitRate) || 0,
            IncomeTakenDelayDays: parseInt(formData.weeklyIncomeDelayDays) || 0
          },
          {
            SubscriptionCycleTypeId: 2, // Annually  
            ProfitRate: parseFloat(formData.annuallyProfitRate) || 0,
            IncomeTakenDelayDays: parseInt(formData.annuallyIncomeDelayDays) || 0
          }
        ],
        PodcastSuggestionConfigInfo: {
          BehaviorLookbackDayCount: parseInt(formData.behaviorLookbackDays) || 0,
          MinChannelQuery: parseInt(formData.minChannelQuery) || 0,
          MinShowQuery: parseInt(formData.minShowQuery) || 0
        },
        BookingConfigInfo: {
          ProfitRate: parseFloat(formData.profitRate) || 0,
          DepositRate: parseFloat(formData.depositRate) || 0,
          PodcastTrackPreviewListenSlot: parseInt(formData.previewListenSlot) || 0,
          PreviewResponseAllowedDays: parseInt(formData.previewResponseDays) || 0,
          ProducingRequestResponseAllowedDays: parseInt(formData.producingResponseDays) || 0,
          ChatRoomExpiredHours: parseInt(formData.chatRoomExpiredHours) || 0,
          ChatRoomFileMessageExpiredHours: parseInt(formData.fileMessageExpiredHours) || 0,
          FreeInitalBookingStorageSize: parseInt(formData.freeStorageSize) || 0,
          SingleStorageUnitPurchasePrice: parseInt(formData.storageUnitPrice) || 0
        },
        AccountConfigInfo: {
          ViolationPointDecayHours: parseInt(formData.violationPointDecayHours) || 0,
          PodcastListenSlotThreshold: parseInt(formData.listenSlotThreshold) || 0,
          PodcastListenSlotRecoverySeconds: parseInt(formData.slotRecoverySeconds) || 0
        },
        AccountViolationLevelConfigInfoList: config?.AccountViolationLevelConfigList?.map((violation: any) => ({
          ViolationLevel: violation.ViolationLevel,
          ViolationPointThreshold: violation.ViolationPointThreshold,
          PunishmentDays: violation.PunishmentDays
        })) || [],
        ReviewSessionConfigInfo: {
          PodcastBuddyUnResolvedReportStreak: parseInt(formData.buddyUnresolvedStreak) || 0,
          PodcastShowUnResolvedReportStreak: parseInt(formData.showUnresolvedStreak) || 0,
          PodcastEpisodeUnResolvedReportStreak: parseInt(formData.episodeUnresolvedStreak) || 0,
          PodcastEpisodePublishEditRequirementExpiredHours: parseInt(formData.publishEditExpiredHours) || 0
        }
      }
    }

    console.log("Submitting config with correct format:", JSON.stringify(apiPayload, null, 2))

    // Handle add or update logic with apiPayload
    if (isAddMode) {
      console.log("🆕 ADD MODE: Creating new system configuration")
      // TODO: Call POST API endpoint with apiPayload
      // Example: await createSystemConfig(apiPayload)
    } else {
      console.log("✏️ UPDATE MODE: Updating existing system configuration")
      // TODO: Call PUT API endpoint with apiPayload
      // Example: await updateSystemConfig(systemConfigId, apiPayload)
    }

    // For demo purposes, show success message
    alert(`${isAddMode ? 'Created' : 'Updated'} system configuration successfully!`)
  }

  const handleActivate = async () => {
    console.log("Activating config:", systemConfigId)
  }

  const handleDelete = async () => {
    console.log("Deleting config:", systemConfigId)
  }

  const handleAddViolation = () => {
    setEditingViolation(null)
    setViolationModalOpen(true)
  }

  const handleSaveViolation = (violationData: any) => {
    // Ensure config exists and has AccountViolationLevelConfigList
    if (!config) {
      console.error("Config is null")
      return
    }

    // Initialize AccountViolationLevelConfigList if it doesn't exist
    if (!config.AccountViolationLevelConfigList) {
      config.AccountViolationLevelConfigList = []
    }

    let updatedList
    if (editingViolation) {
      // Edit existing
      updatedList = config.AccountViolationLevelConfigList.map((v: any) =>
        v.ViolationLevel === editingViolation.ViolationLevel ? violationData : v
      )
    } else {
      // Add new - auto-increment violation level
      const maxViolationLevel = config.AccountViolationLevelConfigList.length > 0
        ? Math.max(0, ...config.AccountViolationLevelConfigList.map((v: any) => v.ViolationLevel))
        : 0
      const newViolationData = {
        ...violationData,
        ViolationLevel: maxViolationLevel + 1
      }
      updatedList = [...config.AccountViolationLevelConfigList, newViolationData]
    }

    setConfig({
      ...config,
      AccountViolationLevelConfigList: updatedList.sort((a: any, b: any) => a.ViolationLevel - b.ViolationLevel)
    })
  }

  const handleDeleteViolation = (violationLevel: number) => {
    if (config?.AccountViolationLevelConfigList) {
      const updatedList = config.AccountViolationLevelConfigList.filter(
        (v: any) => v.ViolationLevel !== violationLevel
      )
      setConfig({
        ...config,
        AccountViolationLevelConfigList: updatedList
      })
    }
  }

  const handleUpdateViolationInline = (updatedViolation: any) => {
    if (config?.AccountViolationLevelConfigList) {
      const updatedList = config.AccountViolationLevelConfigList.map((v: any) =>
        v.ViolationLevel === updatedViolation.ViolationLevel
          ? { ...updatedViolation, UpdatedAt: new Date().toISOString() }
          : v
      )
      setConfig({
        ...config,
        AccountViolationLevelConfigList: updatedList
      })
    }
  }





  if (isLoading) {
    return <div className="system-config-update__loading">Loading...</div>
  }

  return (
    <div className="system-config-update">
      {!isAddMode && config && (
        <div className="system-config-update__header">
          <div className="system-config-update__header-info">
            <h2 className="system-config-update__name">{config.Name}</h2>
            <div className="system-config-update__id">Configuration ID: #{config.Id}</div>
          </div>
          <div
            className={`system-config-update__status-badge system-config-update__status-badge--${config.IsActive ? "active" : "inactive"}`}
          >
            <span className="system-config-update__status-dot"></span>
            {config.IsActive ? "Active Configuration" : "Inactive Configuration"}
          </div>
        </div>
      )}

      {isAddMode && (
        <div className="system-config-update__header system-config-update__header--add">
          <h2 className="system-config-update__name">Create New System Configuration</h2>
          <p className="system-config-update__description">
            Fill in the details below to create a new system configuration
          </p>
        </div>
      )}

      <CForm noValidate onSubmit={handleSubmit} className="system-config-update__form">
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Basic Information</h3>
          <CRow className="g-3">
            <CCol md={12}>
              <div className="system-config-update__field">
                <CFormLabel htmlFor="name" className="system-config-update__label">
                  Configuration Name *
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="name"
                  value={formData.name}
                  onChange={(e) => handleInputChange('name', e.target.value)}
                  required
                  disabled={!isAddMode && config?.IsActive}
                  className="system-config-update__input"
                  placeholder="Enter configuration name"
                />
              </div>
            </CCol>
          </CRow>
        </div>
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Podcast Subscription Configuration</h3>
          <CRow className="g-3">
            {/* Weekly Configuration */}
            <CCol md={6}>
              <div className="config-card config-card--input">
                <div className="config-card__header">
                  <span className="config-card__type">Weekly</span>
                </div>
                <div className="config-card__content">
                  <CRow className="g-2">
                    <CCol md={6}>
                      <div className="system-config-update__field">
                        <CFormLabel className="system-config-update__label">Profit Rate (0-1)</CFormLabel>
                        <CFormInput
                          type="number"
                          step="0.01"
                          min="0"
                          max="1"
                          value={formData.weeklyProfitRate}
                          onChange={(e) => handleInputChange('weeklyProfitRate', e.target.value)}
                          className="system-config-update__input"
                        />
                      </div>
                    </CCol>
                    <CCol md={6}>
                      <div className="system-config-update__field">
                        <CFormLabel className="system-config-update__label">Income Delay Days</CFormLabel>
                        <CFormInput
                          type="number"
                          min="0"
                          value={formData.weeklyIncomeDelayDays}
                          onChange={(e) => handleInputChange('weeklyIncomeDelayDays', e.target.value)}
                          className="system-config-update__input"
                        />
                      </div>
                    </CCol>
                  </CRow>
                </div>
              </div>
            </CCol>

            {/* Annually Configuration */}
            <CCol md={6}>
              <div className="config-card config-card--input">
                <div className="config-card__header">
                  <span className="config-card__type">Annually</span>
                </div>
                <div className="config-card__content">
                  <CRow className="g-2">
                    <CCol md={6}>
                      <div className="system-config-update__field">
                        <CFormLabel className="system-config-update__label">Profit Rate (0-1)</CFormLabel>
                        <CFormInput
                          type="number"
                          step="0.01"
                          min="0"
                          max="1"
                          value={formData.annuallyProfitRate}
                          onChange={(e) => handleInputChange('annuallyProfitRate', e.target.value)}
                          className="system-config-update__input"
                        />
                      </div>
                    </CCol>
                    <CCol md={6}>
                      <div className="system-config-update__field">
                        <CFormLabel className="system-config-update__label">Income Delay Days</CFormLabel>
                        <CFormInput
                          type="number"
                          min="0"
                          value={formData.annuallyIncomeDelayDays}
                          onChange={(e) => handleInputChange('annuallyIncomeDelayDays', e.target.value)}
                          className="system-config-update__input"
                        />
                      </div>
                    </CCol>
                  </CRow>
                </div>
              </div>
            </CCol>
          </CRow>
        </div>
        {/* Podcast Suggestion Configuration */}
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Podcast Suggestion Configuration</h3>
          <CRow className="g-3">
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Behavior Lookback Days</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.behaviorLookbackDays}
                  onChange={(e) => handleInputChange('behaviorLookbackDays', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Min Channel Query</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.minChannelQuery}
                  onChange={(e) => handleInputChange('minChannelQuery', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Min Show Query</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.minShowQuery}
                  onChange={(e) => handleInputChange('minShowQuery', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
          </CRow>
        </div>

        {/* Account Configuration */}
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Account Configuration</h3>
          <CRow className="g-3">
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Violation Point Decay Hours</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.violationPointDecayHours}
                  onChange={(e) => handleInputChange('violationPointDecayHours', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Listen Slot Threshold</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.listenSlotThreshold}
                  onChange={(e) => handleInputChange('listenSlotThreshold', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label"> Listen Slot Recovery Seconds</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.slotRecoverySeconds}
                  onChange={(e) => handleInputChange('slotRecoverySeconds', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
          </CRow>
        </div>

        {/* Booking Configuration */}
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Booking Configuration</h3>
          <CRow className="g-3">
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Profit Rate</CFormLabel>
                <CFormInput
                  type="number"
                  step="0.01"
                  min="0"
                  max="1"
                  value={formData.profitRate}
                  onChange={(e) => handleInputChange('profitRate', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Deposit Rate</CFormLabel>
                <CFormInput
                  type="number"
                  step="0.01"
                  min="0"
                  max="1"
                  value={formData.depositRate}
                  onChange={(e) => handleInputChange('depositRate', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Preview Listen Slot</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.previewListenSlot}
                  onChange={(e) => handleInputChange('previewListenSlot', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Preview Response Days</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.previewResponseDays}
                  onChange={(e) => handleInputChange('previewResponseDays', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Producing Response Days</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.producingResponseDays}
                  onChange={(e) => handleInputChange('producingResponseDays', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Chat Room Expired Hours</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.chatRoomExpiredHours}
                  onChange={(e) => handleInputChange('chatRoomExpiredHours', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">File Message Expired Hours</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.fileMessageExpiredHours}
                  onChange={(e) => handleInputChange('fileMessageExpiredHours', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Free Storage Size (MB)</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.freeStorageSize}
                  onChange={(e) => handleInputChange('freeStorageSize', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={12}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Storage Unit Price (Points)</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.storageUnitPrice}
                  onChange={(e) => handleInputChange('storageUnitPrice', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
          </CRow>
        </div>

        {/* Review Session Configuration */}
        <div className="system-config-update__section">
          <h3 className="system-config-update__section-title">Review Session Configuration</h3>
          <CRow className="g-3">
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Buddy Unresolved Streak</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.buddyUnresolvedStreak}
                  onChange={(e) => handleInputChange('buddyUnresolvedStreak', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Show Unresolved Streak</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.showUnresolvedStreak}
                  onChange={(e) => handleInputChange('showUnresolvedStreak', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Episode Unresolved Streak</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.episodeUnresolvedStreak}
                  onChange={(e) => handleInputChange('episodeUnresolvedStreak', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="system-config-update__field">
                <CFormLabel className="system-config-update__label">Publish Edit Expired Hours</CFormLabel>
                <CFormInput
                  type="number"
                  value={formData.publishEditExpiredHours}
                  onChange={(e) => handleInputChange('publishEditExpiredHours', e.target.value)}
                  className="system-config-update__input"
                />
              </div>
            </CCol>
          </CRow>
        </div>

        {/* Account Violation Level Configuration */}
        <div className="system-config-update__section">
          <div className="system-config-update__section-header">
            <h3 className="system-config-update__section-title">Account Violation Level Configuration</h3>
            <CButton
              color="primary"
              onClick={handleAddViolation}
              className="system-config-update__add-btn"
            >
              Add Level
            </CButton>
          </div>

          {config?.AccountViolationLevelConfigList && config.AccountViolationLevelConfigList.length > 0 && (
            <div className="system-config-update__violation-table">
              <AgGridReact
                columnDefs={[
                  {
                    headerName: "Violation Level",
                    field: "ViolationLevel",
                    flex: 1,
                    editable: false, // Không cho edit level vì nó auto-increment
                    cellRenderer: (params: any) => (
                      <span className="violation-level-badge">Level {params.value}</span>
                    )
                  },
                  {
                    headerName: "Point Threshold",
                    field: "ViolationPointThreshold",
                    flex: 1,
                    editable: true,
                    cellEditor: 'agNumberCellEditor',
                    cellEditorParams: {
                      min: 1,
                      max: 999
                    },
                    valueParser: (params: any) => Number(params.newValue),
                    onCellValueChanged: (params: any) => {
                      // Auto-save when cell value changes
                      handleUpdateViolationInline(params.data)
                    }
                  },
                  {
                    headerName: "Punishment Days",
                    field: "PunishmentDays",
                    flex: 1,
                    editable: true,
                    cellEditor: 'agNumberCellEditor',
                    cellEditorParams: {
                      min: 1,
                      max: 365
                    },
                    valueParser: (params: any) => Number(params.newValue),
                    onCellValueChanged: (params: any) => {
                      // Auto-save when cell value changes
                      handleUpdateViolationInline(params.data)
                    }
                  },
                  {
                    headerName: "Updated At",
                    field: "UpdatedAt",
                    flex: 1.2,
                    editable: false,
                    valueFormatter: (params: any) => formatDate(params.value)
                  },
                  {
                    headerName: "Created At",
                    field: "CreatedAt",
                    flex: 1.2,
                    editable: false,
                    valueFormatter: (params: any) => formatDate(params.value)
                  },
                  {
                    headerName: "Actions",
                    cellClass: "d-flex justify-content-center py-0",
                    editable: false,
                    cellRenderer: (params: any) => (
                      <div className="d-flex gap-1 align-items-center">
                        <CButton
                          size="sm"
                          color="danger"
                          variant="outline"
                          onClick={() => handleDeleteViolation(params.data.ViolationLevel)}
                          className="px-2 py-1" // Thêm padding cố định
                          style={{
                            minHeight: '32px',
                            height: '32px',
                            width: '50px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center'
                          }}
                        >

                          <Trash size={14} />
                        </CButton>
                      </div>
                    ),
                    flex: 1
                  }
                ]}
                rowData={config.AccountViolationLevelConfigList || []} // Fallback to empty array
                defaultColDef={{
                  flex: 1,
                  resizable: true,
                  cellClass: "d-flex align-items-center",
                  headerClass: "violation-header"
                }}
                rowHeight={50}
                headerHeight={45}
                domLayout="autoHeight"
                suppressHorizontalScroll={true}
                singleClickEdit={true}
                stopEditingWhenCellsLoseFocus={true}
              />
            </div>
          )}
        </div>
        <div className="system-config-update__actions">
          {isAddMode ? (
            <>
              <CButton className="system-config-update__btn system-config-update__btn--add" type="submit">
                Add Configuration
              </CButton>
            </>
          ) : (
            <>
              {!config?.IsActive && (
                <>
                  <CButton
                    className="system-config-update__btn system-config-update__btn--delete"
                    onClick={handleDelete}
                  >
                    Delete Configuration
                  </CButton>
                  <CButton
                    className="system-config-update__btn system-config-update__btn--activate"
                    onClick={handleActivate}
                  >
                    Activate Configuration
                  </CButton>
                  <CButton className="system-config-update__btn system-config-update__btn--update" type="submit">
                    Update Configuration
                  </CButton>
                </>
              )}
            </>
          )}
        </div>
      </CForm>

      {/* Violation Level Form Modal */}
      <ViolationLevelForm
        isOpen={violationModalOpen}
        onClose={() => setViolationModalOpen(false)}
        onSave={handleSaveViolation}
        editingData={editingViolation}
      />


    </div>
  )
}

const SystemConfigUpdate: React.FC<SystemConfigUpdateProps> = (props) => {
  return <SystemConfigForm {...props} />
}

export default SystemConfigUpdate
