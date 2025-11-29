import React, { useContext, useState } from 'react';
import {
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import './styles.scss';
import logo from "../../../../assets/logoMTP2.png"

type ModalButtonContext = {
  handleDataChange: () => void;
  // add other context properties if needed
};

interface ModalButtonProps {
  context?: React.Context<ModalButtonContext>;
  color?: 'inherit' | 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning';
  variant?: 'text' | 'outlined' | 'contained';
  disabled?: boolean;
  content?: React.ReactNode;
  size?: "sm" | "md" | "lg" | "xl";
  title?: React.ReactNode;
  children?: React.ReactNode;
  fullWidth?: boolean;
  startIcon?: React.ReactNode; // Thêm prop này
  className?: string; // Thêm prop này
}

const Modal_Button = (props: ModalButtonProps) => {
  const [open, setOpen] = useState(false);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const contextValue = props.context ? useContext(props.context) : null;

  const handleClose = () => {
    setOpen(false);
    if (contextValue) {
      const { handleDataChange } = contextValue;
      handleDataChange();
    }
  };

  const getMaxWidth = () => {
    switch (props.size) {
      case 'sm': return 'sm';
      case 'md': return 'md';
      case 'lg': return 'lg';
      case 'xl': return 'xl';
      default: return 'xl';
    }
  };

  return (
    <>
      <Button
        className={props.className}
        color={props.color || 'primary'}
        variant={props.variant || 'contained'}
        onClick={() => setOpen(true)}
        disabled={props.disabled}
        fullWidth={props.fullWidth}
        startIcon={props.startIcon} // Thêm startIcon
      >
        {props.content}
      </Button>

      <Dialog
        open={open}
        onClose={handleClose}
        maxWidth={getMaxWidth()}
        fullWidth
        fullScreen={isMobile}
        className="custom-modal"
        PaperProps={{
          sx: {
            borderRadius: isMobile ? 0 : 2,
            maxHeight: '90vh',
          }
        }}
        disableEscapeKeyDown
      >
        <DialogTitle
          className="custom-modal-header"
          sx={{

            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          {props.title ? (
            <Typography
              variant="h6"
              component="div"
              className="custom-modal-title"
            >
              {props.title}
            </Typography>
          ) : (
            <>
              <div className='flex'>
                <img src={logo || "/placeholder.svg"} alt="Logo" className="custom-modal-header-brand-logo w-10 h-10 aspect-square rounded-full object-cover" />
                <div className="custom-modal-header-brand-text">
                  <span className="custom-modal-header-brand-text-title">Mystic Tales</span>
                  <span className="custom-modal-header-brand-text-subtitle">STUDIO</span>
                </div>  
              </div>
            </>
          )}

          <IconButton
            aria-label="close"
            onClick={handleClose}
            sx={{
              color: 'var(--primary-green)',
            }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>

        <DialogContent
          sx={{
            p: 0,
            '&.MuiDialogContent-root': {
              paddingTop: 0,
            }
          }}
        >
          {React.isValidElement(props.children)
            ? React.cloneElement(props.children as React.ReactElement<any>, { onClose: handleClose })
            : props.children}
        </DialogContent>
      </Dialog>
    </>
  )
}

export default Modal_Button