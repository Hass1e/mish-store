import {HeroBanner} from './HeroBanner'

const HERO_SLIDES = [
  {
    id: 1,
    imageUrl: 'https://placehold.co/1280x540/ff6b35/fff?text=Акция',
    title: 'Скидки на летнюю коллекцию',
    subtitle: 'Только до конца месяца',
    ctaText: 'Купить сейчас',
    ctaLink: '/catalog?sale=true',
  },
  {
    id: 2,
    imageUrl: 'https://placehold.co/1280x540/fd3b35/fff?text=Новинка',
    title: 'Новая коллекция осень-зима',
    subtitle: 'Откройте для себя новые тренды',
    ctaText: 'Посмотреть коллекцию',
    ctaLink: '/catalog?/new',

  },
  {
    id: 3,
    imageUrl: 'https://placehold.co/1280x540/dd3b35/fff?text=Внимание',
    title: 'Акция до конца сезона',
    subtitle: 'Бери 1+1',
    ctaText: 'Посмотреть коллекцию',
    ctaLink: '/catalog?/new',

  },

]

export function Home()  {
    return (
        <>
        <HeroBanner
        slides={HERO_SLIDES}
        autoPlayDelay={6000} 
        />
        <main>  
            <section className="home__featured"></section>
        </main>
        </>
    )
}
