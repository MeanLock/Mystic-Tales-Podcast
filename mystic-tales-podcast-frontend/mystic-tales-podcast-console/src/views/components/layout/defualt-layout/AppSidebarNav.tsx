import React, { useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { CBadge, CNavLink, CSidebarNav } from '@coreui/react'
// Define types for nav items
interface Badge {
  color: string;
  text: string;
}

interface NavItemType {
  component: React.ElementType;
  name?: React.ReactNode;
  badge?: Badge;
  icon?: React.ReactNode;
  to?: string;
  href?: string;
  items?: NavItemType[];
  [key: string]: any;
}

interface AppSidebarNavProps {
  items: NavItemType[];
}

export const AppSidebarNav = ({ items }: AppSidebarNavProps) => {
  const navLink = (
    name: React.ReactNode,
    icon?: React.ReactNode,
    badge?: Badge,
    indent: boolean = false
  ) => {
   
    return (
      <>
        {icon
          ? icon
          : indent && (
            <span className="nav-icon">
              <span className="nav-icon-bullet"></span>
            </span>
          )}
          <span className="nav-text">{name}</span>

        {/* {name && name} */}
        {badge && (
          <CBadge color={badge.color} className="ms-auto" size="sm">
            {badge.text}
          </CBadge>
        )}
      </>
    )
  }

  const navItem = (item: NavItemType, index: number, indent: boolean = false) => {
    const { component, name, badge, icon, ...rest } = item
    const Component = component
    return (
      <Component as="div" key={index}>
        {rest.to || rest.href ? (
          <CNavLink 
            {...(rest.to && { as: NavLink })}
            {...(rest.href && { target: '_blank', rel: 'noopener noreferrer' })}
            {...rest}
          >
            {navLink(name, icon, badge, indent)}
          </CNavLink>
        ) : (
          navLink(name, icon, badge, indent)
        )}
      </Component>
    )
  }

  const navGroup = (item: NavItemType, index: number) => {
    const { component, name, icon, items, to, ...rest } = item
    const Component = component
    return (
      <Component compact as="div" key={index} toggler={navLink(name, icon)} {...rest}>
        {items?.map((item, index) =>
          item.items ? navGroup(item, index) : navItem(item, index, true),
        )}
      </Component>
    )
  }

  return (
    <CSidebarNav className="app-sidebar__nav">
      {items &&
        items.map((item, index) => (item.items ? navGroup(item, index) : navItem(item, index)))}
    </CSidebarNav>
  )
}

AppSidebarNav.propTypes = {
  items: PropTypes.arrayOf(PropTypes.any).isRequired,
}
