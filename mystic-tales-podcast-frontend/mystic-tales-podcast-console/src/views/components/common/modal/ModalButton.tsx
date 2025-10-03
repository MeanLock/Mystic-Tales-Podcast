import React, { Children, useContext, useState } from 'react';
import {
  CButton,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
} from '@coreui/react';

type ModalButtonContext = {
  handleDataChange: () => void;
  // add other context properties if needed
};

interface ModalButtonProps {
  context?: React.Context<ModalButtonContext>;
  color?: string;
  disabled?: boolean;
  content?: React.ReactNode;
  size?: "sm" | "lg" | "xl";
  title?: React.ReactNode;
  children?: React.ReactNode;
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
        <CButton className="w-100 h-100 px-0 py-0" style={{border:'none'}}   color={props.color} onClick={() => setVisible(!visible)} disabled={props.disabled}>
          {props.content}
        </CButton>
        <CModal size={props.size ? props.size : 'xl'} className='custom-modal' backdrop="static"  visible={visible}         onClose={handleClose}
>
          <CModalHeader>
            <CModalTitle>{props.title}</CModalTitle>
          </CModalHeader>
          <CModalBody>

            { React.isValidElement(props.children)
              ? React.cloneElement(props.children as React.ReactElement<any>, { onClose: handleClose })
              : props.children}
            
          </CModalBody>
        </CModal>
      </>
    )
  }

  export default Modal_Button