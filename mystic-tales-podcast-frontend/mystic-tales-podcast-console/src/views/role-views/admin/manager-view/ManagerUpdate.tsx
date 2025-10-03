import React, { useContext, useEffect, useState, useRef, FormEvent } from "react";
import {
  CButton,
  CCol,
  CForm,
  CFormInput,
  CFormFeedback,
  CFormLabel
} from '@coreui/react';
import { Account } from "../../../../core/types";
import { deactivateAccount, updateAccount } from "../../../../core/services/account/account.service";
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
import { ManagerViewContext } from ".";
import { toast } from "react-toastify";
import { formatDate } from "../../../../core/utils/date.util";

interface ManagerUpdateProps {
  account: Account;
  onClose: () => void;
}

const CustomForm: React.FC<ManagerUpdateProps> = ({ account, onClose }) => {
  const context = useContext(ManagerViewContext);
  const fullname = useRef<HTMLInputElement>(null);
  const dob = useRef<HTMLInputElement>(null);
  const gender = useRef<HTMLInputElement>(null);
  const phone = useRef<HTMLInputElement>(null);
  const address = useRef<HTMLInputElement>(null);
  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const data = {
      FullName: fullname.current?.value,
      Dob: dob.current?.value,
      Gender: gender.current?.value,
      Address: address.current?.value,
      Phone: phone.current?.value,
    };
    try {
      const res = await updateAccount(adminAxiosInstance, account.Id, data);
      if (res.success) {
        onClose();
        context?.handleDataChange();
        toast.success(`Tài khoản ${account.Id} cập nhật thành công!`);
      }
    } catch (error) {
      console.error("Error deactivating account:", error);
    }
  };
  const handleDeactivate = async (event: React.FormEvent, isDeactivate: boolean) => {
    event.preventDefault();
    try {
      const res = await deactivateAccount(adminAxiosInstance, account.Id, isDeactivate);
      if (res.success) {
        onClose();
        context?.handleDataChange();
        toast.success(`Tài khoản ${isDeactivate ? 'vô hiệu hóa' : 'kích hoạt'} thành công!`);
      }
    } catch (error) {
      console.error("Error deactivating account:", error);
    }
  };
  return (
    <CForm className="row g-3 needs-validation" noValidate onSubmit={handleSubmit}>
      <CCol md={12}>
        <CFormLabel htmlFor="address">Email</CFormLabel>
        <CFormInput type="text" id="address" defaultValue={account.Email} disabled />
      </CCol>
      {account.DeactivatedAt !== null && (
        <CCol md={12}>
          <CFormLabel htmlFor="address">Ngày vô hiệu hóa</CFormLabel>
          <CFormInput type="text" id="address" defaultValue={formatDate(account.DeactivatedAt)} disabled />
        </CCol>
      )}

      <CCol md={12}>
        <CFormLabel htmlFor="fullname">Họ và Tên</CFormLabel>
        <CFormInput type="text" id="fullname" defaultValue={account.FullName} ref={fullname} required />
        <CFormFeedback valid>Looks good!</CFormFeedback>
      </CCol>

      <CCol md={12}>
        <CFormLabel htmlFor="dob">Ngày sinh</CFormLabel>
        <CFormInput type="date" id="dob" defaultValue={account.Dob} ref={dob} required />
        <CFormFeedback valid>Looks good!</CFormFeedback>
      </CCol>

      <CCol md={12}>
        <CFormLabel htmlFor="gender">Giới Tính</CFormLabel>
        <CFormInput type="text" id="gender" defaultValue={account.Gender} ref={gender} required />
        <CFormFeedback valid>Looks good!</CFormFeedback>
      </CCol>

      <CCol md={12}>
        <CFormLabel htmlFor="phone">SĐT</CFormLabel>
        <CFormInput type="text" id="phone" defaultValue={account.Phone} ref={phone} required />
        <CFormFeedback valid>Looks good!</CFormFeedback>
      </CCol>

      <CCol md={12}>
        <CFormLabel htmlFor="address">Địa chỉ</CFormLabel>
        <CFormInput type="text" id="address" defaultValue={account.Address} ref={address} required />
        <CFormFeedback valid>Looks good!</CFormFeedback>
      </CCol>

      <CCol xs={12} className="d-flex justify-content-center align-items-center">
        <CButton
          className="mx-2"
          color={account.DeactivatedAt === null ? "danger" : "info"}
          style={{ color: "white" }}
          onClick={(e) => handleDeactivate(e, account.DeactivatedAt === null ? true : false)}
        >
          {account.DeactivatedAt === null ? 'Vô hiệu hóa' : 'Kích hoạt'}
        </CButton>
        |
        <CButton className="mx-2" color="success" type="submit" style={{ color: "white" }} onClick={handleSubmit}
        >
          Cập nhật
        </CButton>
      </CCol>
    </CForm>
  );
};


const ManagerUpdate: React.FC<ManagerUpdateProps> = (props) => {
  return <CustomForm {...props} />;
};

export default ManagerUpdate;
