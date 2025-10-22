import React, { useContext, useState, useRef, FormEvent } from "react";
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

interface StaffRegisterProps {
  onClose: () => void;
}

const StaffForm: React.FC<StaffRegisterProps> = ({ onClose }) => {
  const context = useContext(StaffViewContext);

  // Form refs
  const email = useRef<HTMLInputElement>(null);
  const password = useRef<HTMLInputElement>(null);
  const fullname = useRef<HTMLInputElement>(null);
  const dob = useRef<HTMLInputElement>(null);
  const gender = useRef<HTMLSelectElement>(null);
  const address = useRef<HTMLInputElement>(null);
  const phone = useRef<HTMLInputElement>(null);
  const mainImageFile = useRef<HTMLInputElement>(null);

  // State for form validation and UI
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string>("");
  const [imagePreview, setImagePreview] = useState<string>("");
  // Handle image file selection
  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        setImagePreview(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

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

    if (password.current?.value && password.current.value.length < 6) {
      setError("Password must be at least 6 characters long");
      return false;
    }

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

    try {
      const formData = new FormData();
      formData.append("Email", email.current?.value || "");
      formData.append("Password", password.current?.value || "");
      formData.append("Fullname", fullname.current?.value || "");
      formData.append("Dob", dob.current?.value || "");
      formData.append("Gender", gender.current?.value || "");
      formData.append("Address", address.current?.value || "");
      formData.append("Phone", phone.current?.value || "");

      if (mainImageFile.current?.files?.[0]) {
        formData.append("MainImageFile", mainImageFile.current.files[0]);
      }

      // const response = await adminAxiosInstance.post("/staff/register", formData, {
      //   headers: {
      //     "Content-Type": "multipart/form-data",
      //   },
      // });
      const response = await axios.post("https://a03f85e45b62.ngrok-free.app//api/auth/register/staff", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      console.log(response);
      if (response.data.SagaInstanceId) {
        const sagaId = response.data.SagaInstanceId
        setSagaInstanceId(sagaId)
        toast.success("Staff registered successfully!");
        context?.handleDataChange();
        onClose();
      } else {
        setError(response.data.message || "Registration failed");
      }
    } catch (error: any) {
      console.error("Error registering staff:", error);
      setError(error.response?.data?.message || "An error occurred during registration");
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
        <CAlert color="danger" className="mb-4">
          {error}
        </CAlert>
      )}

      <CForm noValidate onSubmit={handleSubmit} className="staff-register__form">
        {/* Profile Image Section */}
        <div className="staff-register__section">
          <h3 className="staff-register__section-title">Profile Image</h3>
          <div className="staff-register__image-upload">
            <div className="staff-register__image-preview">
              <img
                src={imagePreview || "/placeholder.svg"}
                alt="Profile Preview"
                className="staff-register__avatar"
              />
            </div>
            <div className="staff-register__upload-controls">
              <CFormLabel htmlFor="mainImageFile" className="staff-register__label">
                Profile Image
              </CFormLabel>
              <CFormInput
                type="file"
                id="mainImageFile"
                ref={mainImageFile}
                accept="image/*"
                onChange={handleImageChange}
                className="staff-register__input"
              />
            </div>
          </div>
        </div>

        {/* Account Credentials Section */}
        <div className="staff-register__section">
          <h3 className="staff-register__section-title">Account Credentials</h3>
          <CRow className="g-3">
            <CCol md={6}>
              <div className="staff-register__field">
                <CFormLabel htmlFor="email" className="staff-register__label">
                  Email Address
                </CFormLabel>
                <CFormInput
                  type="email"
                  id="email"
                  ref={email}
                  required
                  placeholder="Enter email address"
                  className="staff-register__input"
                />
                <CFormFeedback valid>Looks good!</CFormFeedback>
              </div>
            </CCol>
            <CCol md={6}>
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
          </CRow>
        </div>

        {/* Personal Information Section */}
        <div className="staff-register__section">
          <h3 className="staff-register__section-title">Personal Information</h3>
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
