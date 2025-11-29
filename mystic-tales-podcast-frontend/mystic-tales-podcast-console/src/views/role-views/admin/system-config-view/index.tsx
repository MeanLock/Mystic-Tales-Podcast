import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButtonGroup, CCol, CRow } from "@coreui/react"
import { AllCommunityModule, type ColDef, ModuleRegistry } from "ag-grid-community"
import Modal_Button from "../../../components/common/modal/ModalButton"
import { formatDate } from "../../../../core/utils/date.util"
import type { SystemConfig, SystemConfigList } from "@/core/types/system-config"
import SystemConfigUpdate from "./SystemConfigUpdate"
import { Eye } from "phosphor-react"
import { getActiveConfig, getConfigList } from "@/core/services/system/system.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v1/admin-instance"
import Loading from "@/views/components/common/loading"

export const mockData: any = {
  SystemConfigList: [
    {
      Id: 1,
      Name: "Configuration A",
      IsActive: true,
      DeletedAt: null,
      CreatedAt: "2025-03-01T10:15:22.000Z",
      UpdatedAt: "2025-08-12T09:05:40.000Z",
    },
    {
      Id: 2,
      Name: "Configuration B",
      IsActive: false,
      DeletedAt: null,
      CreatedAt: "2025-02-20T11:45:00.000Z",
      UpdatedAt: "2025-09-30T13:25:10.000Z",
    },
    {
      Id: 3,
      Name: "Configuration C",
      IsActive: false,
      DeletedAt: null,
      CreatedAt: "2024-12-15T06:00:00.000Z",
      UpdatedAt: "2025-05-10T08:12:50.000Z",
    },
  ],
}

export const mockActive: any = {
  SystemConfig: {
    Id: 1,
    Name: "Configuration ABC",
    IsActive: true,
    DeletedAt: null,
    CreatedAt: "2025-01-01T08:00:00.000Z",
    UpdatedAt: "2025-10-07T05:01:25.665Z",

    PodcastSubsriptionConfigList: [
      {
        SubscriptionCycleType: {
          Id: 1,
          Name: "Monthly",
        },
        ProfitRate: 0.85,
        IncomeTakenDelayDays: 7,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-10-01T05:01:25.665Z",
      },
      {
        SubscriptionCycleType: {
          Id: 2,
          Name: "Annually",
        },
        ProfitRate: 0.9,
        IncomeTakenDelayDays: 14,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-10-01T05:01:25.665Z",
      },
    ],

    PodcastSuggestionConfig: {
      BehaviorLookbackDayCount: 30,
      MinChannelQuery: 5,
      MinShowQuery: 10,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-10-01T05:01:25.665Z",
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
      UpdatedAt: "2025-10-01T05:01:25.665Z",
    },

    AccountConfig: {
      ViolationPointDecayHours: 720,
      PodcastListenSlotThreshold: 10,
      PodcastListenSlotRecoverySeconds: 60,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-10-01T05:01:25.665Z",
    },

    AccountViolationLevelConfigList: [
      {
        ViolationLevel: 1,
        ViolationPointThreshold: 5,
        PunishmentDays: 3,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-10-01T05:01:25.665Z",
      },
      {
        ViolationLevel: 2,
        ViolationPointThreshold: 10,
        PunishmentDays: 7,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-10-01T05:01:25.665Z",
      },
      {
        ViolationLevel: 3,
        ViolationPointThreshold: 20,
        PunishmentDays: 30,
        CreatedAt: "2025-01-01T08:00:00.000Z",
        UpdatedAt: "2025-10-01T05:01:25.665Z",
      },
    ],

    ReviewSessionConfig: {
      PodcastBuddyUnResolvedReportStreak: 3,
      PodcastShowUnResolvedReportStreak: 5,
      PodcastEpisodeUnResolvedReportStreak: 10,
      PodcastEpisodePublishEditRequirementExpiredHours: 72,
      CreatedAt: "2025-01-01T08:00:00.000Z",
      UpdatedAt: "2025-10-01T05:01:25.665Z",
    },
  },
}

ModuleRegistry.registerModules([AllCommunityModule])

