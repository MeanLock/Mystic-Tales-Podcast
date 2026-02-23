import React, { FC, useEffect, useState } from 'react'
import {
  CAvatar,
  CBadge,
  CButtonGroup,
  CDropdown,
  CDropdownDivider,
  CDropdownHeader,
  CDropdownItem,
  CDropdownMenu,
  CDropdownToggle,
} from '@coreui/react'
import {
  cilBell,
  cilCreditCard,
  cilCommentSquare,
  cilEnvelopeOpen,
  cilFile,
  cilLockLocked,
  cilSettings,
  cilTask,
  cilUser,
} from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { AuthState } from '../../../../../redux/auth/auth.slice'
import { avatar8 } from '../../../../../assets/images'
import AvatarInput from '@/views/components/common/avatar'
import Image from '@/views/components/common/image'
import { useSelector } from 'react-redux'
import { RootState } from '@/redux/root-reducer'
import StaffUpdate from './StaffUpdate'
import Modal_Button from '@/views/components/common/modal/ModalButton'

type UserType = AuthState['user']

const AppHeaderDropdown: FC<{ user: AuthState['user'] }> = ({ user }) => {
  const [showProfileModal, setShowProfileModal] = useState(false);

  return (
    <>
      <CDropdown variant="nav-item">
        <CDropdownToggle className="py-0 pe-0" caret={false}>
          <Image mainImageFileKey={user.MainImageFileKey} className="rounded-full w-8 h-8" />
        </CDropdownToggle>
        {user.role_id == 2 && (
          <CDropdownMenu className="pt-0">
            <CDropdownHeader className="bg-body-secondary fw-semibold mb-2">Account</CDropdownHeader>
            <Modal_Button
              disabled={false}
              title="Update Profile"
              className='w-full'
              content={
                <CDropdownItem>
                  <CIcon icon={cilUser} className="me-2" />
                  Profile
                </CDropdownItem>
              }
              color="white"
            >
              <StaffUpdate onClose={() => { }} />
            </Modal_Button>
          </CDropdownMenu>
        )}

      </CDropdown>
    </>
  )
}

export default AppHeaderDropdown