import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navigation from './components/Navigation';
import Page1 from './pages/Page1';
import Page2 from './pages/Page2';
import Page3 from './pages/Page3';
import './App.css'
import LocalStorage from './pages/LocalStorage';
import S3Storage from './pages/S3Storage';

function App() {
  return (
    <Router>
      <div className="app">
        <Navigation />
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Page1 />} />
            <Route path="/page2" element={<Page2 />} />
            <Route path="/page3" element={<Page3 />} />
            <Route path="/LocalStorage" element={<LocalStorage />} />
            <Route path="/S3Storage" element={<S3Storage />} />

          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