type SystemConfigViewProps = {}
interface SystemConfigViewContextProps {
  handleDataChange: () => void
}
interface GridState {
  columnDefs: ColDef[]
  rowData: SystemConfigList[]
}

export const SystemConfigViewContext = createContext<SystemConfigViewContextProps | null>(null)

const state_creator = (table: SystemConfigList[]) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.5 },
      { headerName: "Name", field: "Name", flex: 1.2 },
      {
        headerName: "Is Active",
        field: "IsActive",
        flex: 0.7
      },
      {
        headerName: "Updated At",
        valueGetter: (params: { data: SystemConfigList }) => formatDate(params.data.UpdatedAt),
        flex: 1,
      },
      {
        headerName: "Created At",
        valueGetter: (params: { data: SystemConfigList }) => formatDate(params.data.CreatedAt),
        flex: 1,
      },
      {
        headerName: "Action",
        cellClass: "d-flex justify-content-center py-0",
        cellRenderer: (params: { data: SystemConfigList }) => {
          const Modal_props = {
            updateForm: <SystemConfigUpdate systemConfigId={params.data.Id} onClose={() => { }} />,
            title: "System Configuration [ID: #" + params.data.Id + "]",
            button: <Eye size={27} color='var(--secondary-green)' />,
            update_button_color: "white",
          }
          return (
            <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
              <div className="system-config-view">
                <Modal_Button
                  disabled={false}
                  title={Modal_props.title}
                  content={Modal_props.button}
                  color={Modal_props.update_button_color}
                >
                  {Modal_props.updateForm}
                </Modal_Button>
              </div>
            </CButtonGroup>
          )
        },
        flex: 0.8,
      },
    ],
    rowData: table,
  }
  return state
}

