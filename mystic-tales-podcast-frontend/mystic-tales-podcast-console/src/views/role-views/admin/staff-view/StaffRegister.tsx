import React, { useContext, useState, useRef, FormEvent, useEffect } from "react";
import {
  CButton,
  CCol,
  CForm,
  CFormInput,
  CFormFeedback,
  CFormLabel,
  CFormSelect,
  CAlert,
  CRow
} from '@coreui/react';
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
import { toast } from "react-toastify";
import { StaffViewContext } from ".";
import axios from "axios";
import NotFound from '../../../../assets/images/notfound.png'
import { fromInputDateToISO } from "@/core/utils/date.util";
import { useSagaPolling } from "@/hooks/useSagaPolling";
import { RegisterStaffAccount } from "@/core/services/auth/auth.service";

interface StaffRegisterProps {
  onClose: () => void;
}

const StaffForm: React.FC<StaffRegisterProps> = ({ onClose }) => {
  const context = useContext(StaffViewContext);

  // Form refs
  const email = useRef<HTMLInputElement>(null);
  const password = useRef<HTMLInputElement>(null);
  const confirmPassword = useRef<HTMLInputElement>(null);
  const fullname = useRef<HTMLInputElement>(null);
  const dob = useRef<HTMLInputElement>(null);
  const gender = useRef<HTMLSelectElement>(null);
  const address = useRef<HTMLInputElement>(null);
  const phone = useRef<HTMLInputElement>(null);
  const [mainImageFile, setMainImageFile] = useState<File | null>(null);

  const { startPolling } = useSagaPolling({
    timeoutSeconds: 5,
    intervalSeconds: 0.5,
  })
  // State for form validation and UI
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string>("");
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  // Handle image file selection
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setMainImageFile(file);
  };
  useEffect(() => {
    if (!mainImageFile) {
      setImagePreview(null)
      return
    }
    const url = URL.createObjectURL(mainImageFile)
    setImagePreview(url)
    return () => {
      URL.revokeObjectURL(url)
    }
  }, [mainImageFile])

  // Validate form inputs
  const validateForm = (): boolean => {
    setError("");

    if (!email.current?.value?.trim()) {
      setError("Email is required");
      return false;
    }

    if (!password.current?.value?.trim()) {
      setError("Password is required");
      return false;
    }
    if (!password.current?.value?.trim()) {
      setError("Password is required");
      return false;
    }
    if (password.current?.value !== confirmPassword.current?.value) {
      setError("Passwords do not match");
      return false;
    }
    // if (password.current?.value && password.current.value.length < 6) {
    //   setError("Password must be at least 6 characters long");
    //   return false;
    // }

    if (!fullname.current?.value?.trim()) {
      setError("Full name is required");
      return false;
    }

    if (!dob.current?.value) {
      setError("Date of birth is required");
      return false;
    }

    if (!gender.current?.value) {
      setError("Gender is required");
      return false;
    }

    if (!address.current?.value?.trim()) {
      setError("Address is required");
      return false;
    }

    if (!phone.current?.value?.trim()) {
      setError("Phone number is required");
      return false;
    }

    return true;
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    setError("");
    const data = {

      Email: email.current?.value,
      Password: password.current?.value,
      FullName: fullname.current?.value,
      Dob: fromInputDateToISO(dob.current?.value)!,
      Gender: gender.current?.value,
      Address: address.current?.value,
      Phone: phone.current?.value,
    };
    try {
      const res = await RegisterStaffAccount(adminAxiosInstance, {
        RegisterInfo: data,
        MainImageFile: mainImageFile || null,
      });
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.error("Register account failed, please try again.")
        return
      }
      await startPolling(sagaId, adminAxiosInstance, {
        onSuccess: () => {
          onClose();
          context?.handleDataChange();
          toast.success(`Account registered successfully!`);
        },
        onFailure: (err: any) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })
    } catch (error) {
      toast.error("Error registering account");
    } finally {
      setIsSubmitting(false);
    }
  };


  return (
    <div className="staff-register">
      <div className="staff-register__header">
        <h2 className="staff-register__title">Add New Staff</h2>
        <p className="staff-register__subtitle">
          Create a new staff account with the required information
        </p>
      </div>

      {error && (
        <CAlert color="danger" className="my-4 mx-8">
          ⚠️ {error} !
        </CAlert>
      )}

      <CForm noValidate onSubmit={handleSubmit} className="staff-register__form">
        {/* Profile Image Section */}
        <div className="staff-register__section">
          <div className="flex gap-4 ">
            <div className=" flex flex-col justify-center items-center gap-4 w-1/2 ">
              <div className="staff-register__image-preview">
                <img
                  src={imagePreview || NotFound}
                  alt="Profile Preview"
                  className="staff-register__avatar"
                />
              </div>
              <div className="staff-register__upload-controls w-2/3 ">
                <CFormInput
                  type="file"
                  id="mainImageFile"
                  accept="image/*"
                  onChange={handleImageChange}
                  className="staff-register__input"
                />
              </div>
            </div>
            <CRow className="g-1 w-2/3 flex flex-col justify-center ">
              <CCol className="w-full">
                <div className="staff-register__field">
                  <CFormLabel htmlFor="email" className="staff-register__label">
                    Email
                  </CFormLabel>
                  <CFormInput
                    type="email"
                    id="email"
                    ref={email}
                    required
                    placeholder="Enter email address"
                    className="staff-register__input w-full"
                  />
                  <CFormFeedback valid>Looks good!</CFormFeedback>
                </div>
              </CCol>
              <CCol className="w-full">
                <div className="staff-register__field">
                  <CFormLabel htmlFor="password" className="staff-register__label">
                    Password
                  </CFormLabel>
                  <CFormInput
                    type="password"
                    id="password"
                    ref={password}
                    required
                    placeholder="Enter password (min. 6 characters)"
                    className="staff-register__input"
                  />
                  <CFormFeedback valid>Looks good!</CFormFeedback>
                </div>
              </CCol>
              <CCol className="w-full">
                <div className="staff-register__field">
                  <CFormLabel htmlFor="password" className="staff-register__label">
                    Confirm Password
                  </CFormLabel>
                  <CFormInput
                    type="password"
                    id="password"
                    ref={confirmPassword}
                    required
                    placeholder="Enter password (min. 6 characters)"
                    className="staff-register__input"
                  />
                  <CFormFeedback valid>Looks good!</CFormFeedback>
                </div>
              </CCol>
            </CRow>
          </div>
        </div>

        {/* Personal Information Section */}
        <div className="staff-register__section">
          <CRow className="g-3">
            <CCol md={8}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="fullname" className="staff-register__label">
                  Full Name
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="fullname"
                  ref={fullname}
                  required
                  placeholder="Enter full name"
                  className="staff-register__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="dob" className="staff-register__label">
                  Date of Birth
                </CFormLabel>
                <CFormInput
                  type="date"
                  id="dob"
                  ref={dob}
                  required
                  className="staff-register__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="gender" className="staff-register__label">
                  Gender
                </CFormLabel>
                <CFormSelect
                  id="gender"
                  ref={gender}
                  required
                  className="staff-register__input"
                >
                  <option value="">Select gender</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </CFormSelect>
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="phone" className="staff-register__label">
                  Phone Number
                </CFormLabel>
                <CFormInput
                  type="tel"
                  id="phone"
                  ref={phone}
                  required
                  placeholder="Enter phone number"
                  className="staff-register__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={4}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="address" className="staff-register__label">
                  Address
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="address"
                  ref={address}
                  required
                  placeholder="Enter address"
                  className="staff-register__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
          </CRow>
        </div>

        <div className="staff-register__actions">
          <CButton
            type="button"
            color="secondary"
            onClick={onClose}
            disabled={isSubmitting}
            className="staff-register__btn staff-register__btn--cancel"
          >
            Cancel
          </CButton>
          <CButton
            type="submit"
            color="primary"
            disabled={isSubmitting}
            className="staff-register__btn staff-register__btn--submit"
          >
            {isSubmitting ? "Registering..." : "Register Staff"}
          </CButton>
        </div>
      </CForm>
    </div>
  );
};


const StaffRegister: React.FC<StaffRegisterProps> = (props) => {
  return <StaffForm {...props} />;
};

export default StaffRegister;
