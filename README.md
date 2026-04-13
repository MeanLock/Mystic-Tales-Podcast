# Mystic Tales Podcast Platform

Welcome to the **Mystic Tales Podcast Platform** repository! This project is a comprehensive podcast streaming and management platform built for both web and mobile. It features a robust subscription mechanism, empowers podcasters to seamlessly upload and edit audio content before publishing, and ensures high-level content security and performance by utilizing `hls.js` to split stream audio into encrypted segments.

While there are backend and infrastructure layers to this project, this document focuses primarily on the **Front-End applications** where my efforts were concentrated as a Front-End Developer.

Product Link: https://mystic-tales-podcast.xyz (The backend is currently shut down.).

Team Size: 4 (2 Front-End, 2 Back-End)

<img width="2852" height="1560" alt="image" src="https://github.com/user-attachments/assets/99292086-41ff-4735-88ba-f62919686a0b" />


<img width="2879" height="1564" alt="image" src="https://github.com/user-attachments/assets/dc70b76c-f134-4967-96ca-b227da3a370c" />


---

## 👨‍💻 My Role as a Front-End Developer

In this project, my responsibilities spanned across designing the architecture, building the UI, and solving critical client-side infrastructure problems:

- **Customer UI Construction**: Developed the user interfaces for the listeners on both the web application and the cross-platform mobile application.
- **Audio Streaming Integration**: Directly integrated `hls.js` for fetching, decrypting, and playing audio in segments to protect content from piracy.
- **Content Discovery & Query Systems**: Designed the business-rule layers for varied content queries and multi-faceted discoverability for users (the actual database execution of these queries was handled by the backend, but I designed the query models and integration flows).
- **Audio Integrity & Copyright Verification**: Implemented complex front-end business logic and flows for validating audio uploads to prevent content duplication and integrated a strict DMCA workflow to protect intellectual property.

---
## Document

For more about this project, please check the User Manual in this document:

[Mystic Tales Podcast Document](https://docs.google.com/document/d/13VzIDm5HZIbK4ewQbG2sdWmUuqhIv1Px/edit?usp=sharing&ouid=100826371641594181419&rtpof=true&sd=true)

---

## 📁 Front-End Applications Architecture

The frontend monorepo consists primarily of two core applications:

### 1. `mystic-tales-podcast-platform-web`
This is the web-based entry point to the platform for both podcasters (who need sophisticated dashboards to manage content) and web listeners.

**Tech Stack**:
- **Framework & Build**: React 19, Vite, TypeScript
- **Styling**: Tailwind CSS v4, Radix UI (Headless components), Framer Motion (Animations)
- **State Management & Data Fetching**: Redux Toolkit, RTK Query, Redux Persist
- **Routing**: React Router DOM (v7)
- **Forms & Validation**: React Hook Form, Zod
- **Media & Streaming**: `hls.js`, `react-player`
- **Rich Text Editing**: Quill, Tiptap
- **PDF & Document Handling**: `@pdf-lib`, `react-pdf`

**Directory Structure (`src/`)**:
- `/assets`: Static imagery and styles (`index.css`).
- `/components`: Reusable UI components.
- `/core`: Core utilities, API clients, and foundational logic.
- `/layouts`: Page layout wrappers (e.g., AuthLayout, DashboardLayout).
- `/lib`: Helper functions and third-party wrappers.
- `/pages`: Route-based views.
- `/redux`: Redux slices, RTK Query API endpoints, and store configuration.
- `/route`: React Router configuration mapping.

### 2. `customer-mobile`
This is the dedicated mobile application targeting listeners, providing them with a smooth, native-like podcast listening experience on iOS and Android.

**Tech Stack**:
- **Framework**: React Native with Expo (~v54.0), TypeScript
- **Routing**: Expo Router (File-based routing setup), React Navigation
- **Styling**: Nativewind (Tailwind CSS for React Native)
- **State Management & Data Fetching**: Redux Toolkit, RTK Query, Redux Persist
- **Media & Audio**: Expo AV
- **Storage**: AsyncStorage, Expo Secure Store
- **Other Key Libraries**: Bottom Sheet, Reanimated, Gesture Handler

**Directory Structure (`src/`)**:
- `/app`: Expo Router file-based routing and screens.
- `/components`: Reusable native UI components.
- `/constants`: Theme values, configuration, and static hardcoded data.
- `/core`: General setup modules.
- `/data`: Local data models or adapters.
- `/features`: Domain-driven feature modules.
- `/hooks`: Custom React hooks.
- `/lib`: External service integrations and library wrappers.
- `/store`: Redux store configuration and RTK Query definitions.
- `/types`: Global TypeScript interfaces.
- `/utils`: Helper functions and formatters.

---

## 💡 Key Technical Learnings & Insights

Building these platforms provided several crucial challenges and learning opportunities:

### 1. Handling Heavy Transactions via Long-Polling
To handle complex and long-running backend operations (such as audio processing or heavy content validations), we adopted a **Saga / Long-Polling pattern**. 
- Whenever I triggered a heavy API request, the server immediately returned a `Saga Instance ID`.
- The client would then use this ID to poll a status endpoint to retrieve the eventual result. 
- **The Takeaway**: This was an elegant and highly effective approach to bypass timeouts and handle asynchronous, long-lasting operations seamlessly on the front end without maintaining a continuous open connection like WebSockets for simple request-response checks.

### 2. Secure Cloud Media Delivery
Dealing with media files stored on **Amazon S3** required a hardened approach compared to serving static, public URLs.
- Instead of direct public links, the database provided **media keys**.
- The client application had to explicitly request a signed/accessible URL via a backend intermediary using that key, ensuring that only authenticated users with sufficient permissions could fetch and stream the private media.

### 3. State Management: The RTK Query Trade-offs
Using **RTK Query** from Redux Toolkit drastically simplified data fetching and caching for the vast amounts of content required by a podcast platform. However, I learned an important lesson regarding the *limitations* of aggressive caching:
- Utilizing cache blindly resulted in significant bugs for highly dynamic features. For example, when keeping track of **Listening History** across multiple browser tabs, a stale cache meant the user would not see their most recent listening activity.
- The default behavior kept the cache alive, meaning navigating back to a page wouldn't trigger a reload.
- **The Takeaway**: I learned how to actively balance caching versus real-time data needs. I had to strategically use cache invalidation patterns (`providesTags` / `invalidatesTags`) and force re-fetching strategies (like `refetchOnMountOrArgChange`) where the context demanded live data synchronization rather than displaying outdated views.

---

*This README serves as an overview of my contributions and the core structure of the frontend apps. Please navigate to the respective directories (`mystic-tales-podcast-platform-web` and `customer-mobile`) for further instructions on how to install dependencies and run each project locally.*