const SystemConfigView: FC<SystemConfigViewProps> = () => {
  const [state, setState] = useState<GridState | null>(null)
  const [isLoading, setIsLoading] = useState<boolean>(true)
  const [activateConfig, setActivateConfig] = useState<SystemConfig | null>(null)

  const fetchSystemConfigs = async () => {
    setIsLoading(true);
    try {
      const configList = await getConfigList(adminAxiosInstance);
      if (configList.success) {
        setState(state_creator(configList.data.SystemConfigList));
      } else {
        console.error('API Error:', configList.message);
      }
    } catch (error) {
      console.error('Error fetching system configs:', error);
    } finally {
      setIsLoading(false);
    }
  }

  const fetchActiveSystemConfig = async () => {
     try {
      const active = await getActiveConfig(adminAxiosInstance);
      if (active.success) {
        setActivateConfig(active.data.SystemConfig);
      } else {
        console.error('API Error:', active.message);
      }
    } catch (error) {
      console.error('Error fetching system configs:', error);
    } finally {
      setIsLoading(false);
    }
  }

  const handleDataChange = async () => {
    setIsLoading(false)
    await Promise.all([fetchSystemConfigs(), fetchActiveSystemConfig()])
  }

  const handleInactivate = async () => {
    // Call API to inactivate the active config
    console.log("Inactivating config:", activateConfig?.Id)
  }

  useEffect(() => {
    handleDataChange()
  }, [])

  const defaultColDef = useMemo(() => {
    return {
      flex: 1,
      filter: true,
      autoHeight: true,
      resizable: true,
      wrapText: true,
      cellClass: "d-flex align-items-center",
      editable: false,
    }
  }, [])

  const handleAddConfig = () => {
    // This will be handled by Modal_Button
  }

  return (
    <SystemConfigViewContext.Provider value={{ handleDataChange: handleDataChange }}>
      <div className="system-config-view">
        {activateConfig && (
          <div className="system-config-view__active-section">
            <div className="system-config-view__active-header">
              <div className="system-config-view__active-info">
                <div className="system-config-view__active-name">{activateConfig.Name}</div>
                <div className="system-config-view__active-date">ID: #{activateConfig.Id}</div>
                <div className="system-config-view__active-date">Updated At: {formatDate(activateConfig.UpdatedAt)}</div>
                <div className="system-config-view__active-date">Created At: {formatDate(activateConfig.CreatedAt)}</div>
              </div>
              <div className="system-config-view__active-actions">
                <div className="system-config-view__status-badge system-config-view__status-badge--active-large">
                  <span className="system-config-view__status-dot"></span>
                  Currently Active
                </div>
                <button className="system-config-view__inactive-btn" onClick={handleInactivate}>
                  Set Inactive
                </button>
              </div>
            </div>

            <div className="system-config-view__active-content">
              {/* Top Section: Podcast Subscription Configuration */}
              <div className="config-section">
                <h5 className="config-subsection__title">Podcast Subscription Configuration</h5>
                <div className="config-grid config-grid--compact">
                  {activateConfig.PodcastSubsriptionConfigList?.map((config, index) => (
                    <div key={index} className="config-card config-card--compact">
                      <div className="config-card__header">
                        <span className="config-card__type">{config.SubscriptionCycleType.Name}</span>
                      </div>
                      <div className="config-card__content">
                        <div className="config-item config-item--compact">
                          <span className="config-label">Profit Rate:</span>
                          <span className="config-value">{(config.ProfitRate * 100).toFixed(1)}%</span>
                        </div>
                        <div className="config-item config-item--compact">
                          <span className="config-label">Income Delay:</span>
                          <span className="config-value">{config.IncomeTakenDelayDays} days</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Middle Section: Left-Right Layout */}
              <div className="config-section">
                <div className="config-row">
                  {/* Left Column */}
                  <div className="config-column config-column--left">
                    {/* Podcast Suggestion Config */}
                    {activateConfig.PodcastSuggestionConfig && (
                      <div className="config-subsection">
                        <h5 className="config-subsection__title">Podcast Suggestion</h5>
                        <div className="config-card config-card--compact config-card--single">
                          <div className="config-card__content">
                            <div className="config-item config-item--compact">
                              <span className="config-label">Behavior Lookback:</span>
                              <span className="config-value">{activateConfig.PodcastSuggestionConfig.BehaviorLookbackDayCount} days</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Min Channel Query:</span>
                              <span className="config-value">{activateConfig.PodcastSuggestionConfig.MinChannelQuery}</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Min Show Query:</span>
                              <span className="config-value">{activateConfig.PodcastSuggestionConfig.MinShowQuery}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Account Config */}
                    {activateConfig.AccountConfig && (
                      <div className="config-subsection">
                        <h5 className="config-subsection__title">Account Configuration</h5>
                        <div className="config-card config-card--compact config-card--single">
                          <div className="config-card__content">
                            <div className="config-item config-item--compact">
                              <span className="config-label">Violation Point Decay:</span>
                              <span className="config-value">{activateConfig.AccountConfig.ViolationPointDecayHours}h</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Listen Slot Threshold:</span>
                              <span className="config-value">{activateConfig.AccountConfig.PodcastListenSlotThreshold}</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Slot Recovery Time:</span>
                              <span className="config-value">{activateConfig.AccountConfig.PodcastListenSlotRecoverySeconds}s</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Review Session Config */}
                    {activateConfig.ReviewSessionConfig && (
                      <div className="config-subsection">
                        <h5 className="config-subsection__title">Review Session</h5>
                        <div className="config-card config-card--compact config-card--single">
                          <div className="config-card__content">
                            <div className="config-item config-item--compact">
                              <span className="config-label">Buddy Unresolved:</span>
                              <span className="config-value">{activateConfig.ReviewSessionConfig.PodcastBuddyUnResolvedReportStreak}</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Show Unresolved:</span>
                              <span className="config-value">{activateConfig.ReviewSessionConfig.PodcastShowUnResolvedReportStreak}</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Episode Unresolved:</span>
                              <span className="config-value">{activateConfig.ReviewSessionConfig.PodcastEpisodeUnResolvedReportStreak}</span>
                            </div>
                            <div className="config-item config-item--compact">
                              <span className="config-label">Publish Edit Expired:</span>
                              <span className="config-value">{activateConfig.ReviewSessionConfig.PodcastEpisodePublishEditRequirementExpiredHours}h</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Right Column */}
                  <div className="config-column config-column--right">
                    {/* Booking Configuration */}
                    {activateConfig.BookingConfig && (
                      <div className="config-subsection">
                        <h5 className="config-subsection__title">Booking Configuration</h5>
                        <div className="config-booking">
                          {/* Basic Rates */}
                          <div className="config-card config-card--compact config-card--single">
                            <div className="config-card__content">
                              <div className="config-item config-item--compact">
                                <span className="config-label">Profit Rate:</span>
                                <span className="config-value">{(activateConfig.BookingConfig.ProfitRate * 100).toFixed(1)}%</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Deposit Rate:</span>
                                <span className="config-value">{(activateConfig.BookingConfig.DepositRate * 100).toFixed(1)}%</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Preview Listen Slot:</span>
                                <span className="config-value">{activateConfig.BookingConfig.PodcastTrackPreviewListenSlot}</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Preview Response:</span>
                                <span className="config-value">{activateConfig.BookingConfig.PreviewResponseAllowedDays} days</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Producing Response:</span>
                                <span className="config-value">{activateConfig.BookingConfig.ProducingRequestResponseAllowedDays} days</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Chat Room Expired:</span>
                                <span className="config-value">{activateConfig.BookingConfig.ChatRoomExpiredHours}h</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">File Message Expired:</span>
                                <span className="config-value">{activateConfig.BookingConfig.ChatRoomFileMessageExpiredHours}h</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Free Storage:</span>
                                <span className="config-value">{activateConfig.BookingConfig.FreeInitalBookingStorageSize}MB</span>
                              </div>
                              <div className="config-item config-item--compact">
                                <span className="config-label">Storage Price:</span>
                                <span className="config-value">{activateConfig.BookingConfig.SingleStorageUnitPurchasePrice.toLocaleString()}đ</span>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Bottom Section: Violation Levels */}
              {activateConfig.AccountViolationLevelConfigList && (
                <div className="config-section">
                  <h4 className="config-subsection__title">Account Violation Level Configuration</h4>
                  <div className="violation-table">
                    <AgGridReact
                      columnDefs={[
                        {
                          headerName: "Violation Level",
                          field: "ViolationLevel",
                          flex: 1,
                          cellRenderer: (params: any) => (
                            <span className="violation-level-badge">Level {params.value}</span>
                          )
                        },
                        {
                          headerName: "Point Threshold",
                          field: "ViolationPointThreshold",
                          flex: 1
                        },
                        {
                          headerName: "Punishment Days",
                          field: "PunishmentDays",
                          flex: 1
                        },
                        {
                          headerName: "Created At",
                          field: "CreatedAt",
                          flex: 1.2,
                          valueFormatter: (params: any) => formatDate(params.value)
                        },
                        {
                          headerName: "Updated At",
                          field: "UpdatedAt",
                          flex: 1.2,
                          valueFormatter: (params: any) => formatDate(params.value)
                        }
                      ]}
                      rowData={activateConfig.AccountViolationLevelConfigList}
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

                    />
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        <div className="system-config-view__table-section">
          <div className="system-config-view__table-section-header">
            <h3 className="system-config-view__table-title">All System Configurations</h3>
            <div>
              <Modal_Button
                disabled={false}
                title="Add New System Configuration"
                content={<span className="system-config-view__add-btn ">Add Configuration</span>}
                color="white"
              >
                <SystemConfigUpdate systemConfigId={null} onClose={() => { }} />
              </Modal_Button>
            </div>
          </div>

          <CRow>
            <CCol xs={12}>
              {isLoading ? (
                <Loading />
              ) : (
                <div className="system-config-view__table">
                  <AgGridReact
                    columnDefs={state?.columnDefs}
                    rowData={state?.rowData}
                    defaultColDef={defaultColDef}
                    rowHeight={70}
                    headerHeight={50}
                    pagination={true}
                    paginationPageSize={10}
                    paginationPageSizeSelector={[10, 20, 50, 100]}
                    domLayout="autoHeight"
                  />
                </div>
              )}
            </CCol>
          </CRow>
        </div>
      </div>
    </SystemConfigViewContext.Provider>
  )
}

export default SystemConfigView
