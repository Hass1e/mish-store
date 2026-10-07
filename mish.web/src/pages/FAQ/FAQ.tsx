import React, { useState } from 'react';
import './FAQ.css';

interface FAQItem {
  question: string;
  answer: string;
}

const faqData: FAQItem[] = [
  {
    question: 'Что такое Mish?',
    answer: 'Mish — это учебный пет-проект, созданный для изучения и практики веб-разработки. Здесь я экспериментирую с современными технологиями, такими как React, ASP.NET Core и PostgreSQL, чтобы совершенствовать свои навыки full-stack разработки.'
  },
  {
    question: 'Кто создатель этого магазина?',
    answer: 'Магазин создан начинающим разработчиком в рамках самообучения. Это мой первый серьезный проект, где я применяю полученные знания на практике. Все функции разрабатываются самостоятельно с целью создания полноценного интернет-магазина одежды.'
  },
  {
    question: 'Как сделать заказ?',
    answer: 'Для оформления заказа выберите понравившиеся товары, добавьте их в корзину, перейдите к оформлению, заполните данные для доставки и выберите способ оплаты. После подтверждения заказа вы получите уведомление на email.'
  },
  {
    question: 'Какие способы оплаты доступны?',
    answer: 'На данный момент принимаются оплата банковскими картами (Visa, Mastercard, МИР) и электронными кошельками. В будущем планируется добавить оплату при получении.'
  },
  {
    question: 'Сколько времени занимает доставка?',
    answer: 'Доставка по городу занимает 1-3 рабочих дня. Доставка в другие регионы — от 3 до 7 рабочих дней в зависимости от удаленности. Точные сроки будут указаны при оформлении заказа.'
  },
  {
    question: 'Можно ли вернуть товар?',
    answer: 'Да, вы можете вернуть товар в течение 14 дней с момента получения, если он не был в использовании и сохранены все бирки и упаковка. Для оформления возврата свяжитесь с нашей поддержкой.'
  },
  {
    question: 'Как связаться с поддержкой?',
    answer: 'Вы можете написать нам на email: support@mish-store.ru или воспользоваться формой обратной связи на сайте. Мы стараемся отвечать в течение 24 часов в рабочие дни.'
  },
  {
    question: 'Из каких материалов сделана одежда?',
    answer: 'Мы используем качественные материалы: хлопок, лен, вискозу и смесовые ткани. Подробная информация о составе каждого товара указана на странице продукта. Мы тщательно подбираем поставщиков и контролируем качество.'
  },
  {
    question: 'Как оставить отзыв о товаре?',
    answer: 'После получения заказа вы можете оставить отзыв на странице товара. Для этого необходимо быть зарегистрированным пользователем и иметь подтвержденную покупку данного товара. Ваши отзывы помогают другим покупателям!'
  },
  {
    question: 'Проводятся ли распродажи?',
    answer: 'Да, мы регулярно проводим сезонные распродажи и специальные акции. Следите за новостями на главной странице или подпишитесь на нашу рассылку, чтобы не пропустить выгодные предложения!'
  }
];

const FAQ: React.FC = () => {
  const [openIndexes, setOpenIndexes] = useState<Set<number>>(new Set());

  const toggleQuestion = (index: number) => {
    setOpenIndexes(prev => {
      const newSet = new Set(prev);
      if (newSet.has(index)) {
        newSet.delete(index);
      } else {
        newSet.add(index);
      }
      return newSet;
    });
  };

  return (
    <div className="faq-container">
      <h1 className="faq-title">Часто задаваемые вопросы</h1>
      
      <div className="faq-list">
        {faqData.map((item, index) => (
          <FAQItemComponent
            key={index}
            question={item.question}
            answer={item.answer}
            isOpen={openIndexes.has(index)}
            onToggle={() => toggleQuestion(index)}
          />
        ))}
      </div>
    </div>
  );
};

interface FAQItemProps {
  question: string;
  answer: string;
  isOpen: boolean;
  onToggle: () => void;
}

const FAQItemComponent: React.FC<FAQItemProps> = ({ question, answer, isOpen, onToggle }) => {
  return (
    <div className={`faq-item ${isOpen ? 'open' : ''}`}>
      <button 
        className="faq-question"
        onClick={onToggle}
        aria-expanded={isOpen}
      >
        <span className="faq-question-text">{question}</span>
        <span className={`faq-icon ${isOpen ? 'rotate' : ''}`}>
          ▼
        </span>
      </button>
      
      <div className={`faq-answer-wrapper ${isOpen ? 'open' : ''}`}>
        <div className="faq-answer">
          <p>{answer}</p>
        </div>
      </div>
    </div>
  );
};

export default FAQ;