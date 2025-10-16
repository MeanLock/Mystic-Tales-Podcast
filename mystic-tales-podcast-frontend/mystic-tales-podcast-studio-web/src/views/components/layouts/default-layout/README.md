# Default Layout System

## Tổng quan
Layout system này được thiết kế để hỗ trợ navigation context switching, cho phép sidebar thay đổi theo ngữ cảnh (user profile hoặc show profile).

## Cấu trúc

### 1. Layout Components
- **DefaultLayout**: Main layout container với flexbox structure
- **DefaultLayoutHeader**: Fixed header với logo, search bar và action buttons
- **DefaultLayoutSidebar**: Sidebar navigation với user/show profile và nav items
- **DefaultLayoutContent**: Main content area với routing

### 2. Redux Navigation State
```typescript
interface NavigationState {
  contextType: 'user' | 'show';
  currentContext: {
    id: string;
    name: string;
    avatar?: string;
    email?: string;
    subtitle?: string;
  } | null;
  navItems: Array<{
    label: string;
    icon: string;
    path: string;
  }>;
}
```

### 3. Hook: useNavigationContext
```typescript
const { switchToUserContext, switchToShowContext } = useNavigationContext();

// Switch to user context
switchToUserContext({
  id: 'user1',
  name: 'SAMURICE',
  email: 'samurice@gmail.com',
  avatar: 'avatar-url'
});

// Switch to show context
switchToShowContext({
  id: 'show1',
  name: 'Tech Talk Podcast',
  avatar: 'show-avatar-url'
});
```

## SCSS Structure (BEM)

### Classes
- `.default-layout`
- `.default-layout__header`
- `.default-layout__header-brand`
- `.default-layout__header-search`
- `.default-layout__header-actions`
- `.default-layout__sidebar`
- `.default-layout__sidebar-profile`
- `.default-layout__sidebar-nav`
- `.default-layout__content`

### Context Modifiers
- `.default-layout--show-context`: Applied when contextType is 'show'

## Features

### 1. Context Switching
- **User Context**: Hiển thị thông tin user với navigation items cho podcaster
- **Show Context**: Hiển thị thông tin show với navigation items cho show management

### 2. Responsive Design
- Fixed header (64px height)
- Fixed sidebar (240px width)
- Responsive content area
- Mobile support với collapsible sidebar

### 3. Authentication Guard
- Kiểm tra user có token hợp lệ
- Kiểm tra user có podcasterProfile
- Redirect về /login nếu không đáp ứng điều kiện

## Usage Example

```typescript
// In a show detail page
const ShowDetail = () => {
  const { switchToShowContext } = useNavigationContext();
  
  useEffect(() => {
    // Switch navigation context to show
    switchToShowContext({
      id: showId,
      name: showData.name,
      avatar: showData.avatar
    });
  }, [showId, showData]);
  
  return (
    <div>Show content...</div>
  );
};
```

## Navigation Items

### User Navigation (_podcasterNav)
- Dashboard
- Earn
- Copyright
- My Channels
- My Shows
- Booking Management

### Show Navigation (Dynamic)
- Show Dashboard
- Episodes
- Analytics
- Settings
- Back to Profile

## Styling

Sử dụng SCSS với BEM methodology và MUI components. Colors:
- Primary: #4CAF50 (Green)
- Background: #2d2d2d (Dark)
- Show context: #FF9800 (Orange)
- Text: white/gray variations