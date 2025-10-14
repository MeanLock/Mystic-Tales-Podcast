export const getRoleNavItems = (isAdmin: number): {
  label: string,
  path: string,
}[] => {
  const _roleNav = [
    // Admin  
    [
      {
        label: 'Perfumes',
        path: '/perfumes',
      },
      {
        label: 'Comments',
        path: '/comments',
      },
      {
        label: 'Profile',
        path: '/profile',
      },
      {
        label: 'Perfumes Manage',
        path: '/admin/perfumes',
      },
      {
        label: 'Brands Manage',
        path: '/admin/brands',
      },
      {
        label: 'Collectors Manage',
        path: '/admin/collectors',
      },
      {
        label: 'Survey',
        path: '/survey',
      },
    ],
    // Normal Member
    [
      {
        label: 'Perfumes',
        path: '/perfumes',
      },
      {
        label: 'Comments',
        path: '/comments',
      },
      {
        label: 'Profile',
        path: '/profile',
      },
      {
        label: 'Survey',
        path: '/survey',
      },
    ]

  ]
  const roleNav = _roleNav[isAdmin]

  return roleNav;
}

export const _nonLoginNav: {
  label: string,
  path: string,
}[] = [
    {
      label: 'Perfumes',
      path: '/perfumes',
    }
  ]

