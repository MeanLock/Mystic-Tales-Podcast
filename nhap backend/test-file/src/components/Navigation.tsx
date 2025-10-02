import { Link } from 'react-router-dom';
import './Navigation.css';

const Navigation = () => {
  return (
    <nav className="navigation">
      <ul className="nav-list">
        <li className="nav-item">
          <Link to="/" className="nav-link">Page 1 - URLs</Link>
        </li>
        <li className="nav-item">
          <Link to="/page2" className="nav-link">Page 2</Link>
        </li>
        <li className="nav-item">
          <Link to="/page3" className="nav-link">Page 3</Link>
        </li>
        <li className="nav-item">
          <Link to="/LocalStorage" className="nav-link">LocalStorage</Link>
        </li>
        <li className="nav-item">
          <Link to="/S3Storage" className="nav-link">S3Storage</Link>
        </li>
      </ul>
    </nav>
  );
};

export default Navigation;