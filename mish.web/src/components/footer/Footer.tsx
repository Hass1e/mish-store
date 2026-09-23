import "./Footer.css";
import { Link } from "react-router-dom";

export function Footer() {
  return (
    <footer className="site-footer">
      <div className="footer-content">
        <div className="footer-section">
          <h3>О нас</h3>
          <p>Краткая информация о компании или проекте.</p>
            <Link to="/atool" style={{ margin: "110px" }}>
              Create product
            </Link>
        </div>

        <div className="footer-section">
          <h3>Помощь</h3>
          <ul>
            <li><Link to="/faq">FAQ</Link></li>
            <li><Link to="/support">Поддержка</Link></li>
            <li><Link to="/terms">Условия использования</Link></li>
           
           
          </ul>
        </div>
      </div>

      <div className="footer-bottom">
        <p>&copy; {new Date().getFullYear()} Пэт-проект, все исходники в github</p>
      </div>
    </footer>
  );
}

export default Footer;