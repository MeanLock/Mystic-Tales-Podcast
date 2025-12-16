import React, { Children, useContext, useState } from 'react';
import {
  CButton,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
} from '@coreui/react';
import './styles.scss';
import logo from '../../../../assets/brand/logoMTP2.png'
type ModalButtonContext = {
  handleDataChange: () => void;
  // add other context properties if needed
};

interface ModalButtonProps {
  context?: React.Context<ModalButtonContext>;
  color?: string;
  disabled?: boolean;
  content?: React.ReactNode;
  size?: "sm" | "lg" | "xl" ;
  title?: React.ReactNode | null;
  children?: React.ReactNode;
  className?: string;
}

const Modal_Button = (props: ModalButtonProps) => {
  const [visible, setVisible] = useState(false);
  const contextValue = props.context ? useContext(props.context) : null;

  const handleClose = () => {
    setVisible(false);
    if (contextValue) {
      const { handleDataChange } = contextValue;
      handleDataChange();
    }
  };


  return (
    <>
      <CButton className={props.className} style={{ border: 'none' }} color={props.color} onClick={() => setVisible(!visible)} disabled={props.disabled}>
        {props.content}
      </CButton>
      <CModal size={props.size ? props.size : 'xl'} className='custom-modal' backdrop="static" visible={visible} onClose={handleClose}
      >
        <CModalHeader className='custom-modal-header' >
          {props.title === null || props.title === '' ? (
            <div className='d-flex items-center justify-center gap-2'>
              <img src={logo} alt="Logo" className="w-10 h-10" />
              <p className='text-white font-bold m-0'>Mystic Tales Podcast </p>
            </div>
          ) : (
            <CModalTitle className='custom-modal-title'>{props.title}</CModalTitle>
          )}
        </CModalHeader>
        <CModalBody className='p-0'>

          {React.isValidElement(props.children)
            ? React.cloneElement(props.children as React.ReactElement<any>, { onClose: handleClose })
            : props.children}

        </CModalBody>
      </CModal>
    </>
  )
}

export default Modal_Button