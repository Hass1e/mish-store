import React from 'react';
import './Support.css';

const Support: React.FC = () => {
  return (
    <div className="support-container">
      <h1 className="support-title">Поддержка</h1>
      
      <div className="support-content">

        <p className="support-intro">
        MISH — это full-stack pet-проект, с уклоном в бекенд, демонстрирующее применение современных архитектурных паттернов. <br />
        Бэкенд реализован на <strong>ASP.NET Core (.NET 10)</strong> <br />
        Фронтенд построен на <strong>React + TypeScript</strong>
        </p>


        <p className="support-intro">
          Вы можете связаться со мной следующими способами:
        </p>


        <div className="contact-cards">
          <a 
            href="https://t.me/ES_Deuterium" 
            target="_blank" 
            rel="noopener noreferrer" 
            className="contact-card"
          >
            <div className="contact-icon">💬</div>
            <h3>Telegram</h3>
            <span className="contact-link">@ES_Deuterium</span>
          </a>

          <a 
            href="https://github.com/Hass1e/mish-store" 
            target="_blank" 
            rel="noopener noreferrer" 
            className="contact-card"
          >
            <div className="contact-icon">💻</div>
            <h3>GitHub</h3>
            <span className="contact-link">github.com/Hass1e/mish-store</span>
          </a>
        </div>

        <div className="support-info">
          <p className="response-time">
            Отвечаю в течение 1-2 часов. Любая обратная связь приветствуется!
          </p>
        </div>
      </div>
    </div>
  );
};

export default Support;