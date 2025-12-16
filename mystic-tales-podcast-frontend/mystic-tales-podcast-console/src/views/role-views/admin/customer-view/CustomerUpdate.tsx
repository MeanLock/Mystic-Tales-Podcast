import React, { useContext, useEffect, useState, useRef, FormEvent } from "react";
import {
  CButton,
  CCol,
  CForm,
  CFormInput,
  CFormFeedback,
  CFormLabel,
  CFormSelect
} from '@coreui/react';
import { Account } from "../../../../core/types";
import { deactivateAccount, updateAccount } from "../../../../core/services/account/account.service";
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
import { toast } from "react-toastify";
import { CustomerViewContext } from ".";
import { formatDate, fromInputDateToISO } from "../../../../core/utils/date.util";
import Image from "@/views/components/common/image";
import { useSagaPolling } from "@/hooks/useSagaPolling";
import { PencilSimple } from "phosphor-react";
import { confirmAlert } from "@/core/utils/alert.util";

interface CustomerUpdateProps {
  account: Account;
  onClose: () => void;
}

const CustomForm: React.FC<CustomerUpdateProps> = ({ account, onClose }) => {
  const context = useContext(CustomerViewContext);
  const fullname = useRef<HTMLInputElement>(null);
  const dob = useRef<HTMLInputElement>(null);
  const gender = useRef<HTMLSelectElement>(null)
  const phone = useRef<HTMLInputElement>(null);
  const address = useRef<HTMLInputElement>(null);
  const [mainImageFile, setMainImageFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const { startPolling } = useSagaPolling({
    timeoutSeconds: 5,
    intervalSeconds: 0.5,
  })

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setMainImageFile(file);
  };
  useEffect(() => {
    if (!mainImageFile) {
      setPreviewUrl(null)
      return
    }
    const url = URL.createObjectURL(mainImageFile)
    setPreviewUrl(url)
    return () => {
      URL.revokeObjectURL(url)
    }
  }, [mainImageFile])

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const data = {
      FullName: fullname.current?.value,
      Dob: fromInputDateToISO(dob.current?.value)!,
      Gender: gender.current?.value,
      Address: address.current?.value,
      Phone: phone.current?.value,
    };
    try {
      const res = await updateAccount(adminAxiosInstance, account.Id, {
        AccountUpdateInfo: data,
        MainImageFile: mainImageFile || null,
      });
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.error("Update account failed, please try again.")
        return
      }
      await startPolling(sagaId, adminAxiosInstance, {
        onSuccess: () => {
          onClose();
          context?.handleDataChange();
          toast.success(`Account ${account.Id} updated successfully!`);
        },
        onFailure: (err) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })
    } catch (error) {
      toast.error("Error updating account");
    }
  };


  const handleDeactivate = async (event: React.FormEvent, isDeactivate: boolean) => {
    const alert = confirmAlert(`Are you sure to ${isDeactivate ? "deactivate" : "activate"} this account?`);
    if (!(await alert).isConfirmed) return;
    event.preventDefault();
    try {
      const res = await deactivateAccount(adminAxiosInstance, account.Id, isDeactivate);
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.error("Deactivate account failed, please try again.")
        return
      }
      await startPolling(sagaId, adminAxiosInstance, {
        onSuccess: () => {
          onClose();
          context?.handleDataChange();
          toast.success(`Account ${isDeactivate ? 'deactivated' : 'activated'} successfully!`);
        },
        onFailure: (err) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })
    } catch (err) {
      toast.error("Error deactivating account");
    }
  };

  return (
    <div className="account-info">
      <div className="account-info__header">
        <div className="account-info__profile">
          <div className="account-info__avatar-wrapper" style={{ position: "relative", display: "inline-block" }}>
            {previewUrl ? (
              <img
                src={previewUrl}
                alt="Avatar preview"
                className="account-info__avatar"
              />
            ) : (
              <Image mainImageFileKey={account.MainImageFileKey} className="account-info__avatar" />
            )}
            <label htmlFor="avatar-upload" className="account-info__avatar-upload-label">
              <PencilSimple size={22} weight="bold" />
              <input
                id="avatar-upload"
                type="file"
                accept="image/"
                onChange={handleImageChange}
                className="account-info__avatar-input"
                style={{ display: "none" }}
              />
            </label>
          </div>
          <div className="account-info__identity">
            <h2 className="account-info__name">{account.FullName || "N/A"}</h2>
            <p className="account-info__email">{account.Email}</p>
            <div className="account-info__role-badge">{account.Role?.Name || "N/A"}</div>
          </div>
        </div>

        <div className="account-info__status-group">
          <div className="account-info__status-label">Account Verification Status</div>
          <div className={`account-info__badge account-info__badge--${account.IsVerified ? "verified" : "unverified"}`}>
            <span className="account-info__badge-dot"></span>
            {account.IsVerified ? "Account Verified" : "Account Not Verified"}
          </div>
       
          {account.DeactivatedAt && (
            <div className="account-info__badge account-info__badge--danger">
              <span className="account-info__badge-dot"></span>
              Deactivated on {formatDate(account.DeactivatedAt)}
            </div>
          )}
        </div>
      </div>

      <CForm noValidate onSubmit={handleSubmit} className="account-info__form">
        <div className="account-info__section">
          <h3 className="account-info__section-title">Personal Information</h3>
          <div className="row g-3">
            <CCol md={8}>
              <div className="account-info__field">
                <CFormLabel htmlFor="fullname" className="account-info__label">
                  Full Name 
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="fullname"
                  defaultValue={account.FullName}
                  ref={fullname}
                  disabled
                  className="account-info__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="account-info__field">
                <CFormLabel htmlFor="dob" className="account-info__label">
                  Date of Birth 
                </CFormLabel>
                <CFormInput
                  type="date"
                  id="dob"
                  defaultValue={account.Dob}
                  ref={dob}
                  disabled
                  className="account-info__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="account-info__field">
                <CFormLabel htmlFor="gender" className="account-info__label">
                  Gender 
                </CFormLabel>
                <CFormSelect
                  id="gender"
                  ref={gender}
                  defaultValue={account.Gender}
                  disabled
                  className="account-info__input"
                >
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </CFormSelect>
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="account-info__field">
                <CFormLabel htmlFor="phone" className="account-info__label">
                  Phone Number
                </CFormLabel>
                <CFormInput
                  type="tel"
                  id="phone"
                  defaultValue={account.Phone}
                  ref={phone}
                  disabled
                  className="account-info__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="account-info__field">
                <CFormLabel htmlFor="balance" className="account-info__label">
                  Account Balance
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="balance"
                  defaultValue={`${account.Balance || 0} Point`}
                  disabled
                  className="account-info__input account-info__input--disabled"
                />
              </div>
            </CCol>
            <CCol md={12}>
              <div className="account-info__field">
                <CFormLabel htmlFor="address" className="account-info__label">
                  Address 
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="address"
                  defaultValue={account.Address}
                  ref={address}
                  disabled
                  className="account-info__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
          </div>
        </div>

        <div className="account-info__section">
          <h3 className="account-info__section-title">Account Statistics</h3>
          <div className="account-info__stats">
            <div className="account-info__stat-card account-info__stat-card--violation">
              <div className="account-info__stat-label">Violation Points</div>
              <div className="account-info__stat-value">{account.ViolationPoint || 0}</div>
            </div>
            <div className="account-info__stat-card account-info__stat-card--violation">
              <div className="account-info__stat-label">Violation Level</div>
              <div className="account-info__stat-value">{account.ViolationLevel || 0}</div>
            </div>
            <div className="account-info__stat-card account-info__stat-card--podcast">
              <div className="account-info__stat-label">Podcast Listen Slot</div>
              <div className="account-info__stat-value">{account.PodcastListenSlot || 0}</div>
            </div>
          </div>
        </div>

        <div className="account-info__section ">
          <div className="flex gap-5 bg-white justify-between items-center px-4 ">
            <div className="account-info__timeline">
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label">Updated At</div>
                  <div className="account-info__timeline-date">{account.UpdatedAt === null ? "N/A" : formatDate(account.UpdatedAt)}</div>
                </div>
              </div>
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label">Created At</div>
                  <div className="account-info__timeline-date">{formatDate(account.CreatedAt)}</div>
                </div>
              </div>
            </div>
            <div className="account-info__timeline">
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label">Violation Point Changed</div>
                  <div className="account-info__timeline-date">{account.LastViolationPointChanged === null ? "N/A" : formatDate(account.LastViolationPointChanged)}</div>
                </div>
              </div>
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label"> Violation Level Changed</div>
                  <div className="account-info__timeline-date">{account.LastViolationLevelChanged === null ? "N/A" : formatDate(account.LastViolationLevelChanged)}</div>
                </div>
              </div>
            </div>
            <div className="account-info__timeline">
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label"> Deactivated At</div>
                  <div className="account-info__timeline-date">{account.DeactivatedAt === null ? "N/A" : formatDate(account.DeactivatedAt)}</div>
                </div>
              </div>
              <div className="account-info__timeline-item">
                <div className="account-info__timeline-dot"></div>
                <div className="account-info__timeline-content">
                  <div className="account-info__timeline-label"> Listen Slot Changed</div>
                  <div className="account-info__timeline-date">{account.LastPodcastListenSlotChanged === null ? "N/A" : formatDate(account.LastPodcastListenSlotChanged)}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="account-info__actions">
          <CButton
            className={`account-info__btn ${account.DeactivatedAt === null ? "account-info__btn--deactivate" : "account-info__btn--activate"}`}
            onClick={(e) => handleDeactivate(e, account.DeactivatedAt === null ? true : false)}
          >
            {account.DeactivatedAt === null ? "Deactivate Account" : "Activate Account"}
          </CButton>
          {/* <CButton className="account-info__btn account-info__btn--update" type="submit" onClick={handleSubmit}>
            Update Information
          </CButton> */}
        </div>
      </CForm>
    </div>
  );
};


const CustomerUpdate: React.FC<CustomerUpdateProps> = (props) => {
  return <CustomForm {...props} />;
};

export default CustomerUpdate;
